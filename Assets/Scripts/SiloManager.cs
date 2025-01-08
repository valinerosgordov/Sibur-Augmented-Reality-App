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

    public Button nextStepButton; // Button to appear after the transition prefab
    public Button exitButton; // Exit button to appear after final prefab animation
    public Button restartButton; // Restart button to appear after final prefab animation

    private static bool isInteractionLocked = false; // Lock to prevent multiple interactions at the same time
    private static int activeSiloIndex = -1; // Currently active silo index
    private int silosClicked = 0; // Number of silos clicked
    private int totalDestroyedObjects = 0; // Tracks destroyed objects
    private bool[] isSiloCompleted; // Tracks completion status of each silo
    private AudioSource audioSource; // Audio source for playing sounds

    // Expose the interaction lock and active silo index
    public static bool IsInteractionLocked => isInteractionLocked;
    public static int ActiveSiloIndex => activeSiloIndex;

    private void Start()
    {
        // Ensure all buttons are hidden initially
        if (nextStepButton != null)
            nextStepButton.gameObject.SetActive(false);

        if (exitButton != null)
            exitButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        // Initialize silo completion tracking
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
                SpawnTransitionPrefab(); // Spawn transition prefab after all silos are clicked
            }
        }

        UnlockInteraction();
    }

    private void SpawnTransitionPrefab()
    {
        if (transitionPrefab != null && transitionPrefabSpawnPoint != null)
        {
            // Add a 2-second delay before instantiating the transition prefab
            StartCoroutine(DelayedSpawnTransitionPrefab());
        }
    }

    private IEnumerator DelayedSpawnTransitionPrefab()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Instantiate the transition prefab
        GameObject instantiatedPrefab = Instantiate(transitionPrefab, transitionPrefabSpawnPoint.position, Quaternion.identity);
        Animator animator = instantiatedPrefab.GetComponent<Animator>();

        if (animator != null)
        {
            Debug.Log("Triggering animation for transition prefab.");
            animator.SetTrigger("PlayAnimation");

            // Wait for animation to complete before showing the "Next" button
            StartCoroutine(WaitForTransitionAnimationToFinish(animator, instantiatedPrefab));
        }
        else
        {
            Debug.LogWarning("Animator not found on the transition prefab.");
            ShowNextStepButton(); // If no animator, show the "Next" button immediately
        }
    }

    private IEnumerator WaitForTransitionAnimationToFinish(Animator animator, GameObject prefab)
    {
        // Wait for the animation to finish
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Destroy the prefab after animation
        Debug.Log("Transition prefab animation complete. Destroying prefab.");
        Destroy(prefab);

        ShowNextStepButton();
    }

    private void ShowNextStepButton()
    {
        if (nextStepButton != null)
        {
            nextStepButton.gameObject.SetActive(true);
            nextStepButton.onClick.AddListener(HandleNextStepButtonClick);
        }
    }

    private void HandleNextStepButtonClick()
    {
        // Hide the next step button
        if (nextStepButton != null)
        {
            nextStepButton.gameObject.SetActive(false);
            nextStepButton.onClick.RemoveListener(HandleNextStepButtonClick);
        }

        // Instantiate the final prefab
        if (FinalPrefab != null && FinalPrefabSpawnPoint != null)
        {
            GameObject instantiatedPrefab = Instantiate(FinalPrefab, FinalPrefabSpawnPoint.position, Quaternion.identity);
            Animator animator = instantiatedPrefab.GetComponent<Animator>();

            if (animator != null)
            {
                Debug.Log("Triggering animation for final prefab.");
                animator.SetTrigger("PlayAnimation");

                // Wait for animation to complete without affecting buttons
                StartCoroutine(WaitForFinalPrefabAnimationToFinish(animator, instantiatedPrefab));
            }
            else
            {
                Debug.LogWarning("Animator not found on the final prefab.");
            }
        }

        // Show the exit and restart buttons immediately
        if (exitButton != null)
            exitButton.gameObject.SetActive(true);

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        Debug.Log("Exit and Restart buttons are now visible.");
    }

    private IEnumerator WaitForFinalPrefabAnimationToFinish(Animator animator, GameObject prefab)
    {
        // Wait for the animation to finish
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        // Destroy the prefab
        Debug.Log("Final prefab animation complete. Destroying prefab.");
        Destroy(prefab);
    }

    public void NotifyObjectDestroyed()
    {
        totalDestroyedObjects++;
        Debug.Log($"Object destroyed. Total destroyed: {totalDestroyedObjects}");

        if (totalDestroyedObjects >= frontParts.Length)
        {
            SpawnTransitionPrefab(); // Trigger transition after all objects are destroyed
        }
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

    public static void UnlockInteraction()
    {
        isInteractionLocked = false;
        activeSiloIndex = -1;
    }
}
