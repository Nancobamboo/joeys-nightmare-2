using System.Collections.Generic;

/// <summary>
/// Difficulty configuration for Env mode
/// Each difficulty level increases game challenge and unlocks new content
/// </summary>
public class DifficultyConfig
{
	public int difficultyLevel; // Difficulty level (1-8)
	public string description; // Description of what this difficulty unlocks/changes
	public string comment; // Detailed comment
	public string difficultyEffect; // Effect description (will be parsed)
	public int maxUnlockedStage; // Maximum stage level that this difficulty unlocks (0 = use default)

	// Parsed effects
	public int monsterAttackBonus; // Additional attack for monsters
	public int monsterHealthBonus; // Additional health for monsters
	public float shopPriceMultiplier; // Shop price multiplier (1.0 = normal, 1.2 = 20% more expensive)
	public float highGradeCardProbability; // Probability reduction for high-grade cards (-0.1 = -10%)
	public int playerHealthPenalty; // Player starting health penalty
	public int playerMaxHealthPenalty; // Player max health penalty
	public int playerAttackPenalty; // Player attack penalty
	public int playerDefencePenalty; // Player defence penalty
	public List<string> startingCurseCards; // Curse cards added to starting deck

	public DifficultyConfig()
	{
		startingCurseCards = new List<string>();
		shopPriceMultiplier = 1.0f;
		highGradeCardProbability = 0.0f;
		maxUnlockedStage = 0;
	}

	/// <summary>
	/// Parse difficulty effect string and populate effect fields
	/// Format: "key1:value1;key2:value2"
	/// Example: "monsterAttackBonus:1;monsterHealthBonus:5"
	/// </summary>
	public void ParseDifficultyEffect()
	{
		if (string.IsNullOrWhiteSpace(difficultyEffect))
		{
			return;
		}

		string effect = difficultyEffect.Trim();

		// Split by semicolon to get key-value pairs
		string[] pairs = effect.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

		foreach (string pair in pairs)
		{
			// Split by colon to get key and value
			string[] parts = pair.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) continue;

			string key = parts[0].Trim();
			string value = parts[1].Trim();

			// Parse based on key
			switch (key)
			{
				case "monsterAttackBonus":
					if (int.TryParse(value, out int attackBonus))
					{
						monsterAttackBonus = attackBonus;
					}
					break;

				case "monsterHealthBonus":
					if (int.TryParse(value, out int healthBonus))
					{
						monsterHealthBonus = healthBonus;
					}
					break;

				case "shopPriceMultiplier":
					if (float.TryParse(value, out float priceMultiplier))
					{
						shopPriceMultiplier = priceMultiplier;
					}
					break;

				case "highGradeCardProbability":
					if (float.TryParse(value, out float cardProbability))
					{
						highGradeCardProbability = cardProbability;
					}
					break;

				case "playerHealthPenalty":
					if (int.TryParse(value, out int healthPenalty))
					{
						playerHealthPenalty = healthPenalty;
					}
					break;

				case "playerMaxHealthPenalty":
					if (int.TryParse(value, out int maxHealthPenalty))
					{
						playerMaxHealthPenalty = maxHealthPenalty;
					}
					break;

				case "playerAttackPenalty":
					if (int.TryParse(value, out int attackPenalty))
					{
						playerAttackPenalty = attackPenalty;
					}
					break;

				case "playerDefencePenalty":
					if (int.TryParse(value, out int defencePenalty))
					{
						playerDefencePenalty = defencePenalty;
					}
					break;

				case "curseCards":
					// Support multiple curse cards separated by comma
					string[] cardIds = value.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
					foreach (string cardId in cardIds)
					{
						string id = cardId.Trim();
						if (!string.IsNullOrEmpty(id))
						{
							startingCurseCards.Add(id);
						}
					}
					break;

				case "maxUnlockedStage":
					if (int.TryParse(value, out int maxStage))
					{
						maxUnlockedStage = maxStage;
					}
					break;
			}
		}
	}
}

