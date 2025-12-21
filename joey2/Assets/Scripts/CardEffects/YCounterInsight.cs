using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// CounterInsight relic: Dodge and counter-attack after being attacked 5 times
// Count doubles when no defence card equipped
public partial class UIGamePhaseControl
{
    private int m_CounterInsightCounter = 0;
    private const int COUNTER_INSIGHT_THRESHOLD = 1;  // Changed to 1 for testing - triggers every time

    private void ResetCounterInsightState()
    {
        m_CounterInsightCounter = 0;
        
        // Update UI to show reset counter
        UpdateCounterInsightDisplay();
    }

    private void IncrementCounterInsight()
    {
        if (!DataSystem.Instance.HasRelic(ERelicType.CounterInsight))
        {
            return;
        }

        // Check if player has no defence card equipped
        bool hasNoDefence = !HasBagCard(ECardType.defence);
        int increment = hasNoDefence ? 2 : 1;
        
        m_CounterInsightCounter += increment;
        
        Debug.Log($"CounterInsight: Counter increased by {increment}, now at {m_CounterInsightCounter}/{COUNTER_INSIGHT_THRESHOLD}");
        
        // Update the UI display for CounterInsight relic
        UpdateCounterInsightDisplay();
    }

    private void UpdateCounterInsightDisplay()
    {
        if (!DataSystem.Instance.HasRelic(ERelicType.CounterInsight))
        {
            Debug.Log("[CounterInsight] Player doesn't have CounterInsight relic");
            return;
        }

        Debug.Log($"[CounterInsight] UpdateCounterInsightDisplay called, m_RelicList.Count: {m_RelicList.Count}");

        // Find the CounterInsight relic control in the list
        for (int i = 0; i < m_RelicList.Count; i++)
        {
            if (m_RelicList[i].RelicData != null)
            {
                Debug.Log($"[CounterInsight] Checking relic[{i}]: id={m_RelicList[i].RelicData.id}, name={m_RelicList[i].RelicData.name}");
            }
            
            if (m_RelicList[i].RelicData != null && m_RelicList[i].RelicData.id == (int)ERelicType.CounterInsight)
            {
                Debug.Log($"[CounterInsight] Found CounterInsight relic at index {i}");
                
                if (m_CounterInsightCounter >= COUNTER_INSIGHT_THRESHOLD)
                {
                    // Ready to trigger, show max count
                    m_RelicList[i].UpdateCounter(COUNTER_INSIGHT_THRESHOLD, COUNTER_INSIGHT_THRESHOLD);
                }
                else
                {
                    // Show current progress
                    m_RelicList[i].UpdateCounter(m_CounterInsightCounter, COUNTER_INSIGHT_THRESHOLD);
                }
                break;
            }
        }
    }

    private async UniTask<bool> TryCounterInsightDodge(UICardSimpleControl enemyCardControl, int enemyAttack, int envIndex, CancellationToken cancellationToken)
    {
        if (!DataSystem.Instance.HasRelic(ERelicType.CounterInsight))
        {
            return false;
        }

        if (m_CounterInsightCounter < COUNTER_INSIGHT_THRESHOLD)
        {
            return false;
        }

        // Trigger dodge and counter-attack
        Debug.Log($"CounterInsight: Triggered! Dodging attack and counter-attacking");
        
        // Reset counter
        m_CounterInsightCounter = 0;
        
        // Update UI display to show reset
        UpdateCounterInsightDisplay();
        
        // Get weapon card to display defence animation on it
        UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
        if (weaponCard == null)
        {
            weaponCard = m_FistCardCache;
        }
        
        // Play defence animation on weapon card
        if (weaponCard != null)
        {
            var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            float vfxDelay = weaponCard.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
            SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
            await UniTask.WaitForSeconds(vfxDelay > 0f ? vfxDelay : 0.3f, cancellationToken: cancellationToken);
        }
        else
        {
            // Fallback to player visual effect if no weapon card found
            JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_glow, m_View.Joey, 0.5f);
            SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
            await UniTask.WaitForSeconds(0.3f, cancellationToken: cancellationToken);
        }
        
        // Counter-attack with weapon card's attack value (including fist card)
        int counterDamage = 0;
        if (weaponCard != null)
        {
            counterDamage = weaponCard.CardData.currentAttack;
            Debug.Log($"CounterInsight: Using weapon card attack: {counterDamage}");
        }
        else
        {
            // Fallback to player's base attack if no weapon found
            counterDamage = m_DataJoeyPlayer.playerAttack;
            Debug.Log($"CounterInsight: Using player base attack: {counterDamage}");
        }
        
        // Play attack animation on weapon card and deal counter damage
        if (counterDamage > 0)
        {
            Debug.Log($"CounterInsight: Counter-attacking with {counterDamage} damage");
            
            // Play attack animation on weapon card
            if (weaponCard != null)
            {
                float attackAnimDelay = weaponCard.CardEffect?.OnDealDamage() ?? 0.3f;
                await UniTask.WaitForSeconds(attackAnimDelay, cancellationToken: cancellationToken);
            }
            
            // Deal counter damage to enemy
            await AttackSpecialEnemy(enemyCardControl, counterDamage, envIndex, cancellationToken);
        }
        else
        {
            Debug.Log($"CounterInsight: No counter damage (counterDamage = 0)");
        }
        
        return true;
    }
}

