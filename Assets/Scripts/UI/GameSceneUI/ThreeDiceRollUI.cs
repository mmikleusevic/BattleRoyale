using Unity.Netcode;
using UnityEditor.Search;
using UnityEngine;

public class ThreeDiceRollUI : MonoBehaviour
{
    [SerializeField] private RollUI rollUI;
    private void Awake()
    {
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
    }

    private void OnDestroy()
    {
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;
    }

    private void ActionsUI_OnAttackCard(Card obj)
    {
        rollUI.Show();
    }
}
