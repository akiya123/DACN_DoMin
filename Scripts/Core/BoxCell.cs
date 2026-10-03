using UnityEngine;

/// <summary>Dấu hiệu hiển thị trên ô đã mở ở Easy mode.</summary>
public enum CellMark
{
    None,       // hiện con số như bình thường
    Question,   // ô "?": đã mở nhưng ẩn con số
    Heart       // ô tim: đã mở, hồi 1 mạng
}

/// <summary>
/// Một ô trên bàn dò mìn. Thay thế boxScript / sixteenBoxScript / thirtyBoxScript.
/// Các field public giữ nguyên tên cũ để prefab không bị mất tham chiếu sprite.
/// Chuột trái: mở ô. Chuột phải: cắm / gỡ cờ.
/// </summary>
public class BoxCell : MonoBehaviour
{
    private static readonly Color CheatColor = new Color(1f, 0.35f, 0.35f);
    private const float FlagScale = 0.8f;   // cờ chiếm 80% kích thước ô
    private const float MarkScale = 0.75f;  // dấu "?" / tim chiếm 75% kích thước ô

    public bool mine;
    public Sprite[] emptyBoxElement;   // 9 sprite: 0..8 mìn xung quanh
    public Sprite mineElement;

    [System.NonSerialized] public int X;
    [System.NonSerialized] public int Y;
    [System.NonSerialized] public int adjacentMines;
    [System.NonSerialized] public bool heart;   // ô tim (Easy mode): ô an toàn đặc biệt, không mở lan

    public bool Revealed { get; private set; }
    public bool Flagged { get; private set; }
    public CellMark Mark { get; private set; }

    private BoardController board;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer flagRenderer;   // các lớp phủ được tạo khi cần để không tốn hàng trăm object lúc khởi động
    private SpriteRenderer markRenderer;
    private Color baseColor;

    public void Init(BoardController owner, int x, int y)
    {
        board = owner;
        X = x;
        Y = y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    /// <summary>Mở ô. Ô mìn luôn hiện mìn; ô khác hiện con số, hoặc "?" / tim nếu có dấu hiệu.</summary>
    public void Reveal(CellMark mark = CellMark.None)
    {
        SetFlag(false);   // mở ô thì cờ biến mất
        Revealed = true;
        spriteRenderer.color = baseColor;

        if (mine)
        {
            Mark = CellMark.None;
            spriteRenderer.sprite = mineElement;
            return;
        }

        Mark = mark;
        if (mark == CellMark.None)
        {
            spriteRenderer.sprite = emptyBoxElement[adjacentMines];
            return;
        }

        spriteRenderer.sprite = emptyBoxElement[0];   // nền ô đã mở, không số
        if (markRenderer == null)
        {
            markRenderer = CreateOverlay("Mark", MarkScale);
        }
        markRenderer.sprite = (mark == CellMark.Heart) ? MarkArt.Heart : MarkArt.Question;
        markRenderer.gameObject.SetActive(true);
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
            flagRenderer = CreateOverlay("Flag", FlagScale);
            flagRenderer.sprite = FlagArt.Sprite;
        }
        if (flagRenderer != null)
        {
            flagRenderer.gameObject.SetActive(on);
        }
    }

    // Tạo sprite con nằm giữa ô, nằm trên sprite của ô, co theo kích thước ô (đơn vị cục bộ).
    private SpriteRenderer CreateOverlay(string objectName, float scale)
    {
        GameObject overlay = new GameObject(objectName);
        overlay.transform.SetParent(transform, false);

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sortingLayerID = spriteRenderer.sortingLayerID;
        renderer.sortingOrder = spriteRenderer.sortingOrder + 1;

        Sprite cover = spriteRenderer.sprite;
        Vector3 size = cover != null ? cover.bounds.size : Vector3.one;
        Vector3 center = cover != null ? cover.bounds.center : Vector3.zero;
        overlay.transform.localPosition = center;
        overlay.transform.localScale = new Vector3(size.x * scale, size.y * scale, 1f);
        return renderer;
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
