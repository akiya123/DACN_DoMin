using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ScoreEntry
{
    public string player_name;
    public int time_ms;
}

[Serializable]
public class RankingResponse
{
    public ScoreEntry[] entries;
}

[Serializable]
public class SubmitPayload
{
    public string player_name;
    public int mode;
    public int time_ms;
}

/// <summary>
/// Gọi API bảng xếp hạng bằng UnityWebRequest. Dùng với StartCoroutine(...).
/// Nếu API thật trả JSON khác định dạng trong API_CONTRACT.md thì chỉ cần sửa file này.
/// </summary>
public static class ScoreApi
{
    private const string NotConfiguredMessage = "Chưa cấu hình địa chỉ API (xem ApiConfig.cs).";

    public static IEnumerator Submit(string playerName, int mode, int timeMs, Action<bool, string> onDone)
    {
        if (!ApiConfig.IsConfigured)
        {
            onDone(false, NotConfiguredMessage);
            yield break;
        }

        SubmitPayload payload = new SubmitPayload();
        payload.player_name = playerName;
        payload.mode = mode;
        payload.time_ms = timeMs;
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

        using (UnityWebRequest request = new UnityWebRequest(ApiConfig.SubmitUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyCommonSettings(request);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                onDone(false, "Lưu thất bại: " + request.error);
            }
            else
            {
                onDone(true, string.Empty);
            }
        }
    }

    public static IEnumerator GetRanking(int mode, Action<bool, ScoreEntry[], string> onDone)
    {
        if (!ApiConfig.IsConfigured)
        {
            onDone(false, null, NotConfiguredMessage);
            yield break;
        }

        string url = ApiConfig.RankingUrl + "?mode=" + mode + "&limit=" + ApiConfig.RankingLimit;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            ApplyCommonSettings(request);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                onDone(false, null, "Không tải được bảng xếp hạng: " + request.error);
                yield break;
            }

            RankingResponse response = ParseRanking(request.downloadHandler.text);
            if (response == null || response.entries == null)
            {
                onDone(false, null, "Dữ liệu bảng xếp hạng không hợp lệ.");
                yield break;
            }

            onDone(true, response.entries, string.Empty);
        }
    }

    private static void ApplyCommonSettings(UnityWebRequest request)
    {
        request.timeout = ApiConfig.TimeoutSeconds;
        if (!string.IsNullOrEmpty(ApiConfig.ApiKey))
        {
            request.SetRequestHeader("X-Api-Key", ApiConfig.ApiKey);
        }
    }

    // JsonUtility không đọc được mảng ở gốc, nên nếu API trả "[...]" thì bọc lại thành {"entries":[...]}.
    private static RankingResponse ParseRanking(string json)
    {
        try
        {
            string text = json == null ? string.Empty : json.Trim();
            if (text.StartsWith("["))
            {
                text = "{\"entries\":" + text + "}";
            }
            return JsonUtility.FromJson<RankingResponse>(text);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
