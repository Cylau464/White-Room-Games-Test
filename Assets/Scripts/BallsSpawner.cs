using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BallsSpawner : MonoBehaviour
{
    [Header("Texture Properties")]
    [SerializeField] private Renderer _pictureRenderer;
    [SerializeField] private int _minNumberOfColorPixels = 10000;
    private Dictionary<Color, int> _ballsColors;
    private Dictionary<Color, int> _middleColumnsColors;

    [Header("Spawn Properties")]
    [SerializeField] private ColorBall _ballPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _ballsCount = 100;
    [SerializeField] private int _spiralsCount = 3;
    [SerializeField] private int _spiralColumns = 4;
    [SerializeField] private float _minSpiralRadius = .25f;
    [SerializeField] private float _maxSpiralRadius = .4f;
    [SerializeField] private float _spiralShift = .1f;
    [SerializeField] private float _distanceBtwSpirals = 5f;

    private void Start()
    {
        GetBallsColors();
        Spawn();
    }

    private Color[] GetTextureColors()
    {
        Texture2D texture = _pictureRenderer.sharedMaterial.GetTexture("_MainTex") as Texture2D;
        Texture2D newTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        int mipmapOffset = 9; // for get mipmap with 256x256
        Color[] colors = texture.GetPixels(texture.mipmapCount - mipmapOffset);
        newTexture.SetPixels(colors);
        newTexture.Apply();

        return newTexture.GetPixels();
    }

    private Dictionary<Color, int> GetCommonColors(Color[] textureColors, out int pixelsCount)
    {
        pixelsCount = 0;
        Dictionary<Color, int> colorsCount = new Dictionary<Color, int>();

        foreach (Color color in textureColors)
        {
            if (colorsCount.ContainsKey(color) == true)
                colorsCount[color]++;
            else
                colorsCount.Add(color, 1);
        }

        Dictionary<Color, int> colors = new Dictionary<Color, int>();

        foreach (KeyValuePair<Color, int> kvp in colorsCount)
        {
            if (kvp.Value >= textureColors.Length / 10)
            {
                colors.Add(kvp.Key, kvp.Value);
                pixelsCount += kvp.Value;
            }
        }

        return colors.OrderBy(x => x.Value).Take(colors.Count).ToDictionary(x => x.Key, x => x.Value);
    }

    private void GetBallsColors()
    {
        Color[] textureColors = GetTextureColors();
        Dictionary<Color, int> commonColors = GetCommonColors(textureColors, out int pixelsCount);

        int middleColumns = 2;
        _middleColumnsColors = new Dictionary<Color, int>(middleColumns);
        _ballsColors = new Dictionary<Color, int>();
        int ballsCount = _ballsCount * _spiralsCount;
        int count = 0;
        float coeff = 0f;
        float remainingСoeff = 0f;
        int notMiddleColorsCount = commonColors.Count - middleColumns;

        foreach (KeyValuePair<Color, int> kvp in commonColors)
        {
            if (_middleColumnsColors.Count < middleColumns)
            {
                coeff = (float)kvp.Value / pixelsCount;
                count = _ballsCount / (_spiralColumns + 1);
                remainingСoeff += coeff - (float)count / ballsCount;
                _middleColumnsColors.Add(kvp.Key, count);
            }
            else
            {
                coeff = (float)kvp.Value / pixelsCount + (remainingСoeff / notMiddleColorsCount);
                count = Mathf.CeilToInt(coeff * ballsCount);
                _ballsColors.Add(kvp.Key, count);
            }
        }
    }

    private void Spawn()
    {
        GameObject ballHolder = new GameObject("Ball Holder");
        float ballSize = _ballPrefab.GetComponent<Renderer>().bounds.size.x;
        Vector3 spawnPos = _spawnPoint.position;
        Vector3 spawnOffset = Vector3.zero;
        spawnPos.x -= _distanceBtwSpirals;
        float shift = 0f;
        float angle;
        float radius;
        Color ballColor = _ballsColors.Keys.FirstOrDefault();

        for (int i = 0; i < _spiralsCount; i++)
        {
            float spiralColumns;

            if (i == 1)
            {
                spiralColumns = _spiralColumns;
                radius = _minSpiralRadius;
            }
            else
            {
                spiralColumns = _spiralColumns + 1;
                radius = _maxSpiralRadius;
            }

            for (int j = 0; j < _ballsCount;)
            {
                for (int k = 0; k < spiralColumns && j < _ballsCount; k++, j++)
                {
                    if (k < _spiralColumns)
                    {
                        angle = (k * Mathf.PI * 2f / _spiralColumns) + shift;
                        spawnOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                        if (_ballsColors.ContainsKey(ballColor) == false || _ballsColors[ballColor] <= 0)
                            ballColor = _ballsColors.Keys.FirstOrDefault(x => _ballsColors[x] > 0);

                        _ballsColors[ballColor]--;
                    }
                    else
                    {
                        spawnOffset = Vector3.zero;

                        if (_middleColumnsColors.ContainsKey(ballColor) == false || _middleColumnsColors[ballColor] <= 0)
                            ballColor = _middleColumnsColors.Keys.FirstOrDefault(x => _middleColumnsColors[x] > 0);

                        _middleColumnsColors[ballColor]--;
                    }

                    ColorBall ball = Instantiate(_ballPrefab, spawnPos + spawnOffset, Quaternion.identity, ballHolder.transform);
                    ball.Init(ballColor);
                }

                shift += _spiralShift;
                spawnPos.y += ballSize;
            }

            spawnPos = new Vector3(spawnPos.x + _distanceBtwSpirals, _spawnPoint.position.y, spawnPos.z);
        }
    }
}