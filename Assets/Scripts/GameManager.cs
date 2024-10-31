using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int currentSceneIndex;
    private WalkThroughDetection portal;
    private PlayerController player;
    [SerializeField] private Image img;
    public float fadeTime = 1;
    public float sceneLoadWaitTime = 2;

    public static GameManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        FindNewObjects();    
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void FindNewObjects()
    {
        portal = GameObject.FindWithTag("Portal")?.GetComponent<WalkThroughDetection>();
        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();

        if (portal != null)
        {
            portal.WentThroughEvent.AddListener(ChangeScene);
        }
        else
        {
            Debug.Log("No portal object");
        }

        if(player != null)
        {
            player.deathEvent.AddListener(ReloadScene);
        }
        else
        {
            Debug.Log("No player object");
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
        FindNewObjects();

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

    private void ReloadScene()
    {
        StartCoroutine(FadeImage(false));

        StartCoroutine(LoadSceneAndFindPortal(currentSceneIndex));
    }

}
