using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // UI Panels
    public GameObject mainMenuUI;
    public GameObject gameUI;
    public GameObject deathUI;
    public GameObject settingsUI;

    // Main Character reference for dead check
    public GameObject mainCharacter;

    // Game state tracking
    private bool isGameActive = false;

    // Player Save Data Keys
    private const string PlayerPositionKey = "PlayerPosition";
    private const string SceneKey = "Scene";

    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        gameUI.SetActive(false);
        deathUI.SetActive(false);
        isGameActive = false;
        Time.timeScale = 0f; // Pause the game
    }

    public void StartGame(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    // should be called when pressing "Play" from the mainmenuUI
    public void ResumeGame()
    {
        mainMenuUI.SetActive(false);
        gameUI.SetActive(true);
        deathUI.SetActive(false);
        settingsUI.SetActive(false);
        isGameActive = true;
        Time.timeScale = 1f; // Resume the game
    }

    public void Settings()
    {
        settingsUI.SetActive(true);
        mainMenuUI.SetActive(false);
        deathUI.SetActive(false);
    }

    public void OnCharacterDeath()
    {
        if (isGameActive)
        {
            ShowDeathScreen();
        }
    }

    public void ShowDeathScreen()
    {
        // play a sound effect
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnApplicationQuit()
    {
        SaveGame();
        SceneManager.LoadScene("MainMenu");
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
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("No saved game found");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(LoadPlayerPosition());
    }

    private IEnumerator LoadPlayerPosition()
    {
        yield return new WaitUntil(() => mainCharacter != null);

        if (PlayerPrefs.HasKey(PlayerPositionKey + "_X"))
        {
            float x = PlayerPrefs.GetFloat(PlayerPositionKey + "_X");
            float y = PlayerPrefs.GetFloat(PlayerPositionKey + "_Y");
            float z = PlayerPrefs.GetFloat(PlayerPositionKey + "_Z");

            mainCharacter.transform.position = new Vector3(x, y, z);
            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.Log("No saved player position found");
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

    public void SetVolume()
    {
        Slider volumeSlider = GameObject.Find("VolumeSlider").GetComponent<Slider>();
        AudioListener.volume = volumeSlider.value;
    }
}