// Scripts/CardEffects/Effects/YIceMagic.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YIceMagic : YCardEffect
{
    public int baseExtra;

    public YIceMagic(int baseExtra)
    {
        Id = ECardEffectId.IceMagic;
        this.baseExtra = baseExtra;
    }

    public override float UseSkill()
    {
        int damage = baseExtra;
        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            // 触发冰魔法伤害效果
            JoeyGameControl.Instance.QueueAction(EActionId.IceMagicDamage, damage);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    void IceMagicDamage(object[] paraArray)
    {
        int damage = paraArray[0] is int ? (int)paraArray[0] : 0;
        IceMagicDamageAsync(damage).Forget();
    }

    async UniTask IceMagicDamageAsync(int damage)
    {
        // 获取所有有怪物的环境位置
        List<int> enemyIndices = new List<int>();
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf &&
                lastCard.CardType == ECardType.monster && lastCard.CardData.currentHealth > 0)
            {
                enemyIndices.Add(i);
            }
        }

        if (enemyIndices.Count == 0)
        {
            return;
        }

        // 随机选择一个目标
        int targetEnvIndex = enemyIndices[Random.Range(0, enemyIndices.Count)];
        UICardSimpleControl targetCard = GetLastEnvCard(targetEnvIndex);

        if (targetCard == null || !targetCard.gameObject.activeSelf ||
            targetCard.CardType != ECardType.monster || targetCard.CardData.currentHealth <= 0)
        {
            return;
        }

        Debug.Log($"IceMagic: Target envIndex = {targetEnvIndex}, damage = {damage}");

        // 对目标造成伤害
        if (damage > 0)
        {
            CancellationToken token = GetOrCreateCardToken(targetCard);
            await DealDamageToEnvCard(targetCard, damage, targetEnvIndex, EEffectType.IceMagic, token);
            RemoveCardCts(targetCard);
        }

        // 如果怪物还活着，给它添加冰冻效果
        if (targetCard != null && targetCard.gameObject.activeSelf &&
            targetCard.CardData.currentHealth > 0)
        {
            targetCard.AddBuff(EBuffType.Frozen, 1);
            Debug.Log($"IceMagic: Monster at envIndex {targetEnvIndex} is now frozen");
        }
    }
}
