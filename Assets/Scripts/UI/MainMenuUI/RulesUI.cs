using UnityEngine;
using UnityEngine.UI;

public class RulesUI : MonoBehaviour
{
    public static RulesUI Instance { get; private set; }

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        Instance = this;

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
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
