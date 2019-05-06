using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMBaseEmulator : MonoBehaviour
{
    public LMBasePortInput PortInput { get; private set; }
    
    public virtual void Init(LMBasePortInput input) 
    {
        PortInput = input;
    }
}
