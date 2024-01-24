using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform cardBorderTemplate;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Card cardTemplate;
    [SerializeField] private List<CardSO> cardSOs;

    private Dictionary<int, int> randomCardNumberCountChecker;
    private Dictionary<Vector2, Card> spawnedCards;
    private List<int> randomNumberList;

    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Vector2 cardDimensions;

    public void Awake()
    {
        randomNumberList = new List<int>();

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;

        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }

    private void GameManager_OnGameStateChanged(object sender, GameState e)
    {
        if (e == GameState.GamePlaying)
        {
            GetCardDimensions();
            PositionCamera();
            GenerateRandomCardNumbers();
        }
    }

    private void GetCardDimensions()
    {
        Vector3 cardDimensions = cardTemplate.GetComponent<BoxCollider>().size;
        this.cardDimensions.x = cardDimensions.x;
        this.cardDimensions.y = cardDimensions.y;
    }

    private void GenerateRandomCardNumbers()
    {
        if (!IsServer) return;

        randomCardNumberCountChecker = new Dictionary<int, int>();

        while (randomNumberList.Count < tilesToInitialize.Count)
        {
            int randomNumber = Random.Range(0, cardSOs.Count);

            if (!randomCardNumberCountChecker.ContainsKey(randomNumber))
            {
                randomCardNumberCountChecker.Add(randomNumber, 1);
                randomNumberList.Add(randomNumber);
            }
            else if (randomCardNumberCountChecker[randomNumber] < maxNumberOfEachCard)
            {
                randomCardNumberCountChecker[randomNumber]++;
                randomNumberList.Add(randomNumber);
            }
        }

        GenerateGrid();
    }


    private void GenerateGrid()
    {
        spawnedCards = new Dictionary<Vector2, Card>();

        for (int i = 0; i < tilesToInitialize.Count; i++)
        {
            Vector2 tileCoordinates = tilesToInitialize[i];
            CardSO cardSO = cardSOs[randomNumberList[i]];

            Vector2 position = new Vector3((tileCoordinates.x * cardDimensions.x) + tileCoordinates.x * spacing, (tileCoordinates.y * cardDimensions.y) + tileCoordinates.y * spacing);

            Transform cardContainerTransform = SpawnObject(cardContainer, position, Quaternion.identity, transform, $"CardContainer{cardSO.name}");

            SpawnObject(cardBorderTemplate, cardContainerTransform.position, Quaternion.identity, cardContainerTransform, $"CardBorderTemplate{cardSO.name}");

            Transform cardTransform = SpawnObject(cardSO.prefab.transform, cardContainerTransform.position, Quaternion.identity, cardContainerTransform, $"CardContainer{cardSO.name}");
            Card card = cardTransform.GetComponent<Card>();

            spawnedCards[position] = card;
        }
    }

    private Transform SpawnObject(Transform transform, Vector3 position, Quaternion rotation, Transform parent, string objectName)
    {
        Transform transformObject = Instantiate(transform, position, rotation, parent);

        SetNetworkObjectInScene(transformObject, parent, objectName);

        return transformObject;
    }

    private void SetNetworkObjectInScene(Transform transform, Transform parent, string objectName)
    {
        NetworkObject networkObject = transform.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
        networkObject.TrySetParent(parent.transform);
        GameMultiplayer.Instance.SetNameClientRpc(transform.gameObject, objectName);
    }

    private void PositionCamera()
    {
        float halfWidth = ((width * cardDimensions.x) + width * spacing) / 2f;
        float halfHeight = ((height * cardDimensions.y) + height * spacing) / 2f;

        float offsetX = (cardDimensions.x + spacing) / 2f;
        float offsetY = (cardDimensions.y + spacing) / 2f;

        float cameraX = halfWidth - offsetX;
        float cameraY = halfHeight - offsetY;

        Camera.transform.position = new Vector3(cameraX, cameraY, -33);
        Camera.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    private Card GetTileAtPosition(Vector2 position)
    {
        if (spawnedCards.ContainsKey(position))
        {
            return spawnedCards[position];
        }

        return null;
    }
}
