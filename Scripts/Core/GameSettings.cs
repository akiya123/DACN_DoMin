/// <summary>
/// Trạng thái các chế độ hỗ trợ (Cheat / Easy). Là biến static nên giữ nguyên khi chuyển scene
/// trong cùng một lần chạy game (tắt game rồi mở lại sẽ về mặc định: tắt).
/// </summary>
public static class GameSettings
{
    private static bool cheatMode;
    private static bool easyMode;

    public static event System.Action Changed;

    /// <summary>Hiện tất cả mìn trên bàn.</summary>
    public static bool CheatMode
    {
        get { return cheatMode; }
        set
        {
            if (cheatMode == value) return;
            cheatMode = value;
            RaiseChanged();
        }
    }

    /// <summary>Có thêm tim: trúng mìn mất 1 tim thay vì thua ngay.</summary>
    public static bool EasyMode
    {
        get { return easyMode; }
        set
        {
            if (easyMode == value) return;
            easyMode = value;
            RaiseChanged();
        }
    }

    public static bool AnyEnabled
    {
        get { return cheatMode || easyMode; }
    }

    private static void RaiseChanged()
    {
        if (Changed != null) Changed();
    }
}
