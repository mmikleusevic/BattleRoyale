using System.Collections.Generic;
using UnityEngine;

public class TempestAbility : IAbility
{
    private Tempest tempest;
    private AbilityResults abilityResults;

    public TempestAbility(Tempest tempest, AbilityResults abilityResults)
    {
        this.tempest = tempest;
        this.abilityResults = abilityResults;
    }

    private int goal = 3;
    public void Use()
    {
        if (!tempest.AbilityUsed)
        {
            tempest.AbilityUsed = true;

            Vector2[][] movementVectors = GridManager.Instance.FullMovementVectors();
            Dictionary<Vector2, Tile> gridTiles = GridManager.Instance.GetTiles();

            List<ulong> clients = new List<ulong>();

            for (int i = 0; i < movementVectors.Length; i++)
            {
                for (int j = 0; j < movementVectors[i].Length; j++)
                {
                    Vector2 moveVector = movementVectors[i][j];

                    if (moveVector.x == 0 && moveVector.y == 0) continue;

                    Vector2 position = Player.LocalInstance.GridPosition + moveVector;

                    if (gridTiles.ContainsKey(position))
                    {
                        Tile tile = gridTiles[position];

                        List<Player> tilePlayers = tile.GetPlayersOnCard();

                        foreach (Player player in tilePlayers)
                        {
                            clients.Add(player.ClientId.Value);
                        }
                    }
                }
            }

            ulong[] clientsArray = clients.ToArray();

            abilityResults.SetRollOnClientsServerRpc(clientsArray, goal);
        }
    }
}