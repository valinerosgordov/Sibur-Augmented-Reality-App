using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ViewTransitionHandler : MonoBehaviour
{
    public Image fadeImage;        // The UI image used for the fade effect (ensure this covers the screen)
    public float fadeDuration = 3f; // Duration of the fade effect

    public Camera arCamera;        // Camera used for AR
    public Camera normalCamera;    // Camera used for normal view

    public Button cancelARButton;  // The button to cancel AR view (visible in AR mode)
    public Button backToARButton;  // The button to switch to AR view (visible in normal mode)

    private bool isARViewActive = false; // To track whether AR view is active

    private void Start()
    {
        // Initialize the fade image to be transparent
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        // Initially, show the correct button based on the current view
        UpdateButtonVisibility();
    }

    // Called when the "Switch View" button is clicked
    public void SwitchView()
    {
        if (isARViewActive)
        {
            // Switch to normal view
            StartCoroutine(FadeOutToNormalView());
        }
        else
        {
            // Switch to AR view
            StartCoroutine(FadeOutToARView());
        }

        isARViewActive = !isARViewActive;  // Toggle the AR view state
        UpdateButtonVisibility();           // Update button visibility
    }

    // Switch to normal view with fade transition
    private IEnumerator FadeOutToNormalView()
    {
        // Fade out the current view to black
        yield return StartCoroutine(FadeToBlack());

        // Switch camera views
        if (arCamera != null) arCamera.gameObject.SetActive(false);
        if (normalCamera != null) normalCamera.gameObject.SetActive(true);

        // Fade in the normal view from black
        yield return StartCoroutine(FadeFromBlack());
    }

    // Switch to AR view with fade transition
    private IEnumerator FadeOutToARView()
    {
        // Fade out the current view to black
        yield return StartCoroutine(FadeToBlack());

        // Switch camera views
        if (normalCamera != null) normalCamera.gameObject.SetActive(false);
        if (arCamera != null) arCamera.gameObject.SetActive(true);

        // Fade in the AR view from black
        yield return StartCoroutine(FadeFromBlack());
    }

    // Fade the screen to black over time
    private IEnumerator FadeToBlack()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color currentColor = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(currentColor.a, 1f, elapsedTime / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
    }

    // Fade the screen from black over time
    private IEnumerator FadeFromBlack()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color currentColor = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(currentColor.a, 0f, elapsedTime / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
    }

    // Update button visibility based on current AR view state
    private void UpdateButtonVisibility()
    {
        if (isARViewActive)
        {
            cancelARButton.gameObject.SetActive(true);  // Show the cancel button in AR mode
            backToARButton.gameObject.SetActive(false);  // Hide the back button in AR mode
        }
        else
        {
            cancelARButton.gameObject.SetActive(false);  // Hide the cancel button in normal mode
            backToARButton.gameObject.SetActive(true);   // Show the back button in normal mode
        }
    }

    // Called when the "Cancel AR View" button is clicked
    public void CancelARView()
    {
        // Switch to normal view
        StartCoroutine(FadeOutToNormalView());
        isARViewActive = false;
        UpdateButtonVisibility(); // Update button visibility
    }

    // Called when the "Back to AR" button is clicked
    public void BackToAR()
    {
        // Switch to AR view
        StartCoroutine(FadeOutToARView());
        isARViewActive = true;
        UpdateButtonVisibility(); // Update button visibility
    }
}
