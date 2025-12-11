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

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.UpdateAttack, 1);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.UpdateAttack)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                JoeyGameControl.Instance.UpdateBadMonkeyAttack(CardControl);
            }
        }
        return value;
    }
}

public partial class UIGamePhaseControl
{
    public void UpdateBadMonkeyAttack(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardEffect == null || cardControl.CardEffect.Id != ECardEffectId.BadMonkey)
        {
            return;
        }

        int monsterCount = 0;
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
            {
                monsterCount++;
            }
        }

        YBadMonkey badMonkeyEffect = cardControl.CardEffect as YBadMonkey;
        if (badMonkeyEffect != null)
        {
            if (badMonkeyEffect.InitialAttack == 0 && cardControl.CardData != null)
            {
                badMonkeyEffect.InitialAttack = cardControl.CardData.currentAttack;
            }
            Card cardData = cardControl.CardData;
            cardData.SetAttack( monsterCount * badMonkeyEffect.baseExtra + badMonkeyEffect.InitialAttack);
            cardControl.RefreshCard();
        }
    }
}

