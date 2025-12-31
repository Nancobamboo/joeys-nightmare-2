using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YJulietMonkey : YDefaultEffect
{
    public YJulietMonkey()
    {
        Id = ECardEffectId.JulietMonkey;
    }

    public override float OnDead()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.JulietMonkeyDead);
        }
        return base.OnDead();
    }
}

public partial class UIGamePhaseControl
{
    public void OnJulietMonkeyDead()
    {
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardEffect != null && lastCard.CardEffect.Id == ECardEffectId.RomeoMonkey)
            {
                Card cardData = lastCard.CardData;
                cardData.SetAttack(cardData.currentAttack + 5);
                // Update both currentHealth and maxHealth for proper culling blade calculation
                cardData.currentHealth += 10;
                cardData.health += 10;
                lastCard.RefreshCard();
            }
        }
    }
}

