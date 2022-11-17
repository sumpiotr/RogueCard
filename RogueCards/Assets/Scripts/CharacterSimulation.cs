using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSimulation : ICharacter
{

    private Vector2 _position;
    private CharacterStats _stats;
    private Vector2 _destination;
    private ICharacter _target;

    public Vector2 position { get => _position; set => _position = value; }
    public CharacterStats stats { get => _stats; set => _stats = value; }
    public Vector2 destination { get => _destination; set => _destination = value; }

    public CharacterSimulation(BaseCharacter character) 
    {
        position = character.position;
        stats = new CharacterStats(character.stats);
    }

    public void Move(Vector2 targetPosition, int distance, Action callback = null)
    {
        position = targetPosition;
        if(callback != null) 
        {
            callback();
        }
    }

    public void TakeDamage(int power)
    {
        stats.setActualStat(Stats.health, stats.getActualStat(Stats.health) - power);
    }

    public void CardPlayed(Card card) 
    {

    }

    public void EndAction() 
    {
    }
}
