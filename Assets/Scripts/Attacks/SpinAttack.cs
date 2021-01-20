using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAttack : MeleeAoE
{
    //Method to damage target if hit
    protected override void AffectTarget(EntityStatus tgt, float dmg)
    {
        tgt.DamageEntity(dmg);
    }
}
