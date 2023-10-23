using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tile;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private new Transform camera;
    [SerializeField] private List<Vector2> tilesToInitialize;
    [SerializeField] private Vector2 cardDimensions;

    private Dictionary<Vector2, Tile> Tiles;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        Tiles = new Dictionary<Vector2, Tile>();

        foreach (Vector2 position in tilesToInitialize)
        {
            Tile spawnedTile = Instantiate(tile, new Vector3(position.x * cardDimensions.x, position.y * cardDimensions.y), Quaternion.identity, gridContainer);
            spawnedTile.name = $"Tile {position.x}-{position.y}";

            bool isOffset = (position.x + position.y) % 2 == 1;
            spawnedTile.ColorTile(isOffset);

            Tiles[position] = spawnedTile;
        }

        float halfWidth = width * cardDimensions.x / 2f;
        float halfHeight = height * cardDimensions.y / 2f;

        float offsetX = cardDimensions.x / 2f;
        float offsetY = cardDimensions.y / 2f;

        float cameraX = halfWidth - offsetX;
        float cameraY = halfHeight - offsetY;

        camera.transform.position = new Vector3(cameraX, cameraY, -14);
        camera.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    public Tile GetTileAtPosition(Vector2 position)
    {
        if (Tiles.ContainsKey(position))
        {
            return Tiles[position];
        }

        return null;
    }
}
