using UnityEngine;
using System.Collections;

public class TransitionPrefabHandler : MonoBehaviour
{
    public TransitionPrefab transitionPrefabConfig; // Reference to the scriptable object
    public Transform startSpawnPoint; // Starting point of the objects
    public Transform endSpawnPoint; // End point (outside the screen)
    public Transform tableSpawnPoint; // Position for the table

    private GameObject finalTable; // Reference to the final table

    private void Start()
    {
        if (transitionPrefabConfig != null)
        {
            StartCoroutine(HandleTransition());
        }
        else
        {
            Debug.LogError("TransitionPrefabConfig is not assigned!");
        }
    }

    private IEnumerator HandleTransition()
    {
        for (int i = 0; i < transitionPrefabConfig.objectsToAnimate.Count; i++)
        {
            var animatedObject = transitionPrefabConfig.objectsToAnimate[i];

            // Instantiate the object
            GameObject instance = Instantiate(animatedObject.prefab, startSpawnPoint.position, Quaternion.identity);
            instance.transform.localScale = animatedObject.scale;

            // Animate the object
            StartCoroutine(AnimateObject(instance, endSpawnPoint.position, animatedObject.animationDuration, i == transitionPrefabConfig.lastObjectIndex));

            // Wait a short time before instantiating the next object
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AnimateObject(GameObject obj, Vector3 endPosition, float duration, bool isFinalTable)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = obj.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Move the object from start to end position
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // If it's the final table, preserve it and place it at the table spawn point
        if (isFinalTable)
        {
            finalTable = obj;
            finalTable.transform.position = tableSpawnPoint.position;
            Debug.Log("Final table preserved.");
        }
        else
        {
            // Destroy the object once it's off-screen
            Destroy(obj);
        }
    }
}
