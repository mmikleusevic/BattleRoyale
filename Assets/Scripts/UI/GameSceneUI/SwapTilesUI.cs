using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwapTilesUI : MonoBehaviour
{
    public static event Action OnCancel;

    [SerializeField] private Button button;
    [SerializeField] private Image tileToSwapImage;
    [SerializeField] private TextMeshProUGUI swapText;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            OnCancel?.Invoke();
            Hide();
        });

        Hide();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetUI(Sprite sprite, string tileName)
    {
        tileToSwapImage.sprite = sprite;
        swapText.text = "SWAPPING " + tileName;

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        tileToSwapImage.sprite = null;
        swapText.text = string.Empty;

        gameObject.SetActive(false);
    }

    public static void ResetStaticData()
    {
        OnCancel = null;
    }
}
