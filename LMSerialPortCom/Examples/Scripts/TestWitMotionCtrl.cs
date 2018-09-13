using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWitMotionCtrl : TGBaseScene
{
    public Transform obj;
    public LMWitMotionCtrl motionCtrl;

    public override void OnStart()
    {
        base.OnStart();
        motionCtrl = FindObjectOfType<LMWitMotionCtrl>();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        var targetRot = Quaternion.Euler(motionCtrl.euler);
        obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRot, 0.1f);
    }
}
