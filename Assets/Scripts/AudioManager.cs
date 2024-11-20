using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Music and Ambience Settings")]
    public Sound[] musicSounds;
    public Sound[] ambienceSounds;

    [Header("SFX Settings")]
    public Sound[] sfxSounds;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private List<AudioSource> activeAmbienceSources = new List<AudioSource>();

    public static AudioManager instance;

    private int currentSceneIndex = -1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure audio sources are assigned
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }

        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Ensures the AudioSource components are assigned correctly.
    /// </summary>
    private void InitializeAudioSources()
    {
        // Check if the AudioSource components are already assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false; // SFX are usually one-shots
        }
    }

    /// <summary>
    /// Called when a new scene is loaded, used to update ambience and music.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update the current scene index
        currentSceneIndex = scene.buildIndex;

        // Play the appropriate ambience for this scene
        PlaySceneAmbience(currentSceneIndex);

        // Play music for this scene
        PlayMusic(currentSceneIndex);
    }

    /// <summary>
    /// Plays the music track for the given scene index.
    /// </summary>
    /// <param name="sceneIndex">Index of the current scene.</param>
    public void PlayMusic(int sceneIndex)
    {
        musicSource.Stop();

        Sound sceneMusic = Array.Find(musicSounds, x => x.sceneIndex == sceneIndex);

        if (sceneMusic != null)
        {
            musicSource.clip = sceneMusic.clip;
            musicSource.loop = sceneMusic.loop;
            musicSource.volume = sceneMusic.volume;
            musicSource.pitch = sceneMusic.pitch;
            musicSource.Play();
        }
        else
        {
            Debug.Log("No music track found for scene index: " + sceneIndex);
        }
    }

    /// <summary>
    /// Plays the ambience tracks for the given scene index.
    /// Spawns an AudioSource for each ambience sound assigned to the scene.
    /// </summary>
    /// <param name="sceneIndex">Index of the current scene</param>
    public void PlaySceneAmbience(int sceneIndex)
    {
        // Stop and clear any existing ambience sources
        ClearAmbienceSources();

        // Find all ambience sounds for the scene
        Sound[] sceneAmbiences = Array.FindAll(ambienceSounds, x => x.sceneIndex == sceneIndex);

        if (sceneAmbiences.Length > 0)
        {
            foreach (Sound ambience in sceneAmbiences)
            {
                // Create a new AudioSource for each ambience sound
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = ambience.clip;
                newSource.loop = ambience.loop;
                newSource.volume = ambience.volume;
                newSource.pitch = ambience.pitch;
                newSource.playOnAwake = false;

                // Play the ambience sound
                newSource.Play();

                // Add the new AudioSource to the list of active sources
                activeAmbienceSources.Add(newSource);
            }
        }
        else
        {
            Debug.Log("No ambience tracks found for scene index: " + sceneIndex);
        }
    }

    /// <summary>
    /// Stops and destroys all active ambience AudioSources.
    /// </summary>
    private void ClearAmbienceSources()
    {
        foreach (AudioSource source in activeAmbienceSources)
        {
            source.Stop();
            Destroy(source);
        }
        activeAmbienceSources.Clear();
    }

    /// <summary>
    /// Plays the sound effect with the given name.
    /// </summary>
    /// <param name="name">The name of the SFX to play.</param>
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning("SFX: " + name + " not found!");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip, s.volume);
        }
    }

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Mutes or unmutes the music.
    /// </summary>
    public void ToggleMusicMute()
    {
        musicSource.mute = !musicSource.mute;
    }

    /// <summary>
    /// Mutes or unmutes the SFX.
    /// </summary>
    public void ToggleSFXMute()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    /// <summary>
    /// Resets the ambience and music when the player dies or the scene is reloaded.
    /// </summary>
    public void ResetAudioOnDeath()
    {
        PlaySceneAmbience(currentSceneIndex);
        PlayMusic(currentSceneIndex);
    }
}
