using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Các hàm tiện ích tạo UI (uGUI) bằng code, để không phải sửa scene/prefab bằng tay.
/// </summary>
public static class UiFactory
{
    public static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    public static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
    public static readonly Vector2 TopRight = new Vector2(1f, 1f);
    public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    private static Font font;
    private static Sprite gearSprite;

    // Bánh răng vẽ bằng code (không cần file ảnh, không phụ thuộc font ký hiệu).
    public static Sprite GearSprite
    {
        get
        {
            if (gearSprite == null)
            {
                gearSprite = BuildGearSprite(128);
            }
            return gearSprite;
        }
    }

    private static Sprite BuildGearSprite(int size)
    {
        const int teeth = 8;
        float center = (size - 1) * 0.5f;
        float outer = size * 0.46f;   // bán kính đỉnh răng
        float root = size * 0.36f;    // bán kính chân răng
        float hole = size * 0.16f;    // bán kính lỗ giữa

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                float wave = Mathf.Clamp01(Mathf.Sin(angle * teeth) * 2.5f + 0.5f);
                float edge = Mathf.Lerp(root, outer, wave);
                float alpha = Mathf.Clamp01(edge - r + 0.5f) * Mathf.Clamp01(r - hole + 0.5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }

    // Dùng font hệ thống (Arial trên Windows) để hiển thị được tiếng Việt có dấu.
    public static Font DefaultFont
    {
        get
        {
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return font;
        }
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private static RectTransform NewRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    public static void Stretch(RectTransform rt, float padding)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    public static Image CreateImage(Transform parent, string name, Color color)
    {
        RectTransform rt = NewRect(parent, name);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color)
    {
        RectTransform rt = NewRect(parent, name);
        Text text = rt.gameObject.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label, Color color)
    {
        Image background = CreateImage(parent, name, color);
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;

        Text text = CreateText(background.transform, "Label", label, 30, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, 4f);
        return button;
    }

    public static InputField CreateInputField(Transform parent, string name, string placeholder, int characterLimit)
    {
        Image background = CreateImage(parent, name, Color.white);
        InputField input = background.gameObject.AddComponent<InputField>();
        input.targetGraphic = background;

        Text text = CreateText(background.transform, "Text", string.Empty, 32, TextAnchor.MiddleLeft, Color.black);
        text.supportRichText = false;
        Stretch(text.rectTransform, 12f);

        Text hint = CreateText(background.transform, "Placeholder", placeholder, 32, TextAnchor.MiddleLeft, new Color(0.5f, 0.5f, 0.5f));
        hint.fontStyle = FontStyle.Italic;
        Stretch(hint.rectTransform, 12f);

        input.textComponent = text;
        input.placeholder = hint;
        input.characterLimit = characterLimit;
        return input;
    }
}
