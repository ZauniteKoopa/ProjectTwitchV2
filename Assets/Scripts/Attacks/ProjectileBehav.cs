using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehav : MonoBehaviour
{
    private float damage = 0.0f;
    private Vector2 speedVector = Vector2.zero;
    private string tgtTag = "";
    private bool active = true;

    [SerializeField]
    private float projSpeed = 0.0f;
    [SerializeField]
    private float duration = 3.0f;

    //On hit sound effect
    [SerializeField]
    AudioClip onHitSound = null;

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
    protected void DestroyProjectile()
    {
        CancelInvoke();
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
        if (active && (collider.tag == "Wall" || collider.tag == tgtTag))
        {
            if (collider.tag == tgtTag)
            {
                DamageEntity(collider);
            } 
            else
            {
                DestroyProjectile();
            }

        }
    }

    //Method to do damage to an entity
    protected virtual void DamageEntity(Collider2D collider)
    {
        EntityStatus tgtStatus = collider.GetComponent<EntityStatus>();
        tgtStatus.DamageEntity(damage);
        StartCoroutine(PlayOnHitSound(true));
    }

    
    //Method to play OnHit sound effect
    protected IEnumerator PlayOnHitSound(bool dieAfter)
    {
        //If die after, disable
        if (dieAfter)
        {
            active = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }

        //Play sound effect
        if (onHitSound != null)
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = onHitSound;
            audio.Play();

            yield return new WaitForSeconds(0.5f);
        }

        if (dieAfter)
            DestroyProjectile();
    }

}
