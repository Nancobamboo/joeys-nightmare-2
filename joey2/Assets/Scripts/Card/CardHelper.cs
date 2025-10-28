using System.Collections.Generic;
using UnityEngine;

public static class CardHelper
{

    // 把卡牌创建到指定transform下，并设置卡牌状态和位置
    public static GameObject CreateCardToTransform(GameObject cardPrefab, Transform parent, string cardId,CardState state=CardState.Default, CardPosition position=CardPosition.Default, List<GameObject> attachList = null)
    {
        if (parent == null || string.IsNullOrEmpty(cardId)) {
            Debug.LogError($"CreateCard: parent is null or cardId is empty, cardId: {cardId}");
            return null;
        }
        if (!GData.Instance.CardDict.ContainsKey(cardId)) {
            Debug.LogError($"CreateCard: cardId not found in CardDict, cardId: {cardId}");
            return null;
        }

        var go = GameObject.Instantiate(cardPrefab, parent);
        // 视觉倒序：新卡插到最前
        go.transform.SetSiblingIndex(0);
        if (attachList != null) attachList.Add(go);

        var cd = go.GetComponent<CardDisplay>();
        var baseCard = GData.Instance.CardDict[cardId];
        cd.card = baseCard.Clone();
        cd.card.state = state;
        cd.card.position = position;
        // 若有配置效果，则挂载并实例化
        if (cd.card.effectIds != null && cd.card.effectIds.Count > 0)
        {
            var holder = go.GetComponent<EffectHolder>();
            if (holder == null) holder = go.AddComponent<EffectHolder>();
            holder.effects.Clear();
            for (int i = 0; i < cd.card.effectIds.Count; i++)
            {
                var eff = CardEffectRegistry.Create(cd.card.effectIds[i]);
                if (eff != null) holder.effects.Add(eff);
            }
        }
        cd.ShowCard();
        return go;
    }

    // 移动用过的卡牌所在的list,设置卡牌状态和位置
    public static void MoveCard(GameObject cardGO,List<GameObject> fromCardList,List<GameObject> toCardList,CardState state=CardState.Used, CardPosition position=CardPosition.Default)
    {
        if (cardGO == null)
        {
            Debug.LogError("MoveCardUsed: cardGO is null");
            return;
        }
        var cd = cardGO.GetComponent<CardDisplay>();
        if (cd == null || cd.card == null)
        {
            Debug.LogError("DiscardUsedCard: cd or cd.card is null");
            return;
        }
        cd.card.state = state;
        cd.card.position = position;
        fromCardList.Remove(cardGO);
        toCardList.Insert(0, cardGO);
        if (state == CardState.Used)
        {
            cardGO.SetActive(false);
        }
    }


}

