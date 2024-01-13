using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Card cardTemplate;
    [SerializeField] private List<CardSO> cardSOs;

    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Dictionary<int, int> randomCardNumberCountChecker;
    private NetworkList<int> randomNumberList;
    private Vector2 cardDimensions;

    private Dictionary<Vector2, Card> spawnedCards;

    private void Awake()
    {
        randomNumberList = new NetworkList<int>();

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        GetCardDimensions();
        PositionCamera();

        if (IsServer)
        {
            GenerateRandomCardNumbers();
            GenerateGrid();
        }
    }

    public override void OnDestroy()
    {
        randomNumberList.Dispose();
    }

    private void GetCardDimensions()
    {
        Vector3 cardDimensions = cardTemplate.GetComponent<BoxCollider>().size;
        this.cardDimensions.x = cardDimensions.x;
        this.cardDimensions.y = cardDimensions.y;
    }

    private void GenerateRandomCardNumbers()
    {
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
    }

    private void GenerateGrid()
    {
        spawnedCards = new Dictionary<Vector2, Card>();

        for (int i = 0; i < tilesToInitialize.Count; i++)
        {
            Vector2 tileCoordinates = tilesToInitialize[i];
            CardSO cardSO = cardSOs[randomNumberList[i]];

            Vector2 position = new Vector3((tileCoordinates.x * cardDimensions.x) + tileCoordinates.x * spacing, (tileCoordinates.y * cardDimensions.y) + tileCoordinates.y * spacing);

            Transform cardContainer = Instantiate(this.cardContainer, position, Quaternion.identity, transform);

            NetworkObject cardContainerNetworkObject = cardContainer.GetComponent<NetworkObject>();
            cardContainerNetworkObject.Spawn();
            cardContainerNetworkObject.TrySetParent(transform);
            GameMultiplayer.Instance.SetNameClientRpc(cardContainer.gameObject, $"CardContainer{cardSO.name}");

            Card spawnedCard = Instantiate(cardSO.prefab, cardContainer.position, Quaternion.identity, cardContainer);

            NetworkObject spawnedCardNetworkObject = spawnedCard.GetComponent<NetworkObject>();
            spawnedCardNetworkObject.Spawn();
            spawnedCardNetworkObject.TrySetParent(cardContainer.transform);
            GameMultiplayer.Instance.SetNameClientRpc(spawnedCard.gameObject, cardSO.name);

            spawnedCards[position] = spawnedCard;
        }
    }

    public void PositionCamera()
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

    public Card GetTileAtPosition(Vector2 position)
    {
        if (spawnedCards.ContainsKey(position))
        {
            return spawnedCards[position];
        }

        return null;
    }
}
