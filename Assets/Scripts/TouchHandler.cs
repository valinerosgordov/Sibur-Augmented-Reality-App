using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    public delegate void OnTouchDetected(Transform hitTransform);
    public static event OnTouchDetected TouchDetected;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    TouchDetected?.Invoke(hit.transform);
                }
            }
        }
    }
}
