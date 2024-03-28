using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private SetVisual[] diceVisuals;
    [SerializeField] private TextureColorChanger textureColorChanger;

    private void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerData();

        Color color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        textureColorChanger.ChangeTextureColor(color);

        foreach (SetVisual dieVisual in diceVisuals)
        {
            dieVisual.SetColor(color);
        }
    }
}
