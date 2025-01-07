using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
    private SiloObjects siloObjects; // Reference to the parent SiloObjects
    public AudioClip clickSound;     // Sound to play on click
    private AudioSource audioSource; // AudioSource to play the sound

    public void Initialize(SiloObjects parent)
    {
        siloObjects = parent;
        if (!TryGetComponent(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnMouseDown()
    {
        if (siloObjects != null)
        {
            Debug.Log($"Object {gameObject.name} clicked. Triggering destruction...");

            PlaySound();
            siloObjects.DestroyObjects();

            SiloManager siloManager = FindObjectOfType<SiloManager>();
            if (siloManager != null)
            {
                siloManager.NotifyObjectDestroyed();
            }
        }
    }

    private void PlaySound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.clip = clickSound;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"AudioClip or AudioSource missing on {gameObject.name}");
        }
    }
}
