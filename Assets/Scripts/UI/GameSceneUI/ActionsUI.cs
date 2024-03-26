using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : MonoBehaviour
{
    public static event Action<Tile> OnMove;
    public static event Action<Tile, string[]> OnAttackCard;
    public static event Action<Tile> OnAttackPlayer;

    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackCardButton;
    [SerializeField] private Button attackPlayerButton;

    private Tile tile;

    private void Awake()
    {
        moveButton.onClick.AddListener(() =>
        {
            OnMove?.Invoke(tile);
        });

        attackCardButton.onClick.AddListener(() =>
        {
            RollType.rollType = RollTypeEnum.CardAttack;
            OnAttackCard?.Invoke(tile, SendAttackingCardMessage());
        });

        attackPlayerButton.onClick.AddListener(() =>
        {
            OnAttackPlayer?.Invoke(tile);
        });

        Tile.OnTilePressed += Tile_OnTilePressed;
    }

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
        moveButton.onClick.RemoveAllListeners();
        attackCardButton.onClick.RemoveAllListeners();
        attackPlayerButton.onClick.RemoveAllListeners();
    }

    private void OnDisable()
    {
        if (tile == null) return;
        tile.OnTileValueChanged -= Tile_OnTileValueChanged;
    }

    private void Tile_OnTilePressed(Tile tile)
    {
        this.tile = tile;

        this.tile.OnTileValueChanged += Tile_OnTileValueChanged;

        gameObject.SetActive(true);

        GetPossibleActions();
    }

    private void GetPossibleActions()
    {
        Player player = Player.LocalInstance;

        if (tile != null && tile.Interactable)
        {
            bool isPlayerOnCard = player.GridPosition == tile.GridPosition;
            bool canMoveOrUseAction = player.Movement > 0 || player.ActionPoints > 0;

            if (!isPlayerOnCard && canMoveOrUseAction || player.PickPlaceToDie)
            {
                ShowMoveButton();
            }
            else
            {
                HideMoveButton();
            }

            if (isPlayerOnCard && player.ActionPoints > 0 && !tile.AreMultipleAlivePeopleOnTheCard() && !tile.IsClosed)
            {
                ShowAttackCardButton();
            }
            else
            {
                HideAttackCardButton();
            }

            if (isPlayerOnCard && player.ActionPoints > 0 && tile.AreMultipleAlivePeopleOnTheCard())
            {
                ShowAttackPlayerButton();
            }
            else
            {
                HideAttackPlayerButton();
            }
        }
        else
        {
            HideAll();
        }
    }

    private void Tile_OnTileValueChanged()
    {
        GetPossibleActions();
    }

    private void HideAll()
    {
        HideMoveButton();
        HideAttackCardButton();
        HideAttackPlayerButton();
    }

    private void ShowMoveButton()
    {
        moveButton.gameObject.SetActive(true);
    }

    private void HideMoveButton()
    {
        moveButton.gameObject.SetActive(false);
    }

    private void ShowAttackCardButton()
    {
        attackCardButton.gameObject.SetActive(true);
    }

    private void HideAttackCardButton()
    {
        attackCardButton.gameObject.SetActive(false);
    }

    private void ShowAttackPlayerButton()
    {
        attackPlayerButton.gameObject.SetActive(true);
    }

    private void HideAttackPlayerButton()
    {
        attackPlayerButton.gameObject.SetActive(false);
    }

    private string[] SendAttackingCardMessage()
    {
        return new string[] {
            $"YOU'RE ATTACKING {tile.GetCardOrTileName()}",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>" + $"attacking {tile.GetCardOrTileName()}"
        };
    }
}
