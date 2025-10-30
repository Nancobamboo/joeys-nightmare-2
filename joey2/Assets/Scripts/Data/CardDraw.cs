using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public sealed class CardDraw : PureSingleton<CardDraw>
{
    private Dictionary<int, List<List<string>>> _tutorialEnvDeckCache = new Dictionary<int, List<List<string>>>();
    private bool _tutorialEnvDeckLoaded = false;
    private string TutorialEnvDeckCsvPath = "Data/tutorial_env_deck";

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
        // Tutorial levels (1-3) use CSV config
        if (level >= 1 && level <= 3)
        {
            LoadTutorialEnvDeck();
            if (_tutorialEnvDeckCache.ContainsKey(level))
            {
                return _tutorialEnvDeckCache[level];
            }
            Debug.LogWarning($"Tutorial env deck for level {level} not found in CSV, using default");
        }
        
        // Normal levels use default deck (keep original logic for now)
        return new List<List<string>>
        {
            new List<string> { "1001", "1002" },
            new List<string> { "2001", "2002" },
            new List<string> { "3001", "3002" },
            new List<string> { "4001", "4002" },
            new List<string> { "5001", "5002" }
        };
    }

}
