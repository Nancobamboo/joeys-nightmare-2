using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class GData : PureSingleton<GData>
{
	public Dictionary<string, Card> CardDict { get; private set; } = new Dictionary<string, Card>();
	public Dictionary<string, List<string>> LibraryItemDict { get; private set; } = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> DeckItemDict { get; private set; } = new Dictionary<string, List<string>>();

	// 路径策略（简单直观）
	private string DataDir => Application.dataPath + "/Data";
	private string CardCsvPath => Path.Combine(DataDir, "card_info.csv");   // 按你项目实际命名调整
	private string LibraryCsvPath => Path.Combine(DataDir, "library_data.csv");
	private string DeckCsvPath => Path.Combine(DataDir, "deck_data.csv");
    private bool _cardsLoaded = false;
    private bool _libraryLoaded = false;
    private bool _deckLoaded = false;
    private System.DateTime _cardsMTime = System.DateTime.MinValue;
    private System.DateTime _libraryMTime = System.DateTime.MinValue;
    private System.DateTime _deckMTime = System.DateTime.MinValue;
	// 一键加载/保存
	public void LoadAll(bool force = false)
	{
		LoadCards();
		LoadLibrary();
		LoadDeck();
	}
	public void SaveAll()
	{
		SaveLibrary();
		SaveDeck();
	}

	// ---------------- 卡牌数据（原 Store 的纯 C# 版本） ----------------
	public void LoadCards(bool force = false)
	{
        if (!force && _cardsLoaded && !FileChanged(CardCsvPath, ref _cardsMTime)) return;
		CardDict.Clear();
		if (!File.Exists(CardCsvPath))
		{
			Debug.LogWarning("找不到卡牌 CSV: " + CardCsvPath);
			return;
		}
		var lines = File.ReadAllLines(CardCsvPath);
		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var values = line.Split(',');
			if (values.Length < 5) continue;
			if (values[0] == "id") continue;

			// 与你现有 Store.LoadCards 保持一致
			if (values[2].Trim() == "enemy")
			{
				string id = values[0].Trim();
				string name = values[1].Trim();
				string type = values[2].Trim();
				string desc = values[3].Trim();
				int attack = int.Parse(values[4].Trim());
				int hp = int.Parse(values[5].Trim());
				var enemyCard = new EnemyCard(id, name, desc, type, attack, hp);
				CardDict[id] = enemyCard;
			}
			else
			{
				string id = values[0].Trim();
				string name = values[1].Trim();
				string type = values[2].Trim();
				string desc = values[3].Trim();
				int attack = int.Parse(values[4].Trim());
				int heal = int.Parse(values[6].Trim());
				int price = int.Parse(values[7].Trim());
				var itemCard = new ItemCard(id, name, desc, type, attack, heal, price);
				CardDict[id] = itemCard;
			}
		}
		_cardsLoaded = true;
	}

	public Card RandomCard()
	{
		if (CardDict.Count == 0) LoadCards();
		if (CardDict.Count == 0) return null;
		var values = new List<Card>(CardDict.Values);
		int idx = Random.Range(0, values.Count);
		return values[idx];
	}

	// ---------------- 牌库/卡组（原 ItemData 的纯 C# 版本） ----------------
	public void LoadLibrary(bool force = false)
	{
		if (!force && _libraryLoaded && !FileChanged(LibraryCsvPath, ref _libraryMTime)) return;
		LibraryItemDict = LoadTypeListCsv(LibraryCsvPath);
		_libraryLoaded = true;
	}

	public void LoadDeck(bool force = false)
	{
		if (!force && _deckLoaded && !FileChanged(DeckCsvPath, ref _deckMTime)) return;
		DeckItemDict = LoadTypeListCsv(DeckCsvPath);
		_deckLoaded = true;
	}


	public void SaveLibrary()
	{
		SaveTypeListCsv(LibraryCsvPath, LibraryItemDict);
	}

	public void SaveDeck()
	{
		SaveTypeListCsv(DeckCsvPath, DeckItemDict);
	}

	// 通用：读取形如 "id,type" 的 CSV 到 Dictionary<string, List<string>>
	private Dictionary<string, List<string>> LoadTypeListCsv(string path)
	{
		var dict = new Dictionary<string, List<string>>();
		if (!File.Exists(path))
		{
			Debug.LogWarning("CSV 不存在: " + path);
			return dict;
		}
		var lines = File.ReadAllLines(path);
		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			var values = line.Split(',');
			if (values.Length < 2) continue;
			var id = values[0].Trim();
			var type = values[1].Trim();
			if (id == "id") continue;

			if (!dict.TryGetValue(type, out var list))
			{
				list = new List<string>();
				dict[type] = list;
			}
			list.Add(id);
		}
		return dict;
	}

	// 通用：把 Dictionary<string, List<string>> 输出为 "id,type" 的 CSV
	private void SaveTypeListCsv(string path, Dictionary<string, List<string>> data)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			var lines = new List<string>();
			lines.Add("id,type");
			foreach (var kv in data)
			{
				var type = kv.Key;
				var ids = kv.Value;
				if (ids == null) continue;
				for (int i = 0; i < ids.Count; i++)
				{
					var id = ids[i];
					if (string.IsNullOrEmpty(id)) continue;
					lines.Add(id + "," + type);
				}
			}
			File.WriteAllLines(path, lines);
		}
		catch (System.Exception e)
		{
			Debug.LogError("保存 CSV 失败: " + path + " => " + e.Message);
		}
	}
    private bool FileChanged(string path, ref System.DateTime cachedTime)
    {
        var t = SafeGetWriteTime(path);
        if (t != cachedTime)
        {
            cachedTime = t;
            return true; // 变了，需要重载
        }
        return false; // 没变，跳过
    }
    private System.DateTime SafeGetWriteTime(string path)
    {
        return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : System.DateTime.MinValue;
    }


}