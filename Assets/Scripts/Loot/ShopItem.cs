﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopItem : AbstractInteractable
{
    [SerializeField]
    TMP_Text costUI = null;
    [SerializeField]
    private int healthCost = 50;
    [SerializeField]
    private int ingredientCost = 40;
    [SerializeField]
    private float ingChance = 0.65f;
    [SerializeField]
    private Transform failPopup = null;

    [SerializeField]
    private float healthGain = 12.5f;
    [SerializeField]
    private bool canBeHealth = true;
    private int cost;
    private Ingredient ing;

    //Who owns the object
    [SerializeField]
    private RelationshipManager.CharacterName seller = RelationshipManager.CharacterName.ChumpWhump;

    private AudioSource audioFX;


    // Start is called before the first frame update
    void Start()
    {
        audioFX = GetComponent<AudioSource>();
        float select = (canBeHealth) ? Random.Range(0.0f, 1.0f) : 0.0f;

        if (select < ingChance)
        {
            cost = ingredientCost;
            ing = new Ingredient(Ingredient.GetRandomType(), 1);
            GetComponent<SpriteRenderer>().color = ing.GetColor();
        }
        else
        {
            cost = healthCost;
            ing = null;
            GetComponent<SpriteRenderer>().color = Color.white;
        }

        costUI.text = "" + cost;
    }

    //When interacting with the shop, get the object if you have money, display error popup if don't have money
    protected override IEnumerator Interact(TwitchController twitch)
    {
        if (twitch.CanAfford(cost))
        {
            //Get cost
            twitch.AddCogs(-1 * cost);

            //Give player object
            if (ing != null)
                twitch.AddToInventory(ing);
            else
                twitch.GetComponent<EntityStatus>().Heal(healthGain);
            
            //Disable shop object
            costUI.gameObject.SetActive(false);
            GetComponent<SpriteRenderer>().color = Color.black;

            //Notify shop keeper that someone bought it
            ShopkeeperRelationship shopkeeper = (ShopkeeperRelationship)RelationshipManager.GetRelationship(seller);
            if (ing == null)
            {
                shopkeeper.OnBuyHealthPack();
            }
            else
            {
                shopkeeper.OnBuyIngredient(ing.GetIngType());
            }

            //Play sound effect
            audioFX.Play(0);
        }
        else
        {
            //Display popup and allow future activation
            yield return new WaitForSeconds(0.4f);
            Transform curPopup = Object.Instantiate(failPopup, transform);
            curPopup.GetComponent<TextPopup>().SetUpPopup("NOT ENOUGH MONEY");
            yield return new WaitForSeconds(0.4f);
            SetCanActivate(true);
        }
    }
}
