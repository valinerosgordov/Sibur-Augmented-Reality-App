using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SiloManager : MonoBehaviour
{
    public GameObject[] frontParts; // Silo front parts
    public GameObject[] tables; // Silo tables
    public AudioClip[] frontPartClickSounds; // Sounds for silo front clicks
    public AudioClip[] tableClickSounds; // Sounds for table clicks
    public TransitionPrefab transitionPrefabConfig; // ScriptableObject for transition objects

    public GameObject finalPrefab; // Final prefab to spawn
    public float finalPrefabDistance = 5.0f; // Spawn point for the final prefab
    public GameObject extraFinalPrefab; // Additional prefab to spawn after the final prefab
    public float extraFinalPrefabDistance = 7.0f; // Spawn point for the extra prefab

    public Button nextStepButton; // Button for proceeding after transition
    public Button exitButton; // Exit button after final stage
    public Button restartButton; // Restart button after final stage

    [SerializeField] private float transitionSpeed = 0.5f; // Speed for prefab movement
    [SerializeField] private float cameraDistance = 5.0f; // Distance from the camera for objects
    [SerializeField] private float delayBetweenObjects = 0.5f; // Delay between object animations
    [SerializeField] private float transitionPrefabDistance = 2.0f; // Distance from the camera for table alignment


    private static bool isInteractionLocked = false; // Lock to prevent multiple interactions
    private static int activeSiloIndex = -1; // Active silo index
    private GameObject lastObjectInstance; // Reference to the last transition object
    private int silosClicked = 0; // Count of completed silos
    private int totalDestroyedObjects = 0; // Tracks destroyed objects
    private bool[] isSiloCompleted; // Tracks completion of silos
    private AudioSource audioSource;

    public static bool IsInteractionLocked => isInteractionLocked;
    public static int ActiveSiloIndex => activeSiloIndex;

    private void Start()
    {
        nextStepButton?.gameObject.SetActive(false);
        exitButton?.gameObject.SetActive(false);
        restartButton?.gameObject.SetActive(false);

        isSiloCompleted = new bool[frontParts.Length];
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void HandleSiloClick(int siloIndex)
    {
        if (isInteractionLocked || siloIndex < 0 || siloIndex >= frontParts.Length) return;

        if (isSiloCompleted[siloIndex])
        {
            Debug.Log($"Silo {siloIndex} has already been completed.");
            return;
        }

        isInteractionLocked = true;
        activeSiloIndex = siloIndex;

        PlaySound(frontPartClickSounds, siloIndex);
        HideSiloFront(siloIndex);
        ShowTable(siloIndex);
    }

    private void HideSiloFront(int index)
    {
        if (index >= 0 && index < frontParts.Length && frontParts[index] != null)
        {
            frontParts[index].SetActive(false);
            Debug.Log($"Silo front hidden: {index}");
        }
    }

    private void ShowTable(int index)
    {
        if (index >= 0 && index < tables.Length && tables[index] != null)
        {
            PlaySound(tableClickSounds, index);
            tables[index].SetActive(true);
            AlignTableWithCamera(tables[index]);
            Debug.Log($"Table shown: {index}");
        }
    }

    private void AlignTableWithCamera(GameObject table)
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward.normalized;
        table.transform.position = cameraPosition + forward * transitionPrefabDistance;
        table.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        Debug.Log("Table aligned with camera.");
    }

    public void MarkSiloAsCompleted(int siloIndex)
    {
        if (!isSiloCompleted[siloIndex])
        {
            isSiloCompleted[siloIndex] = true;
            silosClicked++;

            Debug.Log($"Silo {siloIndex} completed. Total completed: {silosClicked}/{frontParts.Length}");

            if (silosClicked >= frontParts.Length)
            {
                StartCoroutine(SpawnAndAnimateObjects());
            }
        }

        UnlockInteraction();
    }

    private IEnumerator SpawnAndAnimateObjects()
    {
        for (int i = 0; i < transitionPrefabConfig.objectsToAnimate.Count; i++)
        {
            var objConfig = transitionPrefabConfig.objectsToAnimate[i];

            // Calculate dynamic positions based on the camera view
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found!");
                yield break;
            }

            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 cameraRight = mainCamera.transform.right.normalized;
            Vector3 cameraForward = mainCamera.transform.forward.normalized;

            // Off-screen spawn position (to the left of the camera view)
            Vector3 offScreenPosition = cameraPosition + cameraForward * cameraDistance - cameraRight * cameraDistance;

            // On-screen target position (center of the camera view)
            Vector3 targetPosition = cameraPosition + cameraForward * cameraDistance;

            // Exit position (to the right of the camera view)
            Vector3 exitPosition = cameraPosition + cameraForward * cameraDistance + cameraRight * cameraDistance;

            // Instantiate the object
            Quaternion spawnRotation = objConfig.customRotation != Vector3.zero
                ? Quaternion.Euler(objConfig.customRotation)
                : Quaternion.identity;

            GameObject instance = Instantiate(objConfig.prefab, offScreenPosition, spawnRotation);
            instance.transform.localScale = objConfig.scale;

            // Animate the object from off-screen to on-screen
            yield return StartCoroutine(AnimateObject(instance, targetPosition, objConfig.animationDuration));

            // Handle the last object differently
            if (i == transitionPrefabConfig.lastObjectIndex)
            {
                lastObjectInstance = instance;
                Debug.Log("Last object remains on scene.");
            }
            else if (i == transitionPrefabConfig.lastObjectIndex - 1)
            {
                // Handle second-to-last object: ensure it has a valid exit path
                Debug.Log("Second-to-last object exiting scene.");
                yield return StartCoroutine(AnimateObject(instance, exitPosition, objConfig.animationDuration));
                Destroy(instance);
            }
            else
            {
                // Handle regular objects
                yield return StartCoroutine(AnimateObject(instance, exitPosition, objConfig.animationDuration));
                Destroy(instance);
            }

            // Delay before spawning the next object
            yield return new WaitForSeconds(delayBetweenObjects);
        }

        // Show the "Next" button after all animations are completed
        nextStepButton?.gameObject.SetActive(true);
        nextStepButton?.onClick.AddListener(HandleNextStepButtonClick);
    }



    private IEnumerator AnimateObject(GameObject obj, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = obj.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Move the object without altering its scale or rotation
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }
    }

    private void HandleNextStepButtonClick()
    {
        nextStepButton?.gameObject.SetActive(false);
        nextStepButton?.onClick.RemoveListener(HandleNextStepButtonClick);

        // Destroy the last object when the "Next" button is clicked
        if (lastObjectInstance != null)
        {
            Destroy(lastObjectInstance);
            Debug.Log("Last object destroyed.");
        }

        StartCoroutine(HandleFinalStage());
    }

    private IEnumerator HandleFinalStage()
    {
        GameObject finalPrefabInstance = null;
        GameObject extraFinalPrefabInstance = null;

        // Spawn the final prefab in front of the camera
        if (finalPrefab != null)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * finalPrefabDistance;
            Quaternion spawnRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);

            finalPrefabInstance = Instantiate(finalPrefab, spawnPosition, spawnRotation);
            finalPrefabInstance.transform.localScale = finalPrefab.transform.localScale;

            Vector3 targetPosition = spawnPosition; // No additional movement
            yield return StartCoroutine(MoveObject(finalPrefabInstance, targetPosition, transitionSpeed));
        }

        yield return new WaitForSeconds(1f);

        // Spawn the extra final prefab in front of the camera
        if (extraFinalPrefab != null)
        {
            Vector3 extraSpawnPosition = Camera.main.transform.position + Camera.main.transform.forward * extraFinalPrefabDistance;
            Quaternion extraSpawnRotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);

            extraFinalPrefabInstance = Instantiate(extraFinalPrefab, extraSpawnPosition, extraSpawnRotation);
            extraFinalPrefabInstance.transform.localScale = extraFinalPrefab.transform.localScale;

            Vector3 extraTargetPosition = extraSpawnPosition; // No additional movement
            yield return StartCoroutine(MoveObject(extraFinalPrefabInstance, extraTargetPosition, transitionSpeed));
        }

        yield return new WaitForSeconds(1f);

        // Cleanup final prefab only
        if (finalPrefabInstance != null) Destroy(finalPrefabInstance);

        // Show exit and restart buttons
        exitButton?.gameObject.SetActive(true);
        restartButton?.gameObject.SetActive(true);
    }


    private IEnumerator MoveObject(GameObject obj, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = obj.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Move the object
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }
    }

    public static void UnlockInteraction()
    {
        isInteractionLocked = false;
        activeSiloIndex = -1;
    }

    private void PlaySound(AudioClip[] sounds, int index)
    {
        if (audioSource != null && index >= 0 && index < sounds.Length && sounds[index] != null)
        {
            audioSource.clip = sounds[index];
            audioSource.Play();
        }
    }

    public void NotifyObjectDestroyed()
    {
        totalDestroyedObjects++;
        if (totalDestroyedObjects >= frontParts.Length)
        {
            StartCoroutine(SpawnAndAnimateObjects());
        }
    }
}
