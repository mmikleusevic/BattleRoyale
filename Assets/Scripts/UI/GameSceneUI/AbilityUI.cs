using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public static event Action OnAbilityUsed;

    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        cancelButton.onClick.AddListener(() => Hide());

        ActionsUI.OnAbility += ActionsUI_OnAbility;

        template.gameObject.SetActive(false);

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
                Transform cardTransform = Instantiate(template, container);

                Image image = cardTransform.GetComponent<Image>();
                image.sprite = card.Sprite;

                Button button = cardTransform.GetComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    Player.LocalInstance.SubtractActionPoints();

                    card.Ability.Use();

                    Hide();

                    OnAbilityUsed?.Invoke();
                });

                cardTransform.gameObject.SetActive(true);
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
            if (child == template) continue;
            Destroy(child.gameObject);
        }
    }
}
