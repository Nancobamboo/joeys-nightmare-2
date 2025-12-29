using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem
{
    // -------- Growth unlock mappings (keep in sync with Resources/Data/growth.csv) --------
    // Card unlock: when NOT unlocked => price=0 (excluded from shop/reward pools). When unlocked => price restored (>0).
    private static readonly Dictionary<string, int> CardUnlockNodeByCardId = new Dictionary<string, int>
    {
        // id, nodeId
        { "4013", 3 },   // 魔力召唤
        { "1023", 5 },   // 骑士剑
        { "1020", 7 },   // 噩梦长枪
        { "4015", 8 },   // 冰霜魔法
        { "1021", 9 },   // 魔法剑
        { "2015", 10 },  // 骑士盾
        { "1024", 11 },  // 螺旋手里剑
        { "2012", 14 },  // 复仇之盾
        { "2013", 15 },  // 破盾的救赎
        { "3012", 16 },  // 沸腾兽血
        { "3013", 17 },  // 鲜血护符
        { "1022", 18 },  // 魔杖
        { "2014", 19 },  // 魔法盾
        { "3014", 20 },  // 魔法药水
        { "4014", 21 },  // 烈焰火球
        { "4011", 24 },  // 宁死不屈
        { "4012", 25 },  // 浴血奋战
        { "4016", 26 },  // 技能强化
    };

    // Relic unlock: when NOT unlocked => canDraw=false (excluded from relic selection pool).
    private static readonly Dictionary<int, int> RelicUnlockNodeByRelicId = new Dictionary<int, int>
    {
        { (int)ERelicType.BBQDelight, 0 },            // 烤肉香香
        { (int)ERelicType.WeaponParry, 12 },          // 弹刀
        { (int)ERelicType.CounterInsight, 13 },       // 看破
        { (int)ERelicType.DualWieldMastery, 23 },     // 双持精通
        { (int)ERelicType.HighArt, 27 },              // 高等艺术
        { (int)ERelicType.BareHandParry, 28 },        // 空手接白刃
        { (int)ERelicType.ShieldReflect, 29 },        // 护盾反伤
        { (int)ERelicType.RegenerationAmulet, 30 },   // 再生护符
        { (int)ERelicType.HalfHealthAmulet, 31 },     // 半血护符
        { (int)ERelicType.BloodyGloves, 32 },         // 染血拳法
        { (int)ERelicType.ArcaneOrb, 33 },            // 奥术宝珠
        { (int)ERelicType.MagicSwordsmanRing, 34 },   // 魔剑士指环
    };

    // Start-run stat bonuses (keep in sync with Resources/Data/growth.csv)
    // - Card limit +1 nodes: 35 / 48
    // - Weapon attack +1 nodes: 36 / 39 / 44
    // - Armor defence +1 nodes: 37 / 40 / 45
    // - HP cap +4 nodes: 41/43/47/52
    // - Starting coins +40 nodes: 46/51/54
    // - High-grade card probability +5% nodes: 38/42/49/50
    private static readonly int[] StartEnvCardLimitPlus1NodeIds = { 35, 48, 53 };
    private static readonly int[] StartWeaponAttackPlus1NodeIds = { 36, 39, 44 };
    private static readonly int[] StartArmorDefencePlus1NodeIds = { 37, 40, 45 };
    private static readonly int[] StartMaxHealthPlus4NodeIds = { 41, 43, 47, 52 };
    private static readonly int[] StartCoinsPlus40NodeIds = { 46, 51, 54 };
    private static readonly int[] HighGradeCardProbabilityPlus5NodeIds = { 38, 42, 49, 50 };

    /// <summary>
    /// Growth bonus: +5% per unlocked node (growth.csv: 38/42/49/50).
    /// This is applied as a probability modifier to high-grade card selection (stars 2/3),
    /// and is designed to stack with difficulty penalties.
    /// </summary>
    public float GetGrowthHighGradeCardProbabilityBonus()
    {
        DataGrowth growth = GetDataGrowth();
        if (growth == null) return 0f;

        float bonus = 0f;
        for (int i = 0; i < HighGradeCardProbabilityPlus5NodeIds.Length; i++)
        {
            if (growth.IsUnlocked(HighGradeCardProbabilityPlus5NodeIds[i]))
            {
                bonus += 0.05f;
            }
        }
        return bonus;
    }

    // Growth-applied bonus tracking (to avoid double-applying when ApplyGrowthUnlocks is called multiple times)
    private int m_AppliedEnvCardLimitGrowthBonus = 0;
    private int m_AppliedWeaponAttackGrowthBonus = 0;
    private int m_AppliedArmorDefenceGrowthBonus = 0;

    // Cache original prices from card_info.csv so we can restore after unlocking
    private Dictionary<string, int> m_BaseCardPriceById = new Dictionary<string, int>();

    private static bool ReplaceFirst(List<string> list, string fromId, string toId)
    {
        if (list == null || list.Count == 0) return false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == fromId)
            {
                list[i] = toId;
                return true;
            }
        }
        return false;
    }

    private void ApplyGrowthToStartLoadout(
        List<string> equipmentAttack,
        List<string> equipmentDefence,
        List<string> equipmentItem,
        ref int coins,
        ref int maxHealth,
        List<int> extraRelics)
    {
        DataGrowth growth = GetDataGrowth();
        bool Unlocked(int id) => growth != null && growth.IsUnlocked(id);

        // 初始遗物（growth.csv: 0 / 28 / 32 / 33）
        if (Unlocked(0)) extraRelics?.Add((int)ERelicType.BBQDelight);      // 烤肉香香
        if (Unlocked(28)) extraRelics?.Add((int)ERelicType.BareHandParry);  // 空手接白刃
        if (Unlocked(32)) extraRelics?.Add((int)ERelicType.BloodyGloves);   // 染血拳法
        if (Unlocked(33)) extraRelics?.Add((int)ERelicType.ArcaneOrb);      // 奥术宝珠
        // 初始装备增加一个小血瓶（growth.csv: 1，card_info.csv: 3001）
        if (Unlocked(1) && equipmentItem != null && !equipmentItem.Contains("3001"))
        {
            equipmentItem.Add("3001");
        }

        // 局外成长：开局属性加成（growth.csv: 35-54）
        // hp 上限 +4（可叠加）
        for (int i = 0; i < StartMaxHealthPlus4NodeIds.Length; i++)
        {
            if (Unlocked(StartMaxHealthPlus4NodeIds[i])) maxHealth += 4;
        }
        // 初始金币 +40（可叠加）
        for (int i = 0; i < StartCoinsPlus40NodeIds.Length; i++)
        {
            if (Unlocked(StartCoinsPlus40NodeIds[i])) coins += 40;
        }

        // 初始装备替换（growth.csv: 2 / 4 / 6 / 22）
        if (Unlocked(4)) ReplaceFirst(equipmentDefence, "2001", "2009"); // 破盾 -> 马甲
        if (Unlocked(2)) ReplaceFirst(equipmentAttack, "1002", "1004");  // 断剑 -> 木棒
        if (Unlocked(6)) ReplaceFirst(equipmentAttack, "1004", "1010");  // 木棒 -> kejiaren
        if (Unlocked(22)) ReplaceFirst(equipmentAttack, "1003", "1013"); // 手里剑 -> 噬魂手里剑

    }

    /// <summary>
    /// Apply growth unlocks to runtime config:
    /// - Locked cards: price = 0 (excluded from shop/reward pools)
    /// - Locked relics: RelicInfo.canDraw = false (excluded from relic selection pool)
    /// Call this after loading growth data, and after unlocking a node.
    /// </summary>
    public void ApplyGrowthUnlocks()
    {
        ApplyGrowthCardUnlocks();
        ApplyGrowthRelicUnlocks();
        ApplyGrowthWeaponArmorStatBonus();
        ApplyGrowthEnvCardLimitBonus();
    }

    /// <summary>
    /// Apply growth nodes:
    /// - 36/39/44: Weapon attack +1 (applies to attack cards)
    /// - 37/40/45: Armor defence +1 (applies to defence cards)
    /// This is applied as a delta so it won't stack if called multiple times.
    /// </summary>
    private void ApplyGrowthWeaponArmorStatBonus()
    {
        DataGrowth growth = GetDataGrowth();

        int weaponAtkBonus = 0;
        int armorDefBonus = 0;
        if (growth != null)
        {
            for (int i = 0; i < StartWeaponAttackPlus1NodeIds.Length; i++)
            {
                if (growth.IsUnlocked(StartWeaponAttackPlus1NodeIds[i])) weaponAtkBonus += 1;
            }
            for (int i = 0; i < StartArmorDefencePlus1NodeIds.Length; i++)
            {
                if (growth.IsUnlocked(StartArmorDefencePlus1NodeIds[i])) armorDefBonus += 1;
            }
        }

        int atkDelta = weaponAtkBonus - m_AppliedWeaponAttackGrowthBonus;
        int defDelta = armorDefBonus - m_AppliedArmorDefenceGrowthBonus;
        if (atkDelta == 0 && defDelta == 0) return;
        m_AppliedWeaponAttackGrowthBonus = weaponAtkBonus;
        m_AppliedArmorDefenceGrowthBonus = armorDefBonus;

        // 1) Apply to base card configs so all future created cards inherit the buff
        GData.Instance.LoadCards();
        foreach (var kv in GData.Instance.CardDict)
        {
            Card cfg = kv.Value;
            if (cfg == null) continue;
            ECardType type = cfg.GetCardType();
            if (type == ECardType.attack && atkDelta != 0)
            {
                cfg.SetAttack(Mathf.Max(0, cfg.currentAttack + atkDelta));
            }
            else if (type == ECardType.defence && defDelta != 0)
            {
                cfg.SetDefence(Mathf.Max(0, cfg.currentDefence + defDelta));
            }
        }

        // 2) Apply to current player-owned cards (including cached Env cards) so UI/actual values are consistent
        DataJoeyPlayer player = GetDataJoeyPlayer();
        if (player == null) return;

        void ApplyDeltaToCard(Card c)
        {
            if (c == null) return;
            ECardType t = c.GetCardType();
            if (t == ECardType.attack && atkDelta != 0)
            {
                c.SetAttack(Mathf.Max(0, c.currentAttack + atkDelta));
            }
            else if (t == ECardType.defence && defDelta != 0)
            {
                c.SetDefence(Mathf.Max(0, c.currentDefence + defDelta));
            }
        }

        if (player.SelfCardDict != null)
        {
            foreach (var kv in player.SelfCardDict)
            {
                ApplyDeltaToCard(kv.Value);
            }
        }
        if (player.EnvCardDict != null)
        {
            foreach (var kv in player.EnvCardDict)
            {
                ApplyDeltaToCard(kv.Value);
            }
        }
    }

    /// <summary>
    /// Apply growth "卡牌上限 +1" nodes to Env mode card limit (RoguelikeCharacter.envCardLimit).
    /// This is applied as a delta so it remains compatible with other runtime modifiers (e.g. relics).
    /// </summary>
    private void ApplyGrowthEnvCardLimitBonus()
    {
        DataGrowth growth = GetDataGrowth();

        int bonus = 0;
        if (growth != null)
        {
            for (int i = 0; i < StartEnvCardLimitPlus1NodeIds.Length; i++)
            {
                if (growth.IsUnlocked(StartEnvCardLimitPlus1NodeIds[i])) bonus += 1;
            }
        }

        int delta = bonus - m_AppliedEnvCardLimitGrowthBonus;
        if (delta == 0) return;
        m_AppliedEnvCardLimitGrowthBonus = bonus;

        // Apply to all roguelike characters (index defaults to 0 in most call sites)
        GData.Instance.LoadRoguelikeCharacter();
        var list = GData.Instance.RoguelikeCharacterList;
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            RoguelikeCharacter ch = list[i];
            if (ch == null) continue;
            ch.envCardLimit += delta;
        }
    }

    private void EnsureBaseCardPricesCached()
    {
        if (m_BaseCardPriceById != null && m_BaseCardPriceById.Count > 0) return;
        if (m_BaseCardPriceById == null) m_BaseCardPriceById = new Dictionary<string, int>();

        GData.Instance.LoadCards();
        foreach (var kv in GData.Instance.CardDict)
        {
            if (kv.Value == null) continue;
            m_BaseCardPriceById[kv.Key] = kv.Value.price;
        }
    }

    private void ApplyGrowthCardUnlocks()
    {
        DataGrowth growth = GetDataGrowth();
        if (growth == null) return;

        GData.Instance.LoadCards();
        EnsureBaseCardPricesCached();

        foreach (var kv in CardUnlockNodeByCardId)
        {
            string cardId = kv.Key;
            int nodeId = kv.Value;

            if (!GData.Instance.CardDict.TryGetValue(cardId, out Card cfg) || cfg == null) continue;

            bool unlocked = growth.IsUnlocked(nodeId);
            if (!unlocked)
            {
                cfg.price = 0;
                continue;
            }

            // Restore price from CSV; if CSV price is 0, ensure it's still "unlockable" (price > 0)
            int basePrice = 0;
            m_BaseCardPriceById.TryGetValue(cardId, out basePrice);
            if (basePrice > 0)
            {
                cfg.price = basePrice;
            }
            else
            {
                // Fallback: stars * 100 (keeps it consistent with existing economy tiers)
                cfg.price = Mathf.Max(1, cfg.stars * 100);
                Debug.LogWarning($"[GrowthUnlock] Card {cardId} base price is {basePrice} in card_info.csv, fallback to {cfg.price} on unlock.");
            }
        }
    }

    private void ApplyGrowthRelicUnlocks()
    {
        DataGrowth growth = GetDataGrowth();
        if (growth == null) return;

        GData.Instance.LoadRelicInfo();
        foreach (var kv in GData.Instance.RelicInfoDict)
        {
            RelicInfo info = kv.Value;
            if (info == null) continue;

            // Default: drawable unless gated by growth
            if (RelicUnlockNodeByRelicId.TryGetValue(info.id, out int nodeId))
            {
                info.canDraw = growth.IsUnlocked(nodeId);
            }
            else
            {
                info.canDraw = true;
            }
        }
    }

    public Dictionary<int, float> VFXDelayTimeDict = new Dictionary<int, float>();
    public Dictionary<int, float> AnimDelayTimeDict = new Dictionary<int, float>();
    public bool IsHardGame = false;

    public const bool isNew = true;
    public bool isFinishGame = false;
    public bool IsNewCardUnlock = true;
    public bool IsGrowthUnlock = true;
    public void LoadGameData()
    {
        Debug.Log("LoadGameData called");
        LoadDataJoeyPlayer();
        LoadVFX();
        LoadDataCardProgress();
        LoadDataAchievement();
        LoadDataGrowth();
        LoadDataDifficulty();

        // Ensure growth unlocks are applied to card/relic pools for this session
        ApplyGrowthUnlocks();
    }

    public void LoadVFX()
    {
        VFXDelayTimeDict[(int)EVFXName.VFX_Dun] = 1.0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_boom] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Shouji] = 1.0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_LeiDan] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_appear] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_disappear] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Dunsui] = 1f;
        VFXDelayTimeDict[(int)EVFXName.VFX_glow] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Yishun] = 10f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Fanjia] = 1f;
        VFXDelayTimeDict[(int)EVFXName.VFX_joey_souji] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_HuiXue] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_FanJia_shouji] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Shihun] = 1f;
        VFXDelayTimeDict[(int)EVFXName.VFX_HuoQiu] = 0.4f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Bing] = 0.4f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Bing2] = 0.4f;

        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_diaoluo_anim] = 0.4f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_dunpai] = 0.3f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_gongji] = 0.5f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_shouji] = 0.25f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_feitian] = 0.3f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_pailai] = 0.5f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_xiaoshi] = 0.3f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_guaiwugongji] = 0.5f;
        AnimDelayTimeDict[(int)ECardAnimName.Idle] = 0f;
    }

    public float GetVFXDelayTime(EVFXName vfxName)
    {
        if (VFXDelayTimeDict.TryGetValue((int)vfxName, out float delayTime))
        {
            return delayTime;
        }
        return 0f;
    }

    public float GetAnimDelayTime(ECardAnimName animName)
    {
        if (AnimDelayTimeDict.TryGetValue((int)animName, out float delayTime))
        {
            return delayTime;
        }
        return 0f;
    }

    public Card CreateCard(string cardId)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();

        // Get base card config to check card type
        Card configCard = GData.Instance.GetCardConfigById(cardId);
        if (configCard == null)
        {
            Debug.LogError($"Card config not found for ID: {cardId}");
            return null;
        }

        // Monster cards should NOT use cached data because they need difficulty bonuses applied fresh each time
        // Only non-monster cards (attack/defence/skill/item) use EnvCardDict for permanent upgrades
        if (configCard.GetCardType() != ECardType.monster)
        {
            Card cachedCard = dataJoeyPlayer.GetEnvCardDictData(cardId);
            if (cachedCard != null)
            {
                return cachedCard;
            }
        }

        // Create fresh card (either monster, or non-monster that's not cached)
        Card card = configCard.Clone();
        dataJoeyPlayer.UniqueIdGen++;
        card.UniqueId = dataJoeyPlayer.UniqueIdGen;

        DataCardProgress cardProgress = GetDataCardProgress();
        cardProgress.AddCardIdDictData(cardId, 1);
        SaveDataCardProgress();

        // Apply difficulty effects to player cards in Env mode ONLY when first created (not cached)
        // This ensures difficulty penalties are applied exactly once, not every time the card is loaded
        if (JoeyGameControl.Instance != null && JoeyGameControl.Instance.GameMode == EGameMode.Env)
        {
            ECardType cardType = card.GetCardType();
            if (cardType == ECardType.attack || cardType == ECardType.defence)
            {
                ApplyEnvDifficultyToPlayerCard(card);
            }
        }

        return card;
    }

    /// <summary>
    /// Apply difficulty penalties to player cards in env (cumulative from all unlocked difficulties)
    /// This should only be called when a card is first created, NOT when loaded from cache
    /// </summary>
    private void ApplyEnvDifficultyToPlayerCard(Card playerCard)
    {
        int difficultyLevel = GetCurrentDifficulty();

        // Apply cumulative penalties from difficulty levels 2 and up
        for (int level = 2; level <= difficultyLevel; level++)
        {
            DifficultyConfig config = GData.Instance.GetDifficultyConfig(level);
            if (config == null) continue;

            // Apply attack penalty to attack cards
            if (playerCard.GetCardType() == ECardType.attack && config.playerAttackPenalty != 0)
            {
                playerCard.currentAttack += config.playerAttackPenalty;
                if (playerCard.currentAttack < 0) playerCard.currentAttack = 0;
            }

            // Apply defence penalty to defence cards
            if (playerCard.GetCardType() == ECardType.defence && config.playerDefencePenalty != 0)
            {
                playerCard.currentDefence += config.playerDefencePenalty;
                if (playerCard.currentDefence < 0) playerCard.currentDefence = 0;
            }
        }
    }

    public bool HasRelic(ERelicType relicType)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        return dataJoeyPlayer.RelicList.Contains((int)relicType);
    }

    public void InitRoguelikeCharacterData(RoguelikeCharacter characterData)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();

        // Apply latest growth unlocks before initializing run data
        ApplyGrowthUnlocks();

        int coins = characterData.coins;
        int maxHealth = characterData.maxHealth;
        var equipAttack = new List<string>(characterData.equipmentAttack);
        var equipDefence = new List<string>(characterData.equipmentDefence);
        var equipItem = new List<string>(characterData.equipmentItem);
        var extraRelics = new List<int>();
        ApplyGrowthToStartLoadout(equipAttack, equipDefence, equipItem, ref coins, ref maxHealth, extraRelics);

        for (int i = 0; i < characterData.cardDeck.Count; i++)
        {
            string cardId = characterData.cardDeck[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
        }

        for (int i = 0; i < equipAttack.Count; i++)
        {
            string cardId = equipAttack[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            dataJoeyPlayer.AddEquipedAttackListData(card.UniqueId);
        }

        for (int i = 0; i < equipDefence.Count; i++)
        {
            string cardId = equipDefence[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            dataJoeyPlayer.AddEquipedDefenceListData(card.UniqueId);
        }

        for (int i = 0; i < equipItem.Count; i++)
        {
            string cardId = equipItem[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            dataJoeyPlayer.AddEquipedItemListData(card.UniqueId);
        }

        for (int i = 0; i < characterData.equipmentSkill.Count; i++)
        {
            string cardId = characterData.equipmentSkill[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            dataJoeyPlayer.AddEquipedSkillListData(card.UniqueId);
        }

        // Initialize equipment relics
        for (int i = 0; i < characterData.equipmentRelic.Count; i++)
        {
            string relicIdStr = characterData.equipmentRelic[i];
            if (string.IsNullOrEmpty(relicIdStr)) continue;
            if (int.TryParse(relicIdStr, out int relicId))
            {
                dataJoeyPlayer.AddRelicListData(relicId);
            }
        }

        for (int i = 0; i < extraRelics.Count; i++)
        {
            int rid = extraRelics[i];
            if (!dataJoeyPlayer.RelicList.Contains(rid)) dataJoeyPlayer.AddRelicListData(rid);
        }

        dataJoeyPlayer.Coin = coins;

        if (isNew)
        {
            dataJoeyPlayer.MaxEquipedAttackNum = 7;
            dataJoeyPlayer.MaxEquipedDefenceNum = 7;
            dataJoeyPlayer.MaxEquipedItemNum = 7;
            dataJoeyPlayer.MaxEquipedSkillNum = 7;
        }
        else
        {
            dataJoeyPlayer.MaxEquipedAttackNum = 3;
            dataJoeyPlayer.MaxEquipedDefenceNum = 3;
            dataJoeyPlayer.MaxEquipedItemNum = 3;
            dataJoeyPlayer.MaxEquipedSkillNum = 3;
        }

        if (maxHealth > 0)
        {
            dataJoeyPlayer.playerMaxHealth = maxHealth;
            dataJoeyPlayer.playerHealth = maxHealth;
            dataJoeyPlayer.stageStartHealth = maxHealth; // Initialize stage start health
        }

        RoguelikeStage firstStage = GData.Instance.GetRoguelikeStage(0);
        if (firstStage != null && firstStage.level.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, firstStage.level.Count);
            string levelIdStr = firstStage.level[randomIndex];
            if (int.TryParse(levelIdStr, out int levelId))
            {
                dataJoeyPlayer.currentLevel = levelId;
            }
        }

        // Save the difficulty level when this save was created
        dataJoeyPlayer.savedDifficulty = GetCurrentDifficulty();

        SaveDataJoeyPlayer();
    }

    public void LoadNextRoguelikeStage()
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();

        RoguelikeStage stage = GData.Instance.GetRoguelikeStage(dataJoeyPlayer.StageId);
        if (stage != null && stage.level.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, stage.level.Count);
            string levelIdStr = stage.level[randomIndex];
            if (int.TryParse(levelIdStr, out int levelId))
            {
                dataJoeyPlayer.currentLevel = levelId;
            }
        }

        SaveDataJoeyPlayer();
    }

    public void InitEnvModeCharacterData(RoguelikeCharacter characterData)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();

        // Apply latest growth unlocks before initializing env run data
        ApplyGrowthUnlocks();

        int coins = characterData.coins;
        int maxHealth = characterData.maxHealth;
        var equipAttack = new List<string>(characterData.equipmentAttack);
        var equipDefence = new List<string>(characterData.equipmentDefence);
        var equipItem = new List<string>(characterData.equipmentItem);
        var extraRelics = new List<int>();
        ApplyGrowthToStartLoadout(equipAttack, equipDefence, equipItem, ref coins, ref maxHealth, extraRelics);

        // Safety: ensure a clean env run init (normally EnvCardPool is empty when this is called)
        if (dataJoeyPlayer.EnvCardPool != null) dataJoeyPlayer.EnvCardPool.Clear();
        if (dataJoeyPlayer.EnvCardDict != null) dataJoeyPlayer.EnvCardDict.Clear();

        // Env mode only uses card_deck, not equipment fields.
        // Important: growth "initial equipment replacement" nodes should still affect Env start deck,
        // because the base character CSV puts those equipment cards into card_deck as well.
        var envDeck = new List<string>(characterData.cardDeck);
        {
            DataGrowth growth = GetDataGrowth();
            bool Unlocked(int id) => growth != null && growth.IsUnlocked(id);

            // Keep in sync with ApplyGrowthToStartLoadout (growth.csv: 4 / 6 / 22)
            if (Unlocked(4)) ReplaceFirst(envDeck, "2001", "2009"); // 破盾 -> 马甲
            if (Unlocked(6)) ReplaceFirst(envDeck, "1002", "1004"); // 断剑 -> 木棒
            if (Unlocked(22)) ReplaceFirst(envDeck, "1003", "1013"); // 手里剑 -> 噬魂手里剑
        }

        // Growth may add starting items via equipmentItem (e.g. node 1 adds 3001 小血瓶).
        // Env mode doesn't use equipment lists, so we merge item cards into the Env start deck/pool here.
        if (equipItem != null && equipItem.Count > 0)
        {
            for (int i = 0; i < equipItem.Count; i++)
            {
                string id = equipItem[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!envDeck.Contains(id)) envDeck.Add(id);
            }
        }

        for (int i = 0; i < envDeck.Count; i++)
        {
            string cardId = envDeck[i];
            if (!string.IsNullOrEmpty(cardId))
            {
                dataJoeyPlayer.AddEnvCardPoolData(cardId);
            }
        }

        // Initialize equipment relics for Env mode
        for (int i = 0; i < characterData.equipmentRelic.Count; i++)
        {
            string relicIdStr = characterData.equipmentRelic[i];
            if (string.IsNullOrEmpty(relicIdStr)) continue;
            if (int.TryParse(relicIdStr, out int relicId))
            {
                dataJoeyPlayer.AddRelicListData(relicId);
            }
        }

        for (int i = 0; i < extraRelics.Count; i++)
        {
            int rid = extraRelics[i];
            if (!dataJoeyPlayer.RelicList.Contains(rid)) dataJoeyPlayer.AddRelicListData(rid);
        }

        dataJoeyPlayer.Coin = coins;

        if (maxHealth > 0)
        {
            dataJoeyPlayer.playerMaxHealth = maxHealth;
            dataJoeyPlayer.playerHealth = maxHealth;
            dataJoeyPlayer.stageStartHealth = maxHealth; // Initialize stage start health
        }

        // Initialize base attack and defence for Env mode
        dataJoeyPlayer.playerAttack = 0;
        dataJoeyPlayer.playerDefence = 0;

        dataJoeyPlayer.currentLevel = 1;

        // Apply difficulty effects based on current difficulty level
        ApplyEnvDifficultyEffects(dataJoeyPlayer);

        // Save the difficulty level when this save was created
        dataJoeyPlayer.savedDifficulty = GetCurrentDifficulty();

        Debug.Log($"Env mode initialized: {dataJoeyPlayer.EnvCardPool.Count} cards in pool, difficulty level: {GetCurrentDifficulty()}");
        SaveDataJoeyPlayer();
    }

    /// <summary>
    /// Apply difficulty effects to player stats and card pool
    /// This should be called when initializing env mode or when difficulty changes
    /// </summary>
    private void ApplyEnvDifficultyEffects(DataJoeyPlayer dataJoeyPlayer)
    {
        int difficultyLevel = GetCurrentDifficulty();

        // Apply cumulative effects from all difficulty levels up to current
        for (int level = 2; level <= difficultyLevel; level++)
        {
            DifficultyConfig config = GData.Instance.GetDifficultyConfig(level);
            if (config == null) continue;

            // Apply player stat penalties
            if (config.playerHealthPenalty != 0)
            {
                dataJoeyPlayer.playerHealth += config.playerHealthPenalty;
                if (dataJoeyPlayer.playerHealth < 1) dataJoeyPlayer.playerHealth = 1;
            }

            if (config.playerMaxHealthPenalty != 0)
            {
                dataJoeyPlayer.playerMaxHealth += config.playerMaxHealthPenalty;
                if (dataJoeyPlayer.playerMaxHealth < 1) dataJoeyPlayer.playerMaxHealth = 1;
                // Adjust current health if it exceeds new max
                if (dataJoeyPlayer.playerHealth > dataJoeyPlayer.playerMaxHealth)
                {
                    dataJoeyPlayer.playerHealth = dataJoeyPlayer.playerMaxHealth;
                }
            }

            if (config.playerAttackPenalty != 0)
            {
                dataJoeyPlayer.playerAttack += config.playerAttackPenalty;
                if (dataJoeyPlayer.playerAttack < 0) dataJoeyPlayer.playerAttack = 0;
            }

            if (config.playerDefencePenalty != 0)
            {
                dataJoeyPlayer.playerDefence += config.playerDefencePenalty;
                if (dataJoeyPlayer.playerDefence < 0) dataJoeyPlayer.playerDefence = 0;
            }

            // Add curse cards to starting deck (only add once per difficulty level)
            foreach (string curseCardId in config.startingCurseCards)
            {
                if (!dataJoeyPlayer.EnvCardPool.Contains(curseCardId))
                {
                    dataJoeyPlayer.AddEnvCardPoolData(curseCardId);
                    Debug.Log($"Difficulty {level}: Added curse card {curseCardId} to card pool");
                }
            }

            Debug.Log($"Applied difficulty {level} effects: Health {config.playerHealthPenalty}, MaxHealth {config.playerMaxHealthPenalty}, Attack {config.playerAttackPenalty}, Defence {config.playerDefencePenalty}");
        }
    }

    public void AddRelic(ERelicType relicType)
    {
        m_DataJoeyPlayer.AddRelicListData((int)relicType);
    }

    public void AddCoin(int delta)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        dataJoeyPlayer.Coin += delta;
        YActionSystem.Instance.DispatchAction(EActionId.OnCoinChange, dataJoeyPlayer.Coin, delta);
    }

    public void AddGrowthPoints(int delta)
    {
        DataGrowth dataGrowth = GetDataGrowth();
        dataGrowth.Points += delta;
        if (delta > 0)
        {
            IsGrowthUnlock = true;
        }
        YActionSystem.Instance.DispatchAction(EActionId.OnGrowthPointsChange, dataGrowth.Points, delta);
        SaveDataGrowth();
    }

    /// <summary>
    /// 判断当前是否存在“买得起”的成长点：
    /// - 尚未解锁
    /// - 价格 <= 当前 Points
    /// - 与已解锁节点联通（dependency 指向已解锁节点）
    /// - 并且其所有 dependency（>=0 的部分）都已解锁（即当前可购买）
    ///
    /// 注意：dependency 为空或仅为 -1 的节点视作“根节点”。默认 includeRootNodes=true 时也会纳入判断。
    /// </summary>
    public bool HasAffordableConnectedGrowthNode(bool includeRootNodes = true)
    {
        DataGrowth dataGrowth = GetDataGrowth();
        if (dataGrowth == null) return false;

        int points = dataGrowth.Points;
        var unlockedList = dataGrowth.UnlockedNodes;
        HashSet<int> unlocked = unlockedList != null ? new HashSet<int>(unlockedList) : new HashSet<int>();

        GData.Instance.LoadGrowthInfo();
        foreach (var kv in GData.Instance.GrowthInfoDict)
        {
            GrowthInfo info = kv.Value;
            if (info == null) continue;

            int id = info.id;
            if (unlocked.Contains(id)) continue;               // 已解锁不算
            if (info.price > points) continue;                // 买不起

            bool hasPrereq = false;
            bool allPrereqUnlocked = true;
            bool connectedToUnlocked = false;

            var deps = info.depends;
            if (deps != null)
            {
                for (int i = 0; i < deps.Count; i++)
                {
                    int depId = deps[i];
                    if (depId < 0) continue; // -1 表示无前置

                    hasPrereq = true;
                    if (!unlocked.Contains(depId))
                    {
                        allPrereqUnlocked = false;
                        break;
                    }
                    connectedToUnlocked = true; // 前置已解锁 => 与已解锁联通
                }
            }

            // 根节点：无前置（或前置都是 -1）
            if (!hasPrereq)
            {
                if (!includeRootNodes) continue;
                allPrereqUnlocked = true;
                connectedToUnlocked = true;
            }

            if (allPrereqUnlocked && connectedToUnlocked)
            {
                return true;
            }
        }

        return false;
    }

    public bool AddCardToDataJoeyPlayer(Card card)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        ECardType cardType = card.GetCardType();

        List<int> equipList = null;
        List<int> tempList = null;
        int maxLength = 0;

        switch (cardType)
        {
            case ECardType.attack:
                equipList = dataJoeyPlayer.EquipedAttackList;
                tempList = dataJoeyPlayer.TempAttackList;
                maxLength = dataJoeyPlayer.MaxEquipedAttackNum;
                break;
            case ECardType.defence:
                equipList = dataJoeyPlayer.EquipedDefenceList;
                tempList = dataJoeyPlayer.TempDefenceList;
                maxLength = dataJoeyPlayer.MaxEquipedDefenceNum;
                break;
            case ECardType.item:
                equipList = dataJoeyPlayer.EquipedItemList;
                tempList = dataJoeyPlayer.TempItemList;
                maxLength = dataJoeyPlayer.MaxEquipedItemNum;
                break;
            case ECardType.skill:
                equipList = dataJoeyPlayer.EquipedSkillList;
                tempList = dataJoeyPlayer.TempSkillList;
                maxLength = dataJoeyPlayer.MaxEquipedSkillNum;
                break;
            default:
                return false;
        }

        if (equipList.Count == maxLength)
        {
            if (tempList.Count == 3)
            {
                return false;
            }
            tempList.Add(card.UniqueId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            IsNewCardUnlock = true;
            return true;
        }

        if (equipList.Count < maxLength)
        {
            equipList.Add(card.UniqueId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            IsNewCardUnlock = true;
            return true;
        }

        return false;
    }

    public void ResetDataJoeyPlayer()
    {
        // Difficulty is now tracked in DataDifficulty, no need to preserve in player data
        m_DataJoeyPlayer = new DataJoeyPlayer();

        isFinishGame = false;
        SaveDataJoeyPlayer();

        Debug.Log($"Player data reset. Difficulty level preserved in DataDifficulty system.");
    }

    /// <summary>
    /// Check if the saved game's difficulty matches the current difficulty
    /// Returns true if save can be continued, false if difficulty mismatch
    /// </summary>
    public bool CanContinueSavedGame()
    {
        DataJoeyPlayer playerData = GetDataJoeyPlayer();

        // Check if there's save data
        bool hasSaveData = playerData.EnvCardPool != null && playerData.EnvCardPool.Count > 0;
        if (!hasSaveData)
        {
            return false;
        }

        // Check if difficulty matches (savedDifficulty defaults to 1 for old saves)
        int savedDiff = playerData.savedDifficulty > 0 ? playerData.savedDifficulty : 1;
        int currentDiff = GetCurrentDifficulty();

        if (savedDiff != currentDiff)
        {
            Debug.Log($"Cannot continue: Save is from difficulty {savedDiff}, but current difficulty is {currentDiff}");
            return false;
        }

        return true;
    }
}

