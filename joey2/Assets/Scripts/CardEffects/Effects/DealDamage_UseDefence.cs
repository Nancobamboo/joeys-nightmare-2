// Scripts/CardEffects/Effects/ExtraDamage_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class DealDamage_UseDefence : ICardEffect
{
    public int baseExtra; // CSV 传入的基础额外伤害

    public DealDamage_UseDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }

    public string Id => "DealDamage_UseDefence";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.UseDefence;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        Debug.Log("DealDamage_UseDefence: Execute");
        BattleManager.Instance.ApplyDamageToEnemy(ctx.target, baseExtra,false);
        yield return null;
    }


}