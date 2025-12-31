using System.Collections.Generic;

public class StageReward
{
    public EStageType type;
    public bool hasCardSelect;
    public Dictionary<int, int> cardStarRates = new Dictionary<int, int>(); // key: star level, value: probability (0-100)
    public bool hasRelicSelect;
    public bool hasShop;

    public StageReward()
    {
    }

	/// <summary>
	/// Get a random star level based on the configured probabilities
	/// Applies difficulty penalty to reduce high-grade card probability
	/// </summary>
	/// <returns>Star level (1, 2, or 3)</returns>
	public int GetRandomStarLevel()
	{
		if (cardStarRates.Count == 0)
		{
			return 1; // Default to 1 star if no rates configured
		}

		// Get cumulative difficulty penalty for high-grade cards
		float highGradeReduction = GData.Instance.GetHighGradeCardProbabilityPenalty();

		// Build adjusted probability distribution
		Dictionary<int, int> adjustedRates = new Dictionary<int, int>();
		int totalAdjusted = 0;

		foreach (var kvp in cardStarRates)
		{
			int star = kvp.Key;
			int baseRate = kvp.Value;
			int adjustedRate = baseRate;

			// Apply penalty to high-grade cards (2-star and 3-star)
			if (star >= 2)
			{
				// Direct percentage point reduction
				// highGradeReduction is negative (e.g., -0.1 = -10 percentage points)
				// Convert to absolute value: -0.1 * 100 = -10 points
				int reductionPoints = UnityEngine.Mathf.RoundToInt(highGradeReduction * 100);
				adjustedRate = UnityEngine.Mathf.Max(0, baseRate + reductionPoints);
			}

			adjustedRates[star] = adjustedRate;
			totalAdjusted += adjustedRate;
		}

		// Normalize to ensure probabilities still sum to 100
		if (totalAdjusted != 100)
		{
			Dictionary<int, int> normalizedRates = new Dictionary<int, int>();
			int accumulated = 0;
			int lastStar = 1;

			foreach (var kvp in adjustedRates)
			{
				lastStar = kvp.Key;
				int normalizedRate = UnityEngine.Mathf.RoundToInt((float)kvp.Value * 100f / totalAdjusted);
				normalizedRates[kvp.Key] = normalizedRate;
				accumulated += normalizedRate;
			}

			// Adjust last rate to ensure sum equals 100
			if (normalizedRates.ContainsKey(lastStar))
			{
				normalizedRates[lastStar] += (100 - accumulated);
			}

			adjustedRates = normalizedRates;
		}

		// Select star level based on adjusted probabilities
		int random = UnityEngine.Random.Range(0, 100);
		int cumulative = 0;

		foreach (var kvp in adjustedRates)
		{
			cumulative += kvp.Value;
			if (random < cumulative)
			{
				return kvp.Key;
			}
		}

		// Fallback to the last star level
		int fallbackStar = 1;
		foreach (var kvp in adjustedRates)
		{
			fallbackStar = kvp.Key;
		}
		return fallbackStar;
	}
}

