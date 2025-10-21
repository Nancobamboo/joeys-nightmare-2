using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// 玩家状态（用 JSON 保存，键值结构更简单）
[System.Serializable]
public class PlayerState
{
    public int level = 1;       // 等级（>=1）
    public int exp = 0;         // 当前经验
    public int coins = 0;       // 金币
    public int energy = 0;      // 体力

    public float musicVolume = 1f; // 0~1
    public float sfxVolume = 1f;   // 0~1
    public string language = "zhs";
}

public class PlayerData : MonoBehaviour
{
    // 可选：默认 JSON（首次运行或存档缺失时用作初始值）
    public TextAsset defaultStateJson;

    public PlayerState State = new PlayerState();

    // 存档路径（可写）
    private string SavePath => Path.Combine(Application.persistentDataPath, "player_state.json");

    void Awake()
    {
        Load();
    }

    // 读取：有存档读存档；否则用默认；都没有则用代码默认
    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                var s = JsonUtility.FromJson<PlayerState>(json);
                if (s != null) State = Validate(s);
            }
            else if (defaultStateJson != null && !string.IsNullOrEmpty(defaultStateJson.text))
            {
                var s = JsonUtility.FromJson<PlayerState>(defaultStateJson.text);
                if (s != null) State = Validate(s);
                Save(); // 初始化存档
            }
            else
            {
                State = Validate(new PlayerState());
                Save();
            }
        }
        catch
        {
            State = Validate(new PlayerState());
            Save();
        }
    }

    // 保存（原子替换，避免损坏）
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            var json = JsonUtility.ToJson(State, true);
            var tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tmp, SavePath);
        }
        catch
        {
        }
    }

    // 基础校验与夹逼
    private PlayerState Validate(PlayerState s)
    {
        if (s == null) s = new PlayerState();
        if (s.level < 1) s.level = 1;
        if (s.exp < 0) s.exp = 0;
        if (s.coins < 0) s.coins = 0;
        if (s.energy < 0) s.energy = 0;
        s.musicVolume = Mathf.Clamp01(s.musicVolume);
        s.sfxVolume = Mathf.Clamp01(s.sfxVolume);
        if (string.IsNullOrEmpty(s.language)) s.language = "zhs";
        return s;
    }

    // 简单规则：每级需要 100 * level 经验
    private int LevelNeedExp(int level)
    {
        return 100 * level;
    }

    // 加金币（会自动保存）
    public void AddCoins(int delta)
    {
        State.coins = Mathf.Max(0, State.coins + delta);
        Save();
    }

    // 加经验并尝试升级（会自动保存）
    public void AddExp(int delta)
    {
        State.exp = Mathf.Max(0, State.exp + delta);
        // 循环升级，直到经验不足为止
        while (State.exp >= LevelNeedExp(State.level))
        {
            State.exp -= LevelNeedExp(State.level);
            State.level++;
        }
        Save();
    }

    // 改语言（会自动保存）
    public void SetLanguage(string lang)
    {
        State.language = string.IsNullOrEmpty(lang) ? "zhs" : lang;
        Save();
    }
}