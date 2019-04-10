﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGBasePoolObject : MonoBehaviour, IPoolObject
{
    public bool IsActive { get { return gameObject.activeSelf; } }
    public string PoolName { get { return gameObject.name; } }

    public virtual void Init() {}
    public virtual void Spawn(Vector3 pos, Quaternion rotation)
    {
        transform.position = pos;
        transform.rotation = rotation;
    }

    public virtual void Destroy() 
    {
        gameObject.SetActive(false);
    }

    public virtual void Destroy(float duration)
    {
        Call(Destroy, duration);
    }

    public void Call(System.Action callback, float duration, bool ignoreTimescale = false) {
        StartCoroutine(CallDelay(callback, duration, ignoreTimescale));
    }

    IEnumerator CallDelay(System.Action callback, float duration, bool ignoreTimescale) {
        if (ignoreTimescale) {
            yield return new WaitForSecondsRealtime(duration);
        }
        else {
            yield return new WaitForSeconds(duration);
        }

        callback();
    }
}
