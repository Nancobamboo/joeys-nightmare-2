using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YDartScroll : YCardEffect
{
    public YDartScroll()
    {
        Id = ECardEffectId.DartScroll;
    }

    public override float UseItem()
    {
        YActionSystem.Instance.DispatchAction(EActionId.AddCardsToEnvByCardId, CardControl, "1003", 3);
        return 0f;
    }
}

public partial class UIGamePhaseControl
{
    public void AddCardsToEnvByCardId(UICardSimpleControl cardControl, string cardId, int count)
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < count; i++)
        {
            Card card = GData.Instance.GetCardConfigById(cardId).Clone();
            cards.Add(card);
        }
        int envIndex = Random.Range(0, m_EnvPanels.Count);
        AddEnvDropCard(cards, envIndex);
    }
}
