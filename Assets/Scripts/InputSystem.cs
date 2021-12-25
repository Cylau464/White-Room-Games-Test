using UnityEngine;
using System;

public class InputSystem : MonoBehaviour
{
    private static Plane _plane;

    public static InputSystem Instance { get; private set; }

    public Action OnTouch;
    public Action OnRelease;
    public Action OnDrag;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _plane = new Plane(Vector3.up, 0);
    }

    private void Update()
    {
#if UNITY_ANROID || UNITY_IOS
        if(Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
                OnTouch?.Invoke();
            else if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
                OnRelease?.Invoke();
            else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                OnDrag?.Invoke();
        }
#else
        if (Input.GetMouseButtonDown(0) == true)
            OnTouch?.Invoke();
        else if (Input.GetMouseButtonUp(0) == true)
            OnRelease?.Invoke();
        else if (Input.GetMouseButton(0) == true)
            OnDrag?.Invoke();
#endif
    }

    public static Vector3 GetMouseWorldPosition()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_plane.Raycast(ray, out distance))
            return ray.GetPoint(distance);
        else
            return Vector3.zero;
    }
}
