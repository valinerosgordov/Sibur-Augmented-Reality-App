using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SiloManager : MonoBehaviour
{
    public GameObject[] frontParts; // Silo front parts
    public GameObject[] tables; // Silo tables
    public AudioClip[] frontPartClickSounds; // Sounds for silo front clicks
    public AudioClip[] tableClickSounds; // Sounds for table clicks
    public GameObject transitionPrefab; // Prefab to spawn after all silos are clicked and objects disappear
    public Transform transitionPrefabSpawnPoint; // Spawn point for the transition prefab
    public GameObject FinalPrefab; // Final prefab to instantiate after the transition prefab
    public Transform FinalPrefabSpawnPoint; // Spawn point for the final prefab
    public GameObject extraFinalPrefab; // Additional prefab to spawn after the final prefab
    public Transform extraFinalPrefabSpawnPoint; // Spawn point for the additional prefab

    public Button nextStepButton; // Button to appear after the transition prefab
    public Button exitButton; // Exit button to appear after final prefab animation
    public Button restartButton; // Restart button to appear after final prefab animation

    private static bool isInteractionLocked = false; // Lock to prevent multiple interactions at the same time
    private static int activeSiloIndex = -1; // Currently active silo index
    private int silosClicked = 0; // Number of silos clicked
    private int totalDestroyedObjects = 0; // Tracks destroyed objects
    private bool[] isSiloCompleted; // Tracks completion status of each silo
    private AudioSource audioSource; // Audio source for playing sounds

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
            Debug.Log($"Table shown: {index}");
        }
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
                SpawnTransitionPrefab();
            }
        }

        UnlockInteraction();
    }

    private void SpawnTransitionPrefab()
    {
        if (transitionPrefab != null && transitionPrefabSpawnPoint != null)
        {
            StartCoroutine(DelayedSpawnTransitionPrefab());
        }
    }

    private IEnumerator DelayedSpawnTransitionPrefab()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds before spawning

        GameObject instantiatedPrefab = Instantiate(transitionPrefab, transitionPrefabSpawnPoint.position, Quaternion.identity);
        Animator animator = instantiatedPrefab.GetComponent<Animator>();

        if (animator != null)
        {
            Debug.Log("Triggering animation for transition prefab.");
            animator.SetTrigger("PlayAnimation");
        }

        AddClickHandlerToTransitionPrefab(instantiatedPrefab);

        // Wait for 8 seconds before showing the "Next Step" button
        yield return new WaitForSeconds(8f);
        ShowNextStepButton();
    }

    private void AddClickHandlerToTransitionPrefab(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out Collider collider))
        {
            // Add a collider to detect clicks
            collider = prefab.AddComponent<BoxCollider>();
        }

        prefab.AddComponent<TransitionPrefabClickHandler>();
    }

    public void NotifyObjectDestroyed()
    {
        totalDestroyedObjects++;
        Debug.Log($"Object destroyed. Total destroyed: {totalDestroyedObjects}");

        if (totalDestroyedObjects >= frontParts.Length)
        {
            Debug.Log("All objects destroyed. Spawning transition prefab...");
            SpawnTransitionPrefab();
        }
    }

    public void ShowNextStepButton()
    {
        if (nextStepButton != null)
        {
            nextStepButton.gameObject.SetActive(true);
            nextStepButton.onClick.AddListener(HandleNextStepButtonClick);
        }
    }

    private void HandleNextStepButtonClick()
    {
        nextStepButton?.gameObject.SetActive(false);
        nextStepButton?.onClick.RemoveListener(HandleNextStepButtonClick);

        StartCoroutine(HandleFinalStage());
    }

    private IEnumerator HandleFinalStage()
    {
        GameObject finalPrefabInstance = null;
        GameObject extraPrefabInstance = null;

        // Spawn the final prefab
        if (FinalPrefab != null && FinalPrefabSpawnPoint != null)
        {
            finalPrefabInstance = Instantiate(FinalPrefab, FinalPrefabSpawnPoint.position, Quaternion.identity);
            Animator finalAnimator = finalPrefabInstance.GetComponent<Animator>();

            if (finalAnimator != null)
            {
                Debug.Log("Triggering animation for FinalPrefab.");
                finalAnimator.SetTrigger("PlayAnimation");
            }
        }

        // Wait for 2 seconds before spawning the extra final prefab
        yield return new WaitForSeconds(2f);

        // Spawn the extra final prefab
        if (extraFinalPrefab != null && extraFinalPrefabSpawnPoint != null)
        {
            extraPrefabInstance = Instantiate(extraFinalPrefab, extraFinalPrefabSpawnPoint.position, Quaternion.identity);
            Animator extraAnimator = extraPrefabInstance.GetComponent<Animator>();

            if (extraAnimator != null)
            {
                Debug.Log("Triggering animation for extraFinalPrefab.");
                extraAnimator.SetTrigger("PlayAnimation");
            }
        }

        // Handle FinalPrefab animation and destruction
        if (finalPrefabInstance != null)
        {
            Animator finalAnimator = finalPrefabInstance.GetComponent<Animator>();
            if (finalAnimator != null)
            {
                while (finalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(3f); // Wait an additional 3 seconds
            Destroy(finalPrefabInstance);
            Debug.Log("FinalPrefab destroyed after 3 seconds.");
        }

        // Show exit and restart buttons
        exitButton?.gameObject.SetActive(true);
        restartButton?.gameObject.SetActive(true);
        Debug.Log("Exit and Restart buttons are now visible.");
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
        else
        {
            Debug.LogWarning($"No valid sound for index {index} or audio source is missing.");
        }
    }
}
