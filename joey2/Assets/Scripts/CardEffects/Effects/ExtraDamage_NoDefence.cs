// Scripts/CardEffects/Effects/ExtraDamage_OnDealDamage.cs
using System.Collections;
using UnityEngine;

public class ExtraDamage_NoDefence : ICardEffect
{
    public int baseExtra; // CSV 传入的基础额外伤害

    public ExtraDamage_NoDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }

    public string Id => "ExtraDamage_NoDefence";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {

        GameObject defenceGO = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.defencePanel);
        if (defenceGO == null)
        {
            ctx.source.GetComponent<CardDisplay>().card.currentAttack += baseExtra;
        }
        yield return null;
    }


}