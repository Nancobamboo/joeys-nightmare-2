using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YDodgeChicken : YDefaultEffect
{
    public YDodgeChicken()
    {
        Id = ECardEffectId.DodgeChicken;
    }

    public override float OnDealDamage()
    {
        // Counter attack animation
        return base.OnDealDamage();
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        // Don't swap immediately on taking damage - wait until after counter attack
        return base.OnTakeDamage(effectType, damage);
    }
}

public partial class UIGamePhaseControl
{
    void SwapEnvCardWithRandom(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        SwapEnvCardWithRandom(cardControl);
    }

    void SwapEnvCardWithRandom(UICardSimpleControl cardControl)
    {
        if (cardControl == null || m_EnvCardDict == null || m_EnvCardDict.Count == 0)
        {
            return;
        }

        List<UICardSimpleControl> allEnvCards = new List<UICardSimpleControl>();
        foreach (var kvp in m_EnvCardDict)
        {
            if (kvp.Value != null)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    UICardSimpleControl envCard = kvp.Value[i];
                    if (envCard != null && envCard != cardControl && envCard.gameObject.activeSelf)
                    {
                        allEnvCards.Add(envCard);
                    }
                }
            }
        }

        if (allEnvCards.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, allEnvCards.Count);
        UICardSimpleControl randomCard = allEnvCards[randomIndex];

        int cardControlEnvIndex = cardControl.EnvIndex;
        int randomCardEnvIndex = randomCard.EnvIndex;

        if (m_EnvCardDict.TryGetValue(cardControlEnvIndex, out List<UICardSimpleControl> cardControlList))
        {
            cardControlList.Remove(cardControl);
        }
        if (m_EnvCardDict.TryGetValue(randomCardEnvIndex, out List<UICardSimpleControl> randomCardList))
        {
            randomCardList.Remove(randomCard);
        }

        cardControl.EnvIndex = randomCardEnvIndex;
        randomCard.EnvIndex = cardControlEnvIndex;

        AddEnvCard(randomCardEnvIndex, cardControl);
        AddEnvCard(cardControlEnvIndex, randomCard);

        VerticalLayoutGroup cardControlParent = m_EnvPanels[randomCardEnvIndex];
        VerticalLayoutGroup randomCardParent = m_EnvPanels[cardControlEnvIndex];

        cardControl.CacheTrans.SetParent(cardControlParent.transform);
        randomCard.CacheTrans.SetParent(randomCardParent.transform);

        int cardControlSiblingIndex = cardControl.CacheTrans.GetSiblingIndex();
        int randomCardSiblingIndex = randomCard.CacheTrans.GetSiblingIndex();

        cardControl.CacheTrans.SetSiblingIndex(randomCardSiblingIndex);
        randomCard.CacheTrans.SetSiblingIndex(cardControlSiblingIndex);
    }
}

