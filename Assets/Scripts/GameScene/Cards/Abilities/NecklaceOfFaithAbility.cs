using UnityEngine;

public class NecklaceOfFaithAbility : MonoBehaviour, ICardReroll
{
    private NecklaceOfFaith necklaceOfFaith;

    private void Start()
    {
        necklaceOfFaith = GetComponent<NecklaceOfFaith>();
    }

    public void Use()
    {
        necklaceOfFaith.AbilityUsed = true;
    }
}