using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : AbstractInteractable
{
    [SerializeField]
    private Transform healthPopup = null;

    [SerializeField]
    private float minHealthGain = 8.0f;
    [SerializeField]
    private float maxHealthGain = 12.0f;


    //Overriden method to interact
    protected override IEnumerator Interact(TwitchController player)
    {
        //Open corpse to indicate that it's activated
        GetComponent<SpriteRenderer>().enabled = false;

        //Get health amount and heal
        EntityStatus playerStatus = player.GetComponent<EntityStatus>();
        float healthGain = Random.Range(minHealthGain, maxHealthGain);
        playerStatus.Heal(healthGain);
        GetComponent<AudioSource>().Play(0);

        yield return new WaitForSeconds(0.2f);

        //Do popup, round health gain to the hundrenths
        float rounded = Mathf.Round(healthGain * 100f) * 0.01f;
        string popupText = "+" + rounded + " HP";

        //Display popup
        Transform curPopup = Object.Instantiate(healthPopup, player.transform);
        curPopup.GetComponent<TextPopup>().SetUpPopup(popupText);
    }
}
