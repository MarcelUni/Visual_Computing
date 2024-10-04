using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // UI Panels
    public GameObject mainMenuUI;
    public GameObject gameUI;
    public GameObject deathUI;

    // Main Character reference for dead check
    public GameObject mainCharacter;

    // Game state tracking
    private bool isGameActive = false;

    // Player Save Data Keys
    private const string PlayerPositionKey = "PlayerPosition";
    private const string SceneKey = "Scene";

    // should be called when pressing "Esc"
    private void Start()
    {
        StartGame();
    }
    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        gameUI.SetActive(false);
        deathUI.SetActive(false);
        isGameActive = false;
        Time.timeScale = 0f; // Pause the game
    }

    // should be called when pressing "Play" from the mainmenuUI
    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        gameUI.SetActive(true);
        deathUI.SetActive(false);
        isGameActive = true;
        Time.timeScale = 1f; // Resume the game
    }

    public void OnCharacterDeath()
    {
        if(isGameActive)
        {
            ShowDeathScreen();
        }
    }

    public void ShowDeathScreen()
    {
        deathUI.SetActive(true);
        gameUI.SetActive(false);
        isGameActive = false;
        Time.timeScale = 0f; // Pause the game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SaveGame()
    {
        Vector3 playerPosition = mainCharacter.transform.position;
        PlayerPrefs.SetFloat(PlayerPositionKey + "_X", playerPosition.x);
        PlayerPrefs.SetFloat(PlayerPositionKey + "_Y", playerPosition.y);
        PlayerPrefs.SetFloat(PlayerPositionKey + "_Z", playerPosition.z);

        // Save the current scene
        PlayerPrefs.SetString(SceneKey, SceneManager.GetActiveScene().name);

        PlayerPrefs.Save();
        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SceneKey))
        {
            string sceneName = PlayerPrefs.GetString(SceneKey);
            SceneManager.LoadScene(sceneName);
            StartCoroutine(LoadPlayerPosition());
        }
        else
        {
            Debug.Log("No saved game found");
        }
    }

    private IEnumerator LoadPlayerPosition()
    {
        yield return new WaitForEndOfFrame();

        if(PlayerPrefs.HasKey(PlayerPositionKey + "_X"))
        {
            float x = PlayerPrefs.GetFloat(PlayerPositionKey + "_X");
            float y = PlayerPrefs.GetFloat(PlayerPositionKey + "_Y");
            float z = PlayerPrefs.GetFloat(PlayerPositionKey + "_Z");

            mainCharacter.transform.position = new Vector3(x, y, z);
            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.Log("No saved game found");
        }
    }

    private void Update()
    {
        if (isGameActive && Input.GetKeyDown(KeyCode.K)) // for testing purposes
        {
            OnCharacterDeath();
        }

       if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMainMenu();
        }
    }
}
