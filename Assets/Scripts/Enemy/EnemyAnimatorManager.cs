using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimatorManager : MonoBehaviour
{
    //Reference Variables for manager to refer to
    private AbstractEnemy state;
    private Animator anim;
    private SpriteRenderer sprite;

    // On awake set up reference variables
    void Awake()
    {
        state = GetComponent<AbstractEnemy>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("animState", state.GetAnimState());
        GeneralAnimation.UpdateAnimOrientation(state.GetForwardVector(), anim, sprite);
    }
}
