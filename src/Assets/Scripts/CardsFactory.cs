using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsFactory 
{
    public static Card getCard(CardDataScriptableObject cardData) 
    {
        switch (cardData.card) 
        {
            case Cards.Move:
                return new MoveCard(cardData);
            case Cards.Attack:
                return new AttackCard(cardData);
            case Cards.UniversalCard:
                return new UniversalCard(cardData);
            default:
                return new Card(cardData);
        }
    }
}


public enum Cards
{
    Move = 0,
    Attack,
    UniversalCard,
}
