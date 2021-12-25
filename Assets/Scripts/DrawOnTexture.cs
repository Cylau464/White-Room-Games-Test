using UnityEngine;

public class DrawOnTexture : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private int _minBrushWidth = 100;
    [SerializeField] private int _maxBrushWidth = 200;
    [SerializeField] private int _minBrushHeight = 100;
    [SerializeField] private int _maxBrushHeight = 200;
    [Space]
    [SerializeField] private RenderTexture _rt;
    [SerializeField] private Texture _brushTexture;
    [SerializeField] private Texture _blankTexture;

    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
        DrawBlank();
    }

    // Initialization RenderTexture
    private void DrawBlank()
    {
        //  Activate _rt
        RenderTexture.active = _rt;
        //  Save current state 
        GL.PushMatrix();
        //  Set up the matrix 
        GL.LoadPixelMatrix(0, _rt.width, _rt.height, 0);

        //  Draw maps 
        Rect rect = new Rect(0, 0, _rt.width, _rt.height);
        Graphics.DrawTexture(rect, _blankTexture);

        //  Pop up changes 
        GL.PopMatrix();

        RenderTexture.active = null;
    }

    // Stay RenderTexture Of (x,y) Draw brush patterns at coordinates 
    private void Draw(int x, int y)
    {
        //  Activate rt
        RenderTexture.active = _rt;
        //  Save current state 
        GL.PushMatrix();
        //  Set up the matrix 
        GL.LoadPixelMatrix(0, _rt.width, _rt.height, 0);

        int width = Random.Range(_minBrushWidth, _maxBrushWidth);
        int height = Random.Range(_minBrushHeight, _maxBrushHeight);
        //  Draw maps 
        x -= (int)(width * 0.5f);
        y -= (int)(height * 0.5f);
        Rect rect = new Rect(x, y, width, height);
        Graphics.DrawTexture(rect, _brushTexture);

        //  Pop up changes 
        GL.PopMatrix();

        RenderTexture.active = null;
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButton(0))
    //    {
    //        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
    //        RaycastDraw(ray);
    //    }
    //}

    private void RaycastDraw(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _targetLayer))
        {
            var x = (int)(hit.textureCoord.x * _rt.width);
            var y = (int)(_rt.height - hit.textureCoord.y * _rt.height);
            Draw(x, y);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Ray ray = new Ray(_camera.transform.position, (collision.GetContact(0).point - _camera.transform.position).normalized);
        RaycastDraw(ray);
        collision.gameObject.GetComponent<ColorBall>().Splash();
    }

    private void OnDestroy()
    {
        _rt.Release();
    }
}