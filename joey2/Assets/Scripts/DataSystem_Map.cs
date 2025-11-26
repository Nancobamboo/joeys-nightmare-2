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

    public void LoadGameData()
    {
        Debug.Log("LoadGameData called");
        LoadDataJoeyPlayer();
        LoadVFX();
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

        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_diaoluo_anim] = 0.5833333f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_dunpai] = 0.41666666f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_gongji] = 0.5f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_shouji] = 0.25f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_feitian] = 0.76666665f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_pailai] = 0.6666667f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_xiaoshi] = 0.33333334f;
        AnimDelayTimeDict[(int)ECardAnimName.UI_Carditem_guaiwugongji] = 0.65f;
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
        Card card = GData.Instance.GetCardConfigById(cardId).Clone();
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        dataJoeyPlayer.UniqueIdGen++;
        card.UniqueId = dataJoeyPlayer.UniqueIdGen;
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

        dataJoeyPlayer.Coin = characterData.coins;

        dataJoeyPlayer.MaxEquipedAttackNum = 3;
        dataJoeyPlayer.MaxEquipedDefenceNum = 3;
        dataJoeyPlayer.MaxEquipedItemNum = 3;
        dataJoeyPlayer.MaxEquipedSkillNum = 3;

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
            dataJoeyPlayer.StageId = 0;
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

    public void AddRelic(ERelicType relicType)
    {
        m_DataJoeyPlayer.AddRelicListData((int)relicType);
    }

    public void AddCoin(int delta)
    {
        DataJoeyPlayer dataJoeyPlayer = GetDataJoeyPlayer();
        dataJoeyPlayer.Coin += delta;
        YActionSystem.Instance.DispatchAction(EActionId.OnCoinChange, dataJoeyPlayer.Coin);
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
}

