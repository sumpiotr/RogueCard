using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats
{
    public int maxHealth;
    public int maxStrength;
    public int maxDexterity;
    public int maxDefense;
    public int maxSpeed;
    private Dictionary<string, int> actualStats = new Dictionary<string, int>();

    public CharacterStats(CharacterDataScriptableObject data) 
    {
        maxHealth = data.health;
        maxStrength = data.strength;
        maxDexterity = data.dexterity;
        maxDefense = data.defense;
        maxSpeed = data.speed;
        actualStats.Add("strength", data.strength);
        actualStats.Add("speed", data.speed);
        actualStats.Add("defense", data.defense);
        actualStats.Add("dexterity", data.dexterity);
        actualStats.Add("health", data.health);
    }

    public void setActualStat(string statName, int value) 
    {
        if (actualStats.ContainsKey(statName)) 
        {
            actualStats[statName] = value;
        }
        else
        {
            Debug.LogError("Wrong stat name!");
        }
    }

    public int getActualStat(string statName)
    {
        if (actualStats.ContainsKey(statName))
        {
            return actualStats[statName];
        }
        else
        {
            Debug.LogError("Wrong stat name!");
            return -1;
        }
    }
}

