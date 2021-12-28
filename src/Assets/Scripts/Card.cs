using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public CardDataScriptableObject cardData;

    public Card(CardDataScriptableObject cardData) 
    {
        this.cardData = cardData;
    }

    public virtual bool PrepareCard(BaseCharacter character)
    {
        return true;
    }

    public virtual void CardPlayed(BaseCharacter character)
    {
        character.preparedCard = null;
        character.DiscardCardFromHand(this);
        Debug.Log(cardData.name + " was played");
    }
}
