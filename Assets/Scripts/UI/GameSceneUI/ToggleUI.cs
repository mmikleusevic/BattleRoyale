using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    [SerializeField] RectTransform handleRectTransform;
    [SerializeField] Color backgroundActiveColor;
    [SerializeField] Color handleActiveColor;

    Image backgroundImage;
    Image handleImage;

    Color backgroundDefaultColor;
    Color handleDefaultColor;

    Toggle toggle;
    Vector2 handlePosition;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        handlePosition = handleRectTransform.anchoredPosition;
        backgroundImage = handleRectTransform.parent.GetComponent<Image>();
        handleImage = handleRectTransform.GetComponent<Image>();
        backgroundDefaultColor = backgroundImage.color;
        handleDefaultColor = handleImage.color;
        toggle.onValueChanged.AddListener(OnToggle);
    }

    private void OnToggle(bool isOn)
    {
        if (isOn)
        {
            handleRectTransform.DOAnchorPos(handlePosition * -1, .4f).SetEase(Ease.InOutBack);
            backgroundImage.DOColor(backgroundActiveColor, .6f);
            handleImage.DOColor(handleActiveColor, .4f);

        }
        else
        {
            handleRectTransform.DOAnchorPos(handlePosition, .4f).SetEase(Ease.InOutBack);
            backgroundImage.DOColor(backgroundDefaultColor, .6f);
            handleImage.DOColor(handleDefaultColor, .4f);
        }
    }
}
