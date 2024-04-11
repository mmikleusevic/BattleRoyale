using UnityEngine;

public class LuckyBraceletAbility : MonoBehaviour, IPlayerReroll
{
    private LuckyBracelet luckyBracelet;

    private void Start()
    {
        luckyBracelet = GetComponent<LuckyBracelet>();
    }

    public void Use()
    {
        luckyBracelet.AbilityUsed = true;
    }
}