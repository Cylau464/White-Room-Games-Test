using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private Rigidbody _rigidBody;

    private void Start()
    {
        InputSystem.Instance.OnDrag += Move;
    }

    private void Move()
    {
        Vector3 direction = (InputSystem.GetMouseWorldPosition() - transform.position).normalized;
        direction.y = 0f;
        
        _rigidBody.AddForce(direction * _moveSpeed * Time.deltaTime, ForceMode.Acceleration);
    }

    private void OnDestroy()
    {
        InputSystem.Instance.OnDrag -= Move;
    }
}
