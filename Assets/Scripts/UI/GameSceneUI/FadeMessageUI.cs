using System.Collections;
using TMPro;
using UnityEngine;

public class FadeMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fadeText;

    private float maxFadeTime = 4f;
    private float fadeTime;
    private float maxAlphaValue = 1f;
    private float alphaValue;
    private float fadePerSecond;

    private void Awake()
    {
        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid += PlaceOnGrid_OnPlaceOnGrid;
        PlayerPreturn.OnPlayerPreturn += PlayerPreturn_OnPlayerPreturn;
        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
        Player.OnPlayerResurrected += Player_OnPlayerResurrected;
        CardBattleResults.OnCardLost += CardBattleResults_OnCardLost;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardWon;
        Player.OnPlayerTookCard += Player_OnPlayerTookCard;
        Player.OnPlayerDiedPlayerBattle += Player_OnPlayerDiedPlayerBattle;
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
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid -= PlaceOnGrid_OnPlaceOnGrid;
        PlayerPreturn.OnPlayerPreturn -= PlayerPreturn_OnPlayerPreturn;
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardLost;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardWon;
        Player.OnPlayerDiedPlayerBattle -= Player_OnPlayerDiedPlayerBattle;
        Player.OnPlayerSelectedPlaceToDie -= Player_OnPlayerSelectedPlaceToDie;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        StartFadeMessage(e);
    }

    private void PlaceOnGrid_OnPlaceOnGrid(object sender, string[] e)
    {
        StartFadeMessage(e[0]);
    }

    private void PlayerPreturn_OnPlayerPreturn(object sender, string[] e)
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

    private void Player_OnPlayerTookCard(string[] obj)
    {
        StartFadeMessage(obj[0]);
    }

    private void Player_OnPlayerDiedPlayerBattle(string[] obj)
    {
        KeepMessage(obj[0]);
    }

    private void Player_OnPlayerSelectedPlaceToDie(ulong obj)
    {
        fadeText.text = string.Empty;
    }

    private void KeepMessage(string message)
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
