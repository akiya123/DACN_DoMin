using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sinh bàn chơi và xử lý luật dò mìn. Thay thế boxproduction / sixteenBoxProduction / thirtyBoxProduction.
/// Mỗi scene (10x10, 16x16, 30x30) dùng cùng script này, chỉ khác các tham số trong Inspector.
/// </summary>
public class BoardController : MonoBehaviour
{
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

    public bool IsGameOver { get; private set; }

    private BoxCell[,] cells;
    private bool minesPlaced;
    private int revealedSafe;
    private int safeCellTotal;
    private Text conditionText;

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
                cell.Init(this, x, y);
                cells[x, y] = cell;
            }
        }

        if (!string.IsNullOrEmpty(conditionTag))
        {
            GameObject conditionObject = GameObject.FindGameObjectWithTag(conditionTag);
            if (conditionObject != null)
            {
                conditionText = conditionObject.GetComponent<Text>();
            }
        }
    }

    public void OnCellClicked(BoxCell cell)
    {
        if (IsGameOver || cell.Revealed) return;

        if (!minesPlaced)
        {
            PlaceMines(cell);
            if (FirstReveal != null) FirstReveal();
        }

        if (cell.mine)
        {
            Lose();
            return;
        }

        FloodReveal(cell);

        if (revealedSafe >= safeCellTotal)
        {
            Win();
        }
    }

    // Đặt mìn sau lần bấm đầu tiên để ô đầu tiên luôn an toàn.
    private void PlaceMines(BoxCell safeCell)
    {
        List<BoxCell> candidates = new List<BoxCell>();
        foreach (BoxCell c in cells)
        {
            if (c != safeCell) candidates.Add(c);
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y].adjacentMines = CountAdjacentMines(x, y);
            }
        }

        minesPlaced = true;
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

    // Mở ô bằng hàng đợi, chỉ lan tiếp từ ô có 0 mìn xung quanh.
    private void FloodReveal(BoxCell start)
    {
        Queue<BoxCell> queue = new Queue<BoxCell>();
        start.Reveal();
        revealedSafe++;
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
                    if (neighbour.Revealed || neighbour.mine) continue;

                    neighbour.Reveal();
                    revealedSafe++;
                    queue.Enqueue(neighbour);
                }
            }
        }
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
