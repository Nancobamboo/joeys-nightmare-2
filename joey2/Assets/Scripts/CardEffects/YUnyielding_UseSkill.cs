using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YUnyielding_UseSkill : YCardEffect
{
    public int baseExtra;

    public YUnyielding_UseSkill(int baseExtra)
    {
        Id = ECardEffectId.Unyielding_UseSkill;
        this.baseExtra = baseExtra;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.UnyieldingActivate, baseExtra);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private int m_BlockedDamage = 0;
    private int m_RemainingTurnsPhaseEnd = 0;
    private int m_BlockDamagePhaseEnd = 0;

    private void ResetUnyieldingState()
    {
        PhaseCounter = 0;
        m_BlockedDamage = 0;
        m_RemainingTurnsPhaseEnd = 0;
        m_BlockDamagePhaseEnd = 0;
    }

    void UnyieldingActivate(object[] paraArray)
    {
        int baseExtra = (int)paraArray[0];

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        if (playerData.playerHealth > 1)
        {
            int healthLost = playerData.playerHealth - 1;
            ApplyPlayerHealthChange(-healthLost);
        }

        AddBlockDamagePhaseEnd(baseExtra);
        AddRemainingTurnsPhaseEnd(baseExtra);
    }

    void AddBlockDamagePhase(object[] paraArray)
    {
        int baseExtra = (int)paraArray[0];
        m_BlockDamagePhaseEnd = PhaseCounter + baseExtra;
    }

    private void AddBlockDamagePhaseEnd(int baseExtra)
    {
        m_BlockedDamage = 0;
        m_BlockDamagePhaseEnd = PhaseCounter + baseExtra;
    }

    private void AddRemainingTurnsPhaseEnd(int baseExtra)
    {
        m_RemainingTurnsPhaseEnd = PhaseCounter + baseExtra;
    }

    private bool TryBlockFatalDamage(int damage)
    {
        if (m_BlockDamagePhaseEnd == 0)
        {
            return false;
        }

        if (PhaseCounter >= m_BlockDamagePhaseEnd)
        {
            return false;
        }
        m_BlockedDamage += damage;

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        int currentHealth = playerData.playerHealth;

        if (damage < currentHealth || currentHealth <= 0)
        {
            return false;
        }

        return true;
    }

    private void TryUnyieldingTurnUpdate()
    {
        if (m_RemainingTurnsPhaseEnd > 0 && PhaseCounter == m_RemainingTurnsPhaseEnd && m_BlockedDamage > 0)
        {
            AddHp(m_BlockedDamage);
            m_BlockedDamage = 0;
            m_RemainingTurnsPhaseEnd = 0;
        }
    }
}

