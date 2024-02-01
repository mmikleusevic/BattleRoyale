using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }
     
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Card cardTemplate;
    [SerializeField] private List<CardSO> cardSOs;

    private Dictionary<int, int> randomCardNumberCountChecker;
    private Dictionary<Vector2, Card> gridCards;
    private List<int> randomNumberList;

    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Vector2 cardDimensions;

    public void Awake()
    {
        randomNumberList = new List<int>();
        gridCards = new Dictionary<Vector2, Card>();

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        GameManager.Instance.OnPlayersOrderSet += GameManager_OnPlayersOrderSet;
    }

    public override void OnNetworkDespawn()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        GameManager.Instance.OnPlayersOrderSet -= GameManager_OnPlayersOrderSet;

        base.OnNetworkDespawn();
    }

    private void GameManager_OnPlayersOrderSet(object sender, EventArgs e)
    {
        PlacePlayerOnGrid();
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        GetCardDimensions();
        PositionCamera();
        GenerateRandomCardNumbers();
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
            int randomNumber = UnityEngine.Random.Range(0, cardSOs.Count);

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
        for (int i = 0; i < tilesToInitialize.Count; i++)
        {
            Vector2 tileCoordinates = tilesToInitialize[i];
            CardSO cardSO = cardSOs[randomNumberList[i]];

            Vector2 position = new Vector3((tileCoordinates.x * cardDimensions.x) + tileCoordinates.x * spacing, (tileCoordinates.y * cardDimensions.y) + tileCoordinates.y * spacing);

            Transform cardTransform = SpawnObject(cardSO.prefab.transform, position, new Quaternion(180, 0, 0, 0), transform, $"{cardSO.name}");
            NetworkObject cardNetworkObject = cardTransform.GetComponent<Card>().NetworkObject;

            AddCardToSpawnedCardsOnClientServerRpc(position, cardNetworkObject);
        }
    }

    private Transform SpawnObject(Transform transform, Vector3 position, Quaternion rotation, Transform parent, string objectName)
    {
        Transform transformObject = Instantiate(transform, position, rotation);

        SetNetworkObjectInScene(transformObject, parent, rotation, objectName);

        return transformObject;
    }

    private void SetNetworkObjectInScene(Transform transform, Transform parent, Quaternion rotation, string objectName)
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

    [ServerRpc]
    private void AddCardToSpawnedCardsOnClientServerRpc(Vector2 position, NetworkObjectReference networkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        AddCardToSpawnedCardsOnClientClientRpc(position, networkObjectReference);
    }

    [ClientRpc]
    private void AddCardToSpawnedCardsOnClientClientRpc(Vector2 position, NetworkObjectReference networkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return;

        Card card = networkObject.GetComponent<Card>();

        gridCards[position] = card;
    }

    private void PlacePlayerOnGrid()
    {
        Player player = PlayerManager.Instance.GetNextActivePlayerClientRpc();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.ClientId.Value }
            }
        };

        GameManager.Instance.SetState(StateEnum.PlaceOnGrid, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextClientPlacingServerRpc()
    {
        if (PlayerManager.Instance.ActivePlayer != PlayerManager.Instance.Players[PlayerManager.Instance.Players.Count - 1])
        {
            PlacePlayerOnGrid();
        }
        else
        {

        }
    }

    private Card GetTileAtPosition(Vector2 position)
    {
        if (gridCards.ContainsKey(position))
        {
            return gridCards[position];
        }

        return null;
    }
}
