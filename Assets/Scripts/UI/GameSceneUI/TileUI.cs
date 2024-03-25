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

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
    }

    private void Tile_OnTilePressed(Tile tile)
    {
        image.preserveAspect = true;
        image.sprite = tile.GetCardOrTileSprite();
    }
}
