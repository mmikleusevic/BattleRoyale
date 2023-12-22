using UnityEngine;

public class DiceRollUI : MonoBehaviour
{
    public static DiceRollUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }  

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
