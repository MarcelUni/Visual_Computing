using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum SurfaceType
{
    Default,
    Grass,
    Stone,
    Wood,
    Metal,
    Water,
}

public class AudioManager : MonoBehaviour
{
    [Header("Music and Ambience Settings")]
    public Sound[] musicSounds;
    public Sound[] ambienceSounds;

    [Header("SFX Settings")]
    public Sound[] sfxSounds;

    [Header("Footstep Sounds")]
    public Sound[] footstepSounds;
    public float pitchVariation = 0.1f;
    public float volumeVariation = 0.05f;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource footstepSource;

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

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.loop = false; // Footstep sounds are one-shots
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

        Sound sceneMusic = Array.Find(musicSounds, x => x.sceneIndices.Contains(sceneIndex));

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
        Sound[] sceneAmbiences = Array.FindAll(ambienceSounds, x => x.sceneIndices.Contains(sceneIndex));

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
    /// Plays a footstep sound based on the surface type.
    /// Randomly selects among all available sounds for the surface type.
    /// </summary>
    /// <param name="surfaceType">The type of surface the player is walking on.</param>
    public void PlayFootstepSound(SurfaceType surfaceType) // bliver kaldt fra PlayerController scriptet, som giver den en surfaceType.
    {
        // Find all footstep sounds matching the surface type
        Sound[] matchingSounds = Array.FindAll(footstepSounds, x => x.surfaceType == surfaceType); // Finder alle sounds med den samme surface type.

        if (matchingSounds.Length == 0)
        {
            // No matching sounds found for the surface type, try default surface type
            matchingSounds = Array.FindAll(footstepSounds, x => x.surfaceType == SurfaceType.Default); // Fallback til default sound.
        }

        if (matchingSounds.Length == 0)
        {
            // No matching sounds found at all
            Debug.LogWarning($"No footstep sounds found for surface type {surfaceType}");
            return;
        }

        // Play a random sound from the matching sounds
        Sound s = matchingSounds[UnityEngine.Random.Range(0, matchingSounds.Length)]; // Vælg en random sound fra listen.

        // Apply random pitch and volume variations
        float randomizedPitch = s.pitch + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        float randomizedVolume = s.volume + UnityEngine.Random.Range(-volumeVariation, volumeVariation);

        // Play the selected sound with PlayOneShot
        footstepSource.pitch = randomizedPitch; // Set the pitch to ensure variations.
        footstepSource.PlayOneShot(s.clip, randomizedVolume); // Play the clip with the adjusted volume.
    }

    //side note. Vi bruger PlayOneShot metoden, som er en metode der spiller en lyd en enkelt gang. Den tager to argumenter, en lyd og en volumen.
    //Den gør bare sådan at når man spiller en lyd på en audiosource og spiller en til lyd, vil den første lyd ikke blive overlappet.
    // hvor Play() metoden vil overlappe lyden. så hvis man spiller en lyd på en audiosource og spiller en anden lyd, vil den første lyd blive stoppet og overlappet af den anden lyd.


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
