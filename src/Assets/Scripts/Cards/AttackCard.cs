using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCard : Card
{
    public AttackCard(CardDataScriptableObject cardData) : base(cardData)
    {

    }

    public override void PrepareCard(BaseCharacter character)
    {
        character.PrepareAttack(cardData.range, this);
    }
    public override void CardPlayed(BaseCharacter character)
    {
        character.GetGrid().GetCharacterByPosition(character.destination).TakeDamage(character.stats.getActualStat("strength") + cardData.attack);
        base.CardPlayed(character);
    }
}
