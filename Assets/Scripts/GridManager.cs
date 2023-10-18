using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int Width;
    [SerializeField] private int Height;
    [SerializeField] private Tile Tile;
    [SerializeField] private Transform GridContainer;
    [SerializeField] private Transform Camera;
    [SerializeField] private List<Vector2> TilesToInitialize;
    [SerializeField] private Vector2 CardDimensions;

    private Dictionary<Vector2, Tile> Tiles;

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        Tiles = new Dictionary<Vector2, Tile>();

        foreach (Vector2 position in TilesToInitialize)
        {
            Tile spawnedTile = Instantiate(Tile, new Vector3(position.x * CardDimensions.x, position.y * CardDimensions.y), Quaternion.identity, GridContainer);
            spawnedTile.name = $"Tile {position.x}-{position.y}";

            bool isOffset = (position.x + position.y) % 2 == 1;
            spawnedTile.ColorTile(isOffset);

            Tiles[position] = spawnedTile;
        }

        float halfWidth = Width * CardDimensions.x / 2f;
        float halfHeight = Height * CardDimensions.y / 2f;

        float offsetX = CardDimensions.x / 2f;
        float offsetY = CardDimensions.y / 2f;

        float cameraX = halfWidth - offsetX;
        float cameraY = halfHeight - offsetY;

        Camera.transform.position = new Vector3(cameraX, cameraY, -14);
        Camera.transform.rotation = Quaternion.Euler(0, 0, 90);
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
