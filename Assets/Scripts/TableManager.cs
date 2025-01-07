using System.Collections;

using UnityEngine;

public class TableManager : MonoBehaviour
{
    public GameObject[] tables; // Array of table GameObjects
    public SiloClickHandler siloClickHandler; // Reference to SiloClickHandler
    public AudioClip[] tableClickSounds; // Array of sounds for table clicks

    private AudioSource audioSource;

    private void Start()
    {
        // Initialize the audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void HandleTableClick(int tableIndex)
    {
        // Allow clicking only on the table of the active silo
        if (SiloManager.IsInteractionLocked && SiloManager.ActiveSiloIndex != tableIndex)
        {
            Debug.Log("Interaction is locked. Cannot click on tables of other silos.");
            return;
        }

        if (tableIndex >= 0 && tableIndex < tables.Length)
        {
            PlaySound(tableIndex); // Play the associated sound for the table
            StartCoroutine(PlayDespawnAnimation(tableIndex));
        }
        else
        {
            Debug.LogWarning($"Invalid table index: {tableIndex}");
        }
    }

    private IEnumerator PlayDespawnAnimation(int tableIndex)
    {
        Animator animator = tables[tableIndex].GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("PlayDespawn");

            // Wait for the animation to complete
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
        }

        // Deactivate the table
        tables[tableIndex].SetActive(false);
        Debug.Log($"Table {tableIndex} despawned. Spawning objects...");

        // Notify SiloClickHandler to handle spawning objects
        siloClickHandler.HandleTableClick(tableIndex);
    }

    public void ShowTable(int tableIndex)
    {
        if (tableIndex >= 0 && tableIndex < tables.Length)
        {
            tables[tableIndex].SetActive(true);

            // Play spawn animation
            Animator animator = tables[tableIndex].GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Spawn", 0, 0); // Ensure the animation starts from the beginning
            }

            Debug.Log($"Table spawned: {tableIndex}");
        }
        else
        {
            Debug.LogWarning($"Invalid table index: {tableIndex}");
        }
    }

    private void PlaySound(int index)
    {
        if (audioSource != null && index >= 0 && index < tableClickSounds.Length)
        {
            audioSource.clip = tableClickSounds[index];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"No sound available for table index {index} or audio source missing.");
        }
    }
}
