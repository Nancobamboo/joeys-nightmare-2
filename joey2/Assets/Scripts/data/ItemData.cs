using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ItemData : MonoBehaviour
{


    public TextAsset libraryItemFile;
    public TextAsset deckItemFile;

	public Dictionary<string, List<string>> libraryItemDict = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> deckItemDict = new Dictionary<string, List<string>>();




    // Start is called before the first frame update
    void Start()
    {
        EnsureLoaded();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void LoadLibraryData()
    {
        libraryItemDict.Clear();
        string[] lines = null;
        string dataPath = Application.dataPath + "/Data/library_data.csv";
        if (File.Exists(dataPath))
        {
            lines = File.ReadAllLines(dataPath);
        }
        else
        {
            lines = libraryItemFile.text.Split('\n');
        }
        libraryItemDict = ProcessDataLoad(lines);
    }

    public void LoadDeckData()
    {

        deckItemDict.Clear();
        string[] lines = null;
        string dataPath = Application.dataPath + "/Data/deck_data.csv";
        if (File.Exists(dataPath))
        {
            lines = File.ReadAllLines(dataPath);
        }
        else
        {
            lines = deckItemFile.text.Split('\n');
        }
        deckItemDict = ProcessDataLoad(lines);
    }

    public Dictionary<string, List<string>> ProcessDataLoad(string[] lines)
    {
		Dictionary<string, List<string>> dataDict = new Dictionary<string, List<string>>();
		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line)) continue;
			var values = line.Split(',');
			if (values.Length < 2) continue;
			var id = values[0].Trim();
			var type = values[1].Trim();
			if (id == "id") continue; // 跳过表头
			if (!dataDict.TryGetValue(type, out var list))
			{
				list = new List<string>();
				dataDict[type] = list;
			}
			list.Add(id);
		}

		return dataDict;
    }


    public void EnsureLoaded()
    {

        if (libraryItemFile == null)
        {
            LoadLibraryData();
        }
        if (deckItemFile == null)
        {
            LoadDeckData();
        }
        if (libraryItemDict.Count == 0)
        {
            LoadLibraryData();
        }
        if (deckItemDict.Count == 0)
        {
            LoadDeckData();
        }
    }


    public List<string> ProcessDataSave(Dictionary<string, List<string>> dataDict)
    {
		List<string> data = new List<string>();
		data.Add("id,type");
		foreach (var kv in dataDict)
		{
			var type = kv.Key;
			var ids = kv.Value;
			if (ids == null) continue;
			for (int i = 0; i < ids.Count; i++)
			{
				var id = ids[i];
				if (string.IsNullOrEmpty(id)) continue;
				data.Add(id + "," + type);
			}
		}
		return data;
    }

    public void SaveLibraryData()
    {
        string dataPath = Application.dataPath + "/Data/library_data.csv";
        List<string> data = ProcessDataSave(libraryItemDict);
        File.WriteAllLines(dataPath, data);
    }


    public void SaveDeckData()
    {
        string dataPath = Application.dataPath + "/Data/deck_data.csv";
        List<string> data = ProcessDataSave(deckItemDict);
        File.WriteAllLines(dataPath, data);
    }


    public void SaveData()
    {
        SaveLibraryData();
        SaveDeckData();
    }





}
