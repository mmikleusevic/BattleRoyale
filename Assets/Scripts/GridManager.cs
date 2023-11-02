using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Card cardTemplate;
    [SerializeField] private List<CardSO> cardSOs;

    private float spacing = 0.2f;
    private int maxNumberOfEachCard = 2;
    private Dictionary<int,int> randomCardNumberCountChecker;
    private List<int> randomNumberList;
    private Vector2 cardDimensions;

    private Dictionary<Vector2, Card> cards;

    private void Start()
    {
        GetCardDimensions();
        GenerateRandomCardNumbers();
        GenerateGridServerRpc();
        PositionCamera();
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
        randomNumberList = new List<int>();

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

        foreach (var a in randomNumberList)
        {
            Debug.Log(a);
        }

        foreach (var a in randomCardNumberCountChecker)
        {
            Debug.Log(a.Key + "-" + a.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GenerateGridServerRpc()
    {
        GenerateGridClientRpc();
    }

    [ClientRpc]
    private void GenerateGridClientRpc()
    {
        cards = new Dictionary<Vector2, Card>();

        foreach (Vector2 position in tilesToInitialize)
        {
            Card spawnedCard = Instantiate(cardTemplate, new Vector3((position.x * cardDimensions.x) + position.x * spacing, (position.y * cardDimensions.y) + position.y * spacing), Quaternion.identity, gridContainer);
            //spawnedCard.transform.rotation = Quaternion.Euler(0, 0, 0);
            //spawnedCard.AssignMaterial(cardSOs[index].sprite);
            spawnedCard.name = $"Card {position.x}-{position.y}";

            cards[position] = spawnedCard;
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
        if (cards.ContainsKey(position))
        {
            return cards[position];
        }

        return null;
    }
}
