using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bánh răng cài đặt ở góc trên bên trái màn hình Start và cửa sổ bật/tắt Cheat mode, Easy mode.
/// Được MinesweeperBootstrap tự tạo ở scene Start. Cài đặt áp dụng cho các ván chơi sau đó.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.2f, 0.97f);
    private static readonly Color CloseButtonColor = new Color(0.4f, 0.4f, 0.45f);
    private static readonly Color ToggleOnColor = new Color(0.2f, 0.65f, 0.35f);
    private static readonly Color ToggleOffColor = new Color(0.4f, 0.4f, 0.45f);
    private static readonly Color WarnColor = new Color(1f, 0.85f, 0.4f);

    private GameObject window;
    private Button cheatToggle;
    private Button easyToggle;

    public static SettingsMenu Create()
    {
        GameObject go = new GameObject("SettingsMenu");
        SettingsMenu menu = go.AddComponent<SettingsMenu>();
        menu.BuildUi();
        return menu;
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= Refresh;
    }

    public void Open()
    {
        Refresh();
        window.SetActive(true);
    }

    public void Close()
    {
        window.SetActive(false);
    }

    private void ToggleCheat()
    {
        GameSettings.CheatMode = !GameSettings.CheatMode;
    }

    private void ToggleEasy()
    {
        GameSettings.EasyMode = !GameSettings.EasyMode;
    }

    private void Refresh()
    {
        SetToggleVisual(cheatToggle, GameSettings.CheatMode);
        SetToggleVisual(easyToggle, GameSettings.EasyMode);
    }

    private static void SetToggleVisual(Button toggle, bool on)
    {
        toggle.GetComponentInChildren<Text>().text = on ? "BẬT" : "TẮT";
        toggle.GetComponent<Image>().color = on ? ToggleOnColor : ToggleOffColor;
    }

    private void BuildUi()
    {
        // Sorting order cao hơn bảng Ranking để bánh răng luôn bấm được.
        Canvas canvas = UiFactory.CreateCanvas("SettingsCanvas", 101);
        canvas.transform.SetParent(transform, false);

        // Nút bánh răng (góc trên bên trái).
        Button gear = UiFactory.CreateButton(canvas.transform, "SettingsButton", string.Empty, new Color(0f, 0f, 0f, 0.55f));
        UiFactory.Place(gear.GetComponent<RectTransform>(), UiFactory.TopLeft, UiFactory.TopLeft, new Vector2(30f, -30f), new Vector2(80f, 80f));
        gear.onClick.AddListener(Open);

        Image icon = UiFactory.CreateImage(gear.transform, "GearIcon", Color.white);
        icon.sprite = UiFactory.GearSprite;
        icon.raycastTarget = false;
        UiFactory.Stretch(icon.rectTransform, 12f);

        // Cửa sổ cài đặt.
        Image overlay = UiFactory.CreateImage(canvas.transform, "SettingsOverlay", new Color(0f, 0f, 0f, 0.6f));
        UiFactory.Stretch(overlay.rectTransform, 0f);
        window = overlay.gameObject;

        Image box = UiFactory.CreateImage(overlay.transform, "SettingsBox", PanelColor);
        UiFactory.Place(box.rectTransform, UiFactory.Center, UiFactory.Center, Vector2.zero, new Vector2(780f, 580f));

        Text title = UiFactory.CreateText(box.transform, "Title", "Cài đặt", 46, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(title.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -25f), new Vector2(720f, 64f));

        cheatToggle = BuildToggleRow(box.transform, "Cheat mode", "Hiện tất cả mìn trên bàn", -120f, ToggleCheat);
        easyToggle = BuildToggleRow(box.transform, "Easy mode",
            "Thêm " + BoardController.EasyModeLives + " tim, trúng mìn mất 1 tim thay vì thua", -235f, ToggleEasy);

        Text note = UiFactory.CreateText(box.transform, "Note",
            "Cài đặt áp dụng cho các ván chơi sau. Bật bất kỳ chế độ nào thì kết quả ván chơi sẽ không được tính vào bảng xếp hạng.",
            26, TextAnchor.MiddleCenter, WarnColor);
        UiFactory.Place(note.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -350f), new Vector2(700f, 100f));

        Button close = UiFactory.CreateButton(box.transform, "CloseButton", "Đóng", CloseButtonColor);
        UiFactory.Place(close.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -475f), new Vector2(240f, 70f));
        close.onClick.AddListener(Close);

        GameSettings.Changed += Refresh;
        Refresh();
        window.SetActive(false);
    }

    private Button BuildToggleRow(Transform parent, string title, string description, float y, System.Action onClick)
    {
        Text label = UiFactory.CreateText(parent, title + "Label",
            title + "\n<size=22>" + description + "</size>", 32, TextAnchor.MiddleLeft, Color.white);
        UiFactory.Place(label.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(-100f, y), new Vector2(520f, 96f));

        Button toggle = UiFactory.CreateButton(parent, title + "Toggle", "TẮT", ToggleOffColor);
        UiFactory.Place(toggle.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(270f, y - 16f), new Vector2(170f, 64f));
        toggle.onClick.AddListener(delegate { onClick(); });
        return toggle;
    }
}
