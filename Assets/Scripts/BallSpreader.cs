using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BallSpreader : MonoBehaviour
{
    [SerializeField] private LayerMask _ballLayer;
    [SerializeField] private MeshFilter _target;
    [SerializeField] private Renderer _renderer;

    private Texture2D _texture;
    private Color[] _textureColors;
    private Queue<ColorBall> _spreadQueue;

    private void Start()
    {
        _spreadQueue = new Queue<ColorBall>();
        StartCoroutine(Spread());
        Texture2D texture = _renderer.sharedMaterial.GetTexture("_MainTex") as Texture2D;
        _texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        int mipmapOffset = 8; // for get mipmap with 128x128
        _textureColors = texture.GetPixels(texture.mipmapCount - mipmapOffset);
        _texture.SetPixels(_textureColors);
        _texture.Apply();
    }

    private Vector3 CalculateColorPosition(Color color)
    {
        int index = 0;
        List<int> colorIndexes = new List<int>();

        for(int i = 0; i < _textureColors.Length; i++)
        {
            if(_textureColors[i] == color)
                colorIndexes.Add(i);
        }

        index = colorIndexes[Random.Range(0, colorIndexes.Count)];
        int width = _texture.width;
        Vector2 pixelCenterOffset = new Vector2(0.5f / _texture.width, 0.5f / _texture.height);
        Vector2 uv = new Vector2(index % width, index / width) / width - pixelCenterOffset;

        return FindPosOnMesh(uv);
    }

    private Vector3 FindPosOnMesh(Vector2 uv)
    {
        Mesh mesh = _target.sharedMesh;
        int[] tris = mesh.triangles;
        Vector2[] uvs = mesh.uv;
        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
            Vector2 u2 = uvs[tris[i + 1]];
            Vector2 u3 = uvs[tris[i + 2]];

            // calculate triangle area - if zero, skip it
            float a = Area(u1, u2, u3); if (a == 0) continue;

            // calculate barycentric coordinates of u1, u2 and u3
            // if anyone is negative, point is outside the triangle: skip it
            float a1 = Area(u2, u3, uv) / a; if (a1 < 0) continue;
            float a2 = Area(u3, u1, uv) / a; if (a2 < 0) continue;
            float a3 = Area(u1, u2, uv) / a; if (a3 < 0) continue;

            // point inside the triangle - find mesh position by interpolation...
            Vector3 p3D = a1 * verts[tris[i]] + a2 * verts[tris[i + 1]] + a3 * verts[tris[i + 2]];

            // and return it in world coordinates:
            return _target.transform.TransformPoint(p3D);
        }

        // point outside any uv triangle: return Vector3.zero
        return GetRandomPointAbove();
    }

    private float Area(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 v1 = p1 - p3;
        Vector2 v2 = p2 - p3;

        return (v1.x* v2.y - v1.y* v2.x)/2;
    }

    private Vector3 GetRandomPointAbove()
    {
        Mesh mesh = _target.sharedMesh;
        float minX = mesh.bounds.min.x;
        float minY = mesh.bounds.min.y;
        float maxX = mesh.bounds.max.x;
        float maxY = mesh.bounds.max.y;
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = mesh.bounds.center.z;
        Vector3 pos = new Vector3(x, y, z);
        
        return _target.transform.TransformPoint(pos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if((1 << other.gameObject.layer & _ballLayer) != 0)
        {
            ColorBall ball = other.GetComponent<ColorBall>();

            if (ball.IsLaunch == true || _spreadQueue.Contains(ball) == true) return;

            _spreadQueue.Enqueue(ball);
            ball.Launch(GetRandomPointAbove());
        }
    }

    private IEnumerator Spread()
    {
        int spreadForFrame = 5;

        while(true)
        {
            if(_spreadQueue.Count > 0)
            {
                for(int i = 0; i < spreadForFrame; i++)
                {
                    if (_spreadQueue.Count <= 0) break;

                    ColorBall ball = _spreadQueue.Dequeue();
                    Vector3 targetPos = CalculateColorPosition(ball.MyColor);
                    ball.MoveToPicture(targetPos);
                }
            }

            yield return null;
        }
    }
}