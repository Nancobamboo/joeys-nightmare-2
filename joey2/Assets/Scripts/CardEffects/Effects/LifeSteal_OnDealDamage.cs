// Scripts/CardEffects/Effects/LifeSteal_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class LifeSteal_OnDealDamage : ICardEffect
{
    public int baseExtra; // CSV 传入的基础吸血量
    public LifeSteal_OnDealDamage(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }
    public string Id => "LifeSteal_OnDealDamage";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnDealDamage;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        int heal = baseExtra;
        // 可选：播放吸血特效
        yield return VFX.PlayLifeSteal(ctx.source);

        // Calculate new health (don't exceed max health)
        int newHealth = Mathf.Min(
            PData.Instance.playerHealth + heal,
            PData.Instance.playerMaxHealth
        );
        PData.Instance.SetPlayerHP(newHealth);
        yield break;
    }
}