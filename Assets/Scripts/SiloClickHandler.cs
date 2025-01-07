using UnityEngine;
using System.Collections.Generic;

public class SiloClickHandler : MonoBehaviour
{
    public SiloManager siloManager;   // Reference to the SiloManager script
    public TableManager tableManager; // Reference to the TableManager script
    public List<SiloObjects> siloObjects; // List of SiloObjects (ScriptableObjects)
    public float spawnDistance = 10f;

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
    }

    private void HandleTouch(Vector2 screenPosition)
    {
        // Perform a raycast from the touch position
        Transform hitTransform = RaycastHelper.GetHitTransform(screenPosition, ~0);

        if (hitTransform != null)
        {
            // Determine if the hit object is a silo or a table
            int siloIndex = GetSiloIndexFromHit(hitTransform);
            if (siloIndex != -1)
            {
                Debug.Log($"Silo clicked: Index {siloIndex}");
                siloManager.HandleSiloClick(siloIndex);
                return;
            }

            int tableIndex = GetTableIndexFromHit(hitTransform);
            if (tableIndex != -1)
            {
                Debug.Log($"Table clicked: Index {tableIndex}");
                tableManager.HandleTableClick(tableIndex);
            }
        }
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

    public void HandleTableClick(int siloIndex)
    {
        if (siloIndex < 0 || siloIndex >= siloObjects.Count)
        {
            Debug.LogWarning($"Invalid silo index: {siloIndex}");
            return;
        }

        // Calculate spawn position in front of the camera
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 spawnPosition = cameraPosition + cameraForward.normalized * spawnDistance;

        // Get the silo front part position
        Vector3 siloFrontPartPosition = siloManager.frontParts[siloIndex].transform.position;

        Debug.Log($"Spawning objects for silo index: {siloIndex} at position {spawnPosition}, flying from {siloFrontPartPosition}");

        // Spawn objects using the selected SiloObject
        activeSiloObject = siloObjects[siloIndex];
        activeSiloObject.SpawnObjects(spawnPosition, siloFrontPartPosition);
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
