using UnityEngine;

/// <summary>
/// Dựng sprite từ lưới điểm ảnh viết bằng chuỗi ký tự (không cần file ảnh hay cấu hình import).
/// Ký tự: R = đỏ, K = đen, W = trắng, ký tự khác (thường là ".") = trong suốt.
/// Lưới phải vuông (số dòng bằng độ dài mỗi dòng).
/// </summary>
public static class PixelArt
{
    /// <summary>Tạo sprite rộng đúng 1 đơn vị thế giới, pivot ở giữa.</summary>
    public static Sprite Build(string[] rows)
    {
        int size = rows.Length;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;   // giữ nét pixel sắc cạnh
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color red = new Color(1f, 0f, 0f, 1f);
        Color black = new Color(0f, 0f, 0f, 1f);
        Color white = new Color(1f, 1f, 1f, 1f);

        Color[] pixels = new Color[size * size];
        for (int row = 0; row < size; row++)
        {
            int y = size - 1 - row;   // dòng đầu của chuỗi nằm ở phía trên ảnh
            for (int x = 0; x < size; x++)
            {
                char c = rows[row][x];
                Color color = clear;
                if (c == 'R') color = red;
                else if (c == 'K') color = black;
                else if (c == 'W') color = white;
                pixels[y * size + x] = color;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
