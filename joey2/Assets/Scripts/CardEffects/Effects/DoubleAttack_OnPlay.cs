// Scripts/CardEffects/Effects/DoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class DoubleAttack_OnPlay : ICardEffect
{
    public string Id => "DoubleAttack_OnPlay";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

        public IEnumerator Execute(CardEffectContext ctx)
        {
            PData.Instance.nextAttackPlayTwoCards = true;
            yield return null;
        }
}

