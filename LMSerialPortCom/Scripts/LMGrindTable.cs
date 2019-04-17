using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMGrindTable : LMInput_Port
{
    public Leadiy_M7B m7bResolver;
    public LMBasePortInput m7bPort;

    public const int COLUMN_COUNT = 4;
    public const int ROW_COUNT = 12;

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

            if (!m7bPort.OpenPort())
            {
                Debug.Log(m7bPort.ErrorTxt);
            }

            return true;
        }
        return false;
    }

    public const string PATH_FORMAT = "Y{0}Z";

    public void Write(Vector2[] path, Rect world)
    {
        string content = string.Empty;
        string lastCode = string.Empty;
        string newCode = string.Empty;

        foreach (var p in path)
        {
            newCode = PathToCode(p, world);

            if (lastCode == newCode)
                continue;

            content += newCode;
            lastCode = newCode;
        }
        
        Write(string.Format(PATH_FORMAT, content), false);
    }

    private string PathToCode(Vector2 p, Rect world)
    {
        float ratioX = (p.x - world.x) / (world.width);
        float ratioY = 1f - ((p.y - world.y) / (world.height));


        int colIndex = 65 + Mathf.RoundToInt(ratioX * (COLUMN_COUNT - 1));
        int rowIndex = 65 + Mathf.RoundToInt(ratioY * (ROW_COUNT - 1));

        // Debug.Log("Position: " + p);
        // Debug.Log(string.Format("Rx: {0}, Ry: {1}", ratioX, ratioY));
        // Debug.Log(string.Format("Col: {0}, Row: {1}", colIndex, rowIndex));

        char colChar = (char)colIndex;
        char rowChar = (char)rowIndex;

        return colChar.ToString().ToLower() + rowChar;
    }

    public override IEnumerator OnStart(KeyPortData portData)
    {
        yield return controller.StartCoroutine(base.OnStart(portData));

        if (string.IsNullOrEmpty(ErrorTxt) && m7bPort != null)
        {
            yield return controller.StartCoroutine(m7bPort.OnStart(portData));
            ErrorTxt = m7bPort.ErrorTxt;
        }
        
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

    // public override IEnumerator TestConnect()
    // {
    //     IsPortActive = true;
    //     yield break;
	// 	// for (int i = 0; i < 5; i++)
	// 	// {
	// 	// 	yield return new WaitForSeconds(1f);

    //     //     yield return controller.StartCoroutine(m7bPort.TestConnect());

	// 	// 	if (!m7bPort.IsPortActive)
	// 	// 		continue;
	// 	// }
    // }

//     protected override void ResolveData(byte[] bytes)
//     {

// 		m_bytes = bytes;

// 		HasData = m_bytes != null && m_bytes.Length > 0;

//         m_isConnected = true;

//         ConsoleProDebug.Watch("Has Data", HasData.ToString());

//         if (HasData)
//         {
//             if (CurrentResolver != null)
//                 CurrentResolver.ResolveBytes(m_bytes);
//         }

// 		// if (!HasData)
// 		// {
// 		// 	if (m_isPortWriting)
// 		// 		return;

// 		// 	m_cdTick++;
// 		// }
// 		// else
// 		// {
// 		// 	m_cdTick = 0f;
// 		// 	m_isConnected = true;

// 		// 	if (CurrentResolver != null)
// 		// 		CurrentResolver.ResolveBytes(m_bytes);
// 		// }
//     }
}