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
    private Dictionary<Stats, int> actualStats = new Dictionary<Stats, int>();

    public CharacterStats(CharacterDataScriptableObject data) 
    {
        maxHealth = data.health;
        maxStrength = data.strength;
        maxDexterity = data.dexterity;
        maxDefense = data.defense;
        maxSpeed = data.speed;
        actualStats.Add(Stats.strength, data.strength);
        actualStats.Add(Stats.speed, data.speed);
        actualStats.Add(Stats.defense, data.defense);
        actualStats.Add(Stats.dexterity, data.dexterity);
        actualStats.Add(Stats.health, data.health);
        actualStats.Add(Stats.viewDistance, data.viewDistance);
        actualStats.Add(Stats.dropChance, data.dropChance);
    }

    public static Stats getStatByName(string name) 
    {
        switch (name) 
        {
            case "strength":
                return Stats.strength;
            case "speed":
                return Stats.speed;
            case "defense":
                return Stats.defense;
            case "dexterity":
                return Stats.dexterity;
            case "health":
                return Stats.health;
            case "viewDistance":
                return Stats.viewDistance;
            case "dropChance":
                return Stats.dropChance;
            default:
                throw new System.Exception("Z³a nazwa statystyki");
        }
    }

    public void setActualStat(Stats stat, int value) 
    {
      actualStats[stat] = value;
    }

    public int getActualStat(string statName)
    {
        Stats stat = getStatByName(statName);
        return actualStats[stat];
    }

    public int getActualStat(Stats stat) 
    {
        return actualStats[stat];
    }


}

public enum Stats 
{
    strength = 0,
    speed,
    defense,
    dexterity,
    health,
    viewDistance,
    dropChance,
}

