using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TransitionPrefab", menuName = "Prefabs/Transition Prefab")]
public class TransitionPrefab : ScriptableObject
{
    [System.Serializable]
    public class AnimatedObject
    {
        public GameObject prefab; // Prefab to instantiate
        public Vector3 scale = Vector3.one; // Scale of the object
        public float animationDuration = 2.0f; // Duration of animation
        public Vector3 customRotation = Vector3.zero; // Optional custom rotation in Euler angles
    }

    public List<AnimatedObject> objectsToAnimate; // List of objects to animate
    public int lastObjectIndex; // Index of the last object that remains in the scene
}
