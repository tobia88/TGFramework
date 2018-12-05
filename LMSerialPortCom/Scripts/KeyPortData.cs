using System.Linq;  

[System.Serializable]
public class KeyPortValueData
{
    public string key;
    public string equation;
    
    public string min;
    public string max;
    public float origin;
    public bool reverse;
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
    public string[] name;
    public string type = "key";
    public bool isDegree;
    public bool raw;
    // public string thresholdEquation;
    public int inputTotalGap;
    public KeyPortValueData[] value;
    public KeyPortInputData[] input;
    public float[] levels;
}
