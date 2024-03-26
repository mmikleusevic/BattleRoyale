using System.Collections;
using TMPro;
using UnityEngine;

public class FadeMessageUI : MonoBehaviour
{
    public static FadeMessageUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI fadeText;

    private float maxFadeTime = 4f;
    private float fadeTime;
    private float maxAlphaValue = 1f;
    private float alphaValue;
    private float fadePerSecond;

    private void Awake()
    {
        Instance = this;

        Player.OnPlayerSelectedPlaceToDie += Player_OnPlayerSelectedPlaceToDie;
    }

    private void Start()
    {
        fadeTime = maxFadeTime;
        alphaValue = maxAlphaValue;
        fadePerSecond = 1 / fadeTime;
    }

    private void OnDestroy()
    {
        Player.OnPlayerSelectedPlaceToDie -= Player_OnPlayerSelectedPlaceToDie;
    }

    private void Player_OnPlayerSelectedPlaceToDie(ulong obj)
    {
        fadeText.text = string.Empty;
    }

    public void KeepMessage(string message)
    {
        fadeText.text = message;
        alphaValue = maxAlphaValue;
        fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, maxAlphaValue);
    }

    public void StartFadeMessage(string message)
    {
        StopAllCoroutines();

        alphaValue = maxAlphaValue;
        fadeTime = maxFadeTime;

        StartCoroutine(FadeMessage(message));
    }

    private IEnumerator FadeMessage(string message)
    {
        fadeText.text = message;

        float fullFadeTime = 2f;

        while (fullFadeTime >= 0)
        {
            fullFadeTime -= Time.deltaTime;
            fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, maxAlphaValue);

            yield return null;
        }

        while (fadeTime >= 0)
        {
            fadeTime -= Time.deltaTime;
            alphaValue -= fadePerSecond * Time.deltaTime;
            fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, alphaValue);

            yield return null;
        }

        alphaValue = maxAlphaValue;
        fadeTime = maxFadeTime;
    }
}
