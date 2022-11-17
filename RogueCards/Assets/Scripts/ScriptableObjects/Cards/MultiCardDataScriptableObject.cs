using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Plays few cards as one in cardsData list order
public class MultiCardDataScriptableObject : BasicCardDataScriptableObject
{
    public List<BasicCardDataScriptableObject> cardsData;

    public override CardDataType GetCardType()
    {
        return CardDataType.Multi;
    }
}
