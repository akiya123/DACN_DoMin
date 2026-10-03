using UnityEngine;

/// <summary>Sprite lá cờ, lưới 12x12 lấy từ flag.jpg (đã bỏ nền xám cho trong suốt).</summary>
public static class FlagArt
{
    private static readonly string[] Rows =
    {
        "............",
        ".....RR.....",
        "...RRRR.....",
        "..RRRRR.....",
        "...RRRR.....",
        ".....RR.....",
        "......K.....",
        "......K.....",
        "....KKKK....",
        "..KKKKKKKK..",
        "..KKKKKKKK..",
        "............"
    };

    private static Sprite sprite;

    public static Sprite Sprite
    {
        get
        {
            if (sprite == null)
            {
                sprite = PixelArt.Build(Rows);
            }
            return sprite;
        }
    }
}
