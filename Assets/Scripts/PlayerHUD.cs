using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    private PlayerController pc;
    private InputManager im;
    public float textTurnOffDelay;

    public TMP_Text pathChoiceText;
    public TMP_Text chosenPathText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        im = GetComponentInParent<InputManager>();
        pc = GetComponentInParent<PlayerController>();

        im.PathChosenEvent.AddListener(ChosePathUI);
        chosenPathText.gameObject.SetActive(false);
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

    private void ChosePathUI(int index)
    {
        chosenPathText.gameObject.SetActive(true);
        if(index == 1)
        {
            chosenPathText.text = "You went right!";
        }
        else
        {
            chosenPathText.text = "You went left!";
        }
        StartCoroutine(TurnOffAfterDelay());
    }

    private IEnumerator TurnOffAfterDelay()
    {
        yield return new WaitForSeconds(textTurnOffDelay);

        chosenPathText.gameObject.SetActive(false);
    }
}
