// Scripts/CardEffects/Effects/BounceToRandomEnemy_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BounceToRandomEnemy_OnDealDamage : ICardEffect
{
    public int bounceCount; // 弹射次数
    private static int nestingLevel = 0;
    private static HashSet<GameObject> bouncingCards = new HashSet<GameObject>();
    public BounceToRandomEnemy_OnDealDamage(int bounceCount)
    {
        this.bounceCount = Mathf.Max(0, bounceCount); // 至少弹射0次
    }
    public string Id => "BounceToRandomEnemy_OnDealDamage";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnDealDamage;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // 检查这次伤害是否是由弹射造成的
        if (ctx.extra != null && ctx.extra.ContainsKey("isBounce") && (bool)ctx.extra["isBounce"])
        {
            Debug.Log("BounceToRandomEnemy_OnDealDamage: 跳过弹射伤害触发的弹射");
            yield break;
        }
        if (nestingLevel > 0)
        {
            yield break;
        }
        // 防止同一张卡同时进行多次弹射（防循环）
        if (bouncingCards.Contains(ctx.source))
        {
            yield break;
        }
        
        bouncingCards.Add(ctx.source);
        nestingLevel++;
        try{
            var srcCd = ctx.source.GetComponent<CardDisplay>();
            int damage = Mathf.Max(0, srcCd.card.attack);
            yield return VFX.PlayAnimator(ctx.source, "UI_Carditem_dunpai");

            // 弹射指定次数
            for (int i = 0; i < bounceCount; i++)
            {
                var enemy = BattleManager.Instance.GetRandomEnemy();
                if (enemy == null) break; // 没有敌人了，结束弹射
                // 播放弹射VFX
                Debug.Log("BounceToRandomEnemy_OnDealDamage: enemy = " + enemy.name);
                
                // 播放弹射VFX，并标记这是弹射伤害
                yield return VFX.PlayHit(ctx.source, enemy, damage, false, extra: new Dictionary<string, object> { { "isBounce", true } });
                yield return new WaitForSeconds(0.5f);

            }
        }
        finally
        {
            nestingLevel--;
            bouncingCards.Remove(ctx.source);
        }
        yield return new WaitForSeconds(0.1f);
        GameEvents.RaiseCardFinished(ctx.source);
    }


}