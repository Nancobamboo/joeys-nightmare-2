using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public static class VFXStackHelper
{
    public static string appearVFXPath = "VFX/base/VFX_appear";
    public static string disappearVFXPath = "VFX/base/VFX_disappear";

    public static IEnumerator PlayAppearDisappearVFX(GameObject cardGO, int envListIndex)
    {
        if (cardGO == null) yield break;
        // 在方法开始时声明变量
        GameObject appearVfxInstance = null;
        GameObject disappearVfxInstance = null;

        // 获取cardGO所在的canvas
        Canvas canvas = cardGO.GetComponentInParent<Canvas>();
        Vector3 oldWorldPosition = cardGO.transform.position;
        
        
        GameObject disappearVfxPrefab = Resources.Load<GameObject>(disappearVFXPath);
        if (disappearVFXPath != null)
        {
            PData.Instance.canOperate = false;
            disappearVfxInstance = Object.Instantiate(disappearVfxPrefab, canvas.transform);
            disappearVfxInstance.transform.position = oldWorldPosition;
            // 让布局系统忽略这个特效对象
            var layoutElement = disappearVfxInstance.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = disappearVfxInstance.AddComponent<UnityEngine.UI.LayoutElement>();
            }
            layoutElement.ignoreLayout = true;

            yield return new WaitForSeconds(0.3f);
            // // 将卡片透明度设为0
            // var canvasGroup = cardGO.GetComponent<CanvasGroup>();
            // if (canvasGroup == null)
            // {
            //     canvasGroup = cardGO.AddComponent<CanvasGroup>();
            // }
            // canvasGroup.alpha = 0f;
            PData.Instance.canOperate = true;
            // yield return new WaitForSeconds(0.1f); 
        }

        switch (cardGO.GetComponent<CardDisplay>().card.type)
        {
            case "attack":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:BattleManager.Instance.envCardListList[envListIndex], toCardList:BattleManager.Instance.attackCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(BattleManager.Instance.attackPanel);
                UIGridHelper.RefreshPanel(BattleManager.Instance.envPanels[envListIndex]);
                UIGridHelper.RefreshPanel(BattleManager.Instance.attackPanel);
                break;
            case "defence":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:BattleManager.Instance.envCardListList[envListIndex], toCardList:BattleManager.Instance.defenceCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(BattleManager.Instance.defencePanel);
                UIGridHelper.RefreshPanel(BattleManager.Instance.envPanels[envListIndex]);
                UIGridHelper.RefreshPanel(BattleManager.Instance.defencePanel);
                break;
            case "skill":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:BattleManager.Instance.envCardListList[envListIndex], toCardList:BattleManager.Instance.skillCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(BattleManager.Instance.skillPanel);
                UIGridHelper.RefreshPanel(BattleManager.Instance.envPanels[envListIndex]);
                UIGridHelper.RefreshPanel(BattleManager.Instance.skillPanel);
                break;
            case "item":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:BattleManager.Instance.envCardListList[envListIndex], toCardList:BattleManager.Instance.itemCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(BattleManager.Instance.itemPanel);
                UIGridHelper.RefreshPanel(BattleManager.Instance.envPanels[envListIndex]);
                UIGridHelper.RefreshPanel(BattleManager.Instance.itemPanel);
                break;
            default:
                Debug.LogError("OnEnvCardClicked: 未知的位置");
                yield break;
        }



        GameObject appearVfxPrefab = Resources.Load<GameObject>(appearVFXPath);
        if (appearVfxPrefab != null)
        {
            PData.Instance.canOperate = false;
            appearVfxInstance = Object.Instantiate(appearVfxPrefab, cardGO.transform);
            appearVfxInstance.transform.localPosition = Vector3.zero;
            yield return new WaitForSeconds(0.4f);
            // // 将卡片透明度设为1
            // var canvasGroup = cardGO.GetComponent<CanvasGroup>();
            // if (canvasGroup == null)
            // {
            //     canvasGroup = cardGO.AddComponent<CanvasGroup>();
            // }
            // canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.4f);
            PData.Instance.canOperate = true;
        }
        if (disappearVfxInstance != null)
        {
            Object.Destroy(disappearVfxInstance);
        }
        if (appearVfxInstance != null)
        {
            Object.Destroy(appearVfxInstance);
        }
    }
}
