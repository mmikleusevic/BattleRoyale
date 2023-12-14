using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollUI : MonoBehaviour
{
    [SerializeField] Button rollThreeButton;
    [SerializeField] Button rollSingleButton;
    [SerializeField] List<GameObject> dice;
    [SerializeField] Camera threeDiceCamera;
    [SerializeField] Camera singleDieCamera;

    Vector3 threeDiceCameraPosition;
    Vector3 singleDiceCameraPosition;
    private Vector3[] positions;
    private float interactDistance = 0.51f;
    private float rotationSpeed = 200f;
    private float rotationTime = 3f;
    private float factor;

    private void Awake()
    {
        rollThreeButton.onClick.AddListener(() =>
        {
            StartCoroutine(RotateDice());
            //rollButton.enabled = false;
        });

        rollSingleButton.onClick.AddListener(() =>
        {
            StartCoroutine(RotateDie());
            //rollButton.enabled = false;
        });

        positions = new Vector3[4];
    }

    private void Start()
    {
        for(int i = 0; i < dice.Count; i++)
        {
            Vector3 position = dice[i].transform.position;

            positions[i] = position;
        }

        threeDiceCameraPosition = threeDiceCamera.transform.position;
        singleDiceCameraPosition = singleDieCamera.transform.position;

        factor = rotationSpeed * Time.deltaTime;
    }

    private IEnumerator RotateDice()
    {
        for (int i = 0; i < dice.Count - 1; i++)
        {
            dice[i].transform.rotation = Random.rotation;
        }

        while (rotationTime > 0)
        {
            rotationTime -= Time.deltaTime;

            for (int i = 0; i < dice.Count - 1; i++)
            {
                dice[i].transform.Rotate(new Vector3(Random.Range(0f, 1f) * factor, Random.Range(0f, 1f) * factor, Random.Range(0f, 1f)) * factor);
            }

            yield return null;
        }

        int result = 0;

        for(int i = 0; i < dice.Count - 1; i++)
        {
            Vector3 direction = positions[i] - threeDiceCameraPosition;

            result += CheckSide(direction, threeDiceCameraPosition);
        }

        Debug.Log(result);

        rotationTime = 3f;
    }

    private IEnumerator RotateDie()
    {
        dice[3].transform.rotation = Random.rotation;

        while (rotationTime > 0)
        {
            rotationTime -= Time.deltaTime;

            dice[3].transform.Rotate(new Vector3(Random.Range(0f, 1f) * factor, Random.Range(0f, 1f) * factor, Random.Range(0f, 1f)) * factor);

            yield return null;
        }

        Vector3 direction = positions[3] - singleDiceCameraPosition;

        int result = CheckSide(direction, singleDiceCameraPosition);

        Debug.Log(result);

        rotationTime = 3f;
    }


    public int CheckSide(Vector3 direction, Vector3 cameraPosition)
    {        
        Physics.Raycast(cameraPosition, direction, out RaycastHit raycastHit, interactDistance);

        //TODO face camera when figured which side is hit with raycast

        int.TryParse(raycastHit.collider.name, out int result);        

        return result;
    }
}
