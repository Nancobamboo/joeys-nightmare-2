using Newtonsoft.Json.Linq;

/// <summary>
/// 难度存档数据：记录已解锁最大难度和当前选择难度。
/// 默认：解锁难度1，当前难度1；总共10个难度。
/// </summary>
public class DataDifficulty : IData
{
    public const int MinDifficulty = 1;
    public const int MaxDifficulty = 8; // Changed from 10 to match difficulty_config.csv (8 difficulty levels)

    /// <summary>
    /// 已解锁的最大难度（默认1）
    /// </summary>
    public int MaxUnlocked = 1;

    /// <summary>
    /// 当前选择的难度（默认1）
    /// </summary>
    public int Current = 1;

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public void Normalize()
    {
        MaxUnlocked = Clamp(MaxUnlocked, MinDifficulty, MaxDifficulty);
        // Current 必须在 [1, MaxUnlocked] 范围内
        Current = Clamp(Current, MinDifficulty, MaxUnlocked);
    }

    public void LoadFromJson(JObject jobject)
    {
        if (jobject == null) return;

        if (jobject.ContainsKey("MaxUnlocked"))
            MaxUnlocked = (int)jobject["MaxUnlocked"];

        if (jobject.ContainsKey("Current"))
            Current = (int)jobject["Current"];

        Normalize();
    }

    public void SaveToJson(JObject jobject)
    {
        Normalize();
        jobject.Add("MaxUnlocked", MaxUnlocked);
        jobject.Add("Current", Current);
    }

    public bool IsUnlocked(int difficulty)
    {
        Normalize();
        return difficulty >= MinDifficulty && difficulty <= MaxUnlocked;
    }

    public void UnlockUpTo(int difficulty)
    {
        difficulty = Clamp(difficulty, MinDifficulty, MaxDifficulty);
        if (difficulty > MaxUnlocked)
        {
            MaxUnlocked = difficulty;
        }
        Normalize();
    }

    public int GetNext(bool toRight)
    {
        Normalize();
        int max = MaxUnlocked;
        if (max < MinDifficulty) max = MinDifficulty;

        int next = toRight ? Current + 1 : Current - 1;
        if (next > max) next = MinDifficulty;
        if (next < MinDifficulty) next = max;
        return next;
    }
}

public partial class DataSystem
{
    DataDifficulty m_DataDifficulty;

    public DataDifficulty GetDataDifficulty()
    {
        if (m_DataDifficulty == null)
        {
            m_DataDifficulty = new DataDifficulty();
            m_DataDifficulty.Normalize();
        }
        return m_DataDifficulty;
    }

    public void SaveDataDifficulty()
    {
        if (m_DataDifficulty == null) m_DataDifficulty = new DataDifficulty();
        m_DataDifficulty.Normalize();
        SaveJsonFile("Data_Difficulty", m_DataDifficulty);
    }

    public void LoadDataDifficulty()
    {
        LoadJsonFile("Data_Difficulty", ref m_DataDifficulty);
        if (m_DataDifficulty == null) m_DataDifficulty = new DataDifficulty();
        m_DataDifficulty.Normalize();
    }

    public int GetCurrentDifficulty()
    {
        return GetDataDifficulty().Current;
    }

    public int GetMaxUnlockedDifficulty()
    {
        return GetDataDifficulty().MaxUnlocked;
    }

    public void SetCurrentDifficulty(int difficulty)
    {
        DataDifficulty data = GetDataDifficulty();
        data.Current = DataDifficulty.MinDifficulty;
        if (difficulty < DataDifficulty.MinDifficulty) difficulty = DataDifficulty.MinDifficulty;
        if (difficulty > data.MaxUnlocked) difficulty = data.MaxUnlocked;
        data.Current = difficulty;
        data.Normalize();
        SaveDataDifficulty();
    }
}


