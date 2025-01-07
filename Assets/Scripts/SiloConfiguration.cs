using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "SiloConfiguration", menuName = "Silo/Configuration")]
public class SiloConfiguration : ScriptableObject
{
    public GameObject centerObjectPrefab;
    public Vector3 centerObjectInitialRotation = Vector3.zero;
    public int spawnAmount;
    public float spawnRadius;
    public List<SiloObjects> objectsToSpawn;
}
