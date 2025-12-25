using System.Collections.Generic;

public enum ETheme
{
	monkey,
	turkey,
	donkey,
	deadkey,
	tutorial
}

public class EnvStage
{
	public int level;
	public List<string> monsterIds = new List<string>(); // Base monster IDs
	public Dictionary<int, List<string>> difficultyMonsters = new Dictionary<int, List<string>>(); // Key: difficulty level, Value: additional monster IDs
	public EStageType type;
	public ETheme theme;

	public EnvStage()
	{
	}

	/// <summary>
	/// Get monster IDs for a specific difficulty level (cumulative)
	/// Difficulty is cumulative: returns base + all difficulty monsters up to the specified level
	/// </summary>
	public List<string> GetMonstersByDifficulty(int difficultyLevel)
	{
		List<string> result = new List<string>(monsterIds);
		
		// Add monsters from each difficulty level up to current level (cumulative)
		for (int i = 3; i <= difficultyLevel; i += 2) // 3, 5, 7, 9, etc.
		{
			if (difficultyMonsters.ContainsKey(i))
			{
				result.AddRange(difficultyMonsters[i]);
			}
		}
		
		return result;
	}
}

