using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    public void StartGameButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}
