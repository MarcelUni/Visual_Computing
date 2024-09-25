using System.Collections;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator UpCloseShot; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriggerCloseAnim"))
        {
            UpCloseShot.SetTrigger("CloseZoomTrigger");
            UpCloseShot.Play("TestZoom");
            Debug.Log("Playing UpCloseShot Animation");
        }
        if (other.CompareTag("TriggerWideAnim"))
        {
            UpCloseShot.SetTrigger("WideShotTrigger");
            UpCloseShot.Play("TestWideShot");
            Debug.Log("Playing UpCloseShot Animation");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TriggerCloseAnim"))
        {
            UpCloseShot.SetTrigger("CloseToNormal");
            UpCloseShot.Play("CloseToNormal");
            Debug.Log("Playing CloseToNormal Animation");
        }
        if (other.CompareTag("TriggerWideAnim"))
        {
            UpCloseShot.SetTrigger("WideToNormal");
            UpCloseShot.Play("TestNormalShot");
            Debug.Log("Playing TestNormalShot Animation");
        }
    }

    

   

}
