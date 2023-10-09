using UnityEngine;
using UnityEngine.UI;

public class CardsUI : MonoBehaviour
{
    public static CardsUI Instance { get; private set; }

    [SerializeField] private Button CloseButton;

    private void Awake()
    {
        Instance = this;

        CloseButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        Hide();
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
