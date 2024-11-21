using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop = false;

    // Modified to support multiple scene indices
    public List<int> sceneIndices = new List<int>();

    // Field for surface type
    public SurfaceType surfaceType;
}
