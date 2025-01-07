using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "SiloObject", menuName = "Silo/SiloObject")]
public class SiloObjects : ScriptableObject
{
    [System.Serializable]
    public class SpawnableObject
    {
        public GameObject prefab;       // The object prefab
        public Vector3 initialRotation; // Initial rotation of the object
        public Vector3 scale = Vector3.one; // Scale of the object
    }

    public List<SpawnableObject> objectsToSpawn;  // List of objects to spawn
    public GameObject centerObjectPrefab;         // Center object prefab (optional)
    public Vector3 centerObjectInitialRotation = Vector3.zero;
    public int spawnAmount;
    public float spawnRadius = 10f;
    public float rotationSpeed = 30f;
    public float flyDuration = 2f; // Duration for objects to fly to their positions
    public float spawnDelay = 0.5f; // Delay between spawning objects
    public AudioClip objectClickSound; // Global sound for spawned object clicks

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private GameObject centerObject;

    public void SpawnObjects(Vector3 spawnPosition, Vector3 siloFrontPartPosition, Transform parent = null)
    {
        // Clear previously spawned objects
        DestroyObjects();

        if (spawnAmount <= 0 || objectsToSpawn.Count == 0)
        {
            Debug.LogWarning("No objects to spawn or spawn amount is zero.");
            return;
        }

        // Spawn the center object directly at the spawn position
        if (centerObjectPrefab != null)
        {
            centerObject = Instantiate(centerObjectPrefab, spawnPosition, Quaternion.identity, parent);
            centerObject.transform.localRotation = Quaternion.Euler(centerObjectInitialRotation);
            Debug.Log("Center object spawned at: " + spawnPosition);
        }

        // Start the coroutine to spawn objects one by one
        MonoBehaviour mb = centerObject != null ? centerObject.GetComponent<MonoBehaviour>() : null;
        if (mb != null)
        {
            mb.StartCoroutine(SpawnObjectsSequentially(spawnPosition, siloFrontPartPosition));
        }
    }

    private IEnumerator SpawnObjectsSequentially(Vector3 spawnPosition, Vector3 siloFrontPartPosition)
    {
        float adjustedRadius = spawnRadius + (spawnAmount * 0.5f); // Adjust radius dynamically
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnableObject spawnable = objectsToSpawn[i % objectsToSpawn.Count];
            Vector3 offset = GetXYCircularOffset(i, spawnAmount, adjustedRadius); // Final position in the circle
            Vector3 finalPosition = spawnPosition + offset;

            // Spawn object initially at the silo front part position
            GameObject obj = Instantiate(spawnable.prefab, siloFrontPartPosition, Quaternion.identity, centerObject?.transform);

            // Apply initial rotation and scale
            obj.transform.localScale = spawnable.scale;
            obj.transform.rotation = Quaternion.Euler(spawnable.initialRotation);

            // Attach the click handler
            AddClickHandler(obj);

            // Animate the object flying to its final position
            MonoBehaviour mb = obj.GetComponent<MonoBehaviour>();
            if (mb != null)
            {
                mb.StartCoroutine(FlyToPosition(obj, finalPosition));
            }

            // Store the spawned object for rotation
            spawnedObjects.Add(obj);

            Debug.Log($"Spawned object {i + 1}/{spawnAmount} at {obj.transform.position}");

            // Wait before spawning the next object
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void AddClickHandler(GameObject obj)
    {
        // Add a collider if it doesn't already have one
        if (!obj.TryGetComponent<Collider>(out _))
        {
            obj.AddComponent<BoxCollider>();
        }

        // Add the click handler script
        ObjectClickHandler clickHandler = obj.AddComponent<ObjectClickHandler>();
        clickHandler.Initialize(this); // Pass a reference to this ScriptableObject
        clickHandler.clickSound = objectClickSound; // Assign the global click sound
    }

    private Vector3 GetXYCircularOffset(int index, int totalObjects, float radius)
    {
        float angleStep = 360f / totalObjects; // Uniform angle step
        float angle = Mathf.Deg2Rad * angleStep * index;

        // XY circular offset: Objects arranged in a circle on the XY plane
        return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
    }

    public void RotateObjects()
    {
        if (centerObject == null)
        {
            Debug.LogWarning("Center object is null. Cannot rotate objects.");
            return;
        }

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Quaternion initialRotation = obj.transform.rotation;
                obj.transform.RotateAround(centerObject.transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
                obj.transform.rotation = initialRotation;
            }
        }
    }

    public void DestroyObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                MonoBehaviour mb = obj.GetComponent<MonoBehaviour>();
                if (mb != null)
                {
                    mb.StartCoroutine(ShrinkObject(obj));
                }
            }
        }

        spawnedObjects.Clear();

        if (centerObject != null)
        {
            MonoBehaviour mb = centerObject.GetComponent<MonoBehaviour>();
            if (mb != null)
            {
                mb.StartCoroutine(ShrinkObject(centerObject));
            }

            centerObject = null;
        }

        SiloManager.UnlockInteraction();
    }

    private IEnumerator FlyToPosition(GameObject obj, Vector3 targetPosition)
    {
        Vector3 startPosition = obj.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < flyDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flyDuration;

            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        obj.transform.position = targetPosition;
    }

    private IEnumerator ShrinkObject(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        float shrinkDuration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;

            obj.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        obj.transform.localScale = Vector3.zero;
        Destroy(obj);
    }
}
