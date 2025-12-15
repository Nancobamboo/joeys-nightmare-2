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
        int attemptLimit = 100; // Prevent infinite loop
        int attempts = 0;
        
        while (count > 0 && attempts < attemptLimit)
        {
            attempts++;
            
            // Use deterministic seed based on level seed and counter
            int seed = dataJoeyPlayer.levelRandomSeed + dataJoeyPlayer.giftBoxUseCounter;
            Card originalCard = GData.Instance.RandomCardWithSeed(seed);
            dataJoeyPlayer.giftBoxUseCounter++;
            
            Debug.Log("GiftBox card: " + originalCard.GetCardType());
            // Filter out monster cards and cards with price = 0
            if (originalCard.GetCardType() == ECardType.monster || originalCard.price == 0)
            {
                continue;
            }
            
            // Clone card and get enhanced attributes from EnvCardDict
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

