using Unity.Netcode;

public class Card : NetworkBehaviour
{
    public int Value { get; private set; }
    public string Name { get; private set; }


    [ClientRpc]
    public void InitializeClientRpc(int index)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        Name = cardSO.name.ToUpper();
        Value = cardSO.cost;
    }
}
