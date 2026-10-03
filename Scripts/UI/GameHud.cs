using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Đồng hồ hh:mm:ss.mmm, hiển thị tim ở Easy mode và cửa sổ lưu tên khi thắng.
/// Được MinesweeperBootstrap tự tạo ở mỗi scene có BoardController.
/// </summary>
public class GameHud : MonoBehaviour
{
    // Đặt true nếu muốn đồng hồ chỉ bắt đầu chạy từ lần bấm ô đầu tiên (mặc định: chạy ngay khi vào scene).
    private static readonly bool StartTimerOnFirstClick = false;

    private const int PlayerNameMaxLength = 30;

    private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.2f, 0.97f);
    private static readonly Color ButtonColor = new Color(0.2f, 0.55f, 0.9f);
    private static readonly Color CloseButtonColor = new Color(0.4f, 0.4f, 0.45f);
    private static readonly Color ErrorColor = new Color(1f, 0.5f, 0.5f);
    private static readonly Color OkColor = new Color(0.6f, 1f, 0.6f);
    private static readonly Color WarnColor = new Color(1f, 0.85f, 0.4f);

    private BoardController board;
    private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private Text timerText;
    private Text heartsText;

    private GameObject winPanel;
    private Text winTimeText;
    private InputField nameInput;
    private Button saveButton;
    private Button winCloseButton;
    private Text statusText;

    private long finalMilliseconds;
    private bool saved;

    public static GameHud Create(BoardController owner)
    {
        GameObject go = new GameObject("GameHud");
        GameHud hud = go.AddComponent<GameHud>();
        hud.board = owner;
        hud.BuildUi();
        return hud;
    }

    private void Start()
    {
        board.FirstReveal += OnFirstReveal;
        board.GameWon += OnWon;
        board.GameLost += OnLost;
        board.LivesChanged += RefreshHearts;
        GameSettings.Changed += OnSettingsChanged;

        RefreshHearts();

        if (!StartTimerOnFirstClick)
        {
            stopwatch.Start();
        }
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= OnSettingsChanged;
        if (board != null)
        {
            board.FirstReveal -= OnFirstReveal;
            board.GameWon -= OnWon;
            board.GameLost -= OnLost;
            board.LivesChanged -= RefreshHearts;
        }
    }

    private void Update()
    {
        if (stopwatch.IsRunning)
        {
            timerText.text = TimeFormat.Format(stopwatch.ElapsedMilliseconds);
        }
    }

    // ---------- Sự kiện game ----------

    private void OnFirstReveal()
    {
        if (StartTimerOnFirstClick)
        {
            stopwatch.Reset();
            stopwatch.Start();
        }
    }

    private void OnLost()
    {
        stopwatch.Stop();
        timerText.text = TimeFormat.Format(stopwatch.ElapsedMilliseconds);
    }

    private void OnWon()
    {
        stopwatch.Stop();
        finalMilliseconds = stopwatch.ElapsedMilliseconds;
        timerText.text = TimeFormat.Format(finalMilliseconds);
        winTimeText.text = "Thời gian: " + TimeFormat.Format(finalMilliseconds);

        // Có dùng Cheat/Easy mode trong ván này thì không cho lưu điểm.
        bool ranked = !board.AssistUsed;
        nameInput.gameObject.SetActive(ranked);
        saveButton.gameObject.SetActive(ranked);
        UiFactory.Place(winCloseButton.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter,
            new Vector2(ranked ? 150f : 0f, -390f), new Vector2(250f, 72f));

        saved = false;
        nameInput.text = string.Empty;
        nameInput.interactable = true;
        saveButton.interactable = true;

        if (ranked)
        {
            SetStatus(string.Empty, Color.white);
        }
        else
        {
            SetStatus("Bạn đã dùng chế độ hỗ trợ (Cheat/Easy) nên kết quả không được tính vào bảng xếp hạng.", WarnColor);
        }

        winPanel.SetActive(true);
        if (ranked) nameInput.ActivateInputField();
    }

    private void OnSettingsChanged()
    {
        RefreshHearts();
    }

    // ---------- Cửa sổ lưu tên ----------

    private void OnSaveClicked()
    {
        if (saved || board.AssistUsed) return;

        string playerName = nameInput.text.Trim();
        if (playerName.Length == 0)
        {
            SetStatus("Vui lòng nhập tên.", ErrorColor);
            return;
        }

        saveButton.interactable = false;
        SetStatus("Đang lưu...", Color.white);

        int timeMs = (int)System.Math.Min(finalMilliseconds, (long)int.MaxValue);
        StartCoroutine(ScoreApi.Submit(playerName, board.modeId, timeMs, OnSaveFinished));
    }

    private void OnSaveFinished(bool success, string message)
    {
        if (success)
        {
            saved = true;
            nameInput.interactable = false;
            SetStatus("Đã lưu thành công!", OkColor);
        }
        else
        {
            saveButton.interactable = true;
            SetStatus(message, ErrorColor);
        }
    }

    private void OnWinCloseClicked()
    {
        winPanel.SetActive(false);
    }

    private void SetStatus(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
    }

    // ---------- Tim (Easy mode) ----------

    private void RefreshHearts()
    {
        heartsText.gameObject.SetActive(GameSettings.EasyMode);

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < BoardController.EasyModeLives; i++)
        {
            if (i > 0) builder.Append(' ');
            builder.Append(i < board.Lives ? "<color=#ff4d4d>\u2665</color>" : "<color=#555555>\u2665</color>");
        }
        heartsText.text = builder.ToString();
    }

    // ---------- Dựng giao diện ----------

    private void BuildUi()
    {
        Canvas canvas = UiFactory.CreateCanvas("HudCanvas", 100);
        canvas.transform.SetParent(transform, false);

        BuildTimer(canvas.transform);
        BuildHearts(canvas.transform);
        BuildWinPanel(canvas.transform);   // dựng sau cùng để nằm trên cùng
    }

    private void BuildTimer(Transform parent)
    {
        Image background = UiFactory.CreateImage(parent, "TimerBackground", new Color(0f, 0f, 0f, 0.55f));
        background.raycastTarget = false;
        UiFactory.Place(background.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -20f), new Vector2(380f, 80f));
        timerText = UiFactory.CreateText(background.transform, "TimerText", TimeFormat.Format(0), 46, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Stretch(timerText.rectTransform, 4f);
    }

    private void BuildHearts(Transform parent)
    {
        heartsText = UiFactory.CreateText(parent, "HeartsText", string.Empty, 56, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(heartsText.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -105f), new Vector2(380f, 70f));
    }

    private void BuildWinPanel(Transform parent)
    {
        Image overlay = UiFactory.CreateImage(parent, "WinOverlay", new Color(0f, 0f, 0f, 0.6f));
        UiFactory.Stretch(overlay.rectTransform, 0f);
        overlay.gameObject.AddComponent<HudBlocker>();
        winPanel = overlay.gameObject;

        Image box = UiFactory.CreateImage(overlay.transform, "WinBox", PanelColor);
        UiFactory.Place(box.rectTransform, UiFactory.Center, UiFactory.Center, Vector2.zero, new Vector2(720f, 500f));

        Text title = UiFactory.CreateText(box.transform, "Title", "Chúc mừng! Bạn đã thắng", 44, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(title.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -30f), new Vector2(660f, 64f));

        winTimeText = UiFactory.CreateText(box.transform, "TimeText", string.Empty, 38, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f));
        UiFactory.Place(winTimeText.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -105f), new Vector2(660f, 56f));

        nameInput = UiFactory.CreateInputField(box.transform, "NameInput", "Nhập tên của bạn...", PlayerNameMaxLength);
        UiFactory.Place(nameInput.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -190f), new Vector2(580f, 72f));

        statusText = UiFactory.CreateText(box.transform, "StatusText", string.Empty, 28, TextAnchor.MiddleCenter, Color.white);
        UiFactory.Place(statusText.rectTransform, UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(0f, -275f), new Vector2(660f, 80f));

        saveButton = UiFactory.CreateButton(box.transform, "SaveButton", "Lưu", ButtonColor);
        UiFactory.Place(saveButton.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(-150f, -390f), new Vector2(250f, 72f));
        saveButton.onClick.AddListener(OnSaveClicked);

        winCloseButton = UiFactory.CreateButton(box.transform, "CloseButton", "Đóng", CloseButtonColor);
        UiFactory.Place(winCloseButton.GetComponent<RectTransform>(), UiFactory.TopCenter, UiFactory.TopCenter, new Vector2(150f, -390f), new Vector2(250f, 72f));
        winCloseButton.onClick.AddListener(OnWinCloseClicked);

        winPanel.SetActive(false);
    }
}
