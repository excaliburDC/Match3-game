using UnityEngine.SceneManagement;

public class PauseMenu : SimpleMenu<PauseMenu>
{
	public void OnQuitPressed()
	{
		Hide();
		Destroy(this.gameObject); // This menu does not automatically destroy itself

		GameMenu.Hide();
        SceneManager.LoadScene(0);
        ScoreManager.sInstance.Reset();
	}
}
