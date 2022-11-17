using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCard : Card
{
    private MoveCardDataScriptableObject moveData;
    public MoveCard(MoveCardDataScriptableObject cardData) : base(cardData) 
    {
        moveData = cardData;
    }

    public override bool PrepareCard(BaseCharacter character, bool playAfterPrepare = true)
    {
        this.playAfterPrepare = playAfterPrepare;
        return character.PrepareMove(character.stats.getActualStat(Stats.speed) + moveData.move, this);
    }
    public override void CardPlayed(ICharacter character)
    {
        character.Move(destination, character.stats.getActualStat(Stats.speed) + moveData.move, () =>
        {
            base.CardPlayed(character);
            character.EndAction();
        });
    }
    
    public override void CardPlayed(ICharacter character, Action onPlayed)
    {
        character.Move(destination, character.stats.getActualStat(Stats.speed) + moveData.move, onPlayed);
    }
}
