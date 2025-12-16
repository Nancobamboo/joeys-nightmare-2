using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem
{
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

        for (int i = 0; i < characterData.cardDeck.Count; i++)
        {
            string cardId = characterData.cardDeck[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
        }

        for (int i = 0; i < characterData.equipmentAttack.Count; i++)
        {
            string cardId = characterData.equipmentAttack[i];
            if (string.IsNullOrEmpty(cardId)) continue;
            Card card = CreateCard(cardId);
            dataJoeyPlayer.AddSelfCardDictData(card);
            dataJoeyPlayer.AddEquipedAttackListData(card.UniqueId);
        }

        for (int i = 0; i < characterData.equipmentDefence.Count; i++)
        {
            string cardId = characterData.equipmentDefence[i];
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

        dataJoeyPlayer.Coin = characterData.coins;

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

        if (characterData.maxHealth > 0)
        {
            dataJoeyPlayer.playerMaxHealth = characterData.maxHealth;
            dataJoeyPlayer.playerHealth = characterData.maxHealth;
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

        dataJoeyPlayer.Coin = characterData.coins;

        if (characterData.maxHealth > 0)
        {
            dataJoeyPlayer.playerMaxHealth = characterData.maxHealth;
            dataJoeyPlayer.playerHealth = characterData.maxHealth;
        }

        dataJoeyPlayer.currentLevel = 1;

        Debug.Log($"Env mode initialized: {dataJoeyPlayer.EnvCardPool.Count} cards in pool");
        SaveDataJoeyPlayer();
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

