using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehav : MonoBehaviour
{
    private float damage = 0.0f;
    private Vector2 speedVector = Vector2.zero;
    private string tgtTag = "";

    [SerializeField]
    private float projSpeed = 0.0f;
    [SerializeField]
    private float duration = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyProjectile", duration);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(speedVector * Time.fixedDeltaTime);
    }

    //Destroys projectile
    void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    //Sets direction and damage of projectile
    public void SetProj(Vector2 dir, float dmg, bool isPlayer)
    {
        dir.Normalize();
        speedVector = dir * projSpeed;

        damage = dmg;
        tgtTag = (isPlayer) ? "Enemy" : "Player";

    }

    //If collide with something, Destroy Yourself. If hit tgtTag, do damage to tgtTag
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Wall" || collider.tag == tgtTag)
        {
            if (collider.tag == tgtTag)
            {
                DamageEntity(collider);
            }

            CancelInvoke();
            DestroyProjectile();
        }
    }

    //Method to do damage to an entity
    protected virtual void DamageEntity(Collider2D collider)
    {
        EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();
        tgtStatus.DamageEntity(damage);
    }
}
