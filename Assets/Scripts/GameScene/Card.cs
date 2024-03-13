using Unity.Netcode;
using UnityEngine;

public class Card : NetworkBehaviour
{
    public int Value { get; private set; }
    public string Name { get; private set; }
    public Sprite Sprite { get; private set; }


    [ClientRpc]
    public void InitializeClientRpc(int index)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        Sprite = cardSO.cardSprite;
        Name = cardSO.name.ToUpper();
        Value = cardSO.cost;
    }
}
