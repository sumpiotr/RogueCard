using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsFactory 
{
    public static Card getCard(BasicCardDataScriptableObject cardData) 
    {
        switch (cardData.GetCardType()) 
        {
            case CardDataType.Move:
                switch (cardData.card) 
                {
                    case Cards.Move:
                        return new MoveCard((MoveCardDataScriptableObject)cardData);
                    default:
                        return new Card(cardData);
                }

            case CardDataType.Attack:
                switch (cardData.card)
                {
                    case Cards.Attack:
                        return new AttackCard((AttackCardDataScriptableObject)cardData);
                    case Cards.Charge:
                        return new ChargeCard((AttackCardDataScriptableObject)cardData);
                    default:
                        return new Card(cardData);
                }
            default:
                return new Card(cardData);
        }
    }
}


public enum Cards
{
    Move,
    Attack,
    UniversalCard,
    Charge
}

public enum CardDataType 
{
    Custom = 0,
    Move,
    Attack,
    Multi,
}
