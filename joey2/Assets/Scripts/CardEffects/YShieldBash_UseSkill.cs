// Scripts/CardEffects/YShieldBash_UseSkill.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YShieldBash_UseSkill : YCardEffect
{
    public YShieldBash_UseSkill()
    {
        Id = ECardEffectId.ShieldBash_UseSkill;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.ShieldBashActivate, CardControl);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private bool m_ShieldBashActive = false;

    private void ResetShieldBashState()
    {
        m_ShieldBashActive = false;
    }

    void ShieldBashActivate(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        
        // Get current defence value
        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);
        if (defenceCard != null)
        {
            int defenceValue = defenceCard.CardData.currentDefence;
            int defenceEffect = defenceCard.CardEffect?.GetEffectValue(EEffectType.Defence) ?? 0;
            int totalDefence = defenceValue + defenceEffect;
            
            // Add defence value as extra damage to attack card
            YActionSystem.Instance.DispatchAction(EActionId.AddEffectValueToBagCard, 
                ECardType.attack, EEffectType.Damage, totalDefence);
            
            m_ShieldBashActive = true;
            
            Debug.Log($"ShieldBash activated: Added {totalDefence} extra damage from defence");
        }
        else
        {
            Debug.Log("ShieldBash: No defence card equipped, no extra damage added");
        }
    }

    private void ConsumeShieldBashBuff()
    {
        if (m_ShieldBashActive)
        {
            m_ShieldBashActive = false;
            Debug.Log("ShieldBash consumed");
        }
    }

    public bool IsShieldBashActive()
    {
        return m_ShieldBashActive;
    }
}

