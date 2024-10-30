using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int currentSceneIndex;
    private WalkThroughDetection portal;
    [SerializeField] private Image img;
    public float fadeTime = 1;
    public float sceneLoadWaitTime = 2;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        FindNewPortal();    
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void FindNewPortal()
    {
        portal = GameObject.FindWithTag("Portal")?.GetComponent<WalkThroughDetection>();

        if (portal != null)
        {
            portal.WentThroughEvent.AddListener(ChangeScene);
        }
        else
        {
            Debug.LogError("Portal object not found!");
        }
    }

    private void ChangeScene()
    {
        // Fade screen to black
        StartCoroutine(FadeImage(false));
        
        currentSceneIndex++;

        StartCoroutine(LoadSceneAndFindPortal(currentSceneIndex));
    }

    private IEnumerator LoadSceneAndFindPortal(int sceneIndex)
    {
        yield return new WaitForSeconds(fadeTime + 2);

        // Load the new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Find portal object in the new scene
        FindNewPortal();

        // Fade back into game
        StartCoroutine(FadeImage(true));
    }

    IEnumerator FadeImage(bool fadeAway)
    {
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = fadeTime; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            // loop over 1 second
            for (float i = 0; i <= fadeTime; i += Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(0, 0, 0, i);
                yield return null;
            }
        }
    }

}
