using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBadMonkey : YDefaultEffect
{
    public int baseExtra;
    public int InitialAttack = 0;

    public YBadMonkey(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.BadMonkey;
    }
}

public partial class UIGamePhaseControl
{
    private void UpdateBadMonkeyAttack()
    {
        int monsterCount = 0;
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
            {
                monsterCount++;
            }
        }

        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardEffect != null && lastCard.CardEffect.Id == ECardEffectId.BadMonkey)
            {
                YBadMonkey badMonkeyEffect = lastCard.CardEffect as YBadMonkey;
                if (badMonkeyEffect != null)
                {
                    if (badMonkeyEffect.InitialAttack == 0 && lastCard.CardData != null)
                    {
                        badMonkeyEffect.InitialAttack = lastCard.CardData.attack;
                    }
                    Card cardData = lastCard.CardData;
                    cardData.currentAttack = monsterCount * badMonkeyEffect.baseExtra + badMonkeyEffect.InitialAttack;
                    lastCard.RefreshCard();
                }
            }
        }
    }
}

