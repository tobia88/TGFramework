using System.Collections;
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
    }

    public struct Node
    {
        public int x;
        public int y;
    }

    public Leadiy_M7B m7bResolver;
    public LMBasePortInput m7bPort;

    public int ColumnCount { get; private set; }
    public int RowCount { get; private set; }
    public bool Is3D { get; private set; }
    public const string CLEAR_PATH = "CB01FD";

    private string m_currentKey;
    private string m_currentValue;
    private Rect m_worldBound;
    private LMGrindTableEmulator m_emulator;

    public System.Action onTestFinished;
    public System.Action onTestStarted;
    public System.Action<Vector3> onTurnOffLight;
    public Queue<GrindEvent> eventQueue = new Queue<GrindEvent>();
    public LMGrindTableEmulator GrindTable { get { return Emulator as LMGrindTableEmulator; } }

    public override bool OpenPort()
    {
        if (base.OpenPort())
        {
            int udp = controller.gameConfig.GetValue("UDP", -1);

            if (udp >= 0)
            {
                m7bPort = new LMInput_UDP();
                (m7bPort as LMInput_UDP).Init(controller, KeyportData, udp);
            }
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

        ColumnCount = keyportData.width;
        RowCount = keyportData.height;
    }

    public void InitTable(Rect bound, bool is3D)
    {
        this.Is3D = is3D;
        this.m_worldBound = bound;
    }

    public override bool OnUpdate()
    {
        if (base.OnUpdate())
        {
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
        switch (evt.key)
        {
            case "CJ":
                Debug.Log("Test Started");

                if (onTestStarted != null)
                    onTestStarted();

                break;

            case "CB":
                Debug.Log("Test Ended");

                if (onTestFinished != null)
                    onTestFinished();
                break;

            case "CC":
                Debug.Log("On Turn Off Light: " + m_currentValue);

                if (onTurnOffLight != null)
                    onTurnOffLight(CodeToVector(m_currentValue));
                break;
        }
    }

    public const string PATH_FORMAT = "Y{0}Z";

    public void Write(Vector3[] path)
    {
        string content = string.Empty;
        string lastCode = string.Empty;
        string newCode = string.Empty;

        for ()

        foreach (var p in path)
        {
            var node = GetNode(p);
            Debug.Log("Get Node: " + node.x + ", " + node.y);
            newCode = NodeToCode(node);

            if (lastCode == newCode)
                continue;

            lastCode = newCode;

            if (controller.inputSetting.IsTesting)
            {
                GrindTable.SetBtnEnable(node.x, node.y, EmuTableBtnStates.Waiting);
                continue;
            }

            content += newCode;
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

    public void DrawArc(Vector3 center, float radius, int fromDeg, int toDeg)
    {
        var list = new List<Vector3>();

        for (int i = fromDeg; i <= toDeg; i++)
        {
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
        Write(CLEAR_PATH, false);
        if (m7bPort != null)
            m7bPort.Close();
    }

    private string NodeToCode(Node node)
    {
        int colIndex = 65 + node.x;
        int rowIndex = 65 + node.y;

        // YZ has been used for head and tail, so just skip it
        if (colIndex >= 89)colIndex += 2;
        if (rowIndex >= 89)rowIndex += 2;

        char colChar = (char)colIndex;
        char rowChar = (char)rowIndex;

        return colChar.ToString() + rowChar.ToString();
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

        Node n = new Node();

        n.x = Mathf.RoundToInt(ratioX * (ColumnCount - 1));
        n.y = Mathf.RoundToInt(ratioY * (RowCount - 1));

        return n;
    }

    private Vector3 CodeToVector(string code)
    {
        Debug.Log("Code To Vector: " + code);

        int colIndex = (int)code[0];
        int rowIndex = (int)code[1];

        // YZ has been used for head and tail, so just skip it
        if (colIndex >= 89)colIndex -= 2;
        if (rowIndex >= 89)rowIndex -= 2;

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
        bytes = LMUtility.RemoveSpacing(bytes);

        m_getString += Encoding.UTF8.GetString(bytes);

        if (m_getString.IndexOf(';') >= 0)
        {
            string[] split = m_getString.Split(';');

            for (int i = split.Length - 1; i >= 0; i--)
            {
                if (split[i].Length != 7 || split[i].IndexOf(':') < -1)
                    continue;

                split = split[i].Split(':');

                string key = split[0];
                string value = split[1].TrimStart('0');

                SetValue(key, value);

                m_getString = string.Empty;
                break;
            }
        }
    }

    private void SetValue(string key, string value)
    {
        if (m_currentKey == key && m_currentValue == value)
            return;

        m_currentKey = key;
        m_currentValue = value;

        eventQueue.Enqueue(new GrindEvent(m_currentKey, m_currentKey));
    }
}