using Crystal;
using UnityEngine;

public class SpaceShiftAbility : MonoBehaviour, IAbility
{
    private InteractUI interactUI;
    private SwapTilesUI swapTilesUI;
    private Tile tileToSwap;
    private Tile tileToSwapWith;

    private SpaceShift spaceShift;

    private void Awake()
    {
        SwapTilesUI.OnCancel += ResetTiles;
    }

    private void Start()
    {
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        SafeArea safeArea = canvas.GetComponentInChildren<SafeArea>();

        interactUI = safeArea.GetComponentInChildren<InteractUI>(true);
        swapTilesUI = interactUI.GetComponentInChildren<SwapTilesUI>(true);

        spaceShift = GetComponent<SpaceShift>();
    }

    private void OnDestroy()
    {
        SwapTilesUI.OnCancel -= ResetTiles;
    }

    public void Use()
    {
        Tile tile = interactUI.GetTile();
        SetTile(tile);

        if (tileToSwap == null || tileToSwapWith == null) return;

        spaceShift.AbilityUsed = true;
        swapTilesUI.Hide();

        Player.LocalInstance.SubtractActionPoints();
        GridManager.Instance.SwapTiles(tileToSwap, tileToSwapWith);
        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnTileSwappedMessage());

        ResetTiles();
    }

    private void ResetTiles()
    {
        tileToSwap = null;
        tileToSwapWith = null;
    }

    private void SetTile(Tile tile)
    {
        if (tileToSwap == null)
        {
            tileToSwap = tile;
            swapTilesUI.SetUI(tile.Sprite, tile.name);
        }
        else if (tileToSwapWith == null)
        {
            tileToSwapWith = tile;
        }
    }

    private string[] CreateOnTileSwappedMessage()
    {
        return new string[]
        {
            $"Swapped {tileToSwap.name} with {tileToSwapWith.name}",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> swapped {tileToSwap.name} with {tileToSwapWith.name}"
        };
    }
}