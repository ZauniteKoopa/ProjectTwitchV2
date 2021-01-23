using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : AbstractInteractable
{
    [SerializeField]
    TextAsset script = null;

    protected override IEnumerator Interact(TwitchController twitch)
    {
        DialogueExecuter executer = DialogueExecuter.Instance;
        yield return StartCoroutine(executer.LoadDialogue(script, twitch.GetComponent<EntityStatus>()));
    }
}
