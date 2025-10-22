using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ItemData : MonoBehaviour
{


    public TextAsset libraryItemFile;
    public TextAsset deckItemFile;

    public Dictionary<string, int> libraryItemDict = new Dictionary<string, int>();
    public Dictionary<string, int> deckItemDict = new Dictionary<string, int>();



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

    public Dictionary<string, int> ProcessDataLoad(string[] lines)
    {
        Dictionary<string, int> dataDict = new Dictionary<string, int>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = line.Split(',');
            if (values.Length < 2) continue;
            if (values[0] == "id") continue;
            dataDict[values[0].Trim()] = int.Parse(values[1].Trim());
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


    public List<string> ProcessDataSave(Dictionary<string, int> dataDict)
    {
        List<string> data = new List<string>();
        data.Add("id,num");
        foreach (var item in dataDict)
        {
            data.Add(item.Key + "," + item.Value.ToString());
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
