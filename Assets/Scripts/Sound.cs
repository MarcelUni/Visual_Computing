using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(.1f, 3f)]
    public float pitch = 1f;
    public bool loop;
    public int sceneIndex = -1; // Only used for ambience to link it to a specific scene
}
