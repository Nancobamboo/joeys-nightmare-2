using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl == null)
        {
            return base.OnTakeDamage(effectType, damage);
        }

        if (effectType == EEffectType.ReflectDamage)
        {
            Debug.LogWarning("GrimReaperTakeDamage ReflectDamage");
            return base.OnTakeDamage(effectType, damage);
        }

        int counter = CardControl.GetBuffValue(EBuffType.Counter);

        if (counter >= 3)
        {
            if (ControlUtil.IsRandomSucceed(20))
            {
                YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, true, damage);
            }
            else
            {
                YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, false, damage);
            }
        }
        else if (counter < 3)
        {
            YActionSystem.Instance.DispatchAction(EActionId.GrimReaperTakeDamage, CardControl, false, damage);
        }

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

    public void GrimReaperTakeDamage(UICardSimpleControl grimReaperCard, bool isSuccess, int damage)
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
                Debug.Log($"GrimReaperTakeDamage counter >= 3, isSuccess = true");
                DealDamageToOtherGrimReapers(grimReaperCard, damage)    ;
            }
            else
            {
                Debug.Log($"GrimReaperTakeDamage counter >= 3, isSuccess = false");
                RemoveCardCts(grimReaperCard);
                RemoveEnvCard(envIndex, grimReaperCard);
                m_YGrimReaperList.Remove(grimReaperCard);
                UICardSimpleControl realGrimReaper = GetRealGrimReaper(grimReaperCard);
                int attack = realGrimReaper.CardData.currentAttack;
                int realEnvIndex = realGrimReaper.EnvIndex;
                GetOrCreateCardToken(realGrimReaper);
                TakePlayerDamageAsync(attack, realGrimReaper, realEnvIndex).Forget();
            }
        }
        else if (counter < 3)
        {
            bool isRealGrimReaper = (m_RealGrimReaperEnvIndex == envIndex);

            if (isRealGrimReaper)
            {
                Debug.Log($"GrimReaperTakeDamage counter < 3, isRealGrimReaper = true");
                DealDamageToOtherGrimReapers(grimReaperCard, damage);
            }
            else
            {
                Debug.Log($"GrimReaperTakeDamage counter < 3, isRealGrimReaper = false");
                RemoveCardCts(grimReaperCard);
                RemoveEnvCard(envIndex, grimReaperCard);
                m_YGrimReaperList.Remove(grimReaperCard);
                UICardSimpleControl realGrimReaper = GetRealGrimReaper(grimReaperCard);
                int attack = realGrimReaper.CardData.currentAttack;
                int realEnvIndex = realGrimReaper.EnvIndex;
                GetOrCreateCardToken(realGrimReaper);
                TakePlayerDamageAsync(attack, realGrimReaper, realEnvIndex).Forget();
                SetRealGrimReaperAndAnimate();
            }
        }
    }

    private void DealDamageToOtherGrimReapers(UICardSimpleControl sourceGrimReaper, int damage)
    {
        UICardSimpleControl realGrimReaper = m_YGrimReaperList.FirstOrDefault(g => g.EnvIndex == m_RealGrimReaperEnvIndex);
        for (int i = 0; i < m_YGrimReaperList.Count; i++)
        {
            UICardSimpleControl otherGrimReaper = m_YGrimReaperList[i];
            if (otherGrimReaper != sourceGrimReaper && otherGrimReaper != realGrimReaper)
            {
                int otherEnvIndex = otherGrimReaper.EnvIndex;
                EEffectType effectType = EEffectType.ReflectDamage;

                DealDamageToEnvCard(otherGrimReaper, damage, otherEnvIndex, effectType).Forget();

                if (otherGrimReaper.CardData.currentHealth <= 0)
                {
                    m_YGrimReaperList.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private UICardSimpleControl GetRealGrimReaper(UICardSimpleControl attackedGrimReaper)
    {
        if (m_YGrimReaperList.Count == 0)
        {
            return null;
        }

        UICardSimpleControl realGrimReaper = m_YGrimReaperList.FirstOrDefault(g => g != null && g.EnvIndex == m_RealGrimReaperEnvIndex);
        if (realGrimReaper == null)
        {
            List<UICardSimpleControl> remainingGrimReapers = m_YGrimReaperList.Where(g => g != null && g != attackedGrimReaper).ToList();
            if (remainingGrimReapers.Count == 0)
            {
                return null;
            }
            int randomIndex = Random.Range(0, remainingGrimReapers.Count);
            realGrimReaper = remainingGrimReapers[randomIndex];
            m_RealGrimReaperEnvIndex = realGrimReaper.EnvIndex;
        }
        return realGrimReaper;
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
    }
}
