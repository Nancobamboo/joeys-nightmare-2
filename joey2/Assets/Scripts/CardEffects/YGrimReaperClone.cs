using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class YGrimReaperClone : YDefaultEffect
{
    public UICardSimpleControl m_OriginalGrimReaper;

    public YGrimReaperClone()
    {
        Id = ECardEffectId.GrimReaperClone;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && m_OriginalGrimReaper != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.GrimReaperCloneTakeDamage, CardControl, m_OriginalGrimReaper);
        }
        return base.OnTakeDamage(effectType);
    }
}

public partial class UIGamePhaseControl
{
    public void GrimReaperCloneTakeDamage(UICardSimpleControl cloneCardControl, UICardSimpleControl originalGrimReaper)
    {
        if (cloneCardControl == null || originalGrimReaper == null)
        {
            return;
        }
        bool isClone = cloneCardControl != originalGrimReaper;
        if (isClone && m_GrimReaperCloneList.Contains(cloneCardControl))
        {
            m_GrimReaperCloneList.Remove(cloneCardControl);
        }
        int envIndex = cloneCardControl.EnvIndex;
        if (m_GrimReaperCloneList.Count == 0)
        {

            return;
        }
        if (ControlUtil.IsRandomSucceed(50))
        {
            if (isClone && cloneCardControl.IsEnv && envIndex >= 0)
            {
                RemoveEnvCard(envIndex, cloneCardControl);
            }
            if (originalGrimReaper != null && originalGrimReaper.CardData != null)
            {
                UICardSimpleControl targetEnemy = GetLastEnvCard(envIndex);
                if (targetEnemy != null)
                {
                    int attackCount = 1;
                    YActionSystem.Instance.DispatchAction(EActionId.TakeEnemyDamage, targetEnemy, attackCount, envIndex);
                }
            }
        }
        else
        {
            if (isClone && cloneCardControl.IsEnv && envIndex >= 0)
            {
                RemoveEnvCard(envIndex, cloneCardControl);
            }
            if (m_GrimReaperOriginal != null && m_GrimReaperOriginal.CardData != null)
            {
                MoveGrimReaperToEnvSlot(m_GrimReaperOriginal, envIndex);
            }
        }
    }

    private void MoveGrimReaperToEnvSlot(UICardSimpleControl grimReaperCard, int envIndex)
    {
        if (grimReaperCard == null || grimReaperCard.CardData == null)
        {
            return;
        }
        if (m_EnvPanels == null || envIndex < 0 || envIndex >= m_EnvPanels.Count)
        {
            return;
        }
        ECardType cardType = grimReaperCard.CardType;
        if (m_BagCardDict.TryGetValue((int)cardType, out List<UICardSimpleControl> cardList))
        {
            cardList.Remove(grimReaperCard);
        }
        VerticalLayoutGroup parent = m_EnvPanels[envIndex];
        Card newCard = grimReaperCard.CardData.Clone();
        m_CardDict[newCard.UniqueId] = newCard;
        RemoveCardData(grimReaperCard.CardData.UniqueId);
        grimReaperCard.Return();
        UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
        newCardControl.SetData(newCard, isEnv: true, envIndex: envIndex);
        AddEnvCard(envIndex, newCardControl);
        newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
        m_GrimReaperOriginal = newCardControl;
    }
}

