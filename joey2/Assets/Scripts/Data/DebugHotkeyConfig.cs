using UnityEngine;

/// <summary>
/// 可配置的 Debug 热键开关（放在 Resources/Data/debug_hotkeys.json）。
/// </summary>
[System.Serializable]
public class DebugHotkeyConfig
{
    public bool enableDebugHotkeys = true;
    public bool enableF9UnlockAllDifficulty = true;
    public bool enableF10UnlockAllGrowth = true;

    private static DebugHotkeyConfig s_Cached;

    public static DebugHotkeyConfig Get()
    {
        if (s_Cached != null) return s_Cached;

        // 默认值：如果配置缺失，为安全起见默认关闭（避免发行版意外开启作弊）
        s_Cached = new DebugHotkeyConfig
        {
            enableDebugHotkeys = false,
            enableF9UnlockAllDifficulty = false,
            enableF10UnlockAllGrowth = false
        };

        TextAsset ta = Resources.Load<TextAsset>("Data/debug_hotkeys");
        if (ta == null || string.IsNullOrWhiteSpace(ta.text))
        {
            return s_Cached;
        }

        try
        {
            var cfg = JsonUtility.FromJson<DebugHotkeyConfig>(ta.text);
            if (cfg != null) s_Cached = cfg;
        }
        catch
        {
            // keep defaults
        }

        return s_Cached;
    }
}


