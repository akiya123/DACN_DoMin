using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sinh bàn chơi và xử lý luật dò mìn. Thay thế boxproduction / sixteenBoxProduction / thirtyBoxProduction.
/// Mỗi scene (10x10, 16x16, 30x30) dùng cùng script này, chỉ khác các tham số trong Inspector.
///
/// Easy mode: người chơi có EasyModeLives mạng (trúng mìn mất 1 mạng, hết mạng thì thua).
/// Bàn có thêm ô tim (HeartChance mỗi ô không phải mìn) giúp hồi 1 mạng, và các ô số 1-8 có thể
/// bị ẩn số thành ô "?" với tỉ lệ phụ thuộc số mạng đang còn.
/// </summary>
public class BoardController : MonoBehaviour
{
    /// <summary>Số mạng ở Easy mode, cũng là số mạng tối đa.</summary>
    public const int EasyModeLives = 3;

    /// <summary>Xác suất một ô không phải mìn trở thành ô tim.</summary>
    public const float HeartChance = 0.02f;

    [SerializeField] private GameObject GameBox;   // prefab ô (giữ tên field cũ)

    [Header("Chế độ chơi")]
    public int modeId = 1;        // 1 = 10x10, 2 = 16x16, 3 = 30x30 (khớp cột mode trong database)
    public int width = 10;
    public int height = 10;
    public int mineCount = 10;

    [Header("Bố cục lưới")]
    public float originX = 3f;
    public float originY = 4f;
    public float stepX = 0.33f;
    public float stepY = 0.32f;

    [Header("UI")]
    public string conditionTag = "Condition";   // tag của Text hiện "You Win!/You Lose!"

    public event System.Action FirstReveal;     // lần bấm ô đầu tiên
    public event System.Action GameWon;
    public event System.Action GameLost;
    public event System.Action LivesChanged;

    public bool IsGameOver { get; private set; }

    /// <summary>Easy mode có hiệu lực trong ván này (chốt lúc vào scene).</summary>
    public bool EasyActive { get; private set; }

    /// <summary>True nếu trong ván này từng bật Cheat/Easy mode (kể cả đã tắt lại): kết quả không được xếp hạng.</summary>
    public bool AssistUsed { get; private set; }

    /// <summary>HUD khóa thao tác trên bàn khi cần.</summary>
    public bool InputLocked { get; set; }

    public int Lives { get; private set; }

    private BoxCell[,] cells;
    private bool firstClickDone;
    private int revealedSafe;
    private int safeCellTotal;
    private Text conditionText;

    /// <summary>Xác suất một ô số 1-8 bị ẩn thành "?" theo số mạng đang còn: 3 → 30%, 2 → 15%, 1 → 0,5%.</summary>
    public static float QuestionChanceFor(int lives)
    {
        if (lives >= 3) return 0.07f;
        if (lives == 2) return 0.04f;
        return 0.005f;
    }

    private void Awake()
    {
        cells = new BoxCell[width, height];
        Transform container = new GameObject("Cells").transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(originX + x * stepX, originY + y * stepY, 0f);
                GameObject go = Instantiate(GameBox, position, Quaternion.identity, container);
                BoxCell cell = go.GetComponent<BoxCell>();
                cell.mine = false;
                cell.heart = false;
                cell.Init(this, x, y);
                cells[x, y] = cell;
            }
        }

        EasyActive = GameSettings.EasyMode;
        Lives = EasyModeLives;
        PlaceMines();
        if (EasyActive)
        {
            PlaceHearts();
        }

        AssistUsed = GameSettings.AnyEnabled;
        GameSettings.Changed += OnSettingsChanged;
        ApplyCheatHighlight();

        if (!string.IsNullOrEmpty(conditionTag))
        {
            GameObject conditionObject = GameObject.FindGameObjectWithTag(conditionTag);
            if (conditionObject != null)
            {
                conditionText = conditionObject.GetComponent<Text>();
            }
        }
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        if (GameSettings.AnyEnabled)
        {
            AssistUsed = true;   // đã bật một lần thì cả ván không được xếp hạng
        }
        ApplyCheatHighlight();
    }

    private void ApplyCheatHighlight()
    {
        bool on = GameSettings.CheatMode && !IsGameOver;
        foreach (BoxCell c in cells)
        {
            c.SetCheatHighlight(on);
        }
    }

    public void OnCellClicked(BoxCell cell)
    {
        if (IsGameOver || InputLocked || cell.Revealed || cell.Flagged) return;   // ô đã cắm cờ thì không mở nhầm

        // Tỉ lệ "?" lấy theo số mạng tại thời điểm bấm.
        float questionChance = QuestionChanceFor(Lives);

        if (!firstClickDone)
        {
            firstClickDone = true;
            if (cell.mine)
            {
                RelocateMine(cell);   // ô đầu tiên không bao giờ là mìn (có thể là tim hoặc "?")
            }
            if (FirstReveal != null) FirstReveal();
        }

        if (cell.mine)
        {
            HitMine(cell);
            return;
        }

        if (cell.heart)
        {
            RevealHeart(cell);   // ô tim không mở lan
        }
        else
        {
            FloodReveal(cell, questionChance);
        }

        if (revealedSafe >= safeCellTotal)
        {
            Win();
        }
    }

    public void OnCellFlagClicked(BoxCell cell)
    {
        if (IsGameOver || InputLocked || cell.Revealed) return;
        cell.ToggleFlag();
    }

    // Mìn được đặt ngay khi sinh bàn (để Cheat mode hiện được mìn từ đầu ván).
    private void PlaceMines()
    {
        List<BoxCell> candidates = new List<BoxCell>();
        foreach (BoxCell c in cells)
        {
            candidates.Add(c);
        }

        int count = Mathf.Min(mineCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int r = UnityEngine.Random.Range(i, candidates.Count);
            BoxCell temp = candidates[i];
            candidates[i] = candidates[r];
            candidates[r] = temp;
            candidates[i].mine = true;
        }

        safeCellTotal = width * height - count;
        ComputeAdjacency();
    }

    // Sau khi đã đặt mìn: mỗi ô không phải mìn có HeartChance trở thành ô tim.
    private void PlaceHearts()
    {
        foreach (BoxCell c in cells)
        {
            if (!c.mine && UnityEngine.Random.value < HeartChance)
            {
                c.heart = true;
            }
        }
    }

    // Dời mìn ở ô vừa bấm sang một ô an toàn khác (ưu tiên ô không phải tim).
    private void RelocateMine(BoxCell clicked)
    {
        List<BoxCell> free = new List<BoxCell>();
        foreach (BoxCell c in cells)
        {
            if (!c.mine && !c.heart) free.Add(c);
        }
        if (free.Count == 0)
        {
            // Hầu như không xảy ra: mọi ô còn lại đều là tim. Chấp nhận lấy một ô tim làm chỗ đặt mìn.
            foreach (BoxCell c in cells)
            {
                if (!c.mine) free.Add(c);
            }
        }
        if (free.Count == 0) return;

        BoxCell target = free[UnityEngine.Random.Range(0, free.Count)];
        clicked.mine = false;
        target.mine = true;
        target.heart = false;
        ComputeAdjacency();
        ApplyCheatHighlight();
    }

    // Con số chỉ đếm mìn; ô tim không được tính.
    private void ComputeAdjacency()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y].adjacentMines = CountAdjacentMines(x, y);
            }
        }
    }

    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (cells[nx, ny].mine) count++;
            }
        }
        return count;
    }

    // Mở một ô an toàn (không phải tim). Ô số 1-8 có thể thành "?" ở Easy mode.
    private void RevealSafeCell(BoxCell cell, float questionChance)
    {
        CellMark mark = CellMark.None;
        if (EasyActive && cell.adjacentMines > 0 && UnityEngine.Random.value < questionChance)
        {
            mark = CellMark.Question;
        }
        cell.Reveal(mark);
        revealedSafe++;
    }

    // Mở ô bằng hàng đợi, chỉ lan tiếp từ ô có 0 mìn xung quanh. Không tự mở ô mìn, ô cắm cờ và ô tim.
    private void FloodReveal(BoxCell start, float questionChance)
    {
        Queue<BoxCell> queue = new Queue<BoxCell>();
        RevealSafeCell(start, questionChance);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            BoxCell current = queue.Dequeue();
            if (current.adjacentMines != 0) continue;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = current.X + dx;
                    int ny = current.Y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;

                    BoxCell neighbour = cells[nx, ny];
                    if (neighbour.Revealed || neighbour.mine || neighbour.Flagged || neighbour.heart) continue;

                    RevealSafeCell(neighbour, questionChance);
                    queue.Enqueue(neighbour);
                }
            }
        }
    }

    // Ô tim: tính là ô an toàn đã mở, hồi 1 mạng (không vượt mức tối đa).
    private void RevealHeart(BoxCell cell)
    {
        cell.Reveal(CellMark.Heart);
        revealedSafe++;

        if (Lives < EasyModeLives)
        {
            Lives++;
            if (LivesChanged != null) LivesChanged();
        }
    }

    private void HitMine(BoxCell cell)
    {
        if (!EasyActive)
        {
            Lose();
            return;
        }

        Lives--;
        if (LivesChanged != null) LivesChanged();

        if (Lives <= 0)
        {
            Lose();
            return;
        }
        cell.Reveal();   // ô mìn đã nổ vẫn hiện ra nhưng không tính là ô an toàn
    }

    private void Win()
    {
        IsGameOver = true;
        if (conditionText != null) conditionText.text = "You Win!";
        if (GameWon != null) GameWon();
    }

    private void Lose()
    {
        IsGameOver = true;
        foreach (BoxCell c in cells)
        {
            if (c.mine) c.Reveal();
        }
        if (conditionText != null) conditionText.text = "You Lose!";
        if (GameLost != null) GameLost();
    }
}
