// Scripts/CardEffects/Effects/ExtraDamage_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class ExtraDamage_OnDealDamage : ICardEffect
{
    public int baseExtra; // CSV 传入的基础额外伤害

    public ExtraDamage_OnDealDamage(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }

    public string Id => "ExtraDamage_OnDealDamage";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnDealDamage;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        var enemy = ctx.target != null ? ctx.target : BattleManager.Instance.GetRandomEnemy();
        if (enemy == null) yield break;

        // 计算加成：基础额外伤害 + 根据装备/状态的加成
        // int bonusFromEquip = CalcBonusFromEquip(ctx.source);
        // int extra = Mathf.Max(0, baseExtra + bonusFromEquip);

        // if (extra <= 0) yield break;

        // 可加一个不同的VFX
        yield return VFX.PlayHit(ctx.source, enemy);

        BattleManager.Instance.ApplyDamageToEnemy(enemy, baseExtra);

        // 如需链式效果，可再次广播造成伤害
        EffectRunner.Instance.Raise(CardTrigger.OnDealDamage, ctx.source, enemy, baseExtra);
    }

    // // 示例：根据当前装备/面板做简单加成（你可替换为更真实的规则）
    // private int CalcBonusFromEquip(GameObject source)
    // {
    //     var cd = source.GetComponent<CardDisplay>();
    //     if (cd == null || cd.card == null) return 0;

    //     int bonus = 0;
    //     // 例：按当前攻击力的10%向下取整加成
    //     bonus += Mathf.FloorToInt(cd.card.currentAttack * 0.1f);

    //     // 例：如果背包里有某张“装备”则再+1（这里仅示意，真实实现可遍历你的 attackPanel/defencePanel 列表）
    //     // if (BattleManager.Instance.HasEquip("someId")) bonus += 1;

    //     return bonus;
    // }
}