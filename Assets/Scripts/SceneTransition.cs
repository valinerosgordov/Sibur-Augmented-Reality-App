
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image fadeImage; // The fade image
    public float fadeSpeed = 1f; // Speed of the fade
    private bool isFading = false;

    // Start is called before the first frame update
    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
        }
    }

    // Call this method to trigger the scene transition
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(FadeAndSwitchScene(sceneName));
    }

    // Coroutine for fade in and fade out
    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        // Fade in
        yield return StartCoroutine(Fade(1));

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // Wait a frame for the scene to load
        yield return null;

        // Fade out
        yield return StartCoroutine(Fade(0));
    }

    // Handles the fading effect (alpha from 0 to 1 or vice versa)
    private IEnumerator Fade(float targetAlpha)
    {
        isFading = true;
        float currentAlpha = fadeImage.color.a;
        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime * fadeSpeed;
            float alpha = Mathf.Lerp(currentAlpha, targetAlpha, time);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
        isFading = false;
    }
}
