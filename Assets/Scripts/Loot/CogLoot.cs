using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogLoot : AbstractInteractable
{
    [SerializeField]
    private int coinsGiven = 10;

    //Gives a fixed amount of cogs to player character
    protected override IEnumerator Interact(TwitchController twitch)
    {
        yield return 0;
        twitch.AddCogs(coinsGiven);
        Destroy(gameObject);
    }
}
