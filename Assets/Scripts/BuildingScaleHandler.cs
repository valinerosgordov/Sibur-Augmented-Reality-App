
using UnityEngine;

public class BuildingScaleHandler : MonoBehaviour
{
    public float scaleSpeed = 0.01f; // Speed of scaling
    public float minScale = 0.5f;   // Minimum scale limit
    public float maxScale = 3.0f;   // Maximum scale limit

    private Vector2 previousTouchDistance; // Previous frame's touch distance
    private Vector2 currentTouchDistance;  // Current frame's touch distance
    private bool isScaling = false;        // Flag to track scaling

    private void Update()
    {
        if (Input.touchCount == 2) // Detect two touches
        {
            // Get the positions of the two touches
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Calculate the distance between touches
            currentTouchDistance = touch1.position - touch2.position;

            if (!isScaling) // Initial setup
            {
                previousTouchDistance = currentTouchDistance;
                isScaling = true;
            }

            // Calculate the scale factor based on the change in distance
            float scaleFactor = (currentTouchDistance.magnitude - previousTouchDistance.magnitude) * scaleSpeed;

            // Apply scaling while maintaining constraints
            Vector3 newScale = transform.localScale + Vector3.one * scaleFactor;
            newScale = ClampScale(newScale);

            transform.localScale = newScale;

            // Update the previous touch distance
            previousTouchDistance = currentTouchDistance;
        }
        else
        {
            isScaling = false; // Reset when not touching
        }
    }

    // Clamp the scale to stay within the min and max bounds
    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = Mathf.Clamp(scale.z, minScale, maxScale);
        return scale;
    }
}
