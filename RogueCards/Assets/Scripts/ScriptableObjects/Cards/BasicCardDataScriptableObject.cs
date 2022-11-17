using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCardDataScriptableObject : ScriptableObject
{
    public string cardName;
    public Cards card;
    public string description;
    public int duration;
    public Sprite image;

    public virtual CardDataType GetCardType() 
    {
        return CardDataType.Custom;
    }
}
