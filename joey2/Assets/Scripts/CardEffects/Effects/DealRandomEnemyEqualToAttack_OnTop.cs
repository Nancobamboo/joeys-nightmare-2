// Scripts/CardEffects/Effects/DealRandomEnemyEqualToAttack_OnTop.cs
using System.Collections;
using UnityEngine;

public class DealRandomEnemyEqualToAttack_OnTop : ICardEffect
{
    public string Id => "DealRandomEnemyEqualToAttack_OnTop";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnBecomeTopOfPile;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // 取一个随机怪
        var enemy = BattleManager.Instance.GetRandomEnemy();
        yield return VFX.PlayAnimator(ctx.source, "UI_Carditem_gongji");
        if (enemy == null) yield break;
        Debug.Log("DealRandomEnemyEqualToAttack_OnTop: enemy = " + enemy.name);

        var srcCd = ctx.source.GetComponent<CardDisplay>();
        int damage = Mathf.Max(0, srcCd.card.attack);

        // 可选：播放VFX/Shake
        
        yield return new WaitForSeconds(0.4f);
        // VFX.StoHit(ctx.source, enemy);

        BattleManager.Instance.ApplyDamageToEnemy(enemy, damage,false);
        // 补一个触发：造成伤害
        // EffectRunner.Instance.Raise(CardTrigger.OnDealDamage, ctx.source, enemy, damage);
    }
}