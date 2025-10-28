using System.Collections.Generic;
using System.IO;
using UnityEngine;


public sealed class GData : PureSingleton<GData>
{
	public Dictionary<string, Card> CardDict { get; private set; } = new Dictionary<string, Card>();
	// public Dictionary<string, List<string>> LibraryItemDict { get; private set; } = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> DeckItemDict { get; private set; } = new Dictionary<string, List<string>>();

	// 临时加入卡包的卡的id列表
	public List<string> TempCardIdList { get; set; } = new List<string>();

	// 路径策略（简单直观）
	private string CardCsvPath = "Data/card_info";   // 按你项目实际命名调整
	// private string LibraryCsvPath = "Data/library_data";
	private string DeckCsvPath = "Data/deck_data";
    private bool _cardsLoaded = false;
    // private bool _libraryLoaded = false;
    private bool _deckLoaded = false;
    private System.DateTime _cardsMTime = System.DateTime.MinValue;
    // private System.DateTime _libraryMTime = System.DateTime.MinValue;
    private System.DateTime _deckMTime = System.DateTime.MinValue;
	// 一键加载/保存
	public void LoadAll(bool force = false)
	{
		LoadCards();
		// LoadLibrary();
		LoadDeck();
	}
	public void SaveAll()
	{
		// SaveLibrary();
		SaveDeck();
	}

	// ---------------- 卡牌数据（原 Store 的纯 C# 版本） ----------------
	public void LoadCards(bool force = false)
	{
        if (!force && _cardsLoaded && !FileChanged(CardCsvPath, ref _cardsMTime)) return;
		CardDict.Clear();
		var ta = Resources.Load<TextAsset>(CardCsvPath);
        // Debug.Log($"CardCsvPath: {CardCsvPath}, ta: {ta}, text: {ta.text}");
        var lines = ta.text.Split('\n');
        // Debug.Log($"Lines: {lines.Length}");
        if (lines.Length == 0) { _cardsLoaded = true; return; }
        // Debug.Log($"Lines: {lines[0]}");
        // 解析表头索引
		var header = lines[0].Split(',');
		var idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			var key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}
		int IdIdx = idx.ContainsKey("id") ? idx["id"] : -1;
		int CardImageIdx = idx.ContainsKey("cardImage") ? idx["cardImage"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int CardNameIdx = idx.ContainsKey("cardName") ? idx["cardName"] : -1;
		int DescriptionIdx = idx.ContainsKey("description") ? idx["description"] : -1;
		int AttackIdx = idx.ContainsKey("attack") ? idx["attack"] : -1;
		int DefenceIdx = idx.ContainsKey("defence") ? idx["defence"] : -1;
		int HealthIdx = idx.ContainsKey("health") ? idx["health"] : -1;
		int PriceIdx = idx.ContainsKey("price") ? idx["price"] : -1;
        int StarsIdx = idx.ContainsKey("stars") ? idx["stars"] : -1;
        int EffectIdsIdx = idx.ContainsKey("effectIds") ? idx["effectIds"] : -1;

        // Debug.Log($"IdIdx: {IdIdx}, CardImageIdx: {CardImageIdx}, TypeIdx: {TypeIdx}, CardNameIdx: {CardNameIdx}, DescriptionIdx: {DescriptionIdx}, AttackIdx: {AttackIdx}, DefenceIdx: {DefenceIdx}, HealthIdx: {HealthIdx}, PriceIdx: {PriceIdx}, StarsIdx: {StarsIdx}");

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line) ) continue;

			var values = line.Split(',');
            if (string.IsNullOrEmpty(values[0])) continue;
			// 取值函数
			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}
			// 安全解析整数的函数（处理空值）
			int GetInt(int index, int defaultValue = 0)
			{
				string value = Get(index);
				if (string.IsNullOrWhiteSpace(value)) return defaultValue;
				if (int.TryParse(value, out int result)) return result;
				Debug.LogWarning($"[GData] 第 {i} 行无法解析整数值: '{value}', 使用默认值 {defaultValue}");
				return defaultValue;
			}
            string id = Get(IdIdx);
            string cardImage = Get(CardImageIdx);
            string type = Get(TypeIdx);
            string cardName = Get(CardNameIdx);
            string description = Get(DescriptionIdx);
            int attack = GetInt(AttackIdx, 0);
            int defence = GetInt(DefenceIdx, 0);
            int health = GetInt(HealthIdx, 0);
            int price = GetInt(PriceIdx, 0);
            int stars = GetInt(StarsIdx, 0);
            List<string> effectIds = new List<string>();
            if (EffectIdsIdx >= 0)
            {
                string eff = Get(EffectIdsIdx);
                var parts = eff.Split(new char[]{';','|'}, System.StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < parts.Length; k++)
                {
                    var eid = parts[k].Trim();
                    if (!string.IsNullOrEmpty(eid)) effectIds.Add(eid);
                }
            }

            var card = new Card(id, type, cardImage, cardName, description, attack, defence, health, price, stars, effectIds);

            CardDict[id] = card;
            // Debug.Log($"Card: {card.id}, {card.cardName}, {card.type}, {card.cardImage}, {card.description}, {card.attack}, {card.defence}, {card.health}, {card.price}, {card.stars}");
            // Debug.Log($"CardDict: {CardDict.Count}");
		}
		_cardsLoaded = true;
        Debug.Log("Cards loaded: " + CardDict.Count);
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
	// public void LoadLibrary(bool force = false)
	// {
	// 	if (!force && _libraryLoaded && !FileChanged(LibraryCsvPath, ref _libraryMTime)) return;
	// 	LibraryItemDict = LoadTypeListCsv(LibraryCsvPath);
	// 	Debug.Log($"LibraryItemDict: {LibraryItemDict.Count}");
	// 	_libraryLoaded = true;
	// }

	public void LoadDeck(bool force = false)
	{
		if (!force && _deckLoaded && !FileChanged(DeckCsvPath, ref _deckMTime)) return;
		DeckItemDict = LoadTypeListCsv(DeckCsvPath);
		Debug.Log($"DeckItemDict: {DeckItemDict.Count}");
		_deckLoaded = true;
	}


	// public void SaveLibrary()
	// {
	// 	SaveTypeListCsv(LibraryCsvPath, LibraryItemDict);
	// }

	public void SaveDeck()
	{
		SaveTypeListCsv(DeckCsvPath, DeckItemDict);
	}

	// 通用：读取形如 "id,type" 的 CSV 到 Dictionary<string, List<string>>
	private Dictionary<string, List<string>> LoadTypeListCsv(string path)
	{
		var dict = new Dictionary<string, List<string>>();
        var lines = Resources.Load<TextAsset>(path).text.Split('\n');
		for (int i = 1; i < lines.Length; i++)
		{
			
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			var values = line.Split(',');
            if (string.IsNullOrEmpty(values[0])) continue;
			if (values.Length < 2) continue;
			var id = values[0].Trim();
			var type = values[1].Trim();

			if (!dict.ContainsKey(type))
			{
				dict[type] = new List<string>();
			}
			dict[type].Add(id);

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