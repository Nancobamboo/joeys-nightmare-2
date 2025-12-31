// Scripts/CardEffects/YFortress_UseSkill.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YFortress_UseSkill : YCardEffect
{
    public YFortress_UseSkill()
    {
        Id = ECardEffectId.Fortress_UseSkill;
    }

    public override float UseSkill()
    {
        if (CardControl != null && CardControl.gameObject != null)
        {
            List<EVFXName> vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

            YActionSystem.Instance.DispatchAction(EActionId.FortressActivate, CardControl);

            return 0.3f;
        }
        return base.UseSkill();
    }
}

public partial class UIGamePhaseControl
{
    private int m_FortressDefenceBonus = 0;

    private void ResetFortressState()
    {
        m_FortressDefenceBonus = 0;
    }

    async void FortressActivate(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        
        // Get current defence card
        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);
        if (defenceCard != null)
        {
            // Get total defence value (base + effects)
            int defenceValue = defenceCard.CardData.currentDefence;
            int defenceEffect = defenceCard.CardEffect?.GetEffectValue(EEffectType.Defence) ?? 0;
            int totalDefence = defenceValue + defenceEffect;
            
            // Store the defence bonus for next defence card
            m_FortressDefenceBonus = totalDefence;
            
            Debug.Log($"Fortress activated: Stored {totalDefence} defence bonus, removing current defence card");
            
            // Remove current defence card
            await RemoveBagCard(ECardType.defence, defenceCard);
            
            // Trigger defence card removal effects
            float removeDelayTime = defenceCard.CardEffect?.OnRemoveCard() ?? 0f;
            if (removeDelayTime > 0f)
            {
                await UniTask.WaitForSeconds(removeDelayTime);
            }
        }
        else
        {
            Debug.Log("Fortress: No defence card equipped, skill has no effect");
        }
    }

    private void ApplyFortressBonus(UICardSimpleControl defenceCard)
    {
        if (m_FortressDefenceBonus > 0 && defenceCard != null)
        {
            Debug.Log($"Fortress: Applying {m_FortressDefenceBonus} defence bonus to new defence card");
            defenceCard.AddEffectValue(EEffectType.Defence, m_FortressDefenceBonus);
            defenceCard.RefreshCard();
            
            // Play enhancement visual effect
            if (defenceCard.CacheTrans != null)
            {
                JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, defenceCard.CacheTrans, 1f);
            }
            
            // Consume the fortress bonus
            m_FortressDefenceBonus = 0;
        }
    }
}

