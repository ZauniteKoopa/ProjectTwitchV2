using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : Room
{
    //Who owns the shop
    [SerializeField]
    private RelationshipManager.CharacterName owner = RelationshipManager.CharacterName.ChumpWhump;

    //Override method to reset shopKeeper item count when entering a store
    protected override void SetUpRoom(TwitchController twitch)
    {
        ShopkeeperRelationship shopkeeper = (ShopkeeperRelationship)RelationshipManager.GetRelationship(owner);
        shopkeeper.ResetShop();
    }
}
