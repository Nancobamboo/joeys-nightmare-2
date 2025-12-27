using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public sealed class CardDraw : PureSingleton<CardDraw>
{
    private Dictionary<int, List<List<string>>> _envDeckCache = new Dictionary<int, List<List<string>>>();
    private bool _envDeckLoaded = false;

    // Separated env deck config files
    private static readonly string[] EnvDeckCsvPaths = new string[]
    {
        "Data/tutorial_env_deck",   // Tutorial levels (1-5)
        "Data/battle_env_deck",     // Battle levels (100-115)
        "Data/debug_env_deck"       // Debug level (999)
    };

    private const int ENV_SLOT_COUNT = 5;
    private const int MIN_NON_MONSTER_TOP_CARDS = 3;
    private const string EXIT_CARD_ID = "6001";
    private const string ROMEO_MONKEY_ID = "5017";
    private const string JULIET_MONKEY_ID = "5018";

    private void LoadEnvDeck()
    {
        if (_envDeckLoaded) return;

        _envDeckCache.Clear();

        // Load all env deck files
        foreach (string path in EnvDeckCsvPaths)
        {
            LoadEnvDeckFile(path);
        }

        _envDeckLoaded = true;
        Debug.Log($"Env deck loaded: {_envDeckCache.Count} levels from {EnvDeckCsvPaths.Length} files");
    }

    private void LoadEnvDeckFile(string path)
    {
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogWarning($"Env deck CSV not found: {path}");
            return;
        }

        var lines = ta.text.Split('\n');
        if (lines.Length <= 1)
        {
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
            _envDeckCache[level] = deck;
        }
    }

    public List<List<string>> DrawCardEnv(int level)
    {
        LoadEnvDeck();
        if (_envDeckCache.ContainsKey(level))
        {
            // Filter out EXIT_CARD_ID (6001) from the cached deck
            // KeyPath will automatically appear when all monsters are cleared
            List<List<string>> filteredDeck = new List<List<string>>();
            foreach (var column in _envDeckCache[level])
            {
                List<string> filteredColumn = column.Where(id => id != EXIT_CARD_ID).ToList();
                filteredDeck.Add(filteredColumn);
            }
            return filteredDeck;
        }

        Debug.LogWarning($"Env deck for level {level} not found in CSV, using default");
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
            // Get current difficulty level from DataDifficulty system
            int difficultyLevel = DataSystem.Instance.GetCurrentDifficulty();
            
            // Get monsters for current difficulty (cumulative)
            List<string> monsters = envStage.GetMonstersByDifficulty(difficultyLevel);
            return monsters;
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
    /// <param name="cardLimit">Maximum number of cards to select from pool (0 = no limit)</param>
    /// <param name="randomSeed">Random seed for deterministic card arrangement (0 = use current random state)</param>
    /// <returns>List of 5 columns, each containing card IDs (first=top, last=bottom due to reverse iteration in AddEnvCardList)</returns>
    public List<List<string>> DrawCardEnvMode(int level, List<string> playerCardPool, int cardLimit = 0, int randomSeed = 0)
    {
        // Save current random state if using deterministic seed
        Random.State oldState = Random.state;
        bool useDeterministicSeed = randomSeed != 0;
        
        if (useDeterministicSeed)
        {
            Random.InitState(randomSeed);
            Debug.Log($"Using deterministic seed {randomSeed} for env card arrangement");
        }

        List<string> monsters = GetEnvStageMonsters(level);
        List<string> nonMonsterCards = new List<string>(playerCardPool);

        // Shuffle player cards first
        ShuffleList(nonMonsterCards);

        // Apply card limit if specified (limit > 0 means take at most 'limit' cards)
        if (cardLimit > 0 && nonMonsterCards.Count > cardLimit)
        {
            nonMonsterCards = nonMonsterCards.Take(cardLimit).ToList();
            Debug.Log($"Env mode: Limited player cards from pool to {cardLimit} cards (from {playerCardPool.Count} total)");
        }

        // Separate 3-star monsters from other monsters (3-star monsters should be placed at the bottom)
        List<string> threeStarMonsters = new List<string>();
        List<string> normalMonsters = new List<string>();
        
        // Check if both Romeo and Juliet are present in the monster list
        bool hasRomeo = monsters.Contains(ROMEO_MONKEY_ID);
        bool hasJuliet = monsters.Contains(JULIET_MONKEY_ID);
        
        foreach (string monsterId in monsters)
        {
            if (GData.Instance.CardDict.TryGetValue(monsterId, out Card card))
            {
                if (card.stars == 3)
                {
                    threeStarMonsters.Add(monsterId);
                }
                else
                {
                    normalMonsters.Add(monsterId);
                }
            }
            else
            {
                // If card not found, treat as normal monster
                normalMonsters.Add(monsterId);
            }
        }
        
        // Shuffle monsters
        ShuffleList(normalMonsters);
        ShuffleList(threeStarMonsters);

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

        // Step 3: Combine remaining cards and shuffle (excluding 3-star monsters)
        List<string> remainingCards = new List<string>();
        remainingCards.AddRange(remainingNonMonsterCards);
        remainingCards.AddRange(normalMonsters);
        ShuffleList(remainingCards);

        // Step 4: Distribute remaining cards - FIRST fill each empty column, THEN distribute randomly
        List<int> emptyColumnIndices = new List<int>();
        for (int i = 0; i < ENV_SLOT_COUNT; i++)
        {
            if (columns[i].Count == 0)
            {
                emptyColumnIndices.Add(i);
            }
        }
        
        // Shuffle empty column indices to randomize which columns get filled first
        ShuffleList(emptyColumnIndices);
        
        int cardIndex = 0;
        
        // First pass: Fill empty columns with one card each
        foreach (int colIndex in emptyColumnIndices)
        {
            if (cardIndex < remainingCards.Count)
            {
                columns[colIndex].Add(remainingCards[cardIndex]);
                cardIndex++;
            }
        }
        
        // Second pass: Distribute remaining cards randomly
        while (cardIndex < remainingCards.Count)
        {
            int randomColumn = Random.Range(0, ENV_SLOT_COUNT);
            columns[randomColumn].Add(remainingCards[cardIndex]);
            cardIndex++;
        }

        // Step 5: Add 3-star monsters to the BOTTOM of columns (last position to prevent blocking other cards)
        foreach (string threeStarMonsterId in threeStarMonsters)
        {
            int randomColumn = Random.Range(0, ENV_SLOT_COUNT);
            columns[randomColumn].Add(threeStarMonsterId);
        }

        // Step 6: Fix Romeo at column 1 and Juliet at column 3 (if both are present)
        if (hasRomeo && hasJuliet)
        {
            // Find and remove Romeo and Juliet from their current positions
            for (int col = 0; col < columns.Count; col++)
            {
                columns[col].RemoveAll(cardId => cardId == ROMEO_MONKEY_ID || cardId == JULIET_MONKEY_ID);
            }
            
            // Place Romeo at column 1 (index 1)
            columns[1].Add(ROMEO_MONKEY_ID);
            
            // Place Juliet at column 3 (index 3)
            columns[3].Add(JULIET_MONKEY_ID);
            
            Debug.Log($"Fixed positions: Romeo placed at column 1, Juliet placed at column 3");
        }

        // Note: Exit card (KeyPath) is no longer placed here.
        // It will automatically appear when no monsters remain in the environment.

        Debug.Log($"Env mode cards distributed: {ENV_SLOT_COUNT} columns, " +
                  $"{nonMonsterTopCount} non-monster top cards guaranteed, " +
                  $"{threeStarMonsters.Count} 3-star monsters placed at bottom, " +
                  $"exit will appear when all monsters cleared");

        // Restore previous random state if using deterministic seed
        if (useDeterministicSeed)
        {
            Random.state = oldState;
        }

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
