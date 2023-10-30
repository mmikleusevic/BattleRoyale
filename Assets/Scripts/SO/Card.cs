using UnityEngine;

[CreateAssetMenu()]
public class Card : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public new string name;
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
