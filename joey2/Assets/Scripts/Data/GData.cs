using System.Collections.Generic;
using System.IO;
using UnityEngine;


public sealed class GData : PureSingleton<GData>
{
	public Dictionary<string, Card> CardDict { get; private set; } = new Dictionary<string, Card>();
	public Dictionary<string, List<string>> DeckItemDict { get; private set; } = new Dictionary<string, List<string>>();

	public List<string> TempCardIdList { get; set; } = new List<string>();

	public List<RoguelikeCharacter> RoguelikeCharacterList { get; private set; } = new List<RoguelikeCharacter>();
	public List<RoguelikeStage> RoguelikeStageList { get; private set; } = new List<RoguelikeStage>();
	public List<EnvStage> EnvStageList { get; private set; } = new List<EnvStage>();

	public Dictionary<int, List<EquipmentUnlock>> EquipmentUnlockDict { get; private set; } = new Dictionary<int, List<EquipmentUnlock>>();
	public Dictionary<string, string> KeywordDict { get; private set; } = new Dictionary<string, string>();
	public Dictionary<int, RelicInfo> RelicInfoDict { get; private set; } = new Dictionary<int, RelicInfo>();
	public Dictionary<string, ERelicType> RelicNameToTypeDict { get; private set; } = new Dictionary<string, ERelicType>();
	public Dictionary<EStageType, StageReward> StageRewardDict { get; private set; } = new Dictionary<EStageType, StageReward>();
	public Dictionary<int, GrowthInfo> GrowthInfoDict { get; private set; } = new Dictionary<int, GrowthInfo>();

	private string m_CardCsvPath = "Data/card_info";
	private string m_DeckCsvPath = "Data/deck_data";
	private string m_RoguelikeCharacterCsvPath = "Data/roguelike_character";
	private string m_RoguelikeStageCsvPath = "Data/roguelike_stage";
	private string m_EquipmentUnlockCsvPath = "Data/equipment_unlock";
	private string m_KeywordCsvPath = "Data/keyword";
	private string m_RelicInfoCsvPath = "Data/relic_info";
	private string m_EnvStageCsvPath = "Data/env_stage";
	private string m_StageRewardCsvPath = "Data/stage_reward";
	private string m_GrowthCsvPath = "Data/growth";

	// Separated equipment deck config files
	private static readonly string[] m_EquipmentDeckCsvPaths = new string[]
	{
		"Data/tutorial_equipment_deck",  // Tutorial levels (1-5)
		"Data/debug_equipment_deck"      // Debug level (999)
	};

	// Separated player data config files
	private static readonly string[] m_PlayerDataCsvPaths = new string[]
	{
		"Data/tutorial_player_data",     // Tutorial levels (1-5)
		"Data/debug_player_data"         // Debug level (999)
	};

	private Dictionary<int, Dictionary<string, List<string>>> m_EquipmentDeckCache = new Dictionary<int, Dictionary<string, List<string>>>();
	private Dictionary<int, (int health, int maxHealth)> m_PlayerDataCache = new Dictionary<int, (int, int)>();
	private bool m_CardsLoaded = false;
	private bool m_DeckLoaded = false;
	private System.DateTime m_CardsMTime = System.DateTime.MinValue;
	private System.DateTime m_DeckMTime = System.DateTime.MinValue;

	public void LoadAll(bool force = false)
	{
		LoadCards();
		LoadDeck();
		LoadTutorialEquipmentDeck();
		LoadTutorialPlayerData();
		LoadRoguelikeCharacter();
		LoadRoguelikeStage();
		LoadEquipmentUnlock();
		LoadKeyword();
		LoadRelicInfo();
		LoadEnvStage();
		LoadStageReward();
		LoadGrowthInfo();
	}

	public void SaveAll()
	{
		SaveDeck();
	}

	public void LoadCards(bool force = false)
	{
		if (!force && m_CardsLoaded && !FileChanged(m_CardCsvPath, ref m_CardsMTime)) return;
		CardDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_CardCsvPath);
		string[] lines = ta.text.Split('\n');
		if (lines.Length == 0) { m_CardsLoaded = true; return; }
		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}
		int IdIdx = idx.ContainsKey("id") ? idx["id"] : -1;
		int CardImageIdx = idx.ContainsKey("cardImage") ? idx["cardImage"] : -1;
		int CardBackgroundIdx = idx.ContainsKey("cardBackground") ? idx["cardBackground"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int CardNameIdx = idx.ContainsKey("cardName") ? idx["cardName"] : -1;
		int DescriptionIdx = idx.ContainsKey("description") ? idx["description"] : -1;
		int AttackIdx = idx.ContainsKey("attack") ? idx["attack"] : -1;
		int DefenceIdx = idx.ContainsKey("defence") ? idx["defence"] : -1;
		int HealthIdx = idx.ContainsKey("health") ? idx["health"] : -1;
		int PriceIdx = idx.ContainsKey("price") ? idx["price"] : -1;
		int StarsIdx = idx.ContainsKey("stars") ? idx["stars"] : -1;
		int EffectIdsIdx = idx.ContainsKey("effectIds") ? idx["effectIds"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
			if (values == null || values.Length == 0 || string.IsNullOrEmpty(values[0])) continue;

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
				Debug.LogWarning($"[GData] 第 {i} 行无法解析整数值: '{value}', 使用默认值 {defaultValue}");
				return defaultValue;
			}

			string id = Get(IdIdx);
			string cardImage = Get(CardImageIdx);
			string cardBackground = Get(CardBackgroundIdx);
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
					string[] parts = eff.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length > 0)
					{
						effectId = parts[0].Trim();
					}
				}
			}

			Card card = new Card(id, type, cardImage, cardBackground, cardName, description, attack, defence, health, price, stars, effectId);

			// Initialize default durability for Knight Sword and Knight Shield
			if (effectId == "KnightSword_OnTop" || effectId == "KnightShield_OnTop")
			{
				card.durability = 1;
			}

			CardDict[id] = card;
		}
		m_CardsLoaded = true;
		Debug.Log("Cards loaded: " + CardDict.Count);
	}

	private string[] ParseCSVLine(string line)
	{
		List<string> values = new List<string>();
		bool inQuotes = false;
		string currentValue = "";

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];

			if (c == '"')
			{
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
		values.Add(currentValue);

		return values.ToArray();
	}

	public Card RandomCard()
	{
		if (CardDict.Count == 0) LoadCards();
		if (CardDict.Count == 0) return null;
		List<Card> values = new List<Card>(CardDict.Values);
		int idx = Random.Range(0, values.Count);
		return values[idx];
	}

	/// <summary>
	/// Get a random card with deterministic seed
	/// This ensures consistent random card selection when replaying the same level
	/// </summary>
	/// <param name="seed">Random seed for card selection</param>
	/// <returns>Random card</returns>
	public Card RandomCardWithSeed(int seed)
	{
		if (CardDict.Count == 0) LoadCards();
		if (CardDict.Count == 0) return null;

		// Save current random state
		Random.State oldState = Random.state;

		// Set deterministic seed
		Random.InitState(seed);

		// Get random card
		List<Card> values = new List<Card>(CardDict.Values);
		int idx = Random.Range(0, values.Count);
		Card result = values[idx];

		// Restore previous random state
		Random.state = oldState;

		return result;
	}

	public Card GetCardConfigById(string cardId)
	{
		return CardDict[cardId];
	}

	public void LoadDeck(bool force = false)
	{
		if (!force && m_DeckLoaded && !FileChanged(m_DeckCsvPath, ref m_DeckMTime)) return;
		DeckItemDict = LoadTypeListCsv(m_DeckCsvPath);
		Debug.Log($"DeckItemDict: {DeckItemDict.Count}");
		m_DeckLoaded = true;
	}

	public void SaveDeck()
	{
		SaveTypeListCsv(m_DeckCsvPath, DeckItemDict);
	}

	private Dictionary<string, List<string>> LoadTypeListCsv(string path)
	{
		Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
		string[] lines = Resources.Load<TextAsset>(path).text.Split('\n');
		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			string[] values = line.Split(',');
			if (string.IsNullOrEmpty(values[0])) continue;
			if (values.Length < 2) continue;
			string id = values[0].Trim();
			string type = values[1].Trim();

			if (!dict.ContainsKey(type))
			{
				dict[type] = new List<string>();
			}
			dict[type].Add(id);
		}
		return dict;
	}

	private void SaveTypeListCsv(string path, Dictionary<string, List<string>> data)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			List<string> lines = new List<string>();
			lines.Add("id,type");
			List<string> keys = new List<string>(data.Keys);
			for (int j = 0; j < keys.Count; j++)
			{
				string type = keys[j];
				List<string> ids = data[type];
				if (ids == null) continue;
				for (int i = 0; i < ids.Count; i++)
				{
					string id = ids[i];
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
		System.DateTime t = SafeGetWriteTime(path);
		if (t != cachedTime)
		{
			cachedTime = t;
			return true;
		}
		return false;
	}

	private System.DateTime SafeGetWriteTime(string path)
	{
		return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : System.DateTime.MinValue;
	}

	public void LoadTutorialEquipmentDeck(bool force = false)
	{
		if (!force && m_EquipmentDeckCache.Count > 0) return;

		m_EquipmentDeckCache.Clear();

		// Load all equipment deck files
		foreach (string path in m_EquipmentDeckCsvPaths)
		{
			LoadEquipmentDeckFile(path);
		}

		Debug.Log($"Equipment deck loaded: {m_EquipmentDeckCache.Count} levels from {m_EquipmentDeckCsvPaths.Length} files");
	}

	private void LoadEquipmentDeckFile(string path)
	{
		TextAsset ta = Resources.Load<TextAsset>(path);
		if (ta == null)
		{
			Debug.LogWarning($"Equipment deck CSV not found: {path}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int CardIdsIdx = idx.ContainsKey("cardIds") ? idx["cardIds"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = line.Split(',');
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
				string[] parts = cardIdsStr.Split(new char[] { ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < parts.Length; j++)
				{
					string id = parts[j].Trim();
					if (!string.IsNullOrEmpty(id)) cardIds.Add(id);
				}
			}

			if (!m_EquipmentDeckCache.ContainsKey(level))
			{
				m_EquipmentDeckCache[level] = new Dictionary<string, List<string>>();
			}
			m_EquipmentDeckCache[level][type] = cardIds;
		}
	}

	public Dictionary<string, List<string>> GetTutorialEquipmentDeck(int level)
	{
		LoadTutorialEquipmentDeck();
		if (m_EquipmentDeckCache.ContainsKey(level))
		{
			return m_EquipmentDeckCache[level];
		}
		Debug.LogWarning($"Equipment deck for level {level} not found in CSV");
		return new Dictionary<string, List<string>>();
	}

	public void LoadTutorialPlayerData(bool force = false)
	{
		if (!force && m_PlayerDataCache.Count > 0) return;

		m_PlayerDataCache.Clear();

		// Load all player data files
		foreach (string path in m_PlayerDataCsvPaths)
		{
			LoadPlayerDataFile(path);
		}

		Debug.Log($"Player data loaded: {m_PlayerDataCache.Count} levels from {m_PlayerDataCsvPaths.Length} files");
	}

	private void LoadPlayerDataFile(string path)
	{
		TextAsset ta = Resources.Load<TextAsset>(path);
		if (ta == null)
		{
			Debug.LogWarning($"Player data CSV not found: {path}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int HealthIdx = idx.ContainsKey("health") ? idx["health"] : -1;
		int MaxHealthIdx = idx.ContainsKey("maxHealth") ? idx["maxHealth"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = line.Split(',');
			if (values.Length < 3) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			if (!int.TryParse(Get(LevelIdx), out int level)) continue;
			if (!int.TryParse(Get(HealthIdx), out int health)) health = 30;
			if (!int.TryParse(Get(MaxHealthIdx), out int maxHealth)) maxHealth = 30;

			m_PlayerDataCache[level] = (health, maxHealth);
		}
	}

	public (int health, int maxHealth)? GetTutorialPlayerData(int level)
	{
		LoadTutorialPlayerData();
		if (m_PlayerDataCache.ContainsKey(level))
		{
			return m_PlayerDataCache[level];
		}
		return null;
	}

	public void LoadRoguelikeCharacter(bool force = false)
	{
		if (!force && RoguelikeCharacterList.Count > 0) return;

		RoguelikeCharacterList.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_RoguelikeCharacterCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Roguelike character CSV not found: {m_RoguelikeCharacterCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
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
		int EnvCardLimitIdx = idx.ContainsKey("env_card_limit") ? idx["env_card_limit"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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
					for (int j = 0; j < parts.Length; j++)
					{
						string id = parts[j].Trim();
						if (!string.IsNullOrEmpty(id)) result.Add(id);
					}
				}
				return result;
			}

			string character = Get(CharacterIdx);
			if (string.IsNullOrEmpty(character)) continue;

			RoguelikeCharacter roguelikeCharacter = new RoguelikeCharacter();
			roguelikeCharacter.character = character;
			roguelikeCharacter.maxHealth = GetInt(MaxHealthIdx, 0);
			roguelikeCharacter.equipmentAttack = ParseList(EquipmentAttackIdx);
			roguelikeCharacter.equipmentDefence = ParseList(EquipmentDefenceIdx);
			roguelikeCharacter.equipmentItem = ParseList(EquipmentItemIdx);
			roguelikeCharacter.equipmentSkill = ParseList(EquipmentSkillIdx);
			roguelikeCharacter.equipmentRelic = ParseList(EquipmentRelicIdx);
			roguelikeCharacter.coins = GetInt(CoinsIdx, 0);
			roguelikeCharacter.cardDeck = ParseList(CardDeckIdx);
			roguelikeCharacter.envCardLimit = GetInt(EnvCardLimitIdx, 0); // 0 means no limit

			RoguelikeCharacterList.Add(roguelikeCharacter);
		}

		Debug.Log($"Roguelike character loaded: {RoguelikeCharacterList.Count} characters");
	}

	public void LoadRoguelikeStage(bool force = false)
	{
		if (!force && RoguelikeStageList.Count > 0) return;

		RoguelikeStageList.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_RoguelikeStageCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Roguelike stage CSV not found: {m_RoguelikeStageCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int StagesIdx = idx.ContainsKey("stages") ? idx["stages"] : -1;
		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int ThemeIdx = idx.ContainsKey("theme") ? idx["theme"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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
					for (int j = 0; j < parts.Length; j++)
					{
						string id = parts[j].Trim();
						if (!string.IsNullOrEmpty(id)) result.Add(id);
					}
				}
				return result;
			}

			string stages = Get(StagesIdx);
			if (string.IsNullOrEmpty(stages)) continue;

			RoguelikeStage roguelikeStage = new RoguelikeStage();
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
			string themeStr = Get(ThemeIdx);
			if (!string.IsNullOrEmpty(themeStr) && System.Enum.TryParse<ETheme>(themeStr, true, out ETheme theme))
			{
				roguelikeStage.theme = theme;
			}
			else
			{
				roguelikeStage.theme = ETheme.monkey;
			}

			RoguelikeStageList.Add(roguelikeStage);
		}

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

	public void LoadEnvStage(bool force = false)
	{
		if (!force && EnvStageList.Count > 0) return;

		EnvStageList.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_EnvStageCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Env stage CSV not found: {m_EnvStageCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int LevelIdx = idx.ContainsKey("level") ? idx["level"] : -1;
		int MonsterIdsIdx = idx.ContainsKey("monster_ids") ? idx["monster_ids"] : -1;
		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int ThemeIdx = idx.ContainsKey("theme") ? idx["theme"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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
					for (int j = 0; j < parts.Length; j++)
					{
						string id = parts[j].Trim();
						if (!string.IsNullOrEmpty(id)) result.Add(id);
					}
				}
				return result;
			}

			if (!int.TryParse(Get(LevelIdx), out int level)) continue;

			EnvStage envStage = new EnvStage();
			envStage.level = level;
			envStage.monsterIds = ParseList(MonsterIdsIdx);
			string typeStr = Get(TypeIdx);
			if (!string.IsNullOrEmpty(typeStr) && System.Enum.TryParse<EStageType>(typeStr, true, out EStageType stageType))
			{
				envStage.type = stageType;
			}
			else
			{
				envStage.type = EStageType.normal;
			}
			string themeStr = Get(ThemeIdx);
			if (!string.IsNullOrEmpty(themeStr) && System.Enum.TryParse<ETheme>(themeStr, true, out ETheme theme))
			{
				envStage.theme = theme;
			}
			else
			{
				envStage.theme = ETheme.monkey;
			}

			EnvStageList.Add(envStage);
		}

		Debug.Log($"Env stage loaded: {EnvStageList.Count} stages");
	}

	public EnvStage GetEnvStage(int index)
	{
		LoadEnvStage();
		if (index >= 0 && index < EnvStageList.Count)
		{
			return EnvStageList[index];
		}
		Debug.LogWarning($"Env stage index {index} out of range (count: {EnvStageList.Count})");
		return null;
	}

	public EStageType GetEnvStageType(int index)
	{
		EnvStage envStage = GetEnvStage(index);
		if (envStage != null)
		{
			return envStage.type;
		}
		return EStageType.normal;
	}

	public void LoadEquipmentUnlock(bool force = false)
	{
		if (!force && EquipmentUnlockDict.Count > 0) return;

		EquipmentUnlockDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_EquipmentUnlockCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Equipment unlock CSV not found: {m_EquipmentUnlockCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int IndexIdx = idx.ContainsKey("index") ? idx["index"] : -1;
		int CostIdx = idx.ContainsKey("cost") ? idx["cost"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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

			string typeStr = Get(TypeIdx);
			int index = GetInt(IndexIdx, -1);
			int cost = GetInt(CostIdx, 0);

			if (string.IsNullOrEmpty(typeStr) || index < 0) continue;

			if (System.Enum.TryParse<ECardType>(typeStr, true, out ECardType cardType))
			{
				EquipmentUnlock equipmentUnlock = new EquipmentUnlock();
				equipmentUnlock.id = "";
				equipmentUnlock.type = cardType;
				equipmentUnlock.cost = cost;

				int cardTypeInt = (int)cardType;
				if (!EquipmentUnlockDict.ContainsKey(cardTypeInt))
				{
					EquipmentUnlockDict[cardTypeInt] = new List<EquipmentUnlock>();
				}
				while (EquipmentUnlockDict[cardTypeInt].Count <= index)
				{
					EquipmentUnlockDict[cardTypeInt].Add(null);
				}
				EquipmentUnlockDict[cardTypeInt][index] = equipmentUnlock;
			}
		}

		Debug.Log($"Equipment unlock loaded: {EquipmentUnlockDict.Count} card types");
	}

	public EquipmentUnlock GetEquipmentUnlock(int cardType, int index)
	{
		LoadEquipmentUnlock();
		if (EquipmentUnlockDict.TryGetValue(cardType, out List<EquipmentUnlock> unlockList))
		{
			if (index >= 0 && index < unlockList.Count)
			{
				return unlockList[index];
			}
		}
		return null;
	}

	public void LoadKeyword(bool force = false)
	{
		if (!force && KeywordDict.Count > 0) return;

		KeywordDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_KeywordCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Keyword CSV not found: {m_KeywordCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int KeywordIdx = idx.ContainsKey("keyword") ? idx["keyword"] : -1;
		int DescriptionIdx = idx.ContainsKey("description") ? idx["description"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
			if (values == null || values.Length == 0) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			string keyword = Get(KeywordIdx);
			string description = Get(DescriptionIdx);

			if (string.IsNullOrEmpty(keyword)) continue;

			keyword = keyword.Trim();
			description = description.Trim();
			KeywordDict[keyword] = description;
		}

		Debug.Log($"Keyword loaded: {KeywordDict.Count} keywords");
	}

	public void LoadRelicInfo(bool force = false)
	{
		if (!force && RelicInfoDict.Count > 0) return;

		RelicInfoDict.Clear();
		RelicNameToTypeDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_RelicInfoCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Relic info CSV not found: {m_RelicInfoCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int IdIdx = idx.ContainsKey("id") ? idx["id"] : -1;
		int CardImageIdx = idx.ContainsKey("cardImage") ? idx["cardImage"] : -1;
		int NameIdx = idx.ContainsKey("name") ? idx["name"] : -1;
		int iconImageIdx = idx.ContainsKey("iconImage") ? idx["iconImage"] : -1;
		int starsIdx = idx.ContainsKey("stars") ? idx["stars"] : -1;
		int DescriptionIdx = idx.ContainsKey("description") ? idx["description"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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

			int id = GetInt(IdIdx, 0);
			if (id == 0) continue;

			string cardImage = Get(CardImageIdx);
			string iconImage = Get(iconImageIdx);
			string name = Get(NameIdx);
			string description = Get(DescriptionIdx);
			int stars = GetInt(starsIdx, 0);
			RelicInfo relicInfo = new RelicInfo(id, cardImage, iconImage, name, description, stars);
			RelicInfoDict[id] = relicInfo;

			// Build name to enum mapping and validate ID consistency
			if (System.Enum.IsDefined(typeof(ERelicType), id))
			{
				ERelicType relicType = (ERelicType)id;
				RelicNameToTypeDict[name] = relicType;
			}
			else
			{
				Debug.LogWarning($"Relic ID {id} ({name}) not found in ERelicType enum");
			}
		}

		// Validate enum vs CSV consistency
		ValidateRelicEnumConsistency();

		Debug.Log($"Relic info loaded: {RelicInfoDict.Count} relics, {RelicNameToTypeDict.Count} name mappings");
	}

	/// <summary>
	/// Validate that all ERelicType enum values have corresponding CSV entries
	/// </summary>
	private void ValidateRelicEnumConsistency()
	{
		foreach (ERelicType relicType in System.Enum.GetValues(typeof(ERelicType)))
		{
			int id = (int)relicType;
			// Skip the special CardLimitDebuff enum
			if (id == 9999) continue;

			if (!RelicInfoDict.ContainsKey(id))
			{
				Debug.LogWarning($"ERelicType.{relicType} ({id}) is defined in code but missing in relic_info.csv");
			}
		}
	}

	/// <summary>
	/// Get ERelicType by relic name from CSV
	/// </summary>
	public ERelicType? GetRelicTypeByName(string name)
	{
		LoadRelicInfo();
		if (RelicNameToTypeDict.TryGetValue(name, out ERelicType relicType))
		{
			return relicType;
		}
		return null;
	}

	public RelicInfo GetRelicInfo(ERelicType relicType)
	{
		LoadRelicInfo();
		int id = (int)relicType;
		if (RelicInfoDict.TryGetValue(id, out RelicInfo relicInfo))
		{
			return relicInfo;
		}
		return null;
	}

	public string GetKeyword(string keyword)
	{
		LoadKeyword();
		if (KeywordDict.TryGetValue(keyword, out string description))
		{
			return description;
		}
		return null;
	}

	public List<string> CheckKeywordInDescription(string description)
	{
		//Debug.Log("CheckKeywordInDescription: " + description);
		List<string> result = new List<string>();
		if (string.IsNullOrEmpty(description))
		{
			return result;
		}

		foreach (KeyValuePair<string, string> kvp in KeywordDict)
		{
			string keyword = kvp.Key;
			if (description.Contains(keyword))
			{
				result.Add(keyword + "：" + kvp.Value);
			}
		}

		foreach (string r in result)
		{
			Debug.Log("CheckKeywordInDescription result: " + r);
		}
		return result;
	}

	public void LoadStageReward(bool force = false)
	{
		if (!force && StageRewardDict.Count > 0) return;

		StageRewardDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_StageRewardCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Stage reward CSV not found: {m_StageRewardCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int TypeIdx = idx.ContainsKey("type") ? idx["type"] : -1;
		int HasCardSelectIdx = idx.ContainsKey("has_card_select") ? idx["has_card_select"] : -1;
		int CardStarRatesIdx = idx.ContainsKey("card_star_rates") ? idx["card_star_rates"] : -1;
		int HasRelicSelectIdx = idx.ContainsKey("has_relic_select") ? idx["has_relic_select"] : -1;
		int HasShopIdx = idx.ContainsKey("has_shop") ? idx["has_shop"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
			if (values == null || values.Length == 0) continue;

			string Get(int index)
			{
				if (index < 0 || index >= values.Length) return string.Empty;
				return values[index].Trim();
			}

			bool GetBool(int index, bool defaultValue = false)
			{
				string value = Get(index).ToLower();
				if (string.IsNullOrWhiteSpace(value)) return defaultValue;
				return value == "true" || value == "1" || value == "yes";
			}

			string typeStr = Get(TypeIdx);
			if (string.IsNullOrEmpty(typeStr)) continue;

			if (!System.Enum.TryParse<EStageType>(typeStr, true, out EStageType stageType))
			{
				Debug.LogWarning($"Invalid stage type: {typeStr}");
				continue;
			}

			StageReward stageReward = new StageReward();
			stageReward.type = stageType;
			stageReward.hasCardSelect = GetBool(HasCardSelectIdx, false);
			stageReward.hasRelicSelect = GetBool(HasRelicSelectIdx, false);
			stageReward.hasShop = GetBool(HasShopIdx, false);

			// Parse card star rates (format: "1:60;2:30;3:10")
			string cardStarRatesStr = Get(CardStarRatesIdx);
			if (!string.IsNullOrWhiteSpace(cardStarRatesStr))
			{
				string[] ratePairs = cardStarRatesStr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (string ratePair in ratePairs)
				{
					string[] parts = ratePair.Split(':');
					if (parts.Length == 2)
					{
						if (int.TryParse(parts[0].Trim(), out int star) && int.TryParse(parts[1].Trim(), out int rate))
						{
							stageReward.cardStarRates[star] = rate;
						}
					}
				}
			}

			StageRewardDict[stageType] = stageReward;
		}

		Debug.Log($"Stage reward loaded: {StageRewardDict.Count} stage types");
	}

	public StageReward GetStageReward(EStageType stageType)
	{
		LoadStageReward();
		if (StageRewardDict.TryGetValue(stageType, out StageReward stageReward))
		{
			return stageReward;
		}
		Debug.LogWarning($"Stage reward not found for type: {stageType}");
		return null;
	}

	public void LoadGrowthInfo(bool force = false)
	{
		if (!force && GrowthInfoDict.Count > 0) return;

		GrowthInfoDict.Clear();
		TextAsset ta = Resources.Load<TextAsset>(m_GrowthCsvPath);
		if (ta == null)
		{
			Debug.LogWarning($"Growth CSV not found: {m_GrowthCsvPath}");
			return;
		}

		string[] lines = ta.text.Split('\n');
		if (lines.Length <= 1)
		{
			return;
		}

		string[] header = lines[0].Split(',');
		Dictionary<string, int> idx = new Dictionary<string, int>();
		for (int i = 0; i < header.Length; i++)
		{
			string key = header[i].Trim();
			if (!idx.ContainsKey(key)) idx[key] = i;
		}

		int IdIdx = idx.ContainsKey("id") ? idx["id"] : -1;
		int NameIdx = idx.ContainsKey("name") ? idx["name"] : -1;
		int DependIdx = idx.ContainsKey("dependency") ? idx["dependency"] : -1;
		int DescIdx = idx.ContainsKey("desc") ? idx["desc"] : -1;
		int PriceIdx = idx.ContainsKey("price") ? idx["price"] : -1;

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;

			string[] values = ParseCSVLine(line);
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

			int id = GetInt(IdIdx, -1);
			if (id < 0) continue;

			string name = Get(NameIdx);
			int depend = GetInt(DependIdx, -1);
			string desc = Get(DescIdx);
			int price = GetInt(PriceIdx, 0);

			GrowthInfo growthInfo = new GrowthInfo(id, name, depend, desc, price);
			GrowthInfoDict[id] = growthInfo;
		}

		Debug.Log($"Growth info loaded: {GrowthInfoDict.Count} nodes");
	}

	public GrowthInfo GetGrowthInfo(int id)
	{
		LoadGrowthInfo();
		if (GrowthInfoDict.TryGetValue(id, out GrowthInfo growthInfo))
		{
			return growthInfo;
		}
		return null;
	}

}
