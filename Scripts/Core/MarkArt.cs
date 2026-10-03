using UnityEngine;

/// <summary>Sprite dấu "?" (đen) và trái tim (đỏ) của Easy mode, vẽ bằng lưới 12x12.</summary>
public static class MarkArt
{
    private static readonly string[] QuestionRows =
    {
        "............",
        "...KKKKKK...",
        "..KKKKKKKK..",
        "..KK....KK..",
        "..KK....KK..",
        ".......KK...",
        "......KK....",
        ".....KK.....",
        ".....KK.....",
        "............",
        ".....KK.....",
        ".....KK....."
    };

    private static readonly string[] HeartRows =
    {
        "............",
        "............",
        "..RRR..RRR..",
        ".RRRRRRRRRR.",
        ".RWRRRRRRRR.",
        ".RRRRRRRRRR.",
        "..RRRRRRRR..",
        "...RRRRRR...",
        "....RRRR....",
        ".....RR.....",
        "............",
        "............"
    };

    private static Sprite question;
    private static Sprite heart;

    public static Sprite Question
    {
        get
        {
            if (question == null)
            {
                question = PixelArt.Build(QuestionRows);
            }
            return question;
        }
    }

    public static Sprite Heart
    {
        get
        {
            if (heart == null)
            {
                heart = PixelArt.Build(HeartRows);
            }
            return heart;
        }
    }
}
