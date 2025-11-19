using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YGravityShoes : YDefaultEffect
{
    public YGravityShoes()
    {
        Id = ECardEffectId.GravityShoes;
    }

    public override float UseItem()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.SwapEnvCard);
        }
        return base.UseItem();
    }
}

public partial class UIGamePhaseControl
{
    public void SwapEnvCard()
    {
        foreach (KeyValuePair<int, List<UICardSimpleControl>> kvp in m_EnvCardDict)
        {
            int envIndex = kvp.Key;
            List<UICardSimpleControl> cardList = kvp.Value;
            if (cardList != null && cardList.Count >= 2)
            {
                int lastIndex = cardList.Count - 1;
                int secondLastIndex = cardList.Count - 2;

                UICardSimpleControl lastCard = cardList[lastIndex];
                UICardSimpleControl secondLastCard = cardList[secondLastIndex];

                cardList[lastIndex] = secondLastCard;
                cardList[secondLastIndex] = lastCard;

                int lastSiblingIndex = lastCard.CacheTrans.GetSiblingIndex();
                int secondLastSiblingIndex = secondLastCard.CacheTrans.GetSiblingIndex();
                lastCard.CacheTrans.SetSiblingIndex(secondLastSiblingIndex);
                secondLastCard.CacheTrans.SetSiblingIndex(lastSiblingIndex);
            }
        }
    }
}

