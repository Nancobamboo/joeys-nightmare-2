using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem
{
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
        ref int coins,
        ref int maxHealth,
        List<int> extraRelics)
    {
        DataGrowth growth = GetDataGrowth();
        bool Unlocked(int id) => growth != null && growth.IsUnlocked(id);

        // Node 0: 初始遗物 - 烤肉香香
        if (Unlocked(0))
        {
            extraRelics?.Add((int)ERelicType.BBQDelight);
        }

        // hp +4 nodes: 1,3,14,15,17,20
        int hpNodeCount = 0;
        if (Unlocked(1)) hpNodeCount++;
        if (Unlocked(3)) hpNodeCount++;
        if (Unlocked(14)) hpNodeCount++;
        if (Unlocked(15)) hpNodeCount++;
        if (Unlocked(17)) hpNodeCount++;
        if (Unlocked(20)) hpNodeCount++;
        if (hpNodeCount > 0) maxHealth += hpNodeCount * 4;

        // gold +50 nodes: 2,4,13,16,18,19
        int goldNodeCount = 0;
        if (Unlocked(2)) goldNodeCount++;
        if (Unlocked(4)) goldNodeCount++;
        if (Unlocked(13)) goldNodeCount++;
        if (Unlocked(16)) goldNodeCount++;
        if (Unlocked(18)) goldNodeCount++;
        if (Unlocked(19)) goldNodeCount++;
        if (goldNodeCount > 0) coins += goldNodeCount * 50;

        // 装备替换节点（每个节点只替换一把：ReplaceFirst）
        if (Unlocked(5))
        {
            ReplaceFirst(equipmentDefence, "2001", "2009"); // 破盾 -> 马甲
        }
        if (Unlocked(8))
        {
            ReplaceFirst(equipmentAttack, "1002", "1004"); // 断剑 -> 木棒
        }
        if (Unlocked(10))
        {
            ReplaceFirst(equipmentAttack, "1003", "1013"); // 手里剑 -> 噬魂手里剑
        }
        if (Unlocked(11))
        {
            ReplaceFirst(equipmentAttack, "1002", "1018"); // 断剑 -> 刺客匕首
            ReplaceFirst(equipmentAttack, "1004", "1018");
        }
    }

    public Dictionary<int, float> VFXDelayTimeDict = new Dictionary<int, float>();
    public Dictionary<int, float> AnimDelayTimeDict = new Dictionary<int, float>();
    public bool IsHardGame = false;

    public const bool isNew = true;
    public bool isFinishGame = false;
    public void LoadGameData()
    {
        Debug.Log("LoadGameData called");
        LoadDataJoeyPlayer();
        LoadVFX();
        LoadDataCardProgress();
        LoadDataAchievement();
        LoadDataGrowth();
        LoadDataDifficulty();
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
        VFXDelayTimeDict[(int)EVFXName.VFX_Fanjia] = 1f;
        VFXDelayTimeDict[(int)EVFXName.VFX_joey_souji] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_HuiXue] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_FanJia_shouji] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Shihun] = 1f;

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

        Card card = dataJoeyPlayer.GetEnvCardDictData(cardId);
        if (card != null)
        {
            return card;
        }

        card = GData.Instance.GetCardConfigById(cardId).Clone();
        dataJoeyPlayer.UniqueIdGen++;
        card.UniqueId = dataJoeyPlayer.UniqueIdGen;

        DataCardProgress cardProgress = GetDataCardProgress();
        cardProgress.AddCardIdDictData(cardId, 1);
        SaveDataCardProgress();

        return card;
    }

    public bool HasRelic(ERelicType relicType)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        return dataJoeyPlayer.RelicList.Contains((int)relicType);
    }

    public void InitRoguelikeCharacterData(RoguelikeCharacter characterData)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();

        int coins = characterData.coins;
        int maxHealth = characterData.maxHealth;
        var equipAttack = new List<string>(characterData.equipmentAttack);
        var equipDefence = new List<string>(characterData.equipmentDefence);
        var extraRelics = new List<int>();
        ApplyGrowthToStartLoadout(equipAttack, equipDefence, ref coins, ref maxHealth, extraRelics);

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

        for (int i = 0; i < characterData.equipmentItem.Count; i++)
        {
            string cardId = characterData.equipmentItem[i];
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

        int coins = characterData.coins;
        int maxHealth = characterData.maxHealth;
        var equipAttack = new List<string>(characterData.equipmentAttack);
        var equipDefence = new List<string>(characterData.equipmentDefence);
        var extraRelics = new List<int>();
        ApplyGrowthToStartLoadout(equipAttack, equipDefence, ref coins, ref maxHealth, extraRelics);

        //dataJoeyPlayer.ClearEnvCardPool();

        // Env mode only uses card_deck, not equipment fields
        for (int i = 0; i < characterData.cardDeck.Count; i++)
        {
            string cardId = characterData.cardDeck[i];
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
        }

        dataJoeyPlayer.currentLevel = 1;

        // Initialize difficulty level to 1 for new players
        if (dataJoeyPlayer.EnvDifficultyLevel <= 0)
        {
            dataJoeyPlayer.EnvDifficultyLevel = 1;
        }

        // Apply difficulty effects based on current difficulty level
        ApplyEnvDifficultyEffects(dataJoeyPlayer);

        Debug.Log($"Env mode initialized: {dataJoeyPlayer.EnvCardPool.Count} cards in pool, difficulty level: {dataJoeyPlayer.EnvDifficultyLevel}");
        SaveDataJoeyPlayer();
    }

    /// <summary>
    /// Apply difficulty effects to player stats and card pool
    /// This should be called when initializing env mode or when difficulty changes
    /// </summary>
    private void ApplyEnvDifficultyEffects(DataJoeyPlayer dataJoeyPlayer)
    {
        int difficultyLevel = dataJoeyPlayer.EnvDifficultyLevel;

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
            return true;
        }

        if (equipList.Count < maxLength)
        {
            equipList.Add(card.UniqueId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            return true;
        }

        return false;
    }

    public void ResetDataJoeyPlayer()
    {
        m_DataJoeyPlayer = new DataJoeyPlayer();
        isFinishGame = false;
        SaveDataJoeyPlayer();
    }
}

