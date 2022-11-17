using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDataScriptableObject", menuName = "Card/Attack")]
public class AttackCardDataScriptableObject : BasicCardDataScriptableObject
{
    public int attack;
    public int minRange;
    public int maxRange;

    public override CardDataType GetCardType()
    {
        return CardDataType.Attack;
    }
}
