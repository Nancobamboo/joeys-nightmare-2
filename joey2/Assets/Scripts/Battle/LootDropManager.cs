using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LootDropManager : MonoSingleton<LootDropManager>
{
    private class DropOption
    {
        public float weight;
        public int count;
    }

    private class DropBundle
    {
        public List<DropOption> options = new List<DropOption>();
        public List<string> cardPool = new List<string>();
    }

    private readonly Dictionary<string, DropBundle> _dropTable = new Dictionary<string, DropBundle>();
    private bool _loaded;
    private const string DropCsvPath = "Data/monster_drop";

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        LoadConfig();
        _loaded = true;
    }

    private void LoadConfig()
    {
        _dropTable.Clear();
        var ta = Resources.Load<TextAsset>(DropCsvPath);
        if (ta == null)
        {
            Debug.LogWarning($"LootDropManager: Drop config not found at Resources/{DropCsvPath}");
            return;
        }

        var lines = ta.text.Split('\n');
        if (lines.Length <= 1) return;

        var header = lines[0].Split(',');
        var idx = new Dictionary<string, int>();
        for (int i = 0; i < header.Length; i++)
        {
            var key = header[i].Trim();
            if (!idx.ContainsKey(key) && !string.IsNullOrEmpty(key)) idx[key] = i;
        }

        int monsterIdIdx = idx.ContainsKey("monsterId") ? idx["monsterId"] : -1;
        int weightsIdx = idx.ContainsKey("weights") ? idx["weights"] : -1;
        int countsIdx = idx.ContainsKey("counts") ? idx["counts"] : -1;
        int cardsIdx = idx.ContainsKey("cardPool") ? idx["cardPool"] : -1;

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);
            if (values == null || values.Length == 0) continue;

            string Get(int index)
            {
                if (index < 0 || index >= values.Length) return string.Empty;
                return values[index].Trim();
            }

            string monsterId = Get(monsterIdIdx);
            if (string.IsNullOrEmpty(monsterId)) continue;

            var weightsRaw = Get(weightsIdx);
            var countsRaw = Get(countsIdx);
            var cardsRaw = Get(cardsIdx);

            var bundle = new DropBundle();

            if (!string.IsNullOrEmpty(weightsRaw) && !string.IsNullOrEmpty(countsRaw))
            {
                var weightParts = weightsRaw.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                var countParts = countsRaw.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (weightParts.Length != countParts.Length)
                {
                    Debug.LogWarning($"LootDropManager: weights/counts length mismatch for monster '{monsterId}', skipping drop set.");
                }
                else
                {
                    for (int j = 0; j < weightParts.Length; j++)
                    {
                        var wRaw = weightParts[j].Trim();
                        var cRaw = countParts[j].Trim();

                        if (string.IsNullOrEmpty(wRaw) || string.IsNullOrEmpty(cRaw)) continue;

                        if (!float.TryParse(wRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float weight) || weight <= 0f)
                        {
                            Debug.LogWarning($"LootDropManager: Invalid weight '{wRaw}' for monster '{monsterId}', entry {j}.");
                            continue;
                        }

                        if (!int.TryParse(cRaw, out int count) || count <= 0)
                        {
                            Debug.LogWarning($"LootDropManager: Invalid count '{cRaw}' for monster '{monsterId}', entry {j}.");
                            continue;
                        }

                        bundle.options.Add(new DropOption { weight = weight, count = count });
                    }
                }
            }
            else
            {
                Debug.LogWarning($"LootDropManager: Missing weights or counts for monster '{monsterId}'.");
            }

            if (!string.IsNullOrEmpty(cardsRaw))
            {
                var cardParts = cardsRaw.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in cardParts)
                {
                    var id = part.Trim();
                    if (!string.IsNullOrEmpty(id)) bundle.cardPool.Add(id);
                }
            }
            else
            {
                Debug.LogWarning($"LootDropManager: Missing card pool for monster '{monsterId}'.");
            }

            if (bundle.options.Count == 0 || bundle.cardPool.Count == 0)
            {
                continue;
            }

            _dropTable[monsterId] = bundle;
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        bool inQuotes = false;
        string currentValue = string.Empty;

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
                currentValue = string.Empty;
            }
            else
            {
                currentValue += c;
            }
        }

        values.Add(currentValue);
        return values.ToArray();
    }

    public bool TryGetDropCards(string monsterId, out List<string> cardIds)
    {
        EnsureLoaded();
        cardIds = null;

        if (string.IsNullOrEmpty(monsterId)) return false;
        if (!_dropTable.TryGetValue(monsterId, out var bundle) || bundle == null || bundle.options.Count == 0) return false;
        if (bundle.cardPool == null || bundle.cardPool.Count == 0) return false;

        float totalWeight = 0f;
        for (int i = 0; i < bundle.options.Count; i++)
        {
            totalWeight += Mathf.Max(0f, bundle.options[i].weight);
        }

        if (totalWeight <= 0f) return false;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var option in bundle.options)
        {
            cumulative += Mathf.Max(0f, option.weight);
            if (roll <= cumulative)
            {
                cardIds = DrawCards(option, bundle.cardPool);
                return cardIds != null && cardIds.Count > 0;
            }
        }

        var fallback = bundle.options[bundle.options.Count - 1];
        cardIds = DrawCards(fallback, bundle.cardPool);
        return cardIds != null && cardIds.Count > 0;
    }

    public bool HandleMonsterDrop(string monsterId, int centerIndex)
    {
        // Drop to environment panels and insert at the visual front
        if (!TryGetDropCards(monsterId, out var cardIds) || cardIds == null || cardIds.Count == 0)
        {
            return false;
        }

        var battle = BattleManager.Instance;
        if (battle == null)
        {
            Debug.LogWarning("LootDropManager: BattleManager.Instance is null, cannot spawn drops.");
            return false;
        }

        if (battle.envPanels == null || battle.envPanels.Count == 0)
        {
            Debug.LogWarning("LootDropManager: envPanels is empty, cannot spawn drops.");
            return false;
        }

        centerIndex = Mathf.Clamp(centerIndex, 0, battle.envPanels.Count - 1);

        var usedIndices = new HashSet<int>();
        var touchedPanels = new HashSet<int>();
        var offsetPattern = BuildOffsetPattern(battle.envPanels.Count);

        for (int i = 0; i < cardIds.Count; i++)
        {
            int panelIndex = FindPanelIndex(centerIndex, usedIndices, offsetPattern, battle.envPanels.Count);
            if (panelIndex < 0)
            {
                panelIndex = centerIndex;
            }

            var panel = battle.envPanels[panelIndex];
            var attachList = EnsureEnvAttachList(battle, panelIndex);

            var go = CardHelper.CreateCardToTransform(
                cardPrefab: battle.cardPrefab,
                parent: panel,
                cardId: cardIds[i],
                state: CardState.Inactive,
                position: CardPosition.Env,
                attachList: attachList);

            if (go != null)
            {
                // For environment, make sure it appears at the visual front
                go.transform.SetSiblingIndex(panel.childCount - 1);
            }

            usedIndices.Add(panelIndex);
            touchedPanels.Add(panelIndex);
        }

        foreach (var index in touchedPanels)
        {
            UIGridHelper.RefreshPanel(battle.envPanels[index]);
        }

        return true;
    }

    private List<int> BuildOffsetPattern(int panelCount)
    {
        var offsets = new List<int>();
        offsets.Add(0);
        int step = 1;
        while (offsets.Count < panelCount * 2)
        {
            offsets.Add(-step);
            offsets.Add(step);
            step++;
        }
        return offsets;
    }

    private int FindPanelIndex(int centerIndex, HashSet<int> usedIndices, List<int> offsetPattern, int panelCount)
    {
        for (int i = 0; i < offsetPattern.Count; i++)
        {
            int candidate = centerIndex + offsetPattern[i];
            if (candidate < 0 || candidate >= panelCount) continue;
            if (usedIndices.Contains(candidate)) continue;
            return candidate;
        }

        // No unique panel found
        return Mathf.Clamp(centerIndex, 0, panelCount - 1);
    }

    private List<string> DrawCards(DropOption option, List<string> pool)
    {
        if (option == null || pool == null || pool.Count == 0)
        {
            return null;
        }

        int poolCount = pool.Count;
        if (poolCount == 0) return null;

        int pickCount = Mathf.Max(1, option.count);
        var result = new List<string>(pickCount);

        if (poolCount >= pickCount)
        {
            var indices = new List<int>(poolCount);
            for (int i = 0; i < poolCount; i++)
            {
                indices.Add(i);
            }

            for (int i = indices.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[swapIndex];
                indices[swapIndex] = temp;
            }

            for (int i = 0; i < pickCount && i < indices.Count; i++)
            {
                result.Add(pool[indices[i]]);
            }
        }
        else
        {
            Debug.LogWarning($"LootDropManager: Drop count {option.count} exceeds pool size {poolCount}, sampling with replacement.");
            for (int i = 0; i < pickCount; i++)
            {
                int idx = Random.Range(0, poolCount);
                result.Add(pool[idx]);
            }
        }

        return result;
    }

    private List<GameObject> EnsureEnvAttachList(BattleManager battle, int panelIndex)
    {
        if (battle.envCardListList == null)
        {
            battle.envCardListList = new List<List<GameObject>>(battle.envPanels.Count);
        }

        while (battle.envCardListList.Count <= panelIndex)
        {
            battle.envCardListList.Add(new List<GameObject>());
        }

        var list = battle.envCardListList[panelIndex];
        if (list == null)
        {
            list = new List<GameObject>();
            battle.envCardListList[panelIndex] = list;
        }

        return list;
    }
}

