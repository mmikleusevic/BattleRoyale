using Unity.Netcode;
using UnityEngine;

public class Card : NetworkBehaviour
{
    [SerializeField] private MeshRenderer costRenderer;

    public int Points { get; private set; }
    public int WinValue { get; private set; }
    public string Name { get; private set; }
    public Sprite Sprite { get; private set; }


    [ClientRpc]
    public void InitializeClientRpc(int index)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        Sprite = cardSO.cardSprite;
        Name = cardSO.name.ToUpper();
        Points = cardSO.cost;
        WinValue = cardSO.cost;
    }

    public static Card GetCardFromNetworkReference(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return null;

        return networkObject.GetComponent<Card>();
    }

    [ServerRpc]
    public void SetLastCardValueServerRpc()
    {
        SetLastCardValueClientRpc();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateMessageForLastCard());
    }

    [ClientRpc]
    private void SetLastCardValueClientRpc()
    {
        WinValue = 12;
        costRenderer.material = GridManager.Instance.GetLastCardMaterial();
    }

    private string CreateMessageForLastCard()
    {
        return $"{Name} WIN VALUE CHANGED TO {WinValue} POINTS YOU GET FOR WINNING ARE STILL {Points}";
    }

    public virtual void Equip(Player player) { }
    public virtual void Unequip(Player player) { }
    public virtual void UseAbility() { }
}
