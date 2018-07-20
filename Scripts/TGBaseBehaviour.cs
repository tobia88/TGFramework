using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TGBaseBehaviour : MonoBehaviour
{
    public abstract IEnumerator StartRoutine(TGController _controller);
}
