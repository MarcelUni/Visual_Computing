using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    private PlayerController pc;

    public TMP_Text pathChoiceText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pc.isAtPathChoice)
        {
            pathChoiceText.gameObject.SetActive(true);
        }
        else
        {
            pathChoiceText.gameObject.SetActive(false);
        }
    }
}
