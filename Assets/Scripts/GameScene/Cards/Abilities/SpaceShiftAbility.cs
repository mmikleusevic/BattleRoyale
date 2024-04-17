using UnityEngine;

public class SpaceShiftAbility : MonoBehaviour, IAbility
{
    private SpaceShift spaceShift;

    private void Start()
    {
        spaceShift = GetComponent<SpaceShift>();
    }

    public void Use()
    {
        spaceShift.AbilityUsed = true;
    }
}