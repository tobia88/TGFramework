using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMBaseEmulator : MonoBehaviour
{
    public LMBasePortInput Input { get; private set; }
    
    public virtual void Init(LMBasePortInput input) 
    {
        Input = input;
    }
}
