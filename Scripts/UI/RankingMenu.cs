using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nút "Ranking" ở màn hình Start và bảng top 5 theo từng chế độ.
/// Được MinesweeperBootstrap tự tạo ở scene Start. Muốn dùng nút của riêng bạn thì gọi Open() từ onClick.
/// </summary>
public class RankingMenu : MonoBehaviour
{
    private const int RowCount = 5;
    private static readonly string[] ModeLabels = { "10 x 10", "16 x 16", "30 x 30" };

    private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.2f, 0.97f);
    private static readonly Color ButtonColor = new Color(0.2f, 0.55f, 0.9f);
    private static readonly Color TabInactiveColor = new Color(0.35f, 0.35f, 0.4f);
    private static readonly Color CloseButtonColor = new Color(0.4f, 0.4f, 0.45f);
    private static readonly Color ErrorColor = new Color(1f, 0.5f, 0.5f);

    private GameObject panel;
    private Text statusText;
    private readonly Text[] nameTexts = new Text[RowCount];
    private readonly Text[] timeTexts = new Text[RowCount];
    private readonly Image[] tabImages = new Image[3];

    private int requestId;

    public static RankingMenu Create()
    {
        GameObject go = new GameObject("RankingMenu");
        RankingMenu menu = go.AddComponent<RankingMenu>();
        menu.BuildUi();
        return menu;
    }

    public void Open()
    {
        panel.SetActive(true);
        LoadMode(1);
    }

    public void Close()
    {
        requestId++;
        panel.SetActive(false);
    }

    private void LoadMode(int mode)
    {
        requestId++;
        int currentRequest = requestId;

        for (int i = 0; i < tabImages.Length; i++)
        {
            tabImages[i].color = (i + 1 == mode) ? ButtonColor : TabInactiveColor;
        }
        ClearRows();
        SetStatus("Đang tải...", Color.white);

        StartCoroutine(ScoreApi.GetRanking(mode, delegate (bool success, ScoreEntry[] entries, string message)
        {
            if (currentRequest != requestId) return;   // đã chuyển tab/đóng bảng, bỏ qua kết quả cũ

            if (!success)
            {
                SetStatus(message, ErrorColor);
                return;
            }

            if (entries.Length == 0)
            {
                SetStatus("Chưa có ai trong bảng xếp hạng.", Color.white);
                return;
            }

            SetStatus(string.Empty, Color.white);
            int shown = Mathf.Min(entries.Length, RowCount);
            for (int i = 0; i < shown; i++)
            {
                nameTexts[i].text = (i + 1) + ".  " + entries[i].name;
                timeTexts[i].text = TimeFormat.Format(entries[i].time);
            }
        }));
    }

    private void ClearRows()
    {
        for (int i = 0; i < RowCount; i++)
        {
            nameTexts[i].text = string.Empty;
            timeTexts[i].text = string.Empty;
        }
    }

    private void SetStatus(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
    }

    private void BuildUi()
    {
        Canvas canvas = UiFactory.CreateCanvas("RankingCanvas", 100);
        canvas.transform.SetParent(transform, false);

        // Nút mở bảng xếp hạng (góc trên bên phải).
        Button openButton = UiFactory.CreateButton(canvas.transform, "RankingButton", "Ranking", ButtonColor);
        UiFactory.Place(openButton.GetComponent<RectTransform>(), UiFactory.TopRight, UiFactory.TopRight, new Vector2(-30f, -30f), new Vector2(260f, 80f));
        openButton.onClick.AddListener(Open);

        // Bảng xếp hạng.
        Image overlay = UiFactory.CreateImage(canvas.transform, "RankingOverlay", new Color(0f, 0f, 0f, 0.6f));
        UiFactory.Stretch(overlay.rectTransform, 0f);
        panel = overlay.gameObject;

        Image box = UiFactory.CreateImage(overlay.transform, "RankingBox", PanelColor);
        UiFactory.Place(box.rectTransform, UiFactory.Center, UiFactory.Center, Vector2.zero, new Vector2(800f, 720f));

        Text title = UiFactory.CreateText(box.transform, "Title", "Bảng xếp hạng", 46, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(title.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -25f), new Vector2(740f, 64f));

        for (int i = 0; i < ModeLabels.Length; i++)
        {
            int mode = i + 1;
            Button tab = UiFactory.CreateButton(box.transform, "Tab" + mode, ModeLabels[i], TabInactiveColor);
            UiFactory.Place(tab.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(-240f + i * 240f, -105f), new Vector2(220f, 64f));
            tab.onClick.AddListener(delegate { LoadMode(mode); });
            tabImages[i] = tab.GetComponent<Image>();
        }

        statusText = UiFactory.CreateText(box.transform, "StatusText", string.Empty, 28, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(statusText.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -185f), new Vector2(740f, 44f));

        for (int i = 0; i < RowCount; i++)
        {
            float y = -240f - i * 66f;
            nameTexts[i] = UiFactory.CreateText(box.transform, "Name" + i, string.Empty, 34, TextAnchor.MiddleLeft, Color.white);
            UiFactory.Place(nameTexts[i].rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(-170f, y), new Vector2(440f, 56f));

            timeTexts[i] = UiFactory.CreateText(box.transform, "Time" + i, string.Empty, 34, TextAnchor.MiddleRight, new Color(1f, 0.9f, 0.4f));
            UiFactory.Place(timeTexts[i].rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(250f, y), new Vector2(260f, 56f));
        }

        Button closeButton = UiFactory.CreateButton(box.transform, "CloseButton", "Đóng", CloseButtonColor);
        UiFactory.Place(closeButton.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -610f), new Vector2(240f, 70f));
        closeButton.onClick.AddListener(Close);

        panel.SetActive(false);
    }
}
