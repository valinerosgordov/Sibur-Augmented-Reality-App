using UnityEngine;

public static class RaycastHelper
{
    public static Transform GetHitTransform(Vector2 screenPosition, LayerMask layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return hit.transform;
        }
        return null;
    }
}
