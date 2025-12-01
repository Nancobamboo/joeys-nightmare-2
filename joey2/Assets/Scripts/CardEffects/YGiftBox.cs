using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YGiftBox : YCardEffect
{

    public int baseExtra;
    public YGiftBox(int baseExtra)
    {
        Id = ECardEffectId.GiftBox;
        this.baseExtra = Mathf.Max(0, baseExtra);
    }

    public override float UseItem()
    {
		List<Card> cards = new List<Card>();
        int count = baseExtra;
        while (count > 0)
        {
            Card card = GData.Instance.RandomCard();
            Debug.Log("GiftBox card: " + card.GetCardType());
            // Filter out monster cards and cards with price = 0
            if (card.GetCardType() == ECardType.monster || card.price == 0)
            {
                continue;
            }
            cards.Add(card);
            count--;
        }
		YActionSystem.Instance.DispatchAction(EActionId.AddCardsToEnv, CardControl, cards);
		return 0f;
    }
}

