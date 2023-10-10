using UnityEngine;
using UnityEngine.UI;

public class RulesUI : MonoBehaviour
{
    public static RulesUI Instance { get; private set; }

    [SerializeField] private Button CloseButton;

    private void Awake()
    {
        Hide();

        Instance = this;

        CloseButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
