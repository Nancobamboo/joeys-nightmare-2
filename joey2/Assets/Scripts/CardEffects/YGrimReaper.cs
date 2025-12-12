using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            CardControl.AddBuff(EBuffType.Counter, 6);
            YActionSystem.Instance.DispatchAction(EActionId.CreateGrimReaperClone, CardControl);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                value--;
                if (value == 0)
                {
                    YActionSystem.Instance.DispatchAction(EActionId.GrimReaperEatClones, CardControl);
                    return 6;
                }
            }
        }
        return value;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl == null)
        {
            return base.OnTakeDamage(effectType, damage);
        }

        if (effectType == EEffectType.ReflectDamage)
        {
            return base.OnTakeDamage(effectType, damage);
        }

        YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, false, damage);

        return base.OnTakeDamage(effectType, damage);
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
        if (m_HasCreatedGrimReaper)
        {
            return;
        }

        m_HasCreatedGrimReaper = true;

        for (int i = m_YGrimReaperList.Count - 1; i >= 0; i--)
        {
            UICardSimpleControl clone = m_YGrimReaperList[i];
            if (clone != null && clone != cardControl)
            {
                RemoveCardCts(clone);
                RemoveEnvCard(clone.EnvIndex, clone);
                m_YGrimReaperList.RemoveAt(i);
            }
        }

        if (m_YGrimReaperList.Count == 0)
        {
            m_YGrimReaperList.Add(cardControl);
        }

        int currentEnvIndex = cardControl.EnvIndex;
        List<int> envIndices = new List<int>();
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            envIndices.Add(i);
        }
        envIndices.Remove(currentEnvIndex);
        ControlUtil.ShuffleList(envIndices);

        int createCount = 2;
        for (int i = 0; i < createCount && i < envIndices.Count; i++)
        {
            int envIndex = envIndices[i];
            VerticalLayoutGroup parent = m_EnvPanels[envIndex];
            Card newCard = cardControl.CardData.Clone();
            newCard.effectId = ECardEffectId.GrimReaper.ToString();
            newCard.health = cardControl.CardData.health;
            newCard.currentHealth = cardControl.CardData.currentHealth;
            m_CardDict[newCard.UniqueId] = newCard;
            UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
            newCardControl.SetData(newCard, isEnv: true, envIndex: envIndex);

            AddEnvCard(envIndex, newCardControl);
            m_YGrimReaperList.Add(newCardControl);
            newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
        }

        RandomRealGrimReaperEnvIndex();
    }

    public void GrimReaperEatClones(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        if (m_YGrimReaperList.Count == 0)
        {
            return;
        }

        UICardSimpleControl realGrimReaper = GetRealGrimReaper();
        if (realGrimReaper == null && m_YGrimReaperList.Count > 0)
        {
            realGrimReaper = m_YGrimReaperList[0];
            m_RealGrimReaperEnvIndex = realGrimReaper.EnvIndex;
        }
        if (realGrimReaper == null)
        {
            return;
        }

        int healAmount = (m_YGrimReaperList.Count - 1) * 5;

        for (int i = m_YGrimReaperList.Count - 1; i >= 0; i--)
        {
            UICardSimpleControl clone = m_YGrimReaperList[i];
            if (clone != null && clone != realGrimReaper)
            {
                RemoveCardCts(clone);
                RemoveEnvCard(clone.EnvIndex, clone);
                m_YGrimReaperList.RemoveAt(i);
            }
        }

        if (healAmount > 0)
        {
            Card cardData = realGrimReaper.CardData;
            cardData.currentHealth += healAmount;
            if (cardData.currentHealth > cardData.health)
            {
                cardData.currentHealth = cardData.health;
            }
            realGrimReaper.RefreshCard();
        }

        m_HasCreatedGrimReaper = false;
        CreateGrimReaperClone(realGrimReaper);
    }

    public void GrimReaperTakeDamage(UICardSimpleControl grimReaperCard, bool isSuccess, int damage)
    {
        if (grimReaperCard == null || grimReaperCard.CardData == null)
        {
            return;
        }

        bool isRealGrimReaper = m_RealGrimReaperEnvIndex == grimReaperCard.EnvIndex;

        if (isRealGrimReaper)
        {
            DealDamageToOtherGrimReapers(grimReaperCard, damage);
            RandomRealGrimReaperEnvIndex();
        }
        else
        {
            RemoveCardCts(grimReaperCard);
            RemoveEnvCard(grimReaperCard.EnvIndex, grimReaperCard);
            m_YGrimReaperList.Remove(grimReaperCard);
            UICardSimpleControl realGrimReaper = GetRealGrimReaper();
            if (realGrimReaper == null || realGrimReaper.CardData == null)
            {
                return;
            }
            int attack = realGrimReaper.CardData.currentAttack;
            int realEnvIndex = realGrimReaper.EnvIndex;
            GetOrCreateCardToken(realGrimReaper);
            TakePlayerDamageAsync(attack, realGrimReaper, realEnvIndex).Forget();

        }
    }

    private async void DealDamageToOtherGrimReapers(UICardSimpleControl sourceGrimReaper, int damage)
    {
        UICardSimpleControl realGrimReaper = GetRealGrimReaper();
        for (int i = m_YGrimReaperList.Count - 1; i >= 0; i--)
        {
            UICardSimpleControl otherGrimReaper = m_YGrimReaperList[i];
            if (otherGrimReaper != null && otherGrimReaper != sourceGrimReaper && otherGrimReaper != realGrimReaper)
            {
                int otherEnvIndex = otherGrimReaper.EnvIndex;
                EEffectType effectType = EEffectType.ReflectDamage;

                bool isKilled = await DealDamageToEnvCard(otherGrimReaper, damage, otherEnvIndex, effectType);

                if (isKilled)
                {
                    m_YGrimReaperList.RemoveAt(i);
                }
            }
        }
    }

    private UICardSimpleControl GetRealGrimReaper()
    {
        UICardSimpleControl realGrimReaper = GetLastEnvCard(m_RealGrimReaperEnvIndex);
        if (realGrimReaper == null)
        {
            for (int i = 0; i < m_YGrimReaperList.Count; i++)
            {
                UICardSimpleControl g = m_YGrimReaperList[i];
                if (g != null && g.EnvIndex == m_RealGrimReaperEnvIndex)
                {
                    realGrimReaper = g;
                    break;
                }
            }
        }
        return realGrimReaper;
    }

    private void RandomRealGrimReaperEnvIndex()
    {
        if (m_YGrimReaperList.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, m_YGrimReaperList.Count);
        UICardSimpleControl realGrimReaper = m_YGrimReaperList[randomIndex];
        m_RealGrimReaperEnvIndex = realGrimReaper.EnvIndex;
    }
}

