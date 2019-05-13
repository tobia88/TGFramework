﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LMGrindTable : LMInput_Port
{
    public struct GrindEvent
    {
        public string key;
        public string value;

        public GrindEvent(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("Key:{0}, Value:{1}", key, value);
        }
    }

    public struct Node
    {
        public int x;
        public int y;
    }

    public const string CLEAR_PATH = "CB01FD";
    public const string PATH_FORMAT = "Y{0}Z";
    public const int SKIP_LENGTH = 8; //Skip along from code 88 to 96

    public Leadiy_M7B m7bResolver;
    public LMBasePortInput m7bPort;

    public int ColumnCount { get; private set; }
    public int RowCount { get; private set; }
    public bool Is3D { get; private set; }

    private string m_currentKey;
    private string m_currentValue;

    private Rect m_worldBound;
    public System.Action<bool> onTestFinished;
    public System.Action onTestStarted;
    public System.Action<Vector3> onTurnOffLight;
    public Queue<GrindEvent> eventQueue = new Queue<GrindEvent>();
    public LMGrindTableEmulator GrindTableEmu { get { return Emulator as LMGrindTableEmulator; } }

    public override bool OpenPort()
    {
        if (base.OpenPort())
        {
            int udp = controller.gameConfig.GetValue("UDP", -1);

            //如果监测udp开启了，则使用udp做为3D传感器的端口
            if (udp >= 0)
            {
                m7bPort = new LMInput_UDP();
                (m7bPort as LMInput_UDP).Init(controller, KeyportData, udp);
            }
            //否则使用serial port做为3D传感器端口
            else
            {
                m7bPort = new LMInput_Port();
                (m7bPort as LMInput_Port).Init(controller,
                    KeyportData,
                    controller.gameConfig.GetValue("端口2", -1));
            }
            return true;
        }
        return false;
    }

    public override void Init(TGController _controller, KeyPortData keyportData, int _com)
    {
        base.Init(_controller, keyportData, _com);

        //获取keyInputConfig.json里的行列数
        ColumnCount = keyportData.width;
        RowCount = keyportData.height;
    }

    //清除所有磨砂版的灯
    public void ClearLights()
    {
        Debug.Log("Clear Lights");

        Write(CLEAR_PATH, false);
        eventQueue.Clear();

        if (IsTesting)
            GrindTableEmu.Reset();
    }

    //设置映射的区域
    public void InitTable(Rect bound, bool is3D)
    {
        this.Is3D = is3D;
        this.m_worldBound = bound;
    }

    public override bool OnUpdate()
    {
        if (IsTesting)
            GrindTableEmu.OnUpdate();

        if (base.OnUpdate())
        {
            //如果存在排序中的事件，则按顺序触发该事件
            if (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                TriggerEvent(evt);
            }

            return true;
        }
        return false;
    }

    private void TriggerEvent(GrindEvent evt)
    {
        Debug.Log(string.Format("Event Triggered: " + evt));
        switch (evt.key)
        {
            case "CJ":
                // 任务开始事件
                Debug.Log("Train Start");
                if (onTestStarted != null)
                    onTestStarted();

                break;

            case "CB":
                // 如果获取CB:0001则表示顺利完成任务
                // 反之获取CB:0002则表示任务失败，需要重来
                bool result = evt.value == "1";
                Debug.Log("Train Finished, Result: " + result);
                if (onTestFinished != null)
                    onTestFinished(result);

                break;

            case "CC":
                // 砂磨板消除灯之后获取的事件
                Debug.Log("On Turn Off Light: " + evt.value);

                if (onTurnOffLight != null)
                    onTurnOffLight(CodeToVector(evt.value));
                break;
        }
    }

    public void Write(Vector3[] path)
    {
        // 用来保存需要发送到端口处的转译后的位置编号
        string content = string.Empty;

        // 用来保存最后一次的编号，以避免重复发送同样的编号
        string lastCode = string.Empty;

        // 记录最新的编号
        string newCode = string.Empty;

        for (int i = 0; i < path.Length; i++)
        {
            var node = GetNode(path[i]);
            Debug.Log("Get Node: " + node.x + ", " + node.y);

            // 将位置转译成磨砂版接收的编号
            newCode = NodeToCode(node);

            if (lastCode == newCode)
                continue;

            content += newCode;
            lastCode = newCode;

            // 测试时候发送到模拟器
            if (IsTesting)
            {
                GrindTableEmu.SetBtnEnable(node.x, node.y);
            }

        }

        Write(string.Format(PATH_FORMAT, content), false);
    }

    public void DrawLine(Vector3 from, Vector3 to)
    {
        var list = new List<Vector3>();

        list.Add(from);

        for (float i = 0f, step = 0.05f; i < 1f; i += step)
        {
            var addPos = Vector3.Lerp(from, to, i);
            list.Add(addPos);
        }

        list.Add(to);

        Write(list.ToArray());
    }

    public void DrawArc(Vector3 center, float radius, int fromDeg, int toDeg, bool counterClockwise)
    {
        var list = new List<Vector3>();

        // 顺时针还是逆时针
        var sign = (counterClockwise) ? 1 : -1;

        // 把值域限定在[0,359]之间
        if (fromDeg < 0)fromDeg = (fromDeg % 360) + 360;
        if (toDeg < 0)toDeg = (toDeg % 360) + 360;

        fromDeg %= 360;
        toDeg %= 360;

        for (int i = fromDeg;
            (sign > 0) ? i <= toDeg : i >= toDeg; i += sign)
        {
            // 把i值锁定在[0,360]之间
            if (i < 0)i += 360;

            i %= 360;

            var np = new Vector3();
            var rad = i * Mathf.Deg2Rad;

            if (Is3D)
            {
                np.x = Mathf.Cos(rad) * radius + center.x;
                np.z = Mathf.Sin(rad) * radius + center.z;
            }
            else
            {
                np.x = Mathf.Cos(rad) * radius + center.x;
                np.y = Mathf.Sin(rad) * radius + center.y;
            }

            list.Add(np);
        }

        Write(list.ToArray());
    }

    public override void Close()
    {
        ClearLights();

        if (m7bPort != null)
            m7bPort.Close();

        base.Close();
    }

    private string NodeToCode(Node node)
    {
        int colIndex = 65 + node.x;
        int rowIndex = 65 + node.y;

        // 直接跳过从Y开始到a之前的字符
        if (colIndex >= 89)colIndex += SKIP_LENGTH;
        if (rowIndex >= 89)rowIndex += SKIP_LENGTH;

        char colChar = (char)colIndex;
        char rowChar = (char)rowIndex;

        string retval = colChar.ToString() + rowChar.ToString();

        Debug.Log(string.Format("Writing code x={0} y={1}, return {2}", colIndex, rowIndex, retval));

        return retval;
    }

    private Node GetNode(Vector3 p)
    {
        float ratioX = 0f;
        float ratioY = 0f;

        if (Is3D)
        {
            ratioX = (p.x - m_worldBound.x) / (m_worldBound.width);
            ratioY = (p.z - m_worldBound.y) / (m_worldBound.height);

        }
        else
        {
            ratioX = (p.x - m_worldBound.x) / (m_worldBound.width);
            ratioY = (p.y - m_worldBound.y) / (m_worldBound.height);
        }
        // 逆转Y值
        ratioY = 1f - ratioY;

        ratioX = Mathf.Clamp01(ratioX);
        ratioY = Mathf.Clamp01(ratioY);

        Node n = new Node();

        n.x = Mathf.RoundToInt(ratioX * (ColumnCount - 1));
        n.y = Mathf.RoundToInt(ratioY * (RowCount - 1));

        return n;
    }

    // 将砂磨板接收到的编号转译映射成世界空间坐标系
    private Vector3 CodeToVector(string code)
    {
        Debug.Log("Code To Vector: " + code);

        int colIndex = (int)code[0];
        int rowIndex = (int)code[1];

        if (colIndex >= 89)colIndex -= SKIP_LENGTH;
        if (rowIndex >= 89)rowIndex -= SKIP_LENGTH;

        colIndex -= 65;
        rowIndex -= 65;

        return NodeToVector(colIndex, rowIndex);
    }

    public Vector3 NodeToVector(int x, int y)
    {
        float ratioX = (float)x / (ColumnCount - 1);
        float ratioY = 1f - (float)y / (RowCount - 1);

        float rx = ratioX * m_worldBound.width + m_worldBound.x;
        float ry = ratioY * m_worldBound.height + m_worldBound.y;

        Debug.Log(string.Format("Rx: {0}. Ry: {1}", ratioX, ratioY));

        if (Is3D)
            return new Vector3(rx, 0f, ry);

        return new Vector3(rx, ry);
    }

    public override IEnumerator OnStart(LMBasePortResolver resolver = null)
    {
        yield return controller.StartCoroutine(base.OnStart(resolver));

        // 没有任何错误信息则开启3D传感器的解析器
        if (string.IsNullOrEmpty(ErrorTxt) && m7bPort != null)
        {
            Debug.Log("m7b is started");
            m7bResolver = new Leadiy_M7B();
            yield return controller.StartCoroutine(m7bPort.OnStart(m7bResolver));
            ErrorTxt = m7bPort.ErrorTxt;
        }
    }

    public Vector3 GetAccelerations()
    {
        if (m7bResolver == null)
            return Vector3.zero;

        return m7bResolver.Acceleration;
    }

    public override float GetValue(int index)
    {
        if (m7bResolver == null)
            return 0f;

        return m7bResolver.GetValue(index);
    }

    public override float GetRawValue(int index)
    {
        if (m7bResolver == null)
            return 0f;

        return m7bResolver.GetRawValue(index);
    }

    private string m_getString;

    protected override void ResolveBytes(byte[] bytes)
    {
        // 消除字节中的空值和空格
        bytes = LMUtility.RemoveSpacing(bytes);

        // 把字节转化为字符串并叠加起来
        m_getString += Encoding.UTF8.GetString(bytes);

        // 砂磨板的值会以分号为区分，如CB:0001;CC:00JK;等
        // 因此当分号出现时候，则判断可能获取到数值了
        if (m_getString.IndexOf(';') >= 0)
        {
            // 按分号分割成各别的数组，如[CB:0001,CC:00JK]
            string[] split = m_getString.Split(';');

            // 迭代每个字符串
            for (int i = split.Length - 1; i >= 0; i--)
            {
                Debug.Log("Catch Event: " + split[i]);

                string key = string.Empty;
                string value = string.Empty;

                // 捕获标准数值，如CC:00JK
                if (split[i].Length == 7 && split[i].IndexOf(':') > 0)
                {
                    // 把键值和数值拆分开来，如[CB,0001]
                    split = split[i].Split(':');

                    key = split[0];

                    // 把数值里的0删去
                    value = split[1].TrimStart('0');
                }
                // 捕获其他事件，如CA01FD，CE01FD等
                else
                {
                    key = split[i];
                }

                if (string.IsNullOrEmpty(key))
                    continue;

                Debug.Log("Key: " + key + "，Value: " + value);

                // 存到序列里
                SetValue(key, value);

                // 只要获取任何键值，就清空叠加的字符串
                m_getString = string.Empty;
                break;
            }
        }
    }

    private void SetValue(string key, string value)
    {
        // 由于上位机会重复发送当前的事件，因此需要判断
        // 如果获得的时间存在重复，则跳过
        if (m_currentKey == key && m_currentValue == value)
            return;

        // 把该事件存到事件序列里，依序触发
        var evt = new GrindEvent(key, value);
        eventQueue.Enqueue(evt);

        Debug.Log("Event Enqueue: " + evt);

        m_currentKey = key;
        m_currentValue = value;
    }
}