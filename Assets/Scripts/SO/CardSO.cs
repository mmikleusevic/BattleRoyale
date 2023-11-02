using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    public Sprite cardSprite;
    public new string name;
    public Sprite costSprite;
    public int cost;
    public CardType cardType;
}

public enum CardType
{
    All,
    Magic,
    Curse,
    Spell
}
