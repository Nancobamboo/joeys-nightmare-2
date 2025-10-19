using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerData : MonoBehaviour
{
    public TextAsset playerDataFile;
    // 用户金币卡牌数据的字典。其中key为数据类型（如"coins"或卡牌id），value为对应的数据值
    public Dictionary<string, int> playerDataDict = new Dictionary<string, int>();
    
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
        string[] data = playerDataFile.text.Split('\n');
        foreach (var line in data)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            string[] values = line.Split(',');
            if (values.Length < 2)
            {
                Debug.LogWarning("跳过不完整的行: " + line);
                continue;
            }
            if (values[0] == "id" || values[0] == "num")
            {
                continue;
            }
            else 
            {
                playerDataDict[values[0]] = int.Parse(values[1]);
            }
        }
        Debug.Log("Player data loaded: " + playerDataDict.Count);

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
