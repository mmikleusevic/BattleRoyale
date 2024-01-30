using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private Dictionary<ulong, Player> players;

    public Dictionary<ulong, Player> Players { get => players; set => players = value; }

    private void Awake()
    {
        Instance = this;

        Players = new Dictionary<ulong, Player>();
    }
}
