using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckDataScriptableObject", menuName ="deck")]
public class DeckDataScriptableObject : ScriptableObject
{
    public List<CardDataScriptableObject> startCards;
    public List<CardDataScriptableObject> cards;
}
