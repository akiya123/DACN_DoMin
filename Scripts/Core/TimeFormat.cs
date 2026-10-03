/// <summary>Định dạng thời gian hh:mm:ss.mmm từ số mili giây.</summary>
public static class TimeFormat
{
    public static string Format(long milliseconds)
    {
        if (milliseconds < 0) milliseconds = 0;
        long hours = milliseconds / 3600000;
        int minutes = (int)((milliseconds / 60000) % 60);
        int seconds = (int)((milliseconds / 1000) % 60);
        int millis = (int)(milliseconds % 1000);
        return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, millis);
    }
}
