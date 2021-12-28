using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalCard : Card
{
    
    private Queue<Vector2> destinations = new Queue<Vector2>();
    private Queue<Actions> actionsOrder = new Queue<Actions>();
    private Queue<Actions> actionsToDo = new Queue<Actions>();
    private int steps = 0;
    private int totalSteps = 0;

    public UniversalCard(CardDataScriptableObject cardData) : base(cardData)
    {
        if (cardData.move != 0) totalSteps++;
        if (cardData.attack != -1) totalSteps++;
        if (cardData.move > 0)
        {
            actionsOrder.Enqueue(Actions.Move);
            if (cardData.attack != -1) actionsOrder.Enqueue(Actions.Attack);
        }
        else if (cardData.move < 0)
        {
            if (cardData.attack != -1) actionsOrder.Enqueue(Actions.Attack);
            actionsOrder.Enqueue(Actions.Move);
        }
        else
        {
            actionsOrder.Enqueue(Actions.Attack);
        }
        steps = totalSteps;
    }

    private void SetData(BaseCharacter character) 
    {
        destinations.Clear();
        actionsToDo.Clear();
        steps = totalSteps;
        actionsToDo = new Queue<Actions>(actionsOrder);
        character.destination = character.transform.position;
    }

    public override bool PrepareCard(BaseCharacter character)
    {
        if (steps != actionsToDo.Count) SetData(character);
        Actions action = actionsToDo.Dequeue();
        switch (action) 
        {
            case Actions.Move:
                return character.PrepareMove(character.stats.getActualStat(Stats.speed), this);
            case Actions.Attack:
                return character.PrepareAttack(cardData.range, this, true);
            default:
                return true;
        }
    }
    public override void CardPlayed(BaseCharacter character)
    {
        steps--;
        destinations.Enqueue(character.destination);
        if (steps != 0) 
        {
            PrepareCard(character);
            if (character is SimpleAI)
            {
                CardPlayed(character);
            }
            return; 
        }
        actionsToDo = new Queue<Actions>(actionsOrder);
        Play(character);
        base.CardPlayed(character);
    }

    private void Play(BaseCharacter character) 
    {
        while (destinations.Count != 0 && actionsToDo.Count != 0)
        {
            character.destination = destinations.Dequeue();
            Actions action = actionsToDo.Dequeue();
            switch (action)
            {
                case Actions.Move:
                    character.Move(character.destination, character.stats.getActualStat(Stats.speed), () => { Play(character); });
                    return;
                case Actions.Attack:
                    character.GetGrid().GetCharacterByPosition(character.destination).TakeDamage(character.stats.getActualStat(Stats.strength) + cardData.attack);
                    break;
                default:
                    return;
            }

        }
        SetData(character);
        character.EndAsyncAction();
    }


    private enum Actions 
    {
        Move = 0,
        Attack
    }
}


