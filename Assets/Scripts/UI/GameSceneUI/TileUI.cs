using UnityEngine;
using UnityEngine.UI;

public class TileUI : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();

        Tile.OnTilePressed += Tile_OnTilePressed;
    }

    private void Tile_OnTilePressed(object sender, Player e)
    {
        Tile tile = sender as Tile;
        image.sprite = tile.GetCardOrTileSprite();
    }

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
    }
}
