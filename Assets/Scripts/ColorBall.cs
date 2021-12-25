using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class ColorBall : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private Rigidbody _rigidBody;

    public bool IsLaunch { get; private set; }

    public Color MyColor { get; private set; }

    private MaterialPropertyBlock _propertyBlock;

    private const string CollectedLayer = "Collected Ball";

    public Action<Transform> OnLaunch;

    public void Init(Color color)
    {
        MyColor = color;
        _propertyBlock = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_BaseColor", color);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void Launch(Vector3 targetPos)
    {
        IsLaunch = true;
        OnLaunch?.Invoke(transform);
        transform.parent = null;
        _collider.isTrigger = false;
        _rigidBody.isKinematic = false;
        _rigidBody.useGravity = false;
        _rigidBody.drag = 0f;
        _rigidBody.AddForce(new Vector3(0f, 1f, .4f) * 5f, ForceMode.Impulse);
    }

    public void MoveToPicture(Vector3 targetPos)
    {
        _rigidBody.velocity = Vector3.zero;
        StartCoroutine(MoveToTarget(targetPos));
    }

    private IEnumerator MoveToTarget(Vector3 targetPos)
    {
        float t = 0f;
        Vector3 startPos = transform.position;

        while(t < 1f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    public void Collect()
    {
        _rigidBody.isKinematic = true;
        _collider.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer(CollectedLayer);
    }

    public void Splash()
    {
        Destroy(gameObject);
    }
}