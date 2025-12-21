// Scripts/CardEffects/YStrike_UseSkill.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YStrike_UseSkill : YCardEffect
{
    public YStrike_UseSkill()
    {
        Id = ECardEffectId.Strike_UseSkill;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { EVFXName.VFX_Shouji };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_gongji, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.StrikeActivate, CardControl);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private bool m_StrikeActive = false;
    private int m_StrikeVulnerableDuration = 2;

    private void ResetStrikeState()
    {
        m_StrikeActive = false;
    }

    void StrikeActivate(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        
        // Set flag to apply vulnerable on next attack
        m_StrikeActive = true;
        
        Debug.Log("Strike activated: Next attack will apply 2 turns of Vulnerable debuff");
    }

    private void ApplyStrikeVulnerable(UICardSimpleControl enemyCard)
    {
        if (m_StrikeActive && enemyCard != null && enemyCard.CardType == ECardType.monster)
        {
            // Add vulnerable debuff for 2 turns
            int currentVulnerable = enemyCard.GetBuffValue(EBuffType.Vulnerable);
            if (currentVulnerable < m_StrikeVulnerableDuration)
            {
                enemyCard.AddBuff(EBuffType.Vulnerable, m_StrikeVulnerableDuration);
                Debug.Log($"Strike: Applied Vulnerable debuff to {enemyCard.CardData.cardName} for {m_StrikeVulnerableDuration} turns");
            }
            else
            {
                Debug.Log($"Strike: {enemyCard.CardData.cardName} already has Vulnerable debuff for {currentVulnerable} turns");
            }
            
            // Play debuff visual effect
            if (enemyCard.CacheTrans != null)
            {
                JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shouji_2, enemyCard.CacheTrans, 1f);
            }
            
            // Consume the strike buff
            m_StrikeActive = false;
        }
    }

    private void ConsumeStrikeBuff()
    {
        if (m_StrikeActive)
        {
            m_StrikeActive = false;
            Debug.Log("Strike consumed without applying (enemy killed before attack hit)");
        }
    }

    private void UpdateVulnerableDebuffs()
    {
        // Decrease vulnerable duration for all monsters at the end of each turn
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
            {
                int vulnerableValue = lastCard.GetBuffValue(EBuffType.Vulnerable);
                if (vulnerableValue > 0)
                {
                    lastCard.AddBuff(EBuffType.Vulnerable, vulnerableValue - 1);
                    Debug.Log($"Vulnerable updated: {lastCard.CardData.cardName} now has {vulnerableValue - 1} turns remaining");
                }
            }
        }
    }
}

