using TMPro;
using UnityEngine;

public class ForceTextToUpper : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private void Start()
    {
        inputField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck()
    {
        inputField.text = inputField.text.ToUpper();
    }
}
