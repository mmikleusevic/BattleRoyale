using UnityEngine;

public class Die : MonoBehaviour
{
    [SerializeField] private GameObject[] dieGameObject;

    public bool Reroll { get; set; } = false;

    public bool Rolled { get; set; } = false;
}
