using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Painter : MonoBehaviour
{
    public Material backgroundMaterial;

    public const int TEXTURE_WIDTH = 512;
    public const int TEXTURE_HEIGHT = 512;


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
    Vector3 lastMouseUpPos;

    // Triangle variables
    Vector3 startTrianglePos;
    int lineCount = 0;
    
    public class ShapeModel
    {
        public DrawingMode Mode;
        public List<Vector2> Vertices = new List<Vector2>();
    }

    public List<ShapeModel> ShapeModels = new List<ShapeModel>();

    public ShapeModel currentDrawnShape;

    public ScanLineFill scanLineFill;
    public class Edge
    {
        public int x1, y1, x2, y2;
        public Edge(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    }

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
        // membatalkan proses penggambaran saat proses panning/zooming
        if (Input.touchCount >= 2)
        {
            // membersihkan tampilan temporary
            ClearColor(ref this.temporaryTexture);
            this.temporaryTexture.Apply();

            // reset garis poligon yang sedang dibuat
            this.lineCount = 0;

            // reset bentuk yang sedang digambar
            this.currentDrawnShape = null;
            return;
        }


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

            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Line:
                    currentDrawnShape = new ShapeModel();
                    currentDrawnShape.Mode = DrawingMode.Line;
                    break;

               // ========= Fungsional membuat shape segitiga    ======//
                case DrawingMode.Triangle:
                    // posisi awal menekan mouse
                    if (lineCount == 0)
                    {
                        currentDrawnShape = new ShapeModel();
                        currentDrawnShape.Mode = DrawingMode.Triangle;

                        startTrianglePos = startDownPos;

                        // tambahkan data titik awal
                        currentDrawnShape.Vertices.Add(pixelUV);
                    }
                    break;

                case DrawingMode.Rectangle:
                    currentDrawnShape = new ShapeModel();
                    currentDrawnShape.Mode = DrawingMode.Rectangle;

                    // data titik awal gambar segi empat
                    currentDrawnShape.Vertices.Add(new Vector2(startDownPos.x, startDownPos.y));
                    currentDrawnShape.Vertices.Add(new Vector2(pixelUV.x, startDownPos.y));
                    currentDrawnShape.Vertices.Add(new Vector2(pixelUV.x, pixelUV.y));
                    currentDrawnShape.Vertices.Add(new Vector2(startDownPos.x, pixelUV.y));
                    break;

            }
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

                case DrawingMode.Triangle:
                    tempTargetRender.gameObject.SetActive(true);

                    if (lineCount == 0 && (startDownPos.x != pixelUV.x && startDownPos.y != pixelUV.y))
                    {
                        lastMouseUpPos = startDownPos;
                        lineCount = 1;
                        // tambahkan data titik baru
                        currentDrawnShape.Vertices.Add(pixelUV);
                    }

                    if (currentDrawnShape.Vertices.Count > 0)
                        currentDrawnShape.Vertices[currentDrawnShape.Vertices.Count - 1] = pixelUV;

                    break;
                case DrawingMode.Rectangle:
                    tempTargetRender.gameObject.SetActive(true);

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
             case DrawingMode.Triangle:

                    // menghitung jumlah garis yang sudah digambar
                    // indeks titik 0, 1, dan 2
                    if (lineCount < 2)
                    {

                        if (lineCount == 0 && (startDownPos.x != pixelUV.x && startDownPos.y != pixelUV.y))
                        {
                            lineCount = 1;
                        }
                        else
                        {
                            lineCount++;

                            // tambahkan data titik baru
                            currentDrawnShape.Vertices.Add(pixelUV);
                        }

                    }
                    else
                    {

                        // tambahkan data gambar
                        ShapeModels.Add(currentDrawnShape);

                        // reset indeks garis
                        lineCount = 0;

                        // reset data yang sedang digambar
                        currentDrawnShape = null;

                        ClearColor(ref temporaryTexture);
                        RenderShapes(ref targetTexture);
                    }

                    break;

                case DrawingMode.Rectangle:

                    currentDrawnShape.Vertices[0] = new Vector2(startDownPos.x, startDownPos.y);
                    currentDrawnShape.Vertices[1] = new Vector2(pixelUV.x, startDownPos.y);
                    currentDrawnShape.Vertices[2] = new Vector2(pixelUV.x, pixelUV.y);
                    currentDrawnShape.Vertices[3] = new Vector2(startDownPos.x, pixelUV.y);

                    // tambahkan data gambar persegi
                    ShapeModels.Add(currentDrawnShape);

                    // reset data yang sedang digambar
                    currentDrawnShape = null;

                    ClearColor(ref temporaryTexture);
                    RenderShapes(ref targetTexture);
                    break;
            }
            lastMouseUpPos = pixelUV;

            
            
            
        }

               // Preview update
        if (currentDrawnShape != null && currentDrawnShape.Vertices.Count > 0)
        {

            Vector2 vertex1, vertex2;
   
            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Triangle:
                    // titik yang sedang digerakkan
                    currentDrawnShape.Vertices[currentDrawnShape.Vertices.Count - 1] = pixelUV;
                    break;
                case DrawingMode.Rectangle:

                    currentDrawnShape.Vertices[0] = new Vector2(startDownPos.x, startDownPos.y);
                    currentDrawnShape.Vertices[1] = new Vector2(pixelUV.x, startDownPos.y);
                    currentDrawnShape.Vertices[2] = new Vector2(pixelUV.x, pixelUV.y);
                    currentDrawnShape.Vertices[3] = new Vector2(startDownPos.x, pixelUV.y);

                    break;
            }

            // proses menggambar garis-garis preview
            ClearColor(ref temporaryTexture);
            for (int itVertex = 0; itVertex < currentDrawnShape.Vertices.Count - 1; itVertex++)
            {
                if (itVertex < currentDrawnShape.Vertices.Count - 1)
                {
                    vertex1 = currentDrawnShape.Vertices[itVertex];
                    vertex2 = currentDrawnShape.Vertices[itVertex + 1];

                    int x1 = (int)vertex1.x;
                    int y1 = (int)vertex1.y;
                    int x2 = (int)vertex2.x;
                    int y2 = (int)vertex2.y;

                    // garis penghubung
                    DrawBresenhamLine(ref temporaryTexture, x1, y1, x2, y2);
                }
            }

            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Rectangle:
                    // garis penghubung terakhir untuk gambar segi empat
                    vertex1 = currentDrawnShape.Vertices[currentDrawnShape.Vertices.Count - 1];
                    vertex2 = currentDrawnShape.Vertices[0];
                    int x1 = (int)vertex1.x;
                    int y1 = (int)vertex1.y;
                    int x2 = (int)vertex2.x;
                    int y2 = (int)vertex2.y;
                    DrawBresenhamLine(ref temporaryTexture, x1, y1, x2, y2);
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
        Line,
        Triangle,
        Rectangle
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

    public void RenderShapes()
    {
        RenderShapes(ref this.targetTexture);
    }
    public void RenderShapes(ref Texture2D texture)
    {
        ClearColor(ref texture);

        int x1, y1, x2, y2;
        Vector2 vertex1;
        Vector2 vertex2;

        List<Edge> edges = new List<Edge>();
        for (int i = 0; i < this.ShapeModels.Count; i++)
        {
            ShapeModel imageModel = this.ShapeModels[i];
            edges.Clear();
            // ========================================================
            // ============== Tambahkan proses reset scanline =========
            // ========================================================
            scanLineFill.Clear();

            switch (imageModel.Mode)
            {
                case DrawingMode.Line:
                    x1 = (int)imageModel.Vertices[0].x;
                    y1 = (int)imageModel.Vertices[0].y;
                    x2 = (int)imageModel.Vertices[1].x;
                    y2 = (int)imageModel.Vertices[1].y;
                    edges.Add(new Edge(x1, y1, x2, y2));
                    break;

// ================================
// ========= Tambahkan proses hubungan antar titik
// ================================
                case DrawingMode.Rectangle:
                case DrawingMode.Triangle:

                    for (int itVertex = 0; itVertex < imageModel.Vertices.Count - 1; itVertex++)
                    {
                        if (itVertex < imageModel.Vertices.Count - 1)
                        {
                            vertex1 = imageModel.Vertices[itVertex];
                            vertex2 = imageModel.Vertices[itVertex + 1];

                            x1 = (int)vertex1.x;
                            y1 = (int)vertex1.y;
                            x2 = (int)vertex2.x;
                            y2 = (int)vertex2.y;

                            // garis penghubung
                            edges.Add(new Edge(x1, y1, x2, y2));

                            // ===================================
                            // tambah data edge scanline
                            // ===================================
                            scanLineFill.AddEdge(x1, y1, x2, y2);

                        }
                    }

                    // garis terakhir
                    vertex1 = imageModel.Vertices[imageModel.Vertices.Count - 1];
                    vertex2 = imageModel.Vertices[0];
                    x1 = (int)vertex1.x;
                    y1 = (int)vertex1.y;
                    x2 = (int)vertex2.x;
                    y2 = (int)vertex2.y;
                    edges.Add(new Edge(x1, y1, x2, y2));

                    // ===================================
                    // tambah data edge terakhir di scanline
                    // ===================================
                    scanLineFill.AddEdge(x1, y1, x2, y2);

                    break;
            }

            // proses scanline
            this.scanLineFill.targetTex = texture;
            scanLineFill.ProcessEdgeTable();
            
           // gambar garis dari masing-masing edge
            for (int itEdge = 0; itEdge < edges.Count; itEdge++)
            {
                Edge edge = edges[itEdge];
                DrawBresenhamLine(ref texture, edge.x1, edge.y1, edge.x2, edge.y2);
            }

            texture.Apply();

        }
    }

    public void SetCurrentDrawingMode(string drawingMode)
    {
        switch (drawingMode.ToLower())
        {
            case "line":
                this.CurrentDrawingMode = DrawingMode.Line;
                break;
            case "triangle":
                this.CurrentDrawingMode = DrawingMode.Triangle;
                break;
            case "rectangle":
                this.CurrentDrawingMode = DrawingMode.Rectangle;
                break;
        }
    }

    public void clearShapes()
    {
        ShapeModels.Clear();
        ClearColor(ref temporaryTexture);
        RenderShapes(ref targetTexture);
        lineCount = 0;
        DrawingMode temp = this.CurrentDrawingMode;
        this.CurrentDrawingMode = DrawingMode.Line;
        this.CurrentDrawingMode = temp;
    }

}
