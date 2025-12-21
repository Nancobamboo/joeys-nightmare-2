// Scripts/CardEffects/YDualWield_UseSkill.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YDualWield_UseSkill : YCardEffect
{
    public YDualWield_UseSkill()
    {
        Id = ECardEffectId.DualWield_UseSkill;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.DualWieldActivate, CardControl);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private bool m_DualWieldActive = false;

    private void ResetDualWieldState()
    {
        m_DualWieldActive = false;
    }

    void DualWieldActivate(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        
        // Double the weapon attack damage
        YActionSystem.Instance.DispatchAction(EActionId.DoubleLastWeaponAttack, cardControl);
        
        // Set flag to prevent using defence cards
        m_DualWieldActive = true;
        
        Debug.Log("DualWield activated: Next attack will deal double damage, defence cards disabled");
    }

    private void ConsumeDualWieldBuff()
    {
        if (m_DualWieldActive)
        {
            m_DualWieldActive = false;
            Debug.Log("DualWield consumed: Defence cards enabled again");
        }
    }

    public bool IsDualWieldActive()
    {
        return m_DualWieldActive;
    }
}

