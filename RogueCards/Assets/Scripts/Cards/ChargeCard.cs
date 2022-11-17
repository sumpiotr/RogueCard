using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeCard : Card
{
    private AttackCardDataScriptableObject attackData;
    public ChargeCard(AttackCardDataScriptableObject cardData) : base(cardData)
    {
        attackData = cardData;
    }

    public override bool PrepareCard(BaseCharacter character, bool playAfterPrepare = true)
    {
        this.playAfterPrepare = playAfterPrepare;
        return character.PrepareCharge(attackData.maxRange + character.stats.getActualStat(Stats.speed), attackData.minRange, this);
    }
    public override void CardPlayed(ICharacter character)
    {
        character.Move(destination, attackData.maxRange + character.stats.getActualStat(Stats.speed), () =>
        {
            target.TakeDamage(character.stats.getActualStat(Stats.strength) + attackData.attack);
            base.CardPlayed(character);
            character.EndAction();
        });
    }
    
    public override void CardPlayed(ICharacter character, Action onPlayed)
    {
        character.Move(destination, attackData.maxRange + character.stats.getActualStat(Stats.speed), () =>
        {
            target.TakeDamage(character.stats.getActualStat(Stats.strength) + attackData.attack);
            onPlayed();
        });
    }
    
}
