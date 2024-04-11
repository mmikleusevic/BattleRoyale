using UnityEngine;

public class BlackDeathAbility : MonoBehaviour, ICurse
{
    private BlackDeath blackDeath;

    private void Start()
    {
        blackDeath = GetComponent<BlackDeath>();
    }

    public void Use()
    {
        blackDeath.AbilityUsed = true;
    }
}