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
    void Start()
    {
        //Get reference variables
        state = GetComponent<TwitchController>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        //connect to state machine
        state.OnForwardDirUpdate.AddListener(OnDirectionChange);
        state.OnAnimStateUpdate.AddListener(OnAnimStateChange);
    }

    //Callback function when state changed direction
    public void OnDirectionChange(Vector3 newDir)
    {
        GeneralAnimation.UpdateAnimOrientation(newDir, anim, sprite);
    }

    //Callback function when character changed state
    public void OnAnimStateChange(int state)
    {
        anim.SetInteger("animState", state);
    }
}
