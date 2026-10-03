/// <summary>
/// Cấu hình kết nối API bảng xếp hạng (https://dacn.akiya.io.vn).
/// </summary>
public static class ApiConfig
{
    // Địa chỉ gốc của API (không có dấu "/" ở cuối). Để trống thì game vẫn chạy nhưng không lưu/đọc được bảng xếp hạng.
    public static readonly string BaseUrl = "https://dacn.akiya.io.vn/api";

    public static readonly string SubmitPath = "/players";                                    // POST: lưu điểm
    public static readonly string RankingPathFormat = "/players/top5/shortest-time/mode{0}";  // GET : {0} = 10 / 16 / 30

    // API hiện tại không cần khóa. Nếu sau này cần, điền vào đây, game sẽ gửi kèm header "X-Api-Key".
    public static readonly string ApiKey = "";

    public const int TimeoutSeconds = 10;

    public static bool IsConfigured
    {
        get { return !string.IsNullOrEmpty(BaseUrl); }
    }

    public static string SubmitUrl
    {
        get { return BaseUrl.TrimEnd('/') + SubmitPath; }
    }

    public static string RankingUrl(int boardSize)
    {
        return BaseUrl.TrimEnd('/') + string.Format(RankingPathFormat, boardSize);
    }
}
