using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCard : Card
{

    public MoveCard(CardDataScriptableObject cardData) : base(cardData) 
    {

    }

    public override void PrepareCard(BaseCharacter character)
    {
        character.PrepareMove(character.stats.getActualStat("speed"), this);
    }
    public override void CardPlayed(BaseCharacter character)
    {
        character.Move(character.destination, character.data.speed);
        base.CardPlayed(character);
    }
}
