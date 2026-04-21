using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScratchCardGame : MonoBehaviour
{
    [Header("场景 UI 引用")]
    public RawImage scratchImage;
    public Text prizeText;
    public Text hintText;
    public Text congratsText;
    public Text progressText;
    public Button resetButton;
    public Button exitButton;

    [Header("刮刮卡设置")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public int brushSize = 28;
    [Range(0f, 1f)]
    public float revealThreshold = 0.45f;

    [Header("奖品列表")]
    public string[] prizes = new string[] {
        "一等奖: 100元红包!",
        "二等奖: 50元优惠券",
        "三等奖: 10元红包",
        "幸运奖: 5元优惠券",
        "谢谢惠顾，再试一次!"
    };

    [Header("颜色")]
    public Color scratchColor = new Color(0.78f, 0.78f, 0.78f, 1f);

    private Texture2D scratchTexture;
    private RectTransform cardRect;
    private Color32[] pixels;
    private int totalPixels;
    private int scratchedCount;
    private bool isRevealed;

    // 文字像素掩码（精确到字体笔画位置）
    private bool[] textMask;
    internal int textMaskPixelCount;
    internal int textPixelsScratched;
    private string currentPrize;
    private int frameCounter;

    void Start()
    {
        if (scratchImage != null)
            cardRect = scratchImage.GetComponent<RectTransform>();

        SetupButtonListeners();

        // 强制刷新 Canvas 布局，确保 RectTransform.rect 在 BuildTextMask 时已有正确值
        Canvas.ForceUpdateCanvases();

        NewGame();
    }

    void SetupButtonListeners()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetClicked);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClicked);
        }
    }

    void NewGame()
    {
        currentPrize = prizes[Random.Range(0, prizes.Length)];
        if (prizeText != null) prizeText.text = currentPrize;
        if (congratsText != null) congratsText.text = "";
        if (progressText != null) progressText.text = "";
        if (hintText != null)
        {
            hintText.text = "\u522e\u5f00\u6709\u5956";
            hintText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        if (scratchTexture == null)
        {
            scratchTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            scratchTexture.filterMode = FilterMode.Bilinear;
            scratchTexture.wrapMode = TextureWrapMode.Clamp;
        }
        if (scratchImage != null) scratchImage.texture = scratchTexture;

        pixels = new Color32[textureWidth * textureHeight];
        Color32 fillColor = new Color32(
            (byte)(scratchColor.r * 255),
            (byte)(scratchColor.g * 255),
            (byte)(scratchColor.b * 255),
            (byte)(scratchColor.a * 255)
        );
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fillColor;

        AddNoiseTexture();
        DrawScratchPattern();
        ScratchCardMaskBuilder.Build(prizeText, scratchImage, textureWidth, textureHeight, out textMask, out textMaskPixelCount);
        textPixelsScratched = 0;
        DrawTextMaskOutline();

        scratchTexture.SetPixels32(pixels);
        scratchTexture.Apply();

        isRevealed = false;
        scratchedCount = 0;
        totalPixels = textureWidth * textureHeight;
        frameCounter = 0;
    }

    void AddNoiseTexture()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int noise = Random.Range(-12, 13);
            pixels[i].r = (byte)Mathf.Clamp(pixels[i].r + noise, 0, 255);
            pixels[i].g = (byte)Mathf.Clamp(pixels[i].g + noise, 0, 255);
            pixels[i].b = (byte)Mathf.Clamp(pixels[i].b + noise, 0, 255);
        }
    }

    void DrawScratchPattern()
    {
        int cx = textureWidth / 2;
        int cy = textureHeight / 2;

        // Border
        int borderThickness = 4;
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                if (x < borderThickness || x >= textureWidth - borderThickness ||
                    y < borderThickness || y >= textureHeight - borderThickness)
                {
                    pixels[y * textureWidth + x] = new Color32(140, 140, 140, 255);
                }
            }
        }

        // Decorative dots
        for (int i = 0; i < 80; i++)
        {
            int dx = Random.Range(20, textureWidth - 20);
            int dy = Random.Range(20, textureHeight - 20);
            int r = Random.Range(2, 5);
            Color32 dotColor = new Color32(160, 160, 160, 255);

            for (int py = -r; py <= r; py++)
            {
                for (int px = -r; px <= r; px++)
                {
                    if (px * px + py * py <= r * r)
                    {
                        int xx = dx + px, yy = dy + py;
                        if (xx >= 0 && xx < textureWidth && yy >= 0 && yy < textureHeight)
                            pixels[yy * textureWidth + xx] = dotColor;
                    }
                }
            }
        }

        // Central stripe pattern
        for (int y = cy - 20; y <= cy + 20; y++)
        {
            for (int x = cx - 80; x <= cx + 80; x++)
            {
                if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight && (x / 8) % 2 == 0)
                {
                    pixels[y * textureWidth + x] = new Color32(170, 170, 170, 255);
                }
            }
        }
    }

    void DrawTextMaskOutline()
    {
        if (textMask == null || textMaskPixelCount == 0) return;

        Color32 borderColor = new Color32(180, 60, 60, 255);
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int idx = y * textureWidth + x;
                if (!textMask[idx]) continue;

                bool isEdge = (x > 0 && !textMask[idx - 1]) ||
                              (x < textureWidth - 1 && !textMask[idx + 1]) ||
                              (y > 0 && !textMask[idx - textureWidth]) ||
                              (y < textureHeight - 1 && !textMask[idx + textureWidth]);

                if (isEdge && (x + y) % 3 == 0)
                    pixels[idx] = borderColor;
            }
        }
    }



    void Update()
    {
        if (isRevealed || cardRect == null) return;

        Vector2? inputPos = null;
        if (Input.GetMouseButton(0))
        {
            inputPos = Input.mousePosition;
        }
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Began)
                inputPos = touch.position;
        }

        if (!inputPos.HasValue) return;

        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(cardRect, inputPos.Value, null, out localPos))
            return;

        Rect rect = cardRect.rect;
        if (rect.width <= 0 || rect.height <= 0) return;

        float u = (localPos.x + rect.width * 0.5f) / rect.width;
        float v = (localPos.y + rect.height * 0.5f) / rect.height;
        if (u < 0f || u > 1f || v < 0f || v > 1f) return;

        int texX = Mathf.Clamp((int)(u * textureWidth), 0, textureWidth - 1);
        int texY = Mathf.Clamp((int)(v * textureHeight), 0, textureHeight - 1);

        ScratchAt(texX, texY);
    }

    internal void ScratchAt(int cx, int cy)
    {
        bool changed = false;
        int radius = brushSize;
        int radiusSq = radius * radius;

        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= textureHeight) continue;

            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= textureWidth) continue;

                if (dx * dx + dy * dy <= radiusSq)
                {
                    int idx = y * textureWidth + x;
                    if (pixels[idx].a > 0)
                    {
                        pixels[idx].a = 0;
                        scratchedCount++;

                        if (textMask != null && idx >= 0 && idx < textMask.Length && textMask[idx])
                            textPixelsScratched++;

                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            frameCounter++;
            if (frameCounter % 2 == 0)
            {
                scratchTexture.SetPixels32(pixels);
                scratchTexture.Apply();
            }
            CheckReveal();
            UpdateProgressText();
        }
    }

    internal void CheckReveal()
    {
        float textRatio = (float)textPixelsScratched / Mathf.Max(1, textMaskPixelCount);
        if (textRatio >= 0.90f)
            RevealAll();
    }

    internal void UpdateProgressText()
    {
        if (progressText == null || isRevealed) return;
        float ratio = (float)textPixelsScratched / Mathf.Max(1, textMaskPixelCount) * 100f;
        progressText.text = string.Format("\u6587\u5b57\u5df2\u522e\u5f00: {0:F0}%", ratio);
    }

    internal void RevealAll()
    {
        isRevealed = true;
        for (int i = 0; i < pixels.Length; i++) pixels[i].a = 0;
        scratchTexture.SetPixels32(pixels);
        scratchTexture.Apply();

        if (hintText != null) hintText.text = "";
        if (progressText != null) progressText.text = "";
        if (congratsText != null)
        {
            if (currentPrize.Contains("\u8c22\u8c22"))
            {
                congratsText.text = "\u54ce\u5440\uff0c\u6ca1\u4e2d\u5462";
                congratsText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                congratsText.text = "\u606d\u559c\u4e2d\u5956!";
                congratsText.color = new Color(1f, 0.84f, 0f, 1f);
            }
        }
    }

    void OnResetClicked() => NewGame();

    void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (scratchTexture != null)
            Destroy(scratchTexture);
    }
}
