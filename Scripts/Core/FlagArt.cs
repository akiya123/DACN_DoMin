using UnityEngine;

/// <summary>
/// Sprite lá cờ, dựng từ lưới 12x12 điểm ảnh (lấy từ flag.jpg, bỏ nền xám cho trong suốt).
/// Dựng bằng code nên không cần thêm file ảnh hay cấu hình import vào dự án.
/// R = đỏ, K = đen, . = trong suốt.
/// </summary>
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

    /// <summary>Sprite rộng đúng 1 đơn vị thế giới; BoxCell tự co theo kích thước ô.</summary>
    public static Sprite Sprite
    {
        get
        {
            if (sprite == null)
            {
                sprite = Build();
            }
            return sprite;
        }
    }

    private static Sprite Build()
    {
        int size = Rows.Length;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;   // giữ nét pixel sắc cạnh
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color red = new Color(1f, 0f, 0f, 1f);
        Color black = new Color(0f, 0f, 0f, 1f);

        Color[] pixels = new Color[size * size];
        for (int row = 0; row < size; row++)
        {
            int y = size - 1 - row;   // dòng đầu của chuỗi nằm ở phía trên ảnh
            for (int x = 0; x < size; x++)
            {
                char c = Rows[row][x];
                pixels[y * size + x] = c == 'R' ? red : (c == 'K' ? black : clear);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
