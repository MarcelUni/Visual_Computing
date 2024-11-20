using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoneBreakSequence", menuName = "Traps/Stone Break Sequence")]
public class StoneBreakSequence : ScriptableObject
{
    public float breakDuration = 1.5f; // Time for the stone to visually "break apart"
    public float fallDuration = 2.0f; // Time for the stone pieces to fall
    public float rotationSpeed = 50.0f; // Rotation speed of the pieces
    public float fallSpeed = 5.0f; // Vertical falling speed

    public void StartBreakingSequence(GameObject stoneObject, MonoBehaviour runner)
    {
        if (stoneObject != null)
        {
            runner.StartCoroutine(BreakStone(stoneObject));
        }
    }

    private IEnumerator BreakStone(GameObject stoneObject)
    {
        // Break the stone into pieces
        for (int i = 0; i < stoneObject.transform.childCount; i++)
        {
            Transform piece = stoneObject.transform.GetChild(i);

            // Detach the piece from the main object
            piece.SetParent(null);

            // Add random rotation to simulate breaking
            Vector3 randomRotation = new Vector3(
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed),
                Random.Range(-rotationSpeed, rotationSpeed)
            );

            // Simulate breaking apart (rotation and spreading)
            Vector3 randomSpread = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0f, 1f),
                Random.Range(-1f, 1f)
            ) * 0.5f;

            float elapsed = 0f;

            while (elapsed < breakDuration)
            {
                piece.Rotate(randomRotation * Time.deltaTime);
                piece.position += randomSpread * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // After breaking apart, simulate falling
        float fallElapsed = 0f;

        while (fallElapsed < fallDuration)
        {
            foreach (Transform piece in stoneObject.transform)
            {
                if (piece != null)
                {
                    piece.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime); // Continue rotation
                    piece.position += Vector3.down * fallSpeed * Time.deltaTime; // Move downward
                }
            }

            fallElapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy pieces after falling
        foreach (Transform piece in stoneObject.transform)
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }
    }
}
