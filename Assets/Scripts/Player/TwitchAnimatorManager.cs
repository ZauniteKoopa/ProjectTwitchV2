using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchAnimatorManager : MonoBehaviour
{
    //Reference Variables for manager to refer to
    private TwitchController state;
    private Animator anim;
    private SpriteRenderer sprite;

    // On awake set up reference variables
    void Awake()
    {
        state = GetComponent<TwitchController>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        anim.SetInteger("animState", state.GetAnimState());
        GeneralAnimation.UpdateAnimOrientation(state.GetForwardVector(), anim, sprite);
    }
}
