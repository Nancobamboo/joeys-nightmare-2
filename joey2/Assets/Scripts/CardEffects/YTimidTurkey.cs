using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YTimidTurkey : YDefaultEffect
{
    public YTimidTurkey()
    {
        Id = ECardEffectId.TimidTurkey;
    }

    public override float OnDealDamage()
    {
        return base.OnDealDamage();
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage)
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.SwapTopTwoEnvCards, CardControl);
        }
        return base.OnTakeDamage(effectType);
    }

    public override float OnRemoveCard()
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            Debug.Log("TimidTurkey on remove card");
            DataSystem.Instance.AddCoin(20);
        }
        return base.OnRemoveCard();
    }

}

public partial class UIGamePhaseControl
{
    public void SwapTopTwoEnvCards(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        int envIndex = cardControl.EnvIndex;
        List<UICardSimpleControl> cardList = m_EnvCardDict[envIndex];
        for (int i = 0; i < cardList.Count; i++)
        {
            Debug.Log("TimidTurkey swap before card index: " + i + " card: " + cardList[i].CardData.cardName);
        }
        if (cardList != null && cardList.Count >= 2)
        {
            UICardSimpleControl lastCard = cardList[cardList.Count - 1];
            UICardSimpleControl secondLastCard = cardList[cardList.Count - 2];
            cardList[cardList.Count - 1] = secondLastCard;
            cardList[cardList.Count - 2] = lastCard;
            m_EnvCardDict[envIndex] = cardList;
            // 更新Transform的sibling index以反映UI中的顺序
            Transform parent = lastCard.CacheTrans.parent;
            int lastSiblingIndex = lastCard.CacheTrans.GetSiblingIndex();
            int secondLastSiblingIndex = secondLastCard.CacheTrans.GetSiblingIndex();
            lastCard.CacheTrans.SetSiblingIndex(secondLastSiblingIndex);
            secondLastCard.CacheTrans.SetSiblingIndex(lastSiblingIndex);
            lastCard.RefreshCard();
            secondLastCard.RefreshCard();
        }
        for (int i = 0; i < cardList.Count; i++)
        {
            Debug.Log("TimidTurkey swap after card index: " + i + " card: " + cardList[i].CardData.cardName);
        }
        cardControl.RefreshCard();
    }
}