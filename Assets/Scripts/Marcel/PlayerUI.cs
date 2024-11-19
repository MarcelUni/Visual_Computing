using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public RawImage keyImage, forward, backward, forwardSneak, backwardSneak, noInput, interact;
    public InputManager im;
    public PlayerInteract pi;
    private RawImage[] handSigns;
    public float alphaAtInactive, alphaAtActive, fadeDuration;
    private string inputString;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        handSigns = new RawImage[]{ forward, backward, forwardSneak, backwardSneak, noInput, interact };

        keyImage.CrossFadeAlpha(alphaAtInactive, fadeDuration, true);
    }

    void Update()
    {
        // Turn off signs at start of frame
        for(int i = 0; i < handSigns.Length; i++)
        {
            handSigns[i].CrossFadeAlpha(alphaAtInactive, fadeDuration, true);
        }

        switch (im.inputPerformedString)
        {
            case "Forward":
                forward.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            case "Backward":
                backward.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            case "ForwardSneak":
                forwardSneak.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            case "BackwardSneak":
                backwardSneak.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            case "Interact":
                interact.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            case "Stop":
                noInput.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
                break;
            default:
                break;
        }

        if(pi.hasKey)
        {
            keyImage.CrossFadeAlpha(alphaAtActive, fadeDuration, true);
        }
    }

    public void FindNewObjects()
    {
        pi = FindAnyObjectByType<PlayerInteract>();
        im = FindAnyObjectByType<InputManager>();
        Debug.Log("haha");
    }
}
