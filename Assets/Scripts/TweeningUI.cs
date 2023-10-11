using UnityEngine;

public class TweeningUI : MonoBehaviour
{
    private void OnEnable()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(0, 0, 0);

        LeanTween.scale(GetComponent<RectTransform>(), Vector3.one, 0.3f);
    }

    private void OnDestroy()
    {
        LeanTween.scale(GetComponent<RectTransform>(), new Vector3(0, 0, 0), 1f);
        Destroy(gameObject);
    }
}
