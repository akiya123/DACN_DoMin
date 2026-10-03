using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Đánh dấu một phần tử UI của HUD (nút bánh răng, cửa sổ cài đặt, cửa sổ thắng).
/// Các ô trên bàn dùng OnMouseUpAsButton nên bình thường vẫn nhận click xuyên qua UI;
/// IsPointerOver() giúp bỏ qua click khi con trỏ đang nằm trên các phần tử này.
/// </summary>
public class HudBlocker : MonoBehaviour
{
    private static readonly List<RaycastResult> results = new List<RaycastResult>();

    public static bool IsPointerOver()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        PointerEventData data = new PointerEventData(eventSystem);
        data.position = Input.mousePosition;

        results.Clear();
        eventSystem.RaycastAll(data, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.GetComponentInParent<HudBlocker>() != null)
            {
                return true;
            }
        }
        return false;
    }
}
