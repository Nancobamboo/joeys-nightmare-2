using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YMoveWeaponToEnv : YCardEffect
{
    public YMoveWeaponToEnv()
    {
        Id = ECardEffectId.MoveWeaponToEnv;
    }

    public override float UseSkill()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.AddEnvCardFromBag, CardControl);
        }
        return base.UseSkill();
    }

    public override float UseItem()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.AddEnvCardFromBag, CardControl);
        }
        return base.UseItem();
    }
}

public partial class UIGamePhaseControl
{
    public void AddEnvCardFromBag(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        if (m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, m_EnvPanels.Count);
        VerticalLayoutGroup parent = m_EnvPanels[randomIndex];
        Card newCard = cardControl.CardData.Clone();
        m_CardDict[newCard.UniqueId] = newCard;
        UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
        newCardControl.SetData(newCard, isEnv: true, envIndex: randomIndex);
        AddEnvCard(randomIndex, newCardControl);
        newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
    }
}

