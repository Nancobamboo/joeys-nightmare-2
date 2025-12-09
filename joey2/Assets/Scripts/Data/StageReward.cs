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
    /// </summary>
    /// <returns>Star level (1, 2, or 3)</returns>
    public int GetRandomStarLevel()
    {
        if (cardStarRates.Count == 0)
        {
            return 1; // Default to 1 star if no rates configured
        }

        int random = UnityEngine.Random.Range(0, 100);
        int cumulative = 0;

        foreach (var kvp in cardStarRates)
        {
            cumulative += kvp.Value;
            if (random < cumulative)
            {
                return kvp.Key;
            }
        }

        // Fallback to the last star level
        int lastStar = 1;
        foreach (var kvp in cardStarRates)
        {
            lastStar = kvp.Key;
        }
        return lastStar;
    }
}

