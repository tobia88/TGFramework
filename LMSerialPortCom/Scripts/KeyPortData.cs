using System.Linq;  

[System.Serializable]
public class KeyPortValueData
{
    public string key;
    public string equation;
    
    public float value;
    public bool isDegree;
    public string min;
    public string max;
    public float origin;
}

[System.Serializable]
public class KeyPortInputData
{
    public string key;
    public int length;
}

[System.Serializable]
public class KeyPortData
{
    public string name;
    public string type;
    public string order;
    public string thresholdEquation;
    public KeyPortValueData[] value;
    public KeyPortInputData[] input;

}
