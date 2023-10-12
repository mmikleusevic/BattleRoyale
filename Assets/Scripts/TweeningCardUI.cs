using UnityEngine;

public class TweeningCardUI : MonoBehaviour
{
    public void OnEnable()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.zero;

        LeanTween.scale(GetComponent<RectTransform>(), Vector3.one, 0.4f).setDelay(0.3f);
    }

    public void Destroy()
    {
        LeanTween.scale(GetComponent<RectTransform>(), Vector3.zero, 0.3f).setDestroyOnComplete(true);
    }
}
