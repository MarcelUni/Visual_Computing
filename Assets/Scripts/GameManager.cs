using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int currentSceneIndex;
    private WalkThroughDetection portal;
    private Image fadeToBlackImage;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        fadeToBlackImage = GetComponentInChildren<Image>();
        FindNewPortal();    
        currentSceneIndex = 0;
    }

    private void FindNewPortal()
    {
        portal = GameObject.FindWithTag("Portal").GetComponent<WalkThroughDetection>();
        portal.WentThroughEvent.AddListener(ChangeScene);
    }

    private void ChangeScene()
    {
        // Fade screen to black
        fadeToBlackImage.gameObject.SetActive(true);
        
        currentSceneIndex++;
        // Change scene
        SceneManager.LoadScene(currentSceneIndex);

        // Find portal object
        FindNewPortal();

        // Fade back into game
        fadeToBlackImage.gameObject.SetActive(false);
    }

}
