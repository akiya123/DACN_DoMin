using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự gắn đồng hồ/cửa sổ lưu tên vào scene chơi và nút Ranking và bánh răng cài đặt vào scene Start,
/// nên không cần sửa các file .unity. Không cần gắn vào GameObject nào.
/// </summary>
public static class MinesweeperBootstrap
{
    private const string StartSceneName = "Start";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetUp(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUp(scene);
    }

    private static void SetUp(Scene scene)
    {
        BoardController board = Object.FindObjectOfType<BoardController>();
        if (board != null)
        {
            if (Object.FindObjectOfType<GameHud>() == null)
            {
                GameHud.Create(board);
            }
        }
        else if (scene.name == StartSceneName)
        {
            if (Object.FindObjectOfType<RankingMenu>() == null)
            {
                RankingMenu.Create();
            }
            if (Object.FindObjectOfType<SettingsMenu>() == null)
            {
                SettingsMenu.Create();
            }
        }
    }
}
