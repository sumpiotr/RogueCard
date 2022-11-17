using System.Collections.Generic;
using UnityEngine;

public class UniversalCard //: Card
{

    /*private Queue<Vector2> destinations = new Queue<Vector2>();
    private Queue<ICharacter> targets = new Queue<ICharacter>();
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

    private void SetData(ICharacter character)
    {
        destinations.Clear();
        targets.Clear();
        actionsToDo.Clear();
        steps = totalSteps;
        actionsToDo = new Queue<Actions>(actionsOrder);
        character.destination = character.position;
    }

    public override bool PrepareCard(BaseCharacter character)
    {
        if (steps != actionsToDo.Count) SetData(character);
        Actions action = actionsToDo.Dequeue();
        switch (action)
        {
            case Actions.Move:
                {
                    bool success = character.PrepareMove(character.stats.getActualStat(Stats.speed), this);
                    return DefaultPrepare(success, character);
                }
            case Actions.Attack:
                {
                    bool success = character.PrepareAttack(cardData.maxRange, cardData.minRange, this, true);
                    return DefaultPrepare(success, character);
                }
            default:
                return true;
        }
    }

    private bool DefaultPrepare(bool success, BaseCharacter character) 
    {
        if (character is SimpleAI && success && steps != 1)
        {
            CardPlayed(character);
            return PrepareCard(character);
        }
        return success;
    }

    public override void CardPlayed(ICharacter character)
    {
        steps--;
        destinations.Enqueue(character.destination);
        targets.Enqueue(character.target);
        character.target = null;
        if (steps != 0)
        {
            if (character is PlayerController)
            {
                PrepareCard((PlayerController)character);
            }
            return;
        }
        actionsToDo = new Queue<Actions>(actionsOrder);
        Play(character);
        base.CardPlayed(character);
    }

    private void Play(ICharacter character)
    {
        while (destinations.Count != 0 && actionsToDo.Count != 0)
        {
            character.destination = destinations.Dequeue();
            character.target = targets.Dequeue();
            Actions action = actionsToDo.Dequeue();
            switch (action)
            {
                case Actions.Move:
                    character.Move(character.destination, character.stats.getActualStat(Stats.speed), () => { Play(character); });
                    return;
                case Actions.Attack:
                    character.target.TakeDamage(character.stats.getActualStat(Stats.strength) + cardData.attack);
                    break;
                default:
                    return;
            }

        }
        character.target = null;
        SetData(character);
        character.EndAction();
    }


    private enum Actions
    {
        Move = 0,
        Attack
    }*/
}


