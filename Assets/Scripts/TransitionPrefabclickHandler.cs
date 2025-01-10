using UnityEngine;

public class TransitionPrefabClickHandler : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("Transition prefab clicked. Destroying prefab...");
        Destroy(gameObject);

        // Notify the SiloManager or perform the next step
        SiloManager manager = FindObjectOfType<SiloManager>();
        if (manager != null)
        {
            manager.ShowNextStepButton();
        }
    }
}
