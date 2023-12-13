using UnityEngine;
using UnityEngine.UI;

public class DiceRollUI : MonoBehaviour
{
    [SerializeField] Button rollButton;
    [SerializeField] GameObject dice1;
    [SerializeField] GameObject dice2;
    [SerializeField] GameObject dice3;

    private float rotationSpeed = 2f;
    private float rotationTime = 3f;
    private bool isPressed = false;
    private void Awake()
    {
        rollButton.onClick.AddListener(() =>
        {
            isPressed = true;
            //rollButton.enabled = false;
        });
    }

    private void FixedUpdate()
    {
        if (!isPressed) return;

        RotateDice();
    }

    private void RotateDice()
    {
        dice1.transform.rotation = Random.rotation;
        dice2.transform.rotation = Random.rotation;
        dice3.transform.rotation = Random.rotation;

        rotationTime -= Time.deltaTime;

        if (rotationTime > 0)
        {
            dice1.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            dice2.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            dice3.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        }
        else
        {
            rotationTime = 3f;
            isPressed = false;
            Debug.Log("a");
        }
    }
}
