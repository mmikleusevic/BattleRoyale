using System.Collections;
using TMPro;
using UnityEngine;

public class FadeMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fadeText;

    private float maxFadeTime = 5f;
    private float fadeTime;
    private float maxAlphaValue = 1f;
    private float alphaValue;
    private float fadePerSecond;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid += PlaceOnGrid_OnPlaceOnGrid;
        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
        Player.OnPlayerResurrected += Player_OnPlayerResurrected;
        CardBattleResults.OnCardLost += CardBattleResults_OnCardLost;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardWon;
    }

    private void Start()
    {
        fadeTime = maxFadeTime;
        alphaValue = maxAlphaValue;
        fadePerSecond = 1 / fadeTime;
    }

    private void OnDestroy()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid -= PlaceOnGrid_OnPlaceOnGrid;
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardLost;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardWon;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        StartFadeMessage(e);
    }

    private void PlaceOnGrid_OnPlaceOnGrid(object sender, string[] e)
    {
        StartFadeMessage(e[0]);
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
    {
        StartFadeMessage(e[0]);
    }

    private void Player_OnPlayerResurrected(string[] obj)
    {
        StartFadeMessage(obj[0]);
    }

    private void CardBattleResults_OnCardLost(CardBattleResults.OnCardBattleEventArgs obj)
    {
        StartFadeMessage(obj.messages[0]);
    }

    private void CardBattleResults_OnCardWon(CardBattleResults.OnCardBattleEventArgs obj)
    {
        StartFadeMessage(obj.messages[0]);
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
