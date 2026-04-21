using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SetupScratchCardScene
{
    [MenuItem("Tools/Setup Scratch Card Scene")]
    public static void Setup()
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 删除旧对象
        GameObject oldCanvas = GameObject.Find("ScratchCanvas");
        if (oldCanvas != null) { Undo.DestroyObjectImmediate(oldCanvas); }
        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) { Undo.DestroyObjectImmediate(oldEventSystem); }
        ScratchCardGame oldGame = Object.FindObjectOfType<ScratchCardGame>();
        if (oldGame != null) { Undo.DestroyObjectImmediate(oldGame.gameObject); }

        // 创建 Canvas
        GameObject canvasGO = new GameObject("ScratchCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create ScratchCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // 创建 EventSystem
        GameObject eventSystemGO = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();
        EditorUtility.SetDirty(eventSystemGO);

        // 创建所有 UI 元素
        CreateBackground(canvasGO.transform);
        CreateTitle(canvasGO.transform);
        GameObject prizeGO = CreatePrizeText(canvasGO.transform);
        GameObject scratchGO = CreateScratchLayer(canvasGO.transform);
        GameObject congratsGO = CreateCongratsText(canvasGO.transform);
        GameObject resetBtn = CreateResetButton(canvasGO.transform);
        GameObject exitBtn = CreateExitButton(canvasGO.transform);
        GameObject progressGO = CreateProgressText(canvasGO.transform);
        CreateInstructionText(canvasGO.transform);

        // 查找或创建 ScratchCardGame
        ScratchCardGame game = Object.FindObjectOfType<ScratchCardGame>();
        if (game == null)
        {
            GameObject gameGO = new GameObject("ScratchCardGame");
            Undo.RegisterCreatedObjectUndo(gameGO, "Create ScratchCardGame");
            game = gameGO.AddComponent<ScratchCardGame>();
        }

        // 设置引用
        SerializedObject so = new SerializedObject(game);
        so.FindProperty("scratchImage").objectReferenceValue = scratchGO.GetComponent<RawImage>();
        so.FindProperty("prizeText").objectReferenceValue = prizeGO.GetComponent<Text>();
        so.FindProperty("hintText").objectReferenceValue = scratchGO.transform.Find("HintText").GetComponent<Text>();
        so.FindProperty("congratsText").objectReferenceValue = congratsGO.GetComponent<Text>();
        so.FindProperty("progressText").objectReferenceValue = progressGO.GetComponent<Text>();
        so.FindProperty("resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        so.FindProperty("exitButton").objectReferenceValue = exitBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(game);
        EditorUtility.SetDirty(eventSystemGO);
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, scene.path);
        AssetDatabase.SaveAssets();
        Debug.Log("Scene saved: " + saved + " at " + scene.path);

        Debug.Log("Scratch Card Scene setup complete!");
    }

    static void SetupText(Text text, string content, int fontSize, Color color)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            string[] fallbacks = new[] { "Arial", "DejaVu Sans", "Liberation Sans",
                                         "Noto Sans", "Ubuntu Sans", "FreeSans",
                                         "WenQuanYi Zen Hei", "Source Han Sans" };
            foreach (var name in fallbacks)
            {
                font = Font.CreateDynamicFontFromOSFont(name, 16);
                if (font != null) break;
            }
        }
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
    }

    static void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 0.97f, 0.9f, 1f);

        RectTransform rect = bg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        bg.transform.SetAsFirstSibling();
    }

    static GameObject CreateTitle(Transform parent)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent, false);
        Text title = titleGO.AddComponent<Text>();
        SetupText(title, "\u5e78\u8fd0\u522e\u522e\u4e50", 72, new Color(0.85f, 0.25f, 0.1f, 1f));
        title.fontStyle = FontStyle.Bold;

        RectTransform rect = titleGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.82f);
        rect.anchorMax = new Vector2(1f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return titleGO;
    }

    static GameObject CreatePrizeText(Transform parent)
    {
        GameObject prizeGO = new GameObject("PrizeText");
        prizeGO.transform.SetParent(parent, false);
        Text prizeText = prizeGO.AddComponent<Text>();
        SetupText(prizeText, "", 52, new Color(0.9f, 0.1f, 0.1f, 1f));
        prizeText.fontStyle = FontStyle.Bold;

        RectTransform rect = prizeGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.38f);
        rect.anchorMax = new Vector2(0.85f, 0.62f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return prizeGO;
    }

    static GameObject CreateScratchLayer(Transform parent)
    {
        GameObject scratchGO = new GameObject("ScratchLayer");
        scratchGO.transform.SetParent(parent, false);
        RawImage scratchImage = scratchGO.AddComponent<RawImage>();

        RectTransform cardRect = scratchGO.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.15f, 0.25f);
        cardRect.anchorMax = new Vector2(0.85f, 0.75f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        scratchGO.AddComponent<Mask>();

        GameObject hintGO = new GameObject("HintText");
        hintGO.transform.SetParent(scratchGO.transform, false);
        Text hintText = hintGO.AddComponent<Text>();
        SetupText(hintText, "\u522e\u5f00\u6709\u5956", 64, new Color(0.5f, 0.5f, 0.5f, 1f));
        hintText.fontStyle = FontStyle.Bold;

        RectTransform hintRect = hintGO.GetComponent<RectTransform>();
        hintRect.anchorMin = Vector2.zero;
        hintRect.anchorMax = Vector2.one;
        hintRect.offsetMin = Vector2.zero;
        hintRect.offsetMax = Vector2.zero;

        return scratchGO;
    }

    static GameObject CreateCongratsText(Transform parent)
    {
        GameObject congratsGO = new GameObject("CongratsText");
        congratsGO.transform.SetParent(parent, false);
        Text congratsText = congratsGO.AddComponent<Text>();
        SetupText(congratsText, "", 56, new Color(1f, 0.84f, 0f, 1f));
        congratsText.fontStyle = FontStyle.Bold;

        RectTransform rect = congratsGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.58f);
        rect.anchorMax = new Vector2(0.85f, 0.72f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return congratsGO;
    }

    static GameObject CreateResetButton(Transform parent)
    {
        GameObject btnGO = new GameObject("ResetButton");
        btnGO.transform.SetParent(parent, false);

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.65f, 0.95f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.75f, 1f, 1f);
        colors.pressedColor = new Color(0.15f, 0.55f, 0.85f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        btn.colors = colors;

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.20f, 0.08f);
        rect.anchorMax = new Vector2(0.45f, 0.18f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("ButtonText");
        textGO.transform.SetParent(btnGO.transform, false);
        Text btnText = textGO.AddComponent<Text>();
        SetupText(btnText, "\u518d\u6765\u4e00\u5f20", 42, Color.white);
        btnText.fontStyle = FontStyle.Bold;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnGO;
    }

    static GameObject CreateExitButton(Transform parent)
    {
        GameObject btnGO = new GameObject("ExitButton");
        btnGO.transform.SetParent(parent, false);

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.25f, 0.15f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(1f, 0.4f, 0.3f, 1f);
        colors.pressedColor = new Color(0.7f, 0.2f, 0.1f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        btn.colors = colors;

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.55f, 0.08f);
        rect.anchorMax = new Vector2(0.80f, 0.18f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("ButtonText");
        textGO.transform.SetParent(btnGO.transform, false);
        Text btnText = textGO.AddComponent<Text>();
        SetupText(btnText, "\u9000\u51fa\u6e38\u620f", 42, Color.white);
        btnText.fontStyle = FontStyle.Bold;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btnGO;
    }

    static GameObject CreateProgressText(Transform parent)
    {
        GameObject progressGO = new GameObject("ProgressText");
        progressGO.transform.SetParent(parent, false);
        Text progressText = progressGO.AddComponent<Text>();
        SetupText(progressText, "", 32, new Color(0.15f, 0.65f, 0.25f, 1f));
        progressText.fontStyle = FontStyle.Bold;

        RectTransform rect = progressGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.20f);
        rect.anchorMax = new Vector2(0.85f, 0.245f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return progressGO;
    }

    static void CreateInstructionText(Transform parent)
    {
        GameObject instGO = new GameObject("Instruction");
        instGO.transform.SetParent(parent, false);
        Text inst = instGO.AddComponent<Text>();
        SetupText(inst, "\u6309\u4f4f\u9f20\u6807\u6216\u624b\u6307\u62d6\u52a8\u6765\u522e\u5f00\u6d82\u5c42\uff08\u987b\u522e\u5f00\u4e2d\u5fc3\u533a\u57df\uff09", 26, new Color(0.4f, 0.4f, 0.4f, 1f));

        RectTransform rect = instGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.01f);
        rect.anchorMax = new Vector2(1f, 0.07f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
