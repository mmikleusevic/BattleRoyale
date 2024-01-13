using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCreatorText;
    [SerializeField] private Image image;
    private Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithId(lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        lobbyCreatorText.text = lobby.Data[GameLobby.CREATOR_NAME].Value;
        image.color = GetColor();
    }

    private Color GetColor()
    {
        string[] rgba = lobby.Data[GameLobby.LOBBY_COLOR].Value.Substring(5, lobby.Data[GameLobby.LOBBY_COLOR].Value.Length - 6).Split(", ");
        Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));

        return color;
    }
}
