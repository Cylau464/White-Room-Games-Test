using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallCollector : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private LayerMask _colorBallLayer;
    [SerializeField] private float _attractingSpeed = 5f;
    [SerializeField] private int _ballsLimit = 50;
    [SerializeField] private float _ballsMoveDuration = .1f;

    private float _ballRadius;
    private List<Transform> _attractedBalls;

    private Coroutine _moveBallsCor;

    private void Start()
    {
        _attractedBalls = new List<Transform>();
        _ballRadius = _renderer.bounds.extents.x;
    }

    private void Update()
    {
        foreach (Transform ball in _attractedBalls.ToArray())
        {
            if (Vector3.Distance(transform.position, ball.transform.position) > _ballRadius)
            {
                Vector3 direction = (transform.position - ball.transform.position).normalized;
                ball.position += direction * _attractingSpeed * Time.deltaTime;
            }
            else
            {
                //Vector3 position = transform.position + (ball.transform.position - transform.position).normalized * _ballRadius;
                //ball.position = position;
                _attractedBalls.Remove(ball);

                if (_moveBallsCor != null)
                    StopCoroutine(_moveBallsCor);

                _moveBallsCor = StartCoroutine(MoveBalls());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if((1 << other.gameObject.layer & _colorBallLayer) != 0 && transform.childCount < _ballsLimit)
        {
            ColorBall ball = other.GetComponent<ColorBall>();
            ball.Collect();
            ball.OnLaunch += OnBallLaunch;
            other.transform.SetParent(transform);
            _attractedBalls.Add(other.transform);
        }
    }

    private void OnBallLaunch(Transform ball)
    {
        _attractedBalls.Remove(ball);
    }

    Vector3[] PointsOnSphere(int n)
    {
        List<Vector3> upts = new List<Vector3>(n);
        float inc = Mathf.PI * (3f - Mathf.Sqrt(5f));
        float off = 2.0f / n;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        float r = 0f;
        float phi = 0f;

        for (int k = 0; k < n; k++)
        {
            y = k * off - 1f + (off / 2f);
            r = Mathf.Sqrt(1f - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, y, z).normalized * _ballRadius);
        }

        Vector3[] pts = upts.ToArray();

        return pts;
    }

    private IEnumerator MoveBalls()
    {
        float t = 0f;
        Vector3[] startPositions = new Vector3[transform.childCount];
        Vector3[] targetPositions = new Vector3[transform.childCount];
        List<Vector3> points = PointsOnSphere(transform.childCount).ToList();

        for(int i = 0; i < transform.childCount; i++)
        {
            startPositions[i] = transform.GetChild(i).position;
            points.Remove(startPositions[i]);
        }

        for(int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            targetPositions[i] = Vector3.one * Mathf.Infinity;

            foreach(Vector3 targetPos in points)
            {
                bool targetCloser = Vector3.Distance(child.position, targetPos + transform.position) < Vector3.Distance(child.position, targetPositions[i] + transform.position);

                if (startPositions[i] != targetPos && targetCloser == true)
                    targetPositions[i] = targetPos; 
            }

            points.Remove(targetPositions[i]);
        }

        while (t < 1f)
        {
            t += Time.deltaTime / _ballsMoveDuration;

            if(transform.childCount < startPositions.Length)
            {
                _moveBallsCor = null;
                yield break;
            }

            for(int i = 0; i < startPositions.Length; i++)
            {
                Transform child = transform.GetChild(i);
                child.position = Vector3.Lerp(startPositions[i], targetPositions[i] + transform.position, t);
                
            }

            yield return null;
        }

        _moveBallsCor = null;
    }
}