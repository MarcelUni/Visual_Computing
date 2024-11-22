using UnityEngine;

public class FootStepHelperMethod : MonoBehaviour
{
    public PlayerController playerController;

        public void PlayFootStepSound()
    {
        playerController.PlayFootstep();
    }
}
