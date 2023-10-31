using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Card cardTemplate;
    
    private Vector2 cardDimensions;

    private Dictionary<Vector2, Card> Cards;

    private void Start()
    {
        Vector3 cardDimensions = cardTemplate.GetComponent<BoxCollider>().size;
        this.cardDimensions.x = cardDimensions.x;
        this.cardDimensions.y = cardDimensions.z;

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        Cards = new Dictionary<Vector2, Card>();

        foreach (Vector2 position in tilesToInitialize)
        {
            Card spawnedCard = Instantiate(cardTemplate, new Vector3(position.x * cardDimensions.x, position.y * cardDimensions.y), Quaternion.identity, gridContainer);
            spawnedCard.transform.rotation = Quaternion.Euler(-90, 0, 0);
            spawnedCard.name = $"Card {position.x}-{position.y}";

            Cards[position] = spawnedCard;
        }

        float halfWidth = width * cardDimensions.x / 2f;
        float halfHeight = height * cardDimensions.y / 2f;

        float offsetX = cardDimensions.x / 2f;
        float offsetY = cardDimensions.y / 2f;

        float cameraX = halfWidth - offsetX;
        float cameraY = halfHeight - offsetY;

        Camera.transform.position = new Vector3(cameraX, cameraY, -30);
        Camera.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    public Card GetTileAtPosition(Vector2 position)
    {
        if (Cards.ContainsKey(position))
        {
            return Cards[position];
        }

        return null;
    }
}
