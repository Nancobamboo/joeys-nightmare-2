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
        DataJoeyPlayer dataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
        int count = baseExtra;
        while (count > 0)
        {
            Card originalCard = GData.Instance.RandomCard();
            Debug.Log("GiftBox card: " + originalCard.GetCardType());
            // Filter out monster cards and cards with price = 0
            if (originalCard.GetCardType() == ECardType.monster || originalCard.price == 0)
            {
                continue;
            }
            
            // 克隆卡牌并从 EnvCardDict 获取已增强的属性值
            Card card = originalCard.Clone();
            Card enhancedCard = dataJoeyPlayer.GetEnvCardDictData(card.id);
            if (enhancedCard != null && (enhancedCard.GetCardType() == ECardType.attack || enhancedCard.GetCardType() == ECardType.defence))
            {
                card.SetAttack(enhancedCard.currentAttack);
                card.SetDefence(enhancedCard.currentDefence);
            }
            
            cards.Add(card);
            count--;
        }
		YActionSystem.Instance.DispatchAction(EActionId.AddCardsToEnv, CardControl, cards);
		return 0f;
    }
}

