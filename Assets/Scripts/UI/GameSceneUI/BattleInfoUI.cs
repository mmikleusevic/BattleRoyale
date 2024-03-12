using System.Collections;
using TMPro;
using UnityEngine;

public class BattleInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI battleResultText;

    private void Awake()
    {
        PlayerBattleResults.OnPlayerBattleShowUI += PlayerBattleResults_OnPlayerBattleShowUI;
        PlayerBattleResults.OnPlayerBattleRollOver += PlayerBattleResults_OnPlayerBattleRollOver;

        Hide();
    }

    private void OnDestroy()
    {
        PlayerBattleResults.OnPlayerBattleShowUI -= PlayerBattleResults_OnPlayerBattleShowUI;
        PlayerBattleResults.OnPlayerBattleRollOver -= PlayerBattleResults_OnPlayerBattleRollOver;
    }

    private void PlayerBattleResults_OnPlayerBattleShowUI(string obj)
    {
        battleResultText.text = obj;
        Show();
    }

    private void PlayerBattleResults_OnPlayerBattleRollOver(PlayerBattleResults.OnBattleRollOverEventArgs e)
    {
        battleResultText.text = string.Empty;
        HideWithDelay();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void HideWithDelay()
    {
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }
}
