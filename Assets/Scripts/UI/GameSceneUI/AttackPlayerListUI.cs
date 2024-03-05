using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AttackPlayerListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() => Hide());

        ActionsUI.OnAttackPlayer += ActionsUI_OnAttackPlayer;
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayer;

        Hide();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();

        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayer;
    }

    private void ActionsUI_OnAttackPlayer(Tile tile)
    {
        Show();

        List<Player> players = tile.GetPlayersOnCard();

        foreach (Player player in players)
        {
            if (player == Player.LocalInstance) continue;

            Transform cardTransform = Instantiate(template, container);

            cardTransform.gameObject.SetActive(true);

            AttackPlayerInfoUI attackPlayerInfoUI = cardTransform.GetComponent<AttackPlayerInfoUI>();

            attackPlayerInfoUI.Instantiate(player);
        }
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(NetworkObjectReference arg1, NetworkObjectReference arg2, string arg3)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        container.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        container.gameObject.SetActive(false);

        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }
    }
}
