using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : MonoBehaviour
{
    public static event Action<Tile> OnMove;
    public static event Action<NetworkObjectReference, NetworkObjectReference> OnAttackCard;
    public static event Action<Tile> OnAttackPlayer;
    public static event Action OnAbility;

    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackCardButton;
    [SerializeField] private Button attackPlayerButton;
    [SerializeField] private Button abilitiesButton;

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
            Player.LocalInstance.SubtractActionPoints();
            OnAttackCard?.Invoke(tile.NetworkObject, Player.LocalInstance.NetworkObject);
            MessageUI.Instance.SendMessageToEveryoneExceptMe(SendAttackingCardMessage());
        });

        attackPlayerButton.onClick.AddListener(() =>
        {
            OnAttackPlayer?.Invoke(tile);
        });

        abilitiesButton.onClick.AddListener(() =>
        {
            OnAbility?.Invoke();
        });

        abilitiesButton.gameObject.SetActive(false);

        Tile.OnTilePressed += Tile_OnTilePressed;
        PlayerTurn.OnPlayerTurnOver += PlayerTurn_OnPlayerTurnOver;
    }

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
        PlayerTurn.OnPlayerTurnOver -= PlayerTurn_OnPlayerTurnOver;

        moveButton.onClick.RemoveAllListeners();
        attackCardButton.onClick.RemoveAllListeners();
        attackPlayerButton.onClick.RemoveAllListeners();
        abilitiesButton.onClick.RemoveAllListeners();
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

    private void PlayerTurn_OnPlayerTurnOver()
    {
        HideAbilitiesButton();
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

            if (isPlayerOnCard && player.ActionPoints > 0 && !tile.AreMultipleAlivePlayersOnTheCard() && !tile.IsClosed)
            {
                ShowAttackCardButton();
            }
            else
            {
                HideAttackCardButton();
            }

            if (isPlayerOnCard && player.ActionPoints > 0 && tile.AreMultipleAlivePlayersOnTheCard())
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

        if (player == PlayerManager.Instance.ActivePlayer)
        {
            foreach (Card card in player.EquippedCards)
            {
                if (card.Ability != null)
                {
                    Type abilityType = card.Ability.GetType();
                    Type[] implementedInterfaces = abilityType.GetInterfaces();

                    bool isStrictIAbility = implementedInterfaces.Length == 1 && implementedInterfaces[0] == typeof(IAbility);

                    if (!card.AbilityUsed && player.ActionPoints > 0 && isStrictIAbility)
                    {
                        ShowAbilitiesButton();
                        break;
                    }
                    else
                    {
                        HideAbilitiesButton();
                    }
                }
            }
        }
        else
        {
            HideAbilitiesButton();
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

    private void ShowAbilitiesButton()
    {
        abilitiesButton.gameObject.SetActive(true);
    }

    private void HideAbilitiesButton()
    {
        abilitiesButton.gameObject.SetActive(false);
    }

    private string[] SendAttackingCardMessage()
    {
        return new string[] {
            $"YOU'RE ATTACKING {tile.GetCardOrTileName()} ({tile.Card.WinValue})",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}'s </color>" + $"attacking {tile.GetCardOrTileName()} ({tile.Card.WinValue})"
        };
    }

    public static void ResetStaticData()
    {
        OnMove = null;
        OnAttackCard = null;
        OnAttackPlayer = null;
        OnAbility = null;
    }
}
