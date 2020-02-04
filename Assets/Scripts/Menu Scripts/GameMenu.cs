public class GameMenu : SimpleMenu<GameMenu>
{
	public override void OnBackPressed()
	{
		PauseMenu.Show();
        ScoreManager.sInstance.isTicking = false;
	}
}
