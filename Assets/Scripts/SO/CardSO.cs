using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    public Card prefab;
    public Sprite cardSprite;
    public new string name;
    public int cost;
    public CardType cardType;
}

public enum CardType
{
    All,
    Magic,
    Curse,
    Disease,
    Spell
}
