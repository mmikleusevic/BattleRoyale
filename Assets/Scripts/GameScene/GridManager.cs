using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Tile tileTemplate;
    [SerializeField] private List<CardSO> cardSOs;

    private Dictionary<int, int> randomCardNumberCountChecker;
    private Dictionary<Vector2, Tile> gridTiles;
    private List<int> randomNumberList;

    private Vector2[,] movementVectors;
    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Vector2 cardDimensions;

    public void Awake()
    {
        Instance = this;

        randomNumberList = new List<int>();
        gridTiles = new Dictionary<Vector2, Tile>();
        SetMovementVectors();

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid += PlaceOnGrid_OnPlaceOnGrid;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        Player.OnPlayerMoved += Player_OnPlayerMoved;
        Tile.OnTileClosed += Tile_OnTileClosed;
        Player.OnPlayerDiedCardBattle += Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected += Player_OnPlayerResurrected;
    }

    public override void OnNetworkDespawn()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        PlaceOnGrid.OnPlaceOnGrid -= PlaceOnGrid_OnPlaceOnGrid;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        Player.OnPlayerMoved -= Player_OnPlayerMoved;
        Tile.OnTileClosed -= Tile_OnTileClosed;
        Player.OnPlayerDiedCardBattle -= Player_OnPlayerDiedCardBattle;
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;

        base.OnNetworkDespawn();
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        GetCardDimensions();
        PositionCamera();
        GenerateRandomCardNumbers();
    }

    private void PlaceOnGrid_OnPlaceOnGrid(object sender, string[] e)
    {
        HighlightAllUnoccupiedCards();
    }

    private void Player_OnPlayerTurnSet()
    {
        EnableGridPositionsWherePlayerCanInteract(Player.LocalInstance);
    }

    private void Player_OnPlayerMoved(object sender, string e)
    {
        DisableCards();

        EnableGridPositionsWherePlayerCanInteract(Player.LocalInstance);
    }

    private void Tile_OnTileClosed()
    {
        DisableCards();

        EnableGridPositionsWherePlayerCanInteract(Player.LocalInstance);
    }

    private void Player_OnPlayerDiedCardBattle()
    {
        DisableCards();
    }

    private void Player_OnPlayerResurrected(string[] messages)
    {
        DisableCards();

        EnableGridPositionsWherePlayerCanInteract(Player.LocalInstance);
    }

    //TODO will use on player battle loss
    private void Player_OnPlayerDiedPlayerBattle()
    {
        DisableCards();

        EnableGridPositionsWherePlayerCanGoDie(Player.LocalInstance);
    }

    private void SetMovementVectors()
    {
        movementVectors = new Vector2[3, 3];

        int k = 0;
        int l = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                movementVectors[k, l] = new Vector2(i, j);
                l++;
            }
            l = 0;
            k++;
        }
    }

    private void GetCardDimensions()
    {
        Vector3 cardDimensions = tileTemplate.GetComponent<BoxCollider>().size;
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
        for (int i = 0; i < tilesToInitialize.Count; i++)
        {
            Vector2 gridPosition = tilesToInitialize[i];

            int index = randomNumberList[i];
            CardSO cardSO = cardSOs[index];

            Vector2 position = new Vector3((gridPosition.x * cardDimensions.x) + gridPosition.x * spacing, (gridPosition.y * cardDimensions.y) + gridPosition.y * spacing);

            Transform cardTransform = SpawnObject(cardSO.prefab.transform, position, new Quaternion(180, 0, 0, 0), transform, $"{cardSO.name}");
            Tile tile = cardTransform.GetComponent<Tile>();
            Card card = cardTransform.GetComponent<Card>();
            card.InitializeClientRpc(index);
            tile.InitializeClientRpc(gridPosition, index);
            NetworkObject tileNetworkObject = tile.NetworkObject;

            AddCardToSpawnedCardsOnClientServerRpc(gridPosition, tileNetworkObject);
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

        float distanceMultiplier = 1.4f;
        float cameraDistance = -Mathf.Max(halfWidth, halfHeight) * distanceMultiplier;

        Camera.transform.position = new Vector3(cameraX, cameraY, cameraDistance);
        Camera.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    [ServerRpc]
    private void AddCardToSpawnedCardsOnClientServerRpc(Vector2 position, NetworkObjectReference networkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        AddTileToSpawnedTilesOnClientsClientRpc(position, networkObjectReference);
    }

    [ClientRpc]
    private void AddTileToSpawnedTilesOnClientsClientRpc(Vector2 position, NetworkObjectReference networkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return;

        Tile tile = networkObject.GetComponent<Tile>();

        gridTiles[position] = tile;
    }

    //Mental note --- Next time make a normal grid...
    public void HighlightAllUnoccupiedCards()
    {
        int startHeight = height / 2;
        int mod = height % 2;
        int maxHeight = startHeight + mod;

        for (int i = 0; i < width; i++)
        {
            for (int j = startHeight; j < maxHeight; j++)
            {
                if (j == startHeight || j == maxHeight - 1)
                {
                    Vector3 tilePosition = tilesToInitialize.Where(a => a.x == i && a.y == j).FirstOrDefault();

                    Tile tile = gridTiles[tilePosition];

                    if (!tile.IsOccupiedOnPlacing.Value)
                    {
                        tile.Enable();
                        tile.ShowHighlight();
                    }
                }
            }

            if (i < width / 2)
            {
                startHeight--;
                maxHeight++;
            }
            else
            {
                startHeight++;
                maxHeight--;
            }
        }
    }

    public void EnableGridPositionsWherePlayerCanGoDie(Player player)
    {
        int lengthX = movementVectors.GetLength(0);
        int lengthY = movementVectors.GetLength(1);

        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                Vector2 position = player.GridPosition - movementVectors[i, j];

                if (gridTiles.ContainsKey(position) && (player.Movement > 0 || player.ActionPoints > 0))
                {
                    Tile tile = gridTiles[position];

                    if (player.GridPosition == tile.GridPosition)
                    {
                        continue;
                    }

                    tile.Enable();
                    tile.ShowHighlight();
                }
            }
        }
    }

    public void EnableGridPositionsWherePlayerCanInteract(Player player)
    {
        int lengthX = movementVectors.GetLength(0);
        int lengthY = movementVectors.GetLength(1);

        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                Vector2 position = player.GridPosition - movementVectors[i, j];

                if (gridTiles.ContainsKey(position) && (player.Movement > 0 || player.ActionPoints > 0))
                {
                    Tile tile = gridTiles[position];

                    if (player.GridPosition == tile.GridPosition && tile.IsClosed)
                    {
                        continue;
                    }

                    if (!player.IsDead.Value)
                    {
                        tile.Enable();
                        tile.ShowHighlight();
                    }
                }
            }
        }
    }

    public void DisableCards()
    {
        foreach (var item in gridTiles)
        {
            Tile tile = item.Value;

            tile.Disable();
            tile.HideHighlight();
        }
    }

    public CardSO GetCardSOAtPosition(int index)
    {
        return cardSOs[index];
    }
}
