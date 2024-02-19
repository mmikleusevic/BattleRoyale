using System;
using Unity.Netcode;
using UnityEngine;

public class DieRollUI : NetworkBehaviour
{
    [SerializeField] private RollUI rollUI;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayer;
    }

    private void Start()
    {
        RollResults.OnReRoll += RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver += RollResults_OnBattleRollOver;
    }

    public override void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        RollResults.OnReRoll -= RollResults_OnReRoll;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        RollResults.OnBattleRollOver -= RollResults_OnBattleRollOver;
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayer;

        base.OnDestroy();
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        rollUI.Show();
    }

    private void RollResults_OnReRoll(object sender, EventArgs e)
    {
        rollUI.Show();
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        rollUI.Hide();
    }

    private void RollResults_OnBattleRollOver(object sender, RollResults.OnBattleRollOverEventArgs e)
    {
        rollUI.Hide();
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(AttackPlayerInfoUI.OnAttackPlayerEventArgs obj)
    {
        if(RollType.rollTypeSelf == RollTypeEnum.PlayerAttack && RollType.rollTypeEnemy == RollTypeEnum.PlayerAttack)
        {
            rollUI.Show();
            ShowDieOnEnemyUIServerRpc(obj.player.NetworkObject);
        }            
        else if (RollType.rollTypeSelf == RollTypeEnum.PlayerAttack && RollType.rollTypeEnemy == RollTypeEnum.Disadvantage)
        {
            rollUI.Show();
        }
        else
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
