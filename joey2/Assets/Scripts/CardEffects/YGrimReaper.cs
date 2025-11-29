using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
            CardControl.AddBuff(EBuffType.Counter, 5);
            YActionSystem.Instance.DispatchAction(EActionId.CreateGrimReaperClone, CardControl);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            value--;
        }
        return value;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl == null)
        {
            return base.OnTakeDamage(effectType);
        }

        int counter = CardControl.GetBuffValue(EBuffType.Counter);

        if (counter >= 3)
        {
            if (ControlUtil.IsRandomSucceed(20))
            {
                YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, true);
            }
            else
            {
                YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, false);
            }
        }
        else if (counter < 3)
        {
            YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, false);
        }

        return base.OnTakeDamage(effectType);
    }
}

public partial class UIGamePhaseControl
{
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

        if (m_YGrimReaperList.Count == 0)
        {
            m_YGrimReaperList.Add(cardControl);

            int currentEnvIndex = cardControl.EnvIndex;

            for (int i = 0; i < m_EnvPanels.Count; i++)
            {
                if (i != currentEnvIndex)
                {
                    VerticalLayoutGroup parent = m_EnvPanels[i];
                    Card newCard = cardControl.CardData.Clone();
                    newCard.effectId = ECardEffectId.GrimReaper.ToString();
                    m_CardDict[newCard.UniqueId] = newCard;
                    UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
                    newCardControl.SetData(newCard, isEnv: true, envIndex: i);
                    AddEnvCard(i, newCardControl);
                    m_YGrimReaperList.Add(newCardControl);
                    newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
                }
            }
        }
    }

    public void GrimReaperTakeDamage(UICardSimpleControl grimReaperCard, bool isSuccess)
    {
        if (grimReaperCard == null || grimReaperCard.CardData == null)
        {
            return;
        }

        int counter = grimReaperCard.GetBuffValue(EBuffType.Counter);
        int envIndex = grimReaperCard.EnvIndex;

        if (counter >= 3)
        {
            if (isSuccess)
            {
                int damage = grimReaperCard.CardData.currentHealth / 5;
                DealDamageToOtherGrimReapers(grimReaperCard, damage).Forget();
            }
            else
            {
                int attack = grimReaperCard.CardData.currentAttack;
                TakePlayerDamageAsync(attack, grimReaperCard, envIndex).Forget();
                RemoveEnvCard(envIndex, grimReaperCard);
                m_YGrimReaperList.Remove(grimReaperCard);

                if (counter - 1 < 3)
                {
                    SetRealGrimReaperAndAnimate();
                }
            }
        }
        else if (counter < 3)
        {
            bool isRealGrimReaper = (m_RealGrimReaperEnvIndex == envIndex);

            if (isRealGrimReaper)
            {
                int damage = grimReaperCard.CardData.currentHealth / 5;
                DealDamageToOtherGrimReapers(grimReaperCard, damage).Forget();
            }
            else
            {
                int attack = grimReaperCard.CardData.currentAttack;
                TakePlayerDamageAsync(attack, grimReaperCard, envIndex).Forget();
                RemoveEnvCard(envIndex, grimReaperCard);
                m_YGrimReaperList.Remove(grimReaperCard);
                SetRealGrimReaperAndAnimate();
            }
        }
    }

    private async UniTask DealDamageToOtherGrimReapers(UICardSimpleControl sourceGrimReaper, int damage)
    {
        for (int i = 0; i < m_YGrimReaperList.Count; i++)
        {
            UICardSimpleControl otherGrimReaper = m_YGrimReaperList[i];
            if (otherGrimReaper != sourceGrimReaper && otherGrimReaper != null && otherGrimReaper.gameObject.activeSelf)
            {
                int otherEnvIndex = otherGrimReaper.EnvIndex;
                EEffectType effectType = EEffectType.Other;
                if (otherGrimReaper.CardEffect != null)
                {
                    int reflectDamage = otherGrimReaper.CardEffect.GetEffectValue(EEffectType.ReflectDamage);
                    if (reflectDamage > 0)
                    {
                        effectType = EEffectType.ReflectDamage;
                    }
                }
                await DealDamageToEnvCard(otherGrimReaper, damage, otherEnvIndex, effectType);

                if (otherGrimReaper.CardData.currentHealth <= 0)
                {
                    m_YGrimReaperList.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private void SetRealGrimReaperAndAnimate()
    {
        if (m_YGrimReaperList.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, m_YGrimReaperList.Count);
        UICardSimpleControl realGrimReaper = m_YGrimReaperList[randomIndex];
        m_RealGrimReaperEnvIndex = realGrimReaper.EnvIndex;

        realGrimReaper.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_gongji, EVFXLife.CardLife);
    }
}
