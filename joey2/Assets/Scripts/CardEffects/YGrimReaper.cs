using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YGrimReaper : YDefaultEffect
{
    public YGrimReaper()
    {
        Id = ECardEffectId.GrimReaper;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 3);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter && value == 0)
        {
            if (CardControl != null)
            {
                YActionSystem.Instance.DispatchAction(EActionId.CreateGrimReaperClone, CardControl);
            }
        }
        return value;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.GrimReaperCloneTakeDamage, CardControl, CardControl);
        }
        return base.OnTakeDamage(effectType);
    }
}

public partial class UIGamePhaseControl
{
    private List<UICardSimpleControl> m_GrimReaperCloneList = new List<UICardSimpleControl>();
    private UICardSimpleControl m_GrimReaperOriginal = null;

    public void CreateGrimReaperClone(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        if (m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }
        m_GrimReaperOriginal = cardControl;
        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, m_EnvPanels.Count);
            VerticalLayoutGroup parent = m_EnvPanels[randomIndex];
            Card newCard = cardControl.CardData.Clone();
            newCard.effectId = ECardEffectId.GrimReaperClone.ToString();
            m_CardDict[newCard.UniqueId] = newCard;
            UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
            newCardControl.SetData(newCard, isEnv: true, envIndex: randomIndex);
            if (newCardControl.CardEffect != null && newCardControl.CardEffect is YGrimReaperClone)
            {
                YGrimReaperClone cloneEffect = newCardControl.CardEffect as YGrimReaperClone;
                cloneEffect.m_OriginalGrimReaper = cardControl;
            }
            AddEnvCard(randomIndex, newCardControl);
            m_GrimReaperCloneList.Add(newCardControl);
            newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
        }
        if (cardControl.CardEffect != null && cardControl.CardEffect is YGrimReaper)
        {
            cardControl.AddBuff(EBuffType.Counter, 3);
        }
    }

    private void ClearGrimReaperData()
    {
        m_GrimReaperCloneList.Clear();
        m_GrimReaperOriginal = null;
    }
}

