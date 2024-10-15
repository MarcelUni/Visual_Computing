using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightOrbBehavior : MonoBehaviour
{
    private PlayerController pc;
    public bool hasLightOrb;

    private bool hasBeenDimmed = false;
    private bool hasBeenIncreased = false;

    public void InitializeOrb()
    {
        pc = GetComponentInParent<PlayerController>();  
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
    }

    private void DimLight()
    {
        Debug.Log("DIM LIGHT");
        hasBeenDimmed = true;
        hasBeenIncreased = false;
    }

    private void IncreaseLight()
    {
        Debug.Log("INCREASE LIGHT");
        hasBeenIncreased = true;
        hasBeenDimmed = false;
    }
}
