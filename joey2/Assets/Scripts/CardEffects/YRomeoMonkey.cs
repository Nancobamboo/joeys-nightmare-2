using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YRomeoMonkey : YDefaultEffect
{
    public YRomeoMonkey()
    {
        Id = ECardEffectId.RomeoMonkey;
    }

    public override float OnDead()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.RomeoMonkeyDead);
        }
        return base.OnDead();
    }
}

public partial class UIGamePhaseControl
{
    public void OnRomeoMonkeyDead()
    {
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardEffect != null && lastCard.CardEffect.Id == ECardEffectId.JulietMonkey)
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

