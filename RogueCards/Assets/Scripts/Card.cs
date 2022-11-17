using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Card 
{
    public BasicCardDataScriptableObject cardData;
    protected bool playAfterPrepare = false;
    public Vector2 destination;
    public ICharacter target;

    public UnityEvent onPreperationEnd = new UnityEvent();

    public Card(BasicCardDataScriptableObject cardData) 
    {
        this.cardData = cardData;
    }

    public virtual bool PrepareCard(BaseCharacter character, bool playAfterPrepare = true)
    {
        this.playAfterPrepare = playAfterPrepare;
        return true;
    }

    public void EndPreperation(ICharacter character)
    {
        if(playAfterPrepare)CardPlayed(character);
        onPreperationEnd.Invoke();
    }

    public virtual void CardPlayed(ICharacter character)
    {
        character.CardPlayed(this);
        destination = Vector2.zero;
        target = null;
        Debug.Log(cardData.name + " was played");
    }
    
    public virtual void CardPlayed(ICharacter character, Action afterPlayed)
    {
        CardPlayed(character);
        Debug.Log(cardData.name + " was played");
    }
}
