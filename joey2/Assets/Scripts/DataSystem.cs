using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem : YSingletonModule<DataSystem>
{
    public string SavePath = Application.dataPath + "/Datas";
    string m_JsonFix = ".json";

    protected override void OnInit()
    {
#if UNITY_ANDROID
    SavePath = Application.persistentDataPath;
#endif
        if (!Directory.Exists(this.SavePath))
        {
            Directory.CreateDirectory(this.SavePath);
        }

        LoadGameData();
    }

    #region internal

    public static void CleanData()
    {
        var savePath = Application.dataPath + "/Datas/";

#if UNITY_ANDROID
		savePath = Application.persistentDataPath;
#endif
        string[] jfiles = Directory.GetFiles(savePath, "*.json");
        string[] metafiles = Directory.GetFiles(savePath, "*.json.meta");

        foreach (string jfile in jfiles)
        {
            if (File.Exists(jfile))
            {
                File.Delete(jfile);
            }
        }
        foreach (string jfile in metafiles)
        {
            if (File.Exists(jfile))
            {
                File.Delete(jfile);
            }
        }
        PlayerPrefs.DeleteAll();
    }


    private bool LoadJsonFile<T>(string filename, ref T dat) where T : IData, new()
    {
        var result = PlayerPrefs.GetString(filename, "");
        JArray jarray = null;
        
        if (!string.IsNullOrEmpty(result))
        {
            jarray = (JArray)JsonConvert.DeserializeObject(result);
        }

        if (jarray == null)
        {
            return false;
        }
        dat = Activator.CreateInstance<T>();
        dat.LoadFromJson((JObject)jarray[0]);
        return true;
    }

    private bool SaveJsonFile<T>(string filename, T dat) where T : IData, new()
    {
        List<T> tmp = new List<T>();
        tmp.Add(dat);
        return SaveJsonFile<T>(filename, tmp);
    }

    private bool SaveJsonFile<T>(string filename, List<T> dats) where T : IData, new()
    {
        JArray jrry = new JArray();
        foreach (T dat in dats)
        {
            JObject jobj = new JObject();
            dat.SaveToJson(jobj);
            jrry.Add(jobj);
        }

        string str = JsonConvert.SerializeObject(jrry);
        
        PlayerPrefs.SetString(filename, str);
        PlayerPrefs.Save();

        string tmpFile = SavePath + "/" + filename + "_tmp" + m_JsonFix;
        using (StreamWriter writer = new StreamWriter(tmpFile))
        {
            writer.Write(str);
            writer.Close();
        }

        string saveFile = SavePath + "/" + filename + m_JsonFix;
        string backupFile = SavePath + "/" + filename + "_backup" + m_JsonFix;
        if (File.Exists(saveFile))
        {
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }

            File.Move(saveFile, backupFile);
        }
        if (File.Exists(tmpFile))
        {
            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
            }
            File.Move(tmpFile, saveFile);
        }

        return true;
    }
    #endregion
}
