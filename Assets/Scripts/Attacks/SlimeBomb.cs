using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBomb : MonoBehaviour
{
    [SerializeField]
    float speedReduction = 0.05f;
    [SerializeField]
    float damage = 0.0f;
    private const float REDUCTION_PER_LEVEL = 0.05f;
    [SerializeField]
    float duration = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBomb", duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Bomb set up
    //  Pre: vial's side effect is already slime bomb
    public void SetUpBomb(float dmg, PoisonVial vial)
    {
        Debug.Assert(vial.GetSideEffect() == PoisonVial.SideEffect.SLIME_BOMB);

        speedReduction -= (REDUCTION_PER_LEVEL * vial.GetSideEffectLevel());
        damage = dmg;

        Color bombColor = vial.GetColor();
        bombColor.a = 0.25f;
        GetComponent<SpriteRenderer>().color = bombColor;
    }

    //Destroys bomb
    void DestroyBomb()
    {
        CancelInvoke();
        Destroy(gameObject);
    }

    //On trigger enter / exit
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Enemy")
        {
            EntityStatus tgt = collider.GetComponent<EntityStatus>();
            tgt.ChangeSpeed(speedReduction);
            tgt.DamageEntity(damage);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "Enemy")
        {
            EntityStatus tgt = collider.GetComponent<EntityStatus>();
            tgt.ChangeSpeed(1.0f / speedReduction);
        }
    }
}
