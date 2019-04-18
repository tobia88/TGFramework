using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

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

    public Leadiy_M7B m7bResolver;
    public LMBasePortInput m7bPort;

    public const int COLUMN_COUNT = 4;
    public const int ROW_COUNT = 12;
    public const string CLEAR_PATH = "CB01FD";

    private string m_currentKey;
    private string m_currentValue;

    private Rect m_worldBound;

    public System.Action onTestFinished;
    public System.Action onTestStarted;
    public System.Action<Vector2> onTurnOffLight;
    public Queue<GrindEvent> eventQueue = new Queue<GrindEvent>();

    public override bool OpenPort()
    {
        if (base.OpenPort())
        {
            int udp = controller.gameConfig.GetValue("UDP", -1);

            if (udp >= 0)
            {
                m7bPort = new LMInput_UDP();
                (m7bPort as LMInput_UDP).Init(controller, udp);
            }
            else
            {
                m7bPort = new LMInput_Port();
                (m7bPort as LMInput_Port).Init(controller,
                                               controller.gameConfig.GetValue("端口2", -1));
            }
            return true;
        }
        return false;
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
        switch(evt.key)
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

    public void Write(Vector2[] path)
    {
        string content = string.Empty;
        string lastCode = string.Empty;
        string newCode = string.Empty;

        foreach (var p in path)
        {
            newCode = VectorToCode(p);

            if (lastCode == newCode)
                continue;

            content += newCode;
            lastCode = newCode;
        }
        
        Write(string.Format(PATH_FORMAT, content), false);
    }

    public void SetWorldBound(Rect rect)
    {
        m_worldBound = rect;
    }

    public override void Close()
    {
        Write(CLEAR_PATH, false);
        if (m7bPort != null)
            m7bPort.Close();
    }

    private string VectorToCode(Vector2 p)
    {
        float ratioX = (p.x - m_worldBound.x) / (m_worldBound.width);
        float ratioY = 1f - ((p.y - m_worldBound.y) / (m_worldBound.height));

        int colIndex = 65 + Mathf.RoundToInt(ratioX * (COLUMN_COUNT - 1));
        int rowIndex = 65 + Mathf.RoundToInt(ratioY * (ROW_COUNT - 1));

        char colChar = (char)colIndex;
        char rowChar = (char)rowIndex;

        return colChar.ToString().ToLower() + rowChar;
    }

    private Vector2 CodeToVector(string code)
    {
        Debug.Log("Code To Vector: " + code);
        code = code.ToUpper();

        float ratioX = ((float) code[0] - 65) / (COLUMN_COUNT - 1);
        float ratioY = 1f - ((float) code[1] - 65) / (ROW_COUNT - 1);

        Debug.Log(string.Format("Rx: {0}. Ry: {1}", ratioX, ratioY));

        return new Vector2(ratioX * m_worldBound.width + m_worldBound.x,
                           ratioY * m_worldBound.height + m_worldBound.y);
    }

    public override IEnumerator OnStart(KeyPortData portData, LMBasePortResolver resolver = null)
    {
        yield return controller.StartCoroutine(base.OnStart(portData, resolver));

        if (string.IsNullOrEmpty(ErrorTxt) && m7bPort != null)
        {
            Debug.Log("m7b is started");
            m7bResolver = new Leadiy_M7B();
            yield return controller.StartCoroutine(m7bPort.OnStart(portData, m7bResolver));
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