using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter 
{
   Vector2 position 
    {
        get;
        set;
    }

    CharacterStats stats 
    {
        get;
        set;
    }

    void Move(Vector2 targetPosition, int distance, Action callback = null);

    void TakeDamage(int power);

    void CardPlayed(Card card);

    void EndAction();

}
