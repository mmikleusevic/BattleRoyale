public class SingleDiceRollUI : RollUI
{
    public override void Start()
    {
        base.Start();

        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    public void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, System.EventArgs e)
    {
        Show();
    }
}
