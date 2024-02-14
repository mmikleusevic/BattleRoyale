using System.Collections.Generic;
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

    private void ActionsUI_OnAttackPlayer(Card card)
    {
        Show();

        List<Player> players = card.GetPlayersOnCard();

        foreach (Player player in players)
        {
            if (player == Player.LocalInstance) continue;

            Transform cardTransform = Instantiate(template, container);

            cardTransform.gameObject.SetActive(true);

            AttackPlayerInfoUI attackPlayerInfoUI = cardTransform.GetComponent<AttackPlayerInfoUI>();

            attackPlayerInfoUI.Instantiate(player);
        }
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(AttackPlayerInfoUI.OnAttackPlayerEventArgs obj)
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
