using UnityEngine;

/// <summary>
/// Một ô trên bàn dò mìn. Thay thế boxScript / sixteenBoxScript / thirtyBoxScript.
/// Các field public giữ nguyên tên cũ để prefab không bị mất tham chiếu sprite.
/// Chuột trái: mở ô. Chuột phải: cắm / gỡ cờ.
/// </summary>
public class BoxCell : MonoBehaviour
{
    private static readonly Color CheatColor = new Color(1f, 0.35f, 0.35f);
    private const float FlagScale = 0.8f;   // cờ chiếm 80% kích thước ô

    public bool mine;
    public Sprite[] emptyBoxElement;   // 9 sprite: 0..8 mìn xung quanh
    public Sprite mineElement;

    [System.NonSerialized] public int X;
    [System.NonSerialized] public int Y;
    [System.NonSerialized] public int adjacentMines;

    public bool Revealed { get; private set; }
    public bool Flagged { get; private set; }

    private BoardController board;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer flagRenderer;   // tạo khi cắm cờ lần đầu để không tốn 900 object lúc khởi động
    private Color baseColor;

    public void Init(BoardController owner, int x, int y)
    {
        board = owner;
        X = x;
        Y = y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    public void Reveal()
    {
        SetFlag(false);   // mở ô thì cờ biến mất
        Revealed = true;
        spriteRenderer.color = baseColor;
        spriteRenderer.sprite = mine ? mineElement : emptyBoxElement[adjacentMines];
    }

    public void ToggleFlag()
    {
        if (Revealed) return;
        SetFlag(!Flagged);
    }

    private void SetFlag(bool on)
    {
        Flagged = on;
        if (on && flagRenderer == null)
        {
            CreateFlagRenderer();
        }
        if (flagRenderer != null)
        {
            flagRenderer.gameObject.SetActive(on);
        }
    }

    private void CreateFlagRenderer()
    {
        GameObject flag = new GameObject("Flag");
        flag.transform.SetParent(transform, false);

        flagRenderer = flag.AddComponent<SpriteRenderer>();
        flagRenderer.sprite = FlagArt.Sprite;
        flagRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        flagRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

        // Đặt cờ giữa ô và co theo kích thước sprite của ô (đơn vị cục bộ của ô).
        Sprite cover = spriteRenderer.sprite;
        Vector3 size = cover != null ? cover.bounds.size : Vector3.one;
        Vector3 center = cover != null ? cover.bounds.center : Vector3.zero;
        flag.transform.localPosition = center;
        flag.transform.localScale = new Vector3(size.x * FlagScale, size.y * FlagScale, 1f);
    }

    /// <summary>Cheat mode: tô đỏ các ô có mìn chưa mở.</summary>
    public void SetCheatHighlight(bool on)
    {
        if (Revealed) return;
        spriteRenderer.color = (on && mine) ? CheatColor : baseColor;
    }

    private void OnMouseUpAsButton()
    {
        if (board == null) return;
        if (HudBlocker.IsPointerOver()) return;   // click vào nút/cửa sổ của HUD đang nằm đè lên ô
        board.OnCellClicked(this);
    }

    // OnMouseUpAsButton chỉ báo chuột trái, nên bắt chuột phải ở đây.
    private void OnMouseOver()
    {
        if (board == null || !Input.GetMouseButtonDown(1)) return;
        if (HudBlocker.IsPointerOver()) return;
        board.OnCellFlagClicked(this);
    }
}
