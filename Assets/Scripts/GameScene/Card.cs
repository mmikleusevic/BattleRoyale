using Unity.Netcode;

public class Card : NetworkBehaviour
{
    public string Name { get; private set; }
    public int Value { get; private set; }


    [ClientRpc]
    public void InitializeClientRpc(int index)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        Name = cardSO.name.ToUpper();
        Value = cardSO.cost;
    }
}
