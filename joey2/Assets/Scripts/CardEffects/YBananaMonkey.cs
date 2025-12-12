using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBananaMonkey : YDefaultEffect
{
    public YBananaMonkey()
    {
        Id = ECardEffectId.BananaMonkey;
    }

    public override float OnDead()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.BananaMonkeyDead);
        }
        return base.OnDead();
    }
}

public partial class UIGamePhaseControl
{
    public void OnBananaMonkeyDead()
    {
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
            {
                Card cardData = lastCard.CardData;
                cardData.currentHealth += 3;
                lastCard.RefreshCard();
            }
        }
    }
}

