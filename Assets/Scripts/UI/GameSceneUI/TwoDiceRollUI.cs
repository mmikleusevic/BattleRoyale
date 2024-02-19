using System;
using Unity.Netcode;
using UnityEngine;

public class TwoDiceRollUI : NetworkBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayer;
    }

    public override void OnDestroy()
    {
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayer;

        base.OnDestroy();
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(AttackPlayerInfoUI.OnAttackPlayerEventArgs obj)
    {
        if (RollType.rollTypeSelf == RollTypeEnum.Disadvantage)
        {
            rollUI.Show();
        }
        else if(RollType.rollTypeEnemy == RollTypeEnum.Disadvantage)
        {
            ShowDieOnEnemyUIServerRpc(obj.player.NetworkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowDieOnEnemyUIServerRpc(NetworkObjectReference networkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return;

        Player player = networkObject.GetComponent<Player>();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.ClientId.Value }
            }
        };

        ShowDieOnEnemyUIClientRpc();
    }

    [ClientRpc]
    private void ShowDieOnEnemyUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        rollUI.Show();
    }
}
