using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OpenCreateNewWorldScene()
    {
        SceneManager.LoadScene("NewWorldMenu", LoadSceneMode.Single);
    }
    
    public void OpenExistingWorld()
    {
        SceneManager.LoadScene("LoadWorldMenu", LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
