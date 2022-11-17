using System;

public class AttackCard : Card
{
    private AttackCardDataScriptableObject attackData;

    public AttackCard(AttackCardDataScriptableObject cardData) : base(cardData)
    {
        attackData = cardData;
    }

    public override bool PrepareCard(BaseCharacter character, bool playAfterPrepare = true)
    {
        this.playAfterPrepare = playAfterPrepare;
        return character.PrepareAttack(attackData.maxRange, attackData.minRange, this);
    }
    public override void CardPlayed(ICharacter character)
    {
        target.TakeDamage(character.stats.getActualStat(Stats.strength) + attackData.attack);
        base.CardPlayed(character);
    }
    
    public override void CardPlayed(ICharacter character, Action onPlayed)
    {
        target.TakeDamage(character.stats.getActualStat(Stats.strength) + attackData.attack);
        onPlayed();
    }
}
