using UnityEngine;
using UnityEngine.UI;

public class ScanLineEffect : MonoBehaviour
{
    [Header("Scan Line Settings")]
    [SerializeField] private float scanSpeed = 400f;
    [SerializeField] private Color scanLineColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float scanLineWidth = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float initialDelay = 0f;
    
    [Header("Glow Effect Settings")]
    [SerializeField] private bool useGlow = true;
    [SerializeField] private float glowWidth = 150f;
    [SerializeField] private float glowIntensity = 0.3f;
    [SerializeField] private int glowLayers = 3;

    // Public properties for external configuration
    public float ScanSpeed
    {
        get => scanSpeed;
        set => scanSpeed = value;
    }

    public Color ScanLineColor
    {
        get => scanLineColor;
        set
        {
            scanLineColor = value;
            UpdateScanLineColors();
        }
    }

    private void UpdateScanLineColors()
    {
        if (scanLineImage != null)
        {
            scanLineImage.color = Color.white;
            scanLineImage.material = CreateScanLineMaterial();
        }
        
        if (glowImages != null && useGlow)
        {
            for (int i = 0; i < glowImages.Length; i++)
            {
                if (glowImages[i] != null)
                {
                    glowImages[i].color = Color.white;
                    glowImages[i].material = CreateGlowMaterial(i);
                }
            }
        }
    }

    public float ScanLineWidth
    {
        get => scanLineWidth;
        set
        {
            scanLineWidth = value;
            if (scanLineRect != null)
            {
                scanLineRect.sizeDelta = new Vector2(0, scanLineWidth);
            }
        }
    }
    
    private RectTransform canvasRect;
    private Image scanLineImage;
    private RectTransform scanLineRect;
    private Image[] glowImages;
    private RectTransform[] glowRects;
    private float canvasHeight;
    private bool isScanning = false;
    private float currentY;

    private bool isInitialized = false;

    private void Start()
    {
        if (!isInitialized)
        {
            InitializeScanLine();
        }
        
        if (initialDelay > 0f)
        {
            Invoke(nameof(StartScanning), initialDelay);
        }
        else
        {
            StartScanning();
        }
    }

    public void Initialize()
    {
        if (!isInitialized)
        {
            InitializeScanLine();
            isInitialized = true;
        }
    }

    private void InitializeScanLine()
    {
        // Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ScanLineEffect: Canvas not found!");
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();
        canvasHeight = canvasRect.rect.height;

        // Create glow layers if enabled
        if (useGlow)
        {
            glowImages = new Image[glowLayers];
            glowRects = new RectTransform[glowLayers];
            
            for (int i = 0; i < glowLayers; i++)
            {
                GameObject glowObj = new GameObject($"GlowLayer_{i}");
                glowObj.transform.SetParent(canvasRect, false);
                
                glowRects[i] = glowObj.AddComponent<RectTransform>();
                glowImages[i] = glowObj.AddComponent<Image>();
                
                // Setup RectTransform for glow layer
                glowRects[i].anchorMin = new Vector2(0, 1);
                glowRects[i].anchorMax = new Vector2(1, 1);
                glowRects[i].pivot = new Vector2(0.5f, 0.5f);
                
                // Each glow layer is wider and more transparent
                float layerWidth = scanLineWidth + (glowWidth - scanLineWidth) * (i + 1) / glowLayers;
                glowRects[i].sizeDelta = new Vector2(0, layerWidth);
                
                // Setup Image with gradient texture
                glowImages[i].color = Color.white;
                glowImages[i].material = CreateGlowMaterial(i);
            }
        }

        // Create main scan line GameObject
        GameObject scanLineObj = new GameObject("ScanLine");
        scanLineObj.transform.SetParent(canvasRect, false);
        
        scanLineRect = scanLineObj.AddComponent<RectTransform>();
        scanLineImage = scanLineObj.AddComponent<Image>();
        
        // Setup RectTransform
        scanLineRect.anchorMin = new Vector2(0, 1);
        scanLineRect.anchorMax = new Vector2(1, 1);
        scanLineRect.pivot = new Vector2(0.5f, 0.5f);
        scanLineRect.sizeDelta = new Vector2(0, scanLineWidth);
        
        // Setup Image with gradient texture
        scanLineImage.color = Color.white;
        scanLineImage.material = CreateScanLineMaterial();

        // Set initial position at top (from top to bottom)
        // Anchor is at top, so Y=0 means center of scan line is at top
        currentY = -scanLineWidth / 2f;
        UpdateScanLinePosition();
        UpdateScanLineColors();
        
        isInitialized = true;
    }

    private void UpdateScanLinePosition()
    {
        if (scanLineRect != null)
        {
            scanLineRect.anchoredPosition = new Vector2(0, currentY);
        }
        
        if (glowRects != null)
        {
            for (int i = 0; i < glowRects.Length; i++)
            {
                if (glowRects[i] != null)
                {
                    glowRects[i].anchoredPosition = new Vector2(0, currentY);
                }
            }
        }
    }

    private Material CreateScanLineMaterial()
    {
        return CreateGradientMaterial(scanLineWidth, 1f);
    }

    private Material CreateGlowMaterial(int layerIndex)
    {
        float layerWidth = scanLineWidth + (glowWidth - scanLineWidth) * (layerIndex + 1) / glowLayers;
        float intensity = glowIntensity * (1f - (float)layerIndex / glowLayers);
        return CreateGradientMaterial(layerWidth, intensity);
    }

    private Material CreateGradientMaterial(float width, float intensity)
    {
        // Create a texture with vertical gradient for glow effect
        int textureHeight = Mathf.Max(1, (int)width);
        Texture2D gradientTexture = new Texture2D(1, textureHeight);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        
        Color[] colors = new Color[textureHeight];
        
        for (int i = 0; i < textureHeight; i++)
        {
            float normalizedPos = (float)i / (textureHeight - 1);
            // Create smooth gradient: transparent at edges, bright in center
            float distanceFromCenter = Mathf.Abs(normalizedPos - 0.5f) * 2f;
            // Use smooth falloff curve for better glow effect
            float alpha = Mathf.Pow(1f - distanceFromCenter, 2f);
            alpha = Mathf.Clamp01(alpha) * intensity;
            colors[i] = new Color(scanLineColor.r, scanLineColor.g, scanLineColor.b, alpha);
        }
        
        gradientTexture.SetPixels(colors);
        gradientTexture.Apply();
        
        // Create material with default UI shader
        Shader shader = Shader.Find("UI/Default");
        Material material = new Material(shader);
        material.mainTexture = gradientTexture;
        
        return material;
    }

    private void StartScanning()
    {
        isScanning = true;
    }

    private void Update()
    {
        if (!isScanning || scanLineRect == null) return;

        // Move scan line downward (from top to bottom)
        currentY -= scanSpeed * Time.deltaTime;

        // Update position
        UpdateScanLinePosition();

        // Check if scan line reached the bottom
        if (currentY < -canvasHeight - scanLineWidth / 2f)
        {
            if (loop)
            {
                // Reset to top
                currentY = -scanLineWidth / 2f;
            }
            else
            {
                // Stop scanning
                isScanning = false;
                if (scanLineImage != null)
                    scanLineImage.enabled = false;
                
                if (glowImages != null)
                {
                    foreach (var glow in glowImages)
                    {
                        if (glow != null)
                            glow.enabled = false;
                    }
                }
            }
        }
    }

    public void SetScanSpeed(float speed)
    {
        scanSpeed = speed;
    }

    public void SetScanLineColor(Color color)
    {
        scanLineColor = color;
        UpdateScanLineColors();
    }

    public void StopScanning()
    {
        isScanning = false;
    }

    public void ResumeScanning()
    {
        isScanning = true;
    }
}

