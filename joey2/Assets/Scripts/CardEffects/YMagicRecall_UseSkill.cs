// Scripts/CardEffects/Effects/YMagicRecall_UseSkill.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YMagicRecall_UseSkill : YCardEffect
{
    public int baseExtra;
    // 配置可召唤的技能卡ID
    private static readonly List<string> SkillCardIds = new List<string>
    {
        "4001", // 连续攻击
        "4006", // 连环闪电
        "4007", // 匕首飞来
        "4008", // 丢盔弃甲
    };

    public YMagicRecall_UseSkill(int baseExtra)
    {
        Id = ECardEffectId.MagicRecall_UseSkill;
        this.baseExtra = baseExtra;
    }

    public override float UseSkill()
    {
        int cardCount = baseExtra;
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
        }

        if (SkillCardIds.Count == 0) return 0.3f;

        // 从SkillCardIds中随机选择卡牌ID
        DataJoeyPlayer dataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
        List<string> selectedCardIds = new List<string>();
        
        for (int i = 0; i < cardCount; i++)
        {
            // 使用确定性种子以保证可重放
            int seed = dataJoeyPlayer.levelRandomSeed + dataJoeyPlayer.giftBoxUseCounter;
            dataJoeyPlayer.giftBoxUseCounter++;
            
            // 保存当前随机状态
            Random.State oldState = Random.state;
            Random.InitState(seed);
            
            // 从SkillCardIds中随机选择一个ID
            int idx = Random.Range(0, SkillCardIds.Count);
            selectedCardIds.Add(SkillCardIds[idx]);
            
            // 恢复随机状态
            Random.state = oldState;
        }
        
        // 复用 AddCardsToEnvByCardId，优先选择空的env index
        foreach (string cardId in selectedCardIds)
        {
            YActionSystem.Instance.DispatchAction(EActionId.AddCardsToEnvByCardId, CardControl, cardId, 1);
        }
        
        return 0.3f;
    }
}
