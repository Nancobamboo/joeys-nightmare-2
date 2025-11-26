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

	// Roguelike数据
	public List<RoguelikeCharacter> RoguelikeCharacterList { get; private set; } = new List<RoguelikeCharacter>();
	public List<RoguelikeStage> RoguelikeStageList { get; private set; } = new List<RoguelikeStage>();

	// 路径策略（简单直观）
	private string CardCsvPath = "Data/card_info";   // 按你项目实际命名调整
													 // private string LibraryCsvPath = "Data/library_data";
	private string DeckCsvPath = "Data/deck_data";
	private string TutorialEquipmentDeckCsvPath = "Data/tutorial_equipment_deck";
	private string TutorialPlayerDataCsvPath = "Data/tutorial_player_data";
	private string RoguelikeCharacterCsvPath = "Data/roguelike_character";
	private string RoguelikeStageCsvPath = "Data/roguelike_stage";
	private Dictionary<int, Dictionary<string, List<string>>> _tutorialEquipmentDeckCache = new Dictionary<int, Dictionary<string, List<string>>>();
	private bool _tutorialEquipmentDeckLoaded = false;
	private Dictionary<int, (int health, int maxHealth)> _tutorialPlayerDataCache = new Dictionary<int, (int, int)>();
	private bool _tutorialPlayerDataLoaded = false;
	private bool _cardsLoaded = false;
	// private bool _libraryLoaded = false;
	private bool _deckLoaded = false;
	private bool _roguelikeCharacterLoaded = false;
	private bool _roguelikeStageLoaded = false;
	private System.DateTime _cardsMTime = System.DateTime.MinValue;
	// private System.DateTime _libraryMTime = System.DateTime.MinValue;
	private System.DateTime _deckMTime = System.DateTime.MinValue;
	// 一键加载/保存
	public void LoadAll(bool force = false)
	{
		LoadCards();
		// LoadLibrary();
		LoadDeck();
		LoadTutorialEquipmentDeck();
		LoadTutorialPlayerData();
		LoadRoguelikeCharacter();
		LoadRoguelikeStage();
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
			if (string.IsNullOrWhiteSpace(line)) continue;

			// Parse CSV line handling quoted fields
			var values = ParseCSVLine(line);
			if (values == null || values.Length == 0 || string.IsNullOrEmpty(values[0])) continue;
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
			string effectId = string.Empty;
			if (EffectIdsIdx >= 0)
			{
				string eff = Get(EffectIdsIdx);
				if (!string.IsNullOrEmpty(eff))
				{
					var parts = eff.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length > 0)
					{
						effectId = parts[0].Trim();
					}
				}
			}

			var card = new Card(id, type, cardImage, cardName, description, attack, defence, health, price, stars, effectId);

			CardDict[id] = card;
		}
		_cardsLoaded = true;
		Debug.Log("Cards loaded: " + CardDict.Count);
	}

	// Parse CSV line handling quoted fields with commas
	private string[] ParseCSVLine(string line)
	{
		var values = new List<string>();
		bool inQuotes = false;
		string currentValue = "";

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];

			if (c == '"')
			{
				// Toggle quote state, but don't add quote to value
				inQuotes = !inQuotes;
			}
			else if (c == ',' && !inQuotes)
			{
				values.Add(currentValue);
				currentValue = "";
			}
			else
			{
				currentValue += c;
			}
		}
		values.Add(currentValue); // Add last value

		return values.ToArray();
	}

	public Card RandomCard()
	{
		if (CardDict.Count == 0) LoadCards();
		if (CardDict.Count == 0) return null;
		var values = new List<Card>(CardDict.Values);
		int idx = Random.Range(0, values.Count);
		return values[idx];
	}

	public Card GetCardConfigById(string cardId)
	{
		return CardDict[cardId];
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

	// ---------------- 教程装备卡组 ---------------- 
	public void LoadTutorialEquipmentDeck(bool force = false)
	{
		if (!force && _tutorialEquipmentDeckLoaded) return;

		_tutorialEquipmentDeckCache.Clear();
		var ta = Resources.Load<TextAsset>(TutorialEquipmentDeckCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Tutorial equipment deck CSV not found: {TutorialEquipmentDeckCsvPath}");
			_tutorialEquipmentDeckLoaded = true;
			return;
		}

		var lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			_tutorialEquipmentDeckLoaded = true;
			return;
		}

		// Parse header
		var header = lines[0].Split(',');
		var idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			var key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int CardIdsIdx = idx.ContainsKey("cardIds") ? idx["cardIds"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var values = line.Split(',');
			if (values.Length < 3) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			if (!int.TryParse(Get(LevelIdx), out int level)) continue;
			string type = Get(TypeIdx);
			if (string.IsNullOrEmpty(type)) continue;
			string cardIdsStr = Get(CardIdsIdx);

			List<string> cardIds = new List<string>();
			if (!string.IsNullOrWhiteSpace(cardIdsStr))
			{
				var parts = cardIdsStr.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					var id = part.Trim();
					if (!string.IsNullOrEmpty(id)) cardIds.Add(id);
				}
			}

			if (!_tutorialEquipmentDeckCache.ContainsKey(level))
			{
				_tutorialEquipmentDeckCache[level] = new Dictionary<string, List<string>>();
			}
			_tutorialEquipmentDeckCache[level][type] = cardIds;
		}

		_tutorialEquipmentDeckLoaded = true;
		Debug.Log($"Tutorial equipment deck loaded: {_tutorialEquipmentDeckCache.Count} levels");
	}

	public Dictionary<string, List<string>> GetTutorialEquipmentDeck(int level)
	{
		LoadTutorialEquipmentDeck();
		if (_tutorialEquipmentDeckCache.ContainsKey(level))
		{
			return _tutorialEquipmentDeckCache[level];
		}
		Debug.LogWarning($"Tutorial equipment deck for level {level} not found in CSV");
		return new Dictionary<string, List<string>>();
	}

	// ---------------- 教程关卡血量配置 ---------------- 
	public void LoadTutorialPlayerData(bool force = false)
	{
		if (!force && _tutorialPlayerDataLoaded) return;

		_tutorialPlayerDataCache.Clear();
		var ta = Resources.Load<TextAsset>(TutorialPlayerDataCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Tutorial player data CSV not found: {TutorialPlayerDataCsvPath}");
			_tutorialPlayerDataLoaded = true;
			return;
		}

		var lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			_tutorialPlayerDataLoaded = true;
			return;
		}

		// Parse header
		var header = lines[0].Split(',');
		var idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			var key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int HealthIdx = idx.ContainsKey("health") ? idx["health"] : -1;
		int MaxHealthIdx = idx.ContainsKey("maxHealth") ? idx["maxHealth"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var values = line.Split(',');
			if (values.Length < 3) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			if (!int.TryParse(Get(LevelIdx), out int level)) continue;
			if (!int.TryParse(Get(HealthIdx), out int health)) health = 30;
			if (!int.TryParse(Get(MaxHealthIdx), out int maxHealth)) maxHealth = 30;

			_tutorialPlayerDataCache[level] = (health, maxHealth);
		}

		_tutorialPlayerDataLoaded = true;
		Debug.Log($"Tutorial player data loaded: {_tutorialPlayerDataCache.Count} levels");
	}

	public (int health, int maxHealth)? GetTutorialPlayerData(int level)
	{
		LoadTutorialPlayerData();
		if (_tutorialPlayerDataCache.ContainsKey(level))
		{
			return _tutorialPlayerDataCache[level];
		}
		return null;
	}

	// ---------------- Roguelike角色数据 ---------------- 
	public void LoadRoguelikeCharacter(bool force = false)
	{
		if (!force && _roguelikeCharacterLoaded) return;

		RoguelikeCharacterList.Clear();
		var ta = Resources.Load<TextAsset>(RoguelikeCharacterCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Roguelike character CSV not found: {RoguelikeCharacterCsvPath}");
			_roguelikeCharacterLoaded = true;
			return;
		}

		var lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			_roguelikeCharacterLoaded = true;
			return;
		}

		// Parse header
		var header = lines[0].Split(',');
		var idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			var key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int CharacterIdx = idx.ContainsKey("character") ? idx["character"] : -1;
		int MaxHealthIdx = idx.ContainsKey("max_health") ? idx["max_health"] : -1;
		int EquipmentAttackIdx = idx.ContainsKey("equipment_attack") ? idx["equipment_attack"] : -1;
		int EquipmentDefenceIdx = idx.ContainsKey("equipment_defence") ? idx["equipment_defence"] : -1;
		int EquipmentItemIdx = idx.ContainsKey("equipment_item") ? idx["equipment_item"] : -1;
		int EquipmentSkillIdx = idx.ContainsKey("equipment_skill") ? idx["equipment_skill"] : -1;
		int EquipmentRelicIdx = idx.ContainsKey("equipment_relic") ? idx["equipment_relic"] : -1;
		int CoinsIdx = idx.ContainsKey("coins") ? idx["coins"] : -1;
		int CardDeckIdx = idx.ContainsKey("card_deck") ? idx["card_deck"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var values = ParseCSVLine(line);
			if (values == null || values.Length == 0) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			int GetInt(int index, int defaultValue = 0)
			{
				string value = Get(index);
				if (string.IsNullOrWhiteSpace(value)) return defaultValue;
				if (int.TryParse(value, out int result)) return result;
				return defaultValue;
			}

			List<string> ParseList(int index)
			{
				List<string> result = new List<string>();
				string value = Get(index);
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] parts = value.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < parts.Length; i++)
					{
						string id = parts[i].Trim();
						if (!string.IsNullOrEmpty(id)) result.Add(id);
					}
				}
				return result;
			}

			string character = Get(CharacterIdx);
			if (string.IsNullOrEmpty(character)) continue;

			var roguelikeCharacter = new RoguelikeCharacter();
			roguelikeCharacter.character = character;
			roguelikeCharacter.maxHealth = GetInt(MaxHealthIdx, 0);
			roguelikeCharacter.equipmentAttack = ParseList(EquipmentAttackIdx);
			roguelikeCharacter.equipmentDefence = ParseList(EquipmentDefenceIdx);
			roguelikeCharacter.equipmentItem = ParseList(EquipmentItemIdx);
			roguelikeCharacter.equipmentSkill = ParseList(EquipmentSkillIdx);
			roguelikeCharacter.equipmentRelic = ParseList(EquipmentRelicIdx);
			roguelikeCharacter.coins = GetInt(CoinsIdx, 0);
			roguelikeCharacter.cardDeck = ParseList(CardDeckIdx);

			RoguelikeCharacterList.Add(roguelikeCharacter);
		}

		_roguelikeCharacterLoaded = true;
		Debug.Log($"Roguelike character loaded: {RoguelikeCharacterList.Count} characters");
	}

	// ---------------- Roguelike关卡数据 ---------------- 
	public void LoadRoguelikeStage(bool force = false)
	{
		if (!force && _roguelikeStageLoaded) return;

		RoguelikeStageList.Clear();
		var ta = Resources.Load<TextAsset>(RoguelikeStageCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Roguelike stage CSV not found: {RoguelikeStageCsvPath}");
			_roguelikeStageLoaded = true;
			return;
		}

		var lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			_roguelikeStageLoaded = true;
			return;
		}

		// Parse header
		var header = lines[0].Split(',');
		var idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			var key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int StagesIdx = idx.ContainsKey("stages") ? idx["stages"] : -1;
		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			var values = ParseCSVLine(line);
			if (values == null || values.Length == 0) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			List<string> ParseList(int index)
			{
				List<string> result = new List<string>();
				string value = Get(index);
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] parts = value.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < parts.Length; i++)
					{
						string id = parts[i].Trim();
						if (!string.IsNullOrEmpty(id)) result.Add(id);
					}
				}
				return result;
			}

			string stages = Get(StagesIdx);
			if (string.IsNullOrEmpty(stages)) continue;

			var roguelikeStage = new RoguelikeStage();
			roguelikeStage.stages = stages;
			roguelikeStage.level = ParseList(LevelIdx);
			string typeStr = Get(TypeIdx);
			if (!string.IsNullOrEmpty(typeStr) && System.Enum.TryParse<EStageType>(typeStr, out EStageType stageType))
			{
				roguelikeStage.type = stageType;
			}
			else
			{
				roguelikeStage.type = EStageType.normal;
			}

			RoguelikeStageList.Add(roguelikeStage);
		}

		_roguelikeStageLoaded = true;
		Debug.Log($"Roguelike stage loaded: {RoguelikeStageList.Count} stages");
	}

	public RoguelikeCharacter GetRoguelikeCharacter(int index = 0)
	{
		LoadRoguelikeCharacter();
		if (index >= 0 && index < RoguelikeCharacterList.Count)
		{
			return RoguelikeCharacterList[index];
		}
		Debug.LogWarning($"Roguelike character index {index} out of range (count: {RoguelikeCharacterList.Count})");
		return null;
	}

	public RoguelikeStage GetRoguelikeStage(int index = 0)
	{
		LoadRoguelikeStage();
		if (index >= 0 && index < RoguelikeStageList.Count)
		{
			return RoguelikeStageList[index];
		}
		Debug.LogWarning($"Roguelike stage index {index} out of range (count: {RoguelikeStageList.Count})");
		return null;
	}

}