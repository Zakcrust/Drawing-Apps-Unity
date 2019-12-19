using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painter : MonoBehaviour
{
    public Material backgroundMaterial;

    public const int TEXTURE_WIDTH = 256;
    public const int TEXTURE_HEIGHT = 256;


    [Tooltip("Target bidang gambar")]
    public MeshRenderer targetRender;

    Texture2D targetTexture = null;


    public Material tmpDrawMaterial;

    [Tooltip("Target bidang yang sedang digunakan")]

    public MeshRenderer tempTargetRender;

    Texture2D temporaryTexture = null;
   
    // Camera utama
    Camera cam = default;

    Vector3 lastPixelPosition;
    Vector3 startDownPos;
    void Start()
    {
        // Mendapatkan camera utama
        cam = Camera.main;

        SetDefaultTexture();
        SetDefaultTemporaryTexture();
    }

    void SetDefaultTexture()
    {
        // Target texture yang akan digambar
        Texture2D targetTexture = null;

        // Setup texture yang akan digambar
        targetTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        targetTexture.filterMode = FilterMode.Point;
        targetTexture.wrapMode = TextureWrapMode.Clamp;

        // Beri texture secara default berwarna putih
        Color[] cols = targetTexture.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = Color.white;
        }

        // Set pengaturan texture
        targetTexture.SetPixels(cols);
        targetTexture.Apply();

        // Buat material gambar tidak terpengaruh oleh cahaya
        targetRender.material = new Material(backgroundMaterial);
        targetRender.material.mainTexture = targetTexture;

        this.targetTexture = (Texture2D)targetRender.material.mainTexture;
    }


    void Update()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        Texture2D tex = rend.material.mainTexture as Texture2D;

        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;


        /* if (Input.GetMouseButton(0))
        {
            DrawDot(ref tex, (int)pixelUV.x, (int)pixelUV.y);
        }

        lastPixelPosition = hit.textureCoord;
        tex.Apply(); */

       /*  if (Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            DrawBresenhamLine(ref tex, (int)mousePosition.x, (int)mousePosition.y, (int)pixelUV.x, (int)pixelUV.y);
        }

        lastPixelPosition = hit.textureCoord;
        tex.Apply(); */
         if (Input.GetMouseButtonDown(0))
         {
             startDownPos = pixelUV;
         }


         if (Input.GetMouseButton(0))
        {
            
            Debug.Log(startDownPos);
            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Line:

                    //=========== Aktifkan target render temporary
                    tempTargetRender.gameObject.SetActive(true);

                    if (hit.transform == tempTargetRender.transform)
                    {
                        ClearColor(ref tex);
                        DrawBresenhamLine(ref tex, (int)startDownPos.x, (int)startDownPos.y, (int)pixelUV.x, (int)pixelUV.y);
                    }
                    break;
            }
        }

        //=========== Copy dari temporary ke tekstur akhir dan bersihkan warna di temporary
        if (Input.GetMouseButtonUp(0))
        {
            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Line:

                    ApplyTemporaryTex(ref temporaryTexture, ref targetTexture);
                    targetTexture.Apply();
                    ClearColor(ref temporaryTexture);
                    break;
            }
        }

        lastPixelPosition = hit.textureCoord;
        tex.Apply();
    }

    void DrawDot(ref Texture2D targetTex, int x, int y)
    {
        targetTex.SetPixel(x, y, Color.black);
    }

    void DrawBresenhamLine(ref Texture2D targetTex, int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;
        for (; ; )
        {
            targetTex.SetPixel(x0, y0, Color.black);
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }

    public enum DrawingMode
    {
        Line
    }
    public DrawingMode CurrentDrawingMode = DrawingMode.Line;

    void ClearColor(ref Texture2D targetTex)
    {
        Color transparentColor = new Color(1, 1, 1, 0);
        Color[] cols = targetTex.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = transparentColor;
        }
        targetTex.SetPixels(cols);
    }

    void SetDefaultTemporaryTexture()
    {
        // Target texture yang akan digambar
        Texture2D targetTexture = null;

        // Setup texture yang akan digambar
        targetTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        targetTexture.filterMode = FilterMode.Point;
        targetTexture.wrapMode = TextureWrapMode.Clamp;

        // Beri texture secara default berwarna transparent
        Color transparentColor = new Color(1, 1, 1, 0);
        Color[] cols = targetTexture.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = transparentColor;
        }

        // Set pengaturan texture
        targetTexture.SetPixels(cols);
        targetTexture.Apply();

        // Buat material gambar tidak terpengaruh oleh cahaya
        tempTargetRender.material = new Material(this.tmpDrawMaterial);
        tempTargetRender.material.mainTexture = targetTexture;

        this.temporaryTexture = (Texture2D)tempTargetRender.material.mainTexture;
    }

    void ApplyTemporaryTex(ref Texture2D originTex, ref Texture2D targetTex)
    {
        Color[] originColors = originTex.GetPixels();
        Color[] targetColors = targetTex.GetPixels();
        for (int i = 0; i < targetColors.Length; ++i)
        {
            targetColors[i] = targetColors[i] * originColors[i];
        }
        targetTex.SetPixels(targetColors);
    }
}
