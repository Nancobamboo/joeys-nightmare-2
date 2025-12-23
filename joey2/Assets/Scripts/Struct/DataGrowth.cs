using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class DataGrowth : IData
{
    public int Points;
    public List<int> UnlockedNodes = new List<int>();

    public void LoadFromJson(JObject jobject)
    {
        if (jobject == null) return;

        if (jobject.ContainsKey("Points"))
            Points = (int)jobject["Points"];
        
        UnlockedNodes.Clear();
        if (jobject.ContainsKey("UnlockedNodes"))
        {
            var array = (JArray)jobject["UnlockedNodes"];
            foreach (var item in array)
            {
                UnlockedNodes.Add((int)item);
            }
        }
    }

    public void SaveToJson(JObject jobject)
    {
        jobject.Add("Points", Points);
        
        JArray array = new JArray();
        foreach (var id in UnlockedNodes)
        {
            array.Add(id);
        }
        jobject.Add("UnlockedNodes", array);
    }

    public bool IsUnlocked(int id)
    {
        return UnlockedNodes.Contains(id);
    }

    public void Unlock(int id)
    {
        if (!UnlockedNodes.Contains(id))
        {
            UnlockedNodes.Add(id);
        }
    }
}

public partial class DataSystem
{
    DataGrowth m_DataGrowth;

    public DataGrowth GetDataGrowth()
    {
        if (m_DataGrowth == null)
        {
            m_DataGrowth = new DataGrowth();
        }
        return m_DataGrowth;
    }

    public void SaveDataGrowth()
    {
        SaveJsonFile("Data_Growth", m_DataGrowth);
    }

    public void LoadDataGrowth()
    {
        LoadJsonFile("Data_Growth", ref m_DataGrowth);
    }
    
    // Helper to add growth points
    public void AddGrowthPoints(int amount)
    {
        GetDataGrowth().Points += amount;
        SaveDataGrowth();
    }
}

