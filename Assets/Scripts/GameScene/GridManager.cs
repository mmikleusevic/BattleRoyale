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
    [SerializeField] private Vector2[] placementTiles;
    [SerializeField] private Material lastCardMaterial;


    private Dictionary<int, int> randomCardNumberCountChecker;
    private Dictionary<Vector2, Tile> gridTiles;
    private List<int> randomNumberList;
    private Vector2[][] movementVectors;

    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Vector2 cardDimensions;
    private Tile lastTile;

    public void Awake()
    {
        Instance = this;

        randomNumberList = new List<int>();
        gridTiles = new Dictionary<Vector2, Tile>();
        movementVectors = FullMovementVectors();

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
    }

    public override void OnNetworkDespawn()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;

        base.OnNetworkDespawn();
    }

    private void Initiative_OnInitiativeStart()
    {
        GetCardDimensions();
        PositionCamera();
        GenerateRandomCardNumbers();
    }

    public void GetGridPositionsWherePlayerCanInteract()
    {
        DisableCards();

        if (!lastTile)
        {
            EnableGridPositions();
        }
        else
        {
            EnableGridPositionsWhenLastCardLeft();
        }
    }

    public void ToggleCardToGetGridPositionsWherePlayerCanGoDie()
    {
        DisableCards();
        EnableGridPositionsForDying();
    }

    public Vector2[][] FullMovementVectors()
    {
        Vector2[][] movementVectors = new Vector2[3][];

        int length = 3;

        for (int i = 0; i < length; i++)
        {
            movementVectors[i] = new Vector2[3];

            for (int j = 0; j < length; j++)
            {
                movementVectors[i][j] = new Vector2(i - 1, j - 1);
            }
        }

        return movementVectors;
    }

    public Vector2[][] HalfMovementVectors()
    {
        Vector2[][] movementVectors = new Vector2[3][];

        int length = 3;
        int half = length / 2;

        for (int i = 0; i < length; i++)
        {
            movementVectors[i] = i == half ? new Vector2[3] : new Vector2[1];

            int k = 0;

            for (int j = 0; j < length; j++)
            {
                int absI = Mathf.Abs(i - 1);
                int absJ = Mathf.Abs(j - 1);

                if (absI != absJ && (absI == half || absJ == half) || i == half && j == half)
                {
                    movementVectors[i][k] = new Vector2(i - 1, j - 1);
                    k++;
                }
            }
        }

        return movementVectors;
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

            Transform tileTransform = SpawnObject(cardSO.prefab.transform, position, new Quaternion(180, 0, 0, 0), transform, $"TILE{i}({cardSO.name})");
            Tile tile = tileTransform.GetComponent<Tile>();
            Card card = tileTransform.GetComponent<Card>();
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
        Tile tile = Tile.GetTileFromNetworkReference(networkObjectReference);

        gridTiles[position] = tile;
    }

    public void HighlightAllUnoccupiedCards()
    {
        foreach (Vector2 tileVectors in placementTiles)
        {
            Tile tile = gridTiles[tileVectors];

            if (!tile.IsOccupiedOnPlacing.Value)
            {
                tile.Enable();
                tile.ShowHighlight();
            }
        }
    }

    private void EnableGridPositionsForDying()
    {
        int lengthX = movementVectors.Length;

        Player player = Player.LocalInstance;

        for (int i = 0; i < lengthX; i++)
        {
            int lengthY = movementVectors[i].Length;

            for (int j = 0; j < lengthY; j++)
            {
                Vector2 position = player.GridPosition - movementVectors[i][j];

                if (gridTiles.ContainsKey(position))
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

    private void EnableGridPositions()
    {
        int lengthX = movementVectors.Length;

        Player player = Player.LocalInstance;

        for (int i = 0; i < lengthX; i++)
        {
            int lengthY = movementVectors[i].Length;

            for (int j = 0; j < lengthY; j++)
            {
                Vector2 position = player.GridPosition - movementVectors[i][j];

                CheckIfPlayerCanInteract(position, player);
            }
        }
    }

    private void EnableGridPositionsWhenLastCardLeft()
    {
        Player player = Player.LocalInstance;
        Vector2 directionToLastTile = lastTile.GridPosition - player.GridPosition;

        List<Vector2> enabledTiles = new List<Vector2>();

        if (IsSameOrAdjacentTile(player.GridPosition, lastTile.GridPosition))
        {
            foreach (Vector2[] row in movementVectors)
            {
                foreach (Vector2 rowPosition in row)
                {
                    Vector2 newPosition = lastTile.GridPosition + rowPosition;
                    enabledTiles.Add(newPosition);
                }
            }
        }
        else
        {
            foreach (Vector2[] row in movementVectors)
            {
                foreach (Vector2 adjacentPosition in row)
                {
                    Vector2 newPosition = player.GridPosition + adjacentPosition;
                    Vector2 vectorToPlayer = newPosition - player.GridPosition;

                    bool isAlongDirectionToLastTile = Vector2.Dot(directionToLastTile, vectorToPlayer) > 0;

                    if (isAlongDirectionToLastTile)
                    {
                        enabledTiles.Add(newPosition);
                    }
                }
            }
        }

        foreach (Vector2 position in enabledTiles)
        {
            CheckIfPlayerCanInteract(position, player);
        }
    }

    private void CheckIfPlayerCanInteract(Vector2 position, Player player)
    {
        if (gridTiles.ContainsKey(position) && (player.Movement > 0 || player.ActionPoints > 0))
        {
            Tile tile = gridTiles[position];

            if ((player.GridPosition == tile.GridPosition && tile.IsClosed && tile.AreMultipleAlivePlayersOnTheCard() != true) || (player.GridPosition == tile.GridPosition && player.ActionPoints == 0))
            {
                return;
            }

            if (!player.IsDead.Value)
            {
                tile.Enable();
                tile.ShowHighlight();
            }
        }
    }

    bool IsSameOrAdjacentTile(Vector2 currentPosition, Vector2 targetPosition)
    {
        int deltaX = (int)Mathf.Abs(currentPosition.x - targetPosition.x);
        int deltaY = (int)Mathf.Abs(currentPosition.y - targetPosition.y);

        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1) || (deltaX == 1 && deltaY == 1) || (deltaX == 0 && deltaY == 0);
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

    public void CheckNumberOfLeftCards()
    {
        int numberOfLeftCards = gridTiles.Values.Where(a => a.Card != null).Count();

        if (numberOfLeftCards == 1)
        {
            Tile tile = gridTiles.Values.FirstOrDefault(a => a.Card != null);

            if (tile.Card.WinValue > 12)
            {
                tile.Card.SetLastCardValueServerRpc();
            }

            SetLastTileClientRpc(tile.NetworkObject);

            GameManager.Instance.DisablePlayersOnLastCard();
        }
        else if (numberOfLeftCards == 0)
        {
            GameManager.Instance.DetermineWinnerAndLosers();
        }
    }

    [ClientRpc]
    private void SetLastTileClientRpc(NetworkObjectReference cardNetworkObjectReference)
    {
        Tile lastTile = Tile.GetTileFromNetworkReference(cardNetworkObjectReference);

        this.lastTile = lastTile;

        FadeMessageUI.Instance.StartFadeMessage(GameManager.Instance.CreateOnLastCardLeftGameMessage());
    }

    public Material GetLastCardMaterial()
    {
        return lastCardMaterial;
    }

    public void SetMovementVectors(Vector2[][] movementVectors)
    {
        this.movementVectors = movementVectors;
    }

    public Dictionary<Vector2, Tile> GetTiles()
    {
        return gridTiles;
    }
}
