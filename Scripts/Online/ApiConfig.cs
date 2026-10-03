/// <summary>
/// Cấu hình kết nối API bảng xếp hạng.
/// TODO: điền thông tin do đồng nghiệp cung cấp. Xem API_CONTRACT.md để biết định dạng yêu cầu/phản hồi.
/// </summary>
public static class ApiConfig
{
    // TODO: địa chỉ gốc của API, ví dụ "http://192.168.1.10/minesweeper/api" (không có dấu "/" ở cuối).
    // Để trống thì game vẫn chạy bình thường nhưng không lưu/đọc được bảng xếp hạng.
    public static readonly string BaseUrl = "";

    // TODO: đường dẫn endpoint, sửa cho khớp với API thật.
    public static readonly string SubmitPath = "/submit.php";    // POST: lưu điểm
    public static readonly string RankingPath = "/ranking.php";  // GET : lấy top

    // TODO (tùy chọn): khóa API. Nếu không để trống, game gửi kèm header "X-Api-Key".
    public static readonly string ApiKey = "";

    public const int TimeoutSeconds = 10;
    public const int RankingLimit = 5;

    public static bool IsConfigured
    {
        get { return !string.IsNullOrEmpty(BaseUrl); }
    }

    public static string SubmitUrl
    {
        get { return BaseUrl.TrimEnd('/') + SubmitPath; }
    }

    public static string RankingUrl
    {
        get { return BaseUrl.TrimEnd('/') + RankingPath; }
    }
}
