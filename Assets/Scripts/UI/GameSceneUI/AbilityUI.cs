using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public static event Action OnAbilityUsed;

    [SerializeField] private Transform container;
    [SerializeField] private Button buttonTemplate;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        cancelButton.onClick.AddListener(() => Hide());

        ActionsUI.OnAbility += ActionsUI_OnAbility;

        buttonTemplate.gameObject.SetActive(false);

        Hide();
    }

    private void OnDestroy()
    {
        ActionsUI.OnAbility -= ActionsUI_OnAbility;

        cancelButton.onClick.RemoveAllListeners();
    }

    private void ActionsUI_OnAbility()
    {
        Player player = Player.LocalInstance;

        foreach (Card card in player.EquippedCards)
        {
            if (card.Ability != null && !card.AbilityUsed)
            {
                Button newButton = Instantiate(buttonTemplate, container);

                newButton.GetComponentInChildren<TextMeshProUGUI>().text = card.Name;
                newButton.onClick.AddListener(() =>
                {
                    Player.LocalInstance.SubtractActionPoints();

                    card.Ability.Use();

                    Hide();

                    OnAbilityUsed?.Invoke();
                });

                newButton.gameObject.SetActive(true);
            }
        }

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        DestroyButtons();

        gameObject.SetActive(false);
    }

    private void DestroyButtons()
    {
        foreach (Transform child in container)
        {
            if (child == buttonTemplate.transform) continue;
            Destroy(child.gameObject);
        }
    }
}
