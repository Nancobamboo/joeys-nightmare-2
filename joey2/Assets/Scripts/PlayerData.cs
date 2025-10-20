using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerData : MonoBehaviour
{
    public TextAsset playerDataFile;
    // 用户金币卡牌数据的字典。其中key为数据类型（如"coins"或卡牌id），value为对应的数据值
    public Dictionary<string, int> playerDataDict = new Dictionary<string, int>();
    

    void Awake()
    {
        LoadPlayerData();
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadPlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void LoadPlayerData()
    {
        playerDataDict.Clear();

        string dataPath = Application.dataPath + "/Data/player_data.csv";
        string[] lines = null;

        if (File.Exists(dataPath))
        {
            lines = File.ReadAllLines(dataPath);
        }
        else if (playerDataFile != null)
        {
            lines = playerDataFile.text.Split('\n');
        }
        else
        {
            lines = new string[0];
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = line.Split(',');
            if (values.Length < 2) continue;
            if (values[0] == "id") continue;

            playerDataDict[values[0].Trim()] = int.Parse(values[1].Trim());
        }

        if (!playerDataDict.ContainsKey("coin"))
        {
            playerDataDict["coin"] = 0;
        }
    }

    public void SavePlayerData()
    {
        string dataPath = Application.dataPath + "/Data/player_data.csv";
        List<string> data = new List<string>();
        data.Add("id,num");
        foreach (var item in playerDataDict)
        {
            data.Add(item.Key + "," + item.Value.ToString());
        }
        File.WriteAllLines(dataPath, data);
    }


}
