using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonProjBehav : ProjectileBehav
{
    private PoisonVial vial;

    protected override void DamageEntity(Collider2D collider)
    {
        EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();
        tgtStatus.PoisonDamageEntity(vial.GetDamage(), 1, vial);
    }

    public void SetPoisonProj(Vector2 dir, PoisonVial pv, bool isPlayer)
    {
        vial = pv;
        GetComponent<SpriteRenderer>().color = pv.GetColor();
        SetProj(dir, 0f, isPlayer);
    }
}
