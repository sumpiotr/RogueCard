using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDataScriptableObject", menuName = "Card/Move")]
public class MoveCardDataScriptableObject : BasicCardDataScriptableObject
{
    public int move;

    public override CardDataType GetCardType()
    {
        return CardDataType.Move;
    }
}
