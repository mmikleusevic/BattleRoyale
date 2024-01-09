using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SingleDiceRollUI : Roll
{
    public static SingleDiceRollUI Instance { get; private set; }

    [SerializeField] private Button rollSingleButton;
    [SerializeField] private GameObject die;
    [SerializeField] private Camera singleDieCamera;

    private Vector3 singleDiceCameraPosition;
    private Vector3 diePosition;

    private void Awake()
    {
        Instance = this;

        rollSingleButton.onClick.AddListener(() =>
        {
            rollSingleButton.gameObject.SetActive(false);
            StartCoroutine(RotateDie());
        });

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;

        Hide();
    }

    private void Start()
    {
        diePosition = die.transform.position;

        singleDiceCameraPosition = singleDieCamera.transform.position;

        factor = rotationSpeed * Time.deltaTime;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        Debug.Log("radi");
        Show();
    }

    private IEnumerator RotateDie()
    {
        die.transform.rotation = Random.rotation;

        float xAxis = Random.Range(0.5f, 1f) * factor;
        float yAxis = Random.Range(0.5f, 1f) * factor;
        float zAxis = Random.Range(0.5f, 1f) * factor;

        while (rotationTime > 0)
        {
            rotationTime -= Time.deltaTime;

            die.transform.Rotate(xAxis, yAxis, zAxis);

            yield return null;
        }

        Vector3 direction = diePosition - singleDiceCameraPosition;

        int result = GetSideAndRotate(direction, singleDiceCameraPosition, die);
       
        //TODO remove
        Debug.Log(result);

        rotationTime = 3f;
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
