using System.Collections.Generic;
using UnityEngine;

public static class UIGridHelper
{


	public static List<GameObject> GetCardsAsCardListOrder(Transform panel)
	{
		var list = new List<GameObject>(panel.childCount);
		for (int i = panel.childCount - 1; i >= 0; i--)
		{
			var go = panel.GetChild(i).gameObject;
			if (go.activeInHierarchy) list.Add(go);
		}
		return list;
	}

    public static GameObject GetCardListOrderIndex0(Transform panel)
    {
        // 视觉顺序反转后的第0个 -> 等价于“最后一个激活的视觉子物体”
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            var go = panel.GetChild(i).gameObject;
            if (go.activeInHierarchy) return go;
        }
        return null;
    }


    // 刷新：对 panel 下所有可见卡牌调用 ShowCard
    public static void RefreshPanel(Transform panel)
    {
        if (panel == null) return;
        // 如果没有active的child也return
        bool hasActiveChild = false;
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            var go = panel.GetChild(i).gameObject;
            if (go.activeInHierarchy) 
            {
                go.GetComponent<CardDisplay>().card.state = CardState.Inactive;
                hasActiveChild = true;
            }

        }
        if (!hasActiveChild) return;
        GameObject cardGO = GetCardListOrderIndex0(panel);
        cardGO.GetComponent<CardDisplay>().card.state = CardState.Active;

    }

    public static int FindEnvListIndexByCardGO(GameObject cardGO,List<List<GameObject>> envCardListList)
    {
        if (cardGO == null) return -1;
        foreach (var list in envCardListList)
        {
            if (list != null && list.Contains(cardGO)) return envCardListList.IndexOf(list);
        }
        return -1;
    }

    public static Transform GetPanelByCardGO(GameObject cardGO)
    {
        return cardGO != null ? cardGO.transform.parent : null;
    }

}

