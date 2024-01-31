using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public Player ActivePlayer { get; private set; }

    private int activeIndex = -1;

    public List<Player> Players { get; private set; }

    private void Awake()
    {
        Instance = this;

        Players = new List<Player>();
    }

    [ClientRpc]
    public Player GetNextActivePlayerClientRpc()
    {
        activeIndex = (activeIndex + 1) % Players.Count;

        return ActivePlayer = Players[activeIndex];
    }
}
