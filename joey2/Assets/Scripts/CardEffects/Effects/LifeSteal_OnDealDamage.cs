// Scripts/CardEffects/Effects/LifeSteal_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class LifeSteal_OnDealDamage : ICardEffect
{
    public string Id => "LifeSteal_OnDealDamage";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnDealDamage;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        int heal = Mathf.Max(0, ctx.value);
        // 可选：播放吸血特效
        yield return VFX.PlayLifeSteal(ctx.source);

        PData.Instance.playerHealth += heal;
        yield break;
    }
}