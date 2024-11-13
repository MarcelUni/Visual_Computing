using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class LightOrbBehavior : MonoBehaviour
{
    private PlayerController pc;
    public bool hasLightOrb;

    private bool hasBeenDimmed = false;
    private bool hasBeenIncreased = false;
    private Light orbLight;

    public float orbitSpeed, rotationSpeed, angle;
    public float wavyFreq, amplitude;
    public Vector3 orbitAxis;

    public void InitializeOrb()
    {
        pc = GetComponentInParent<PlayerController>();  
        orbLight = GetComponentInChildren<Light>();
    }

    private void Update()
    {
        if(hasLightOrb == false)
            return;

        if(pc.isSneaking == true && hasBeenDimmed == false)
        {
            DimLight();
        }
        else if (pc.isSneaking == false && hasBeenIncreased == false)
        {
            IncreaseLight();
        }

        CircleAroundPlayer();
    }

    private void DimLight()
    {
        Debug.Log("DIM LIGHT");
        orbLight.intensity = 0.5f;
        hasBeenDimmed = true;
        hasBeenIncreased = false;
    }

    private void IncreaseLight()
    {

        Debug.Log("INCREASE LIGHT");
        orbLight.intensity = 2f;
        hasBeenIncreased = true;
        hasBeenDimmed = false;
    }

    private void CircleAroundPlayer()
    {
        // Orbit in a wavy pattern up and down
        float time = Time.time * wavyFreq;
        
        float wave = Mathf.Cos(time) * amplitude;

        transform.Translate(0, wave * Time.deltaTime, 0);

        // Orbit around the sun
        transform.RotateAround(pc.transform.position, orbitAxis, orbitSpeed * Time.deltaTime);

        // Rotate around itself
        transform.Rotate(0, angle * rotationSpeed * Time.deltaTime, 0);
    }
}
