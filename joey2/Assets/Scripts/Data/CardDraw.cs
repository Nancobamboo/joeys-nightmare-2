using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public sealed class CardDraw : PureSingleton<CardDraw>
{
    private Dictionary<int, List<List<string>>> _tutorialEnvDeckCache = new Dictionary<int, List<List<string>>>();
    private bool _tutorialEnvDeckLoaded = false;
    private string TutorialEnvDeckCsvPath = "Data/tutorial_env_deck";

    private const int ENV_SLOT_COUNT = 5;
    private const int MIN_NON_MONSTER_TOP_CARDS = 3;
    private const string EXIT_CARD_ID = "6001";

    private void LoadTutorialEnvDeck()
    {
        if (_tutorialEnvDeckLoaded) return;

        _tutorialEnvDeckCache.Clear();
        var ta = Resources.Load<TextAsset>(TutorialEnvDeckCsvPath);
        if (ta == null)
        {
            Debug.LogWarning($"Tutorial env deck CSV not found: {TutorialEnvDeckCsvPath}");
            _tutorialEnvDeckLoaded = true;
            return;
        }

        var lines = ta.text.Split('\n');
        if (lines.Length <= 1)
        {
            _tutorialEnvDeckLoaded = true;
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
        int SlotIndexIdx = idx.ContainsKey("slotIndex") ? idx["slotIndex"] : -1;
        int CardIdsIdx = idx.ContainsKey("cardIds") ? idx["cardIds"] : -1;

        // Group by level
        var levelGroups = new Dictionary<int, List<(int slotIndex, List<string> cardIds)>>();

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
            if (!int.TryParse(Get(SlotIndexIdx), out int slotIndex)) continue;
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

            if (!levelGroups.ContainsKey(level))
            {
                levelGroups[level] = new List<(int, List<string>)>();
            }
            levelGroups[level].Add((slotIndex, cardIds));
        }

        // Convert to List<List<string>> format, sorted by slotIndex
        foreach (var kv in levelGroups)
        {
            int level = kv.Key;
            var slots = kv.Value.OrderBy(x => x.slotIndex).ToList();
            var deck = new List<List<string>>();
            foreach (var slot in slots)
            {
                deck.Add(slot.cardIds);
            }
            _tutorialEnvDeckCache[level] = deck;
        }

        _tutorialEnvDeckLoaded = true;
        Debug.Log($"Tutorial env deck loaded: {_tutorialEnvDeckCache.Count} levels");
    }

    public List<List<string>> DrawCardEnv(int level)
    {
        LoadTutorialEnvDeck();
        if (_tutorialEnvDeckCache.ContainsKey(level))
        {
            // Filter out EXIT_CARD_ID (6001) from the cached deck
            // KeyPath will automatically appear when all monsters are cleared
            List<List<string>> filteredDeck = new List<List<string>>();
            foreach (var column in _tutorialEnvDeckCache[level])
            {
                List<string> filteredColumn = column.Where(id => id != EXIT_CARD_ID).ToList();
                filteredDeck.Add(filteredColumn);
            }
            return filteredDeck;
        }

        Debug.LogWarning($"Tutorial env deck for level {level} not found in CSV, using default");
        return new List<List<string>>
        {
            new List<string> { "1001", "1002" },
            new List<string> { "2001", "2002" },
            new List<string> { "3001", "3002" },
            new List<string> { "4001", "4002" },
            new List<string> { "5001", "5002" }
        };
    }

    public List<string> GetEnvStageMonsters(int level)
    {
        EnvStage envStage = GData.Instance.GetEnvStage(level);
        if (envStage != null)
        {
            return new List<string>(envStage.monsterIds);
        }
        Debug.LogWarning($"Env stage for level {level} not found, using default monsters");
        return new List<string> { "5001", "5001", "5003" };
    }

    /// <summary>
    /// Draw cards for Env mode - randomly distribute player cards + monsters into 5 columns
    /// Ensures at least 3 non-monster cards are at the top positions of the 5 columns
    /// Exit card (KeyPath) will automatically appear when no monsters remain in the environment
    /// </summary>
    /// <param name="level">Current level</param>
    /// <param name="playerCardPool">Player's accumulated card pool (card IDs)</param>
    /// <returns>List of 5 columns, each containing card IDs (first=top, last=bottom due to reverse iteration in AddEnvCardList)</returns>
    public List<List<string>> DrawCardEnvMode(int level, List<string> playerCardPool)
    {
        List<string> monsters = GetEnvStageMonsters(level);
        List<string> nonMonsterCards = new List<string>(playerCardPool);

        // Shuffle both lists
        ShuffleList(monsters);
        ShuffleList(nonMonsterCards);

        // Initialize 5 columns
        List<List<string>> columns = new List<List<string>>();
        for (int i = 0; i < ENV_SLOT_COUNT; i++)
        {
            columns.Add(new List<string>());
        }

        // Step 1: Reserve non-monster cards for top positions (will be added first = top visually)
        List<int> topSlotIndices = Enumerable.Range(0, ENV_SLOT_COUNT).ToList();
        ShuffleList(topSlotIndices);

        int nonMonsterTopCount = Mathf.Min(MIN_NON_MONSTER_TOP_CARDS, nonMonsterCards.Count);
        List<int> nonMonsterTopSlots = topSlotIndices.Take(nonMonsterTopCount).ToList();

        // Track which cards are used for top positions
        List<string> topNonMonsterCards = new List<string>();
        for (int i = 0; i < nonMonsterTopCount && i < nonMonsterCards.Count; i++)
        {
            topNonMonsterCards.Add(nonMonsterCards[i]);
        }

        // Remove used cards from the pool
        List<string> remainingNonMonsterCards = nonMonsterCards.Skip(nonMonsterTopCount).ToList();

        // Step 2: Place the reserved non-monster cards at top (add them FIRST to the list)
        for (int i = 0; i < nonMonsterTopCount; i++)
        {
            int slotIndex = nonMonsterTopSlots[i];
            columns[slotIndex].Add(topNonMonsterCards[i]);
        }

        // Step 3: Combine remaining cards and shuffle
        List<string> remainingCards = new List<string>();
        remainingCards.AddRange(remainingNonMonsterCards);
        remainingCards.AddRange(monsters);
        ShuffleList(remainingCards);

        // Step 4: Distribute remaining cards randomly to columns
        foreach (string cardId in remainingCards)
        {
            int randomColumn = Random.Range(0, ENV_SLOT_COUNT);
            columns[randomColumn].Add(cardId);
        }

        // Note: Exit card (KeyPath) is no longer placed here.
        // It will automatically appear when no monsters remain in the environment.

        Debug.Log($"Env mode cards distributed: {ENV_SLOT_COUNT} columns, " +
                  $"{nonMonsterTopCount} non-monster top cards guaranteed, exit will appear when all monsters cleared");

        return columns;
    }

    /// <summary>
    /// Check if a card ID represents a monster card
    /// Monster cards have IDs starting with 5 (5xxx)
    /// </summary>
    private bool IsMonsterCard(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return false;
        return cardId.StartsWith("5");
    }

    /// <summary>
    /// Shuffle a list using Fisher-Yates algorithm
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

}
