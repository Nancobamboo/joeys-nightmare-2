// Scripts/CardEffects/Effects/DoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class DoubleAttack_OnPlay : ICardEffect
{
    public string Id => "DoubleAttack_OnPlay";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // Find the active attack card and attach double attack effect to it
        if (BattleManager.Instance != null && BattleManager.Instance.attackPanel != null)
        {
            GameObject activeAttackCard = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.attackPanel);
            if (activeAttackCard != null)
            {
                var cd = activeAttackCard.GetComponent<CardDisplay>();
                if (cd != null && cd.card != null && cd.card.type == "attack")
                {
                    // Add tracker component
                    var tracker = activeAttackCard.GetComponent<DoubleAttackTracker>();
                    if (tracker == null)
                    {
                        tracker = activeAttackCard.AddComponent<DoubleAttackTracker>();
                    }
                    tracker.isDoubleAttack = true;
                    tracker.attackCount = 0;
                    
                    // Dynamically add DoubleAttack_OnDealDamage effect to the attack card
                    var holder = activeAttackCard.GetComponent<EffectHolder>();
                    if (holder == null)
                    {
                        holder = activeAttackCard.AddComponent<EffectHolder>();
                    }
                    // Check if effect already exists
                    bool hasEffect = false;
                    foreach (var eff in holder.effects)
                    {
                        if (eff != null && eff.Id == "DoubleAttack_OnDealDamage")
                        {
                            hasEffect = true;
                            break;
                        }
                    }
                    if (!hasEffect)
                    {
                        holder.effects.Add(new DoubleAttack_OnDealDamage());
                    }
                    
                    Debug.Log("DoubleAttack_OnPlay: 已为攻击卡挂载双次攻击效果");
                }
            }
        }
        yield return null;
    }
}

// Component to track double attack state on attack card
public class DoubleAttackTracker : MonoBehaviour
{
    public bool isDoubleAttack = false;
    public int attackCount = 0; // Track how many attacks have been performed
}

