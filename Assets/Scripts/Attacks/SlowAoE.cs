using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowAoE : MeleeAoE
{
    //Variables to keep track of slow
    private float slowFactor;
    [SerializeField]
    private float slowTime = 1.5f;
    EntityStatus effectedTgt = null;

    //Method to slow target down if hit
    protected override void AffectTarget(EntityStatus target, float slowF)
    {
        tgt.ChangeSpeed(slowF);
        slowFactor = slowF;
        effectedTgt = tgt;
        Invoke("ReverseSlow", slowTime);

    }

    //Method to reverse slow
    private void ReverseSlow()
    {
        effectedTgt.ChangeSpeed(1f / slowFactor);
    }
}

