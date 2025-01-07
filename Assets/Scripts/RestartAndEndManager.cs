using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartAndEndManager : MonoBehaviour
{
    // Function to restart the project (reload the current scene)
    public void RestartProject()
    {
        Debug.Log("Restarting project...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Function to end the project (quit the application)
    public void EndProject()
    {
        Debug.Log("Ending project...");
        Application.Quit();

        // For testing in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
