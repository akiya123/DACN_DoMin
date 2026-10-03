using UnityEngine;

/// <summary>
/// Một ô trên bàn dò mìn. Thay thế boxScript / sixteenBoxScript / thirtyBoxScript.
/// Các field public giữ nguyên tên cũ để prefab không bị mất tham chiếu sprite.
/// </summary>
public class BoxCell : MonoBehaviour
{
    public bool mine;
    public Sprite[] emptyBoxElement;   // 9 sprite: 0..8 mìn xung quanh
    public Sprite mineElement;

    [System.NonSerialized] public int X;
    [System.NonSerialized] public int Y;
    [System.NonSerialized] public int adjacentMines;

    public bool Revealed { get; private set; }

    private BoardController board;
    private SpriteRenderer spriteRenderer;

    public void Init(BoardController owner, int x, int y)
    {
        board = owner;
        X = x;
        Y = y;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Reveal()
    {
        Revealed = true;
        spriteRenderer.sprite = mine ? mineElement : emptyBoxElement[adjacentMines];
    }

    private void OnMouseUpAsButton()
    {
        if (board != null)
        {
            board.OnCellClicked(this);
        }
    }
}
