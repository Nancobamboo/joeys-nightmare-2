using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YBloodyBattle_UseSkill : YCardEffect
{
    public int baseExtra;

    public YBloodyBattle_UseSkill(int baseExtra)
    {
        Id = ECardEffectId.BloodyBattle_UseSkill;
        this.baseExtra = baseExtra;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.BloodyBattleActivate, baseExtra);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private void ResetBloodyBattleState()
    {
    }

    void BloodyBattleActivate(object[] paraArray)
    {
        int baseExtra = (int)paraArray[0];

        DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
        int currentHealth = playerData.playerHealth;
        int damageToDeal = 0;

        if (currentHealth > 1)
        {
            int healthLost = currentHealth - 1;
            ApplyPlayerHealthChange(-healthLost);
            damageToDeal = healthLost;
        }

        if (damageToDeal < 10)
        {
            damageToDeal = 10;
        }

        JoeyGameControl.Instance.QueueAction(EActionId.TakeAllEnemyDamage, damageToDeal);

        m_BlockDamagePhaseEnd = PhaseCounter + baseExtra;
    }
}

