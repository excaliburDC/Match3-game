using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : SimpleMenu<MainMenu>
{


	public void OnPlayPressed()
	{
        SceneManager.LoadScene(1);
        GameMenu.Show();
        


	}

    public void OnSoundPressed()
    {
       

    }

    public void OnOptionsPressed()
    {
		OptionsMenu.Show();
	}

	public override void OnBackPressed()
	{
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
       
	}
}
