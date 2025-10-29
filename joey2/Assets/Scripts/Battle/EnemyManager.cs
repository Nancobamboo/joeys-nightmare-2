using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager
{

    public static GameObject GetRandomEnemy( List<Transform> envPanels )
    {
        List<GameObject> monsterCards = new List<GameObject>();
        foreach (var panel in envPanels)
        {
            GameObject cardObj = UIGridHelper.GetCardListOrderIndex0(panel);
            if (cardObj != null)
            {
                var cd = cardObj.GetComponent<CardDisplay>();
                if (cd != null && cd.card != null && cd.card.type == "monster" && cd.card.health > 0)
                {
                    monsterCards.Add(cardObj);
                }
            }
        }
        if (monsterCards.Count == 0)
            return null;
        int randIdx = Random.Range(0, monsterCards.Count);
        return monsterCards[randIdx];
    }


}

