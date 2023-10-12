using UnityEngine;

[CreateAssetMenu()]
public class Card : ScriptableObject
{
    public Transform Prefab;
    public Sprite Sprite;
    public string Name;
    public string Description;
    public int Cost;
    public CardType CardType;
}

public enum CardType
{
    All,
    Magic,
    Curse,
    Disease
}
