// Scripts/CardEffects/Effects/DoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class DoubleAttack_OnPlay : ICardEffect
{
    private static bool nextAttackIsDouble = false;

    public string Id => "DoubleAttack_OnPlay";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // Mark next attack as double attack
        nextAttackIsDouble = true;
        Debug.Log("DoubleAttack_OnPlay: 标记下次攻击为双次攻击");
        yield return null;
    }

    // Static method to check and consume the flag
    public static bool CheckAndConsumeDoubleAttackFlag(GameObject attackCard)
    {
        if (nextAttackIsDouble && attackCard != null)
        {
            var cd = attackCard.GetComponent<CardDisplay>();
            if (cd != null && cd.card != null && cd.card.type == "attack")
            {
                nextAttackIsDouble = false;
                // Add a component to track double attack state
                var tracker = attackCard.GetComponent<DoubleAttackTracker>();
                if (tracker == null)
                {
                    tracker = attackCard.AddComponent<DoubleAttackTracker>();
                }
                tracker.isDoubleAttack = true;
                tracker.attackCount = 0;
                
                // Dynamically add DoubleAttack_OnDealDamage effect to the attack card
                var holder = attackCard.GetComponent<EffectHolder>();
                if (holder == null)
                {
                    holder = attackCard.AddComponent<EffectHolder>();
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
                
                Debug.Log("DoubleAttack_OnPlay: 攻击卡已标记为双次攻击");
                return true;
            }
        }
        return false;
    }
}

// Component to track double attack state on attack card
public class DoubleAttackTracker : MonoBehaviour
{
    public bool isDoubleAttack = false;
    public int attackCount = 0; // Track how many attacks have been performed
}

