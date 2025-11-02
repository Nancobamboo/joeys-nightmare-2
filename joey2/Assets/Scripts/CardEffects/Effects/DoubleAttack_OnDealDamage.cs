// Scripts/CardEffects/Effects/DoubleAttack_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleAttack_OnDealDamage : ICardEffect
{
    public string Id => "DoubleAttack_OnDealDamage";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnDealDamage;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // Check if this is a double attack card and hasn't completed second attack yet
        var tracker = ctx.source != null ? ctx.source.GetComponent<DoubleAttackTracker>() : null;
        if (tracker == null || !tracker.isDoubleAttack)
        {
            yield break;
        }

        // Skip if this is the second attack's OnDealDamage (to avoid infinite loop)
        if (ctx.extra != null && ctx.extra.ContainsKey("isSecondAttack") && (bool)ctx.extra["isSecondAttack"])
        {
            // This is the second attack, mark as completed and destroy the card
            tracker.attackCount = 2;
            tracker.isDoubleAttack = false;
            Debug.Log("DoubleAttack_OnDealDamage: 第二次攻击完成，销毁卡牌");
            
            // Wait a bit to ensure all effects are processed
            yield return new WaitForSeconds(0.1f);
            
            // Destroy the card after second attack completes
            GameEvents.RaiseCardFinished(ctx.source);
            yield break;
        }

        tracker.attackCount++;
        
        // If this is the first attack, perform second attack
        if (tracker.attackCount == 1)
        {
            Debug.Log("DoubleAttack_OnDealDamage: 第一次攻击完成，执行第二次攻击");
            
            var srcCd = ctx.source.GetComponent<CardDisplay>();
            if (srcCd == null || srcCd.card == null) yield break;
            
            int damage = Mathf.Max(0, srcCd.card.currentAttack);
            var target = ctx.target != null ? ctx.target : BattleManager.Instance.GetRandomEnemy();
            
            if (target != null)
            {
                // Wait a bit before second attack
                yield return new WaitForSeconds(0.3f);
                
                // Perform second attack (without monster counterattack)
                yield return VFX.PlayHit(ctx.source, target, damage, false, extra: new Dictionary<string, object> { { "isSecondAttack", true } });
            }
        }
    }
}

