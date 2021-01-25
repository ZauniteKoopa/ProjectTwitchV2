using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : AbstractInteractable
{
    [SerializeField]
    RelationshipManager.CharacterName character = RelationshipManager.CharacterName.ChumpWhump;

    protected override IEnumerator Interact(TwitchController twitch)
    {
        //Get script from relationship manager
        TextAsset script = RelationshipManager.GetRelationship(character).TalkToCharacter();

        DialogueExecuter executer = DialogueExecuter.Instance;
        yield return StartCoroutine(executer.LoadDialogue(script, twitch.GetComponent<EntityStatus>()));
    }
}
