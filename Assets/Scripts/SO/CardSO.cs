using UnityEngine;

[CreateAssetMenu()]
public class CardSO : ScriptableObject
{
    public Transform Prefab;
    public Sprite Sprite;
    public string Name;
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
