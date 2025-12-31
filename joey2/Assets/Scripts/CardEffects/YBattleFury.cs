// Scripts/CardEffects/Effects/YBattleFury.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class YBattleFury : YDefaultEffect
{
    // 溅射伤害百分比 (50 = 50%)
    public int splashDamagePercent;

    public YBattleFury(int splashDamagePercent = 50)
    {
        this.splashDamagePercent = Mathf.Max(0, splashDamagePercent);
        Id = ECardEffectId.BattleFury;
    }

    public override float OnDealDamage()
    {
        // 触发溅射伤害效果
        if (CardControl != null)
        {
            int attackDamage = CardControl.CardData.currentAttack + GetEffectValue(EEffectType.Damage);
            int splashDamage = Mathf.Max(1, attackDamage * splashDamagePercent / 100);
            YActionSystem.Instance.DispatchAction(EActionId.DealSplashDamage, CardControl, splashDamage);
        }
        return base.OnDealDamage();
    }
}

public partial class UIGamePhaseControl
{
    /// <summary>
    /// 处理溅射伤害效果 - 对目标周围1格内的怪物造成伤害
    /// </summary>
    void DealSplashDamage(object[] paraArray)
    {
        UICardSimpleControl attackCardControl = (UICardSimpleControl)paraArray[0];
        int splashDamage = (int)paraArray[1];
        DealSplashDamageAsync(attackCardControl, splashDamage).Forget();
    }

    async UniTask DealSplashDamageAsync(UICardSimpleControl attackCardControl, int splashDamage)
    {
        // Capture target index immediately to avoid race conditions
        int targetEnvIndex = m_CurrentAttackTargetEnvIndex;
        
        Debug.Log("DealSplashDamageAsync: " + splashDamage);
        Debug.Log("targetEnvIndex: " + targetEnvIndex);
        if (targetEnvIndex < 0)
        {
            return;
        }

        // 等待主目标伤害完成后再触发溅射伤害
        await UniTask.WaitForSeconds(0.5f);
        
        // 检查左右相邻位置的怪物
        int[] adjacentIndices = new int[] { targetEnvIndex - 1, targetEnvIndex + 1 };

        // 收集所有需要造成伤害的目标
        List<(UICardSimpleControl card, int envIndex)> targets = new List<(UICardSimpleControl, int)>();

        for (int i = 0; i < adjacentIndices.Length; i++)
        {
            int adjIndex = adjacentIndices[i];
            
            // 确保索引有效
            if (adjIndex < 0 || adjIndex >= m_EnvPanels.Count)
            {
                continue;
            }

            // 获取该位置最外层的卡牌
            UICardSimpleControl adjacentCard = GetLastEnvCard(adjIndex);
            
            // 检查是否是怪物且存活
            if (adjacentCard != null && 
                adjacentCard.gameObject.activeSelf && 
                adjacentCard.CardType == ECardType.monster && 
                adjacentCard.CardData.currentHealth > 0)
            {
                Debug.Log("SplashDamage: " + splashDamage);
                Debug.Log("DealSplashDamage to adjacentCard: " + adjacentCard.CardData.id);
                targets.Add((adjacentCard, adjIndex));
            }
        }

        // 同时对所有目标造成伤害
        if (targets.Count > 0)
        {
            List<UniTask> damageTasks = new List<UniTask>();
            foreach (var target in targets)
            {
                // Use CancellationToken.None to avoid OperationCanceledException when target dies
                // DealDamageToEnvCard will handle card cleanup internally
                damageTasks.Add(DealDamageToEnvCard(target.card, splashDamage, target.envIndex, EEffectType.Damage, CancellationToken.None));
            }
            
            await UniTask.WhenAll(damageTasks);
        }
    }
}

