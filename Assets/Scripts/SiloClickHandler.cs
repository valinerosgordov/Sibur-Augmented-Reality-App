using UnityEngine;
using System.Collections.Generic;

public class SiloClickHandler : MonoBehaviour
{
    public SiloManager siloManager;   // Reference to the SiloManager script
    public TableManager tableManager; // Reference to the TableManager script
    public List<SiloObjects> siloObjects; // List of SiloObjects (ScriptableObjects)
    public float spawnDistance = 10f; // Adjustable distance for spawning objects in front of the camera

    private SiloObjects activeSiloObject; // Currently active SiloObject

    private void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleTouch(touch.position);
            }
        }

        // Add support for mouse input in Editor
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
#endif
    }

    private void HandleTouch(Vector2 screenPosition)
    {
        // Perform a raycast from the touch or mouse position
        Transform hitTransform = RaycastHelper.GetHitTransform(screenPosition, ~0);

        if (hitTransform != null)
        {
            // Determine if the hit object is a silo or a table
            int siloIndex = GetSiloIndexFromHit(hitTransform);
            if (siloIndex != -1)
            {
                HandleSiloClick(siloIndex);
                return;
            }

            int tableIndex = GetTableIndexFromHit(hitTransform);
            if (tableIndex != -1)
            {
                HandleTableClick(tableIndex);
            }
        }
    }

    private void HandleSiloClick(int siloIndex)
    {
        // Check if interactions are locked or the silo is already active
        if (SiloManager.IsInteractionLocked)
        {
            Debug.Log("Interaction is locked. Cannot click on another silo.");
            return;
        }

        // Handle the silo click through the SiloManager
        siloManager.HandleSiloClick(siloIndex);
    }

    private int GetSiloIndexFromHit(Transform hitTransform)
    {
        for (int i = 0; i < siloManager.frontParts.Length; i++)
        {
            if (hitTransform == siloManager.frontParts[i].transform)
            {
                return i;
            }
        }
        return -1; // Not a silo
    }

    private int GetTableIndexFromHit(Transform hitTransform)
    {
        for (int i = 0; i < tableManager.tables.Length; i++)
        {
            if (hitTransform == tableManager.tables[i].transform)
            {
                return i;
            }
        }
        return -1; // Not a table
    }

    public void HandleTableClick(int tableIndex)
    {
        // Allow clicking only on the table of the active silo
        if (SiloManager.IsInteractionLocked && SiloManager.ActiveSiloIndex != tableIndex)
        {
            Debug.Log("Interaction is locked. Cannot click on tables of other silos.");
            return;
        }

        if (tableIndex >= 0 && tableIndex < siloObjects.Count)
        {
            // Play the table disappearance animation
            tableManager.HideTable(tableIndex);

            // Get the silo front part position
            Vector3 siloFrontPartPosition = siloManager.frontParts[tableIndex].transform.position;

            Debug.Log($"Spawning objects for silo index: {tableIndex} at distance {spawnDistance}, flying from {siloFrontPartPosition}");

            // Spawn objects using the selected SiloObject
            activeSiloObject = siloObjects[tableIndex];
            activeSiloObject.SpawnObjects(spawnDistance, siloFrontPartPosition); // Correctly passing a float as the first parameter
        }
        else
        {
            Debug.LogWarning($"Invalid table index: {tableIndex}");
        }
    }


    private void FixedUpdate()
    {
        // Rotate objects if there's an active SiloObject
        if (activeSiloObject != null)
        {
            activeSiloObject.RotateObjects();
        }
    }
}
