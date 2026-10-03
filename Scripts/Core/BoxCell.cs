using UnityEngine;

/// <summary>
/// Một ô trên bàn dò mìn. Thay thế boxScript / sixteenBoxScript / thirtyBoxScript.
/// Các field public giữ nguyên tên cũ để prefab không bị mất tham chiếu sprite.
/// </summary>
public class BoxCell : MonoBehaviour
{
    private static readonly Color CheatColor = new Color(1f, 0.35f, 0.35f);

    public bool mine;
    public Sprite[] emptyBoxElement;   // 9 sprite: 0..8 mìn xung quanh
    public Sprite mineElement;

    [System.NonSerialized] public int X;
    [System.NonSerialized] public int Y;
    [System.NonSerialized] public int adjacentMines;

    public bool Revealed { get; private set; }

    private BoardController board;
    private SpriteRenderer spriteRenderer;
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
        Revealed = true;
        spriteRenderer.color = baseColor;
        spriteRenderer.sprite = mine ? mineElement : emptyBoxElement[adjacentMines];
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
}
