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
        // 记录原始位置
        Canvas canvas = cardGO.GetComponentInParent<Canvas>();
        Vector3 startWorldPosition = cardGO.transform.position;
        Vector3 startScale = cardGO.transform.localScale;

        // 创建一个临时的卡牌视觉副本用于飞行动画
        GameObject flyingCard = Object.Instantiate(cardGO, canvas.transform);
        flyingCard.transform.position = startWorldPosition;
        flyingCard.transform.localScale = startScale;
        // 让飞行卡牌忽略布局
        var layoutElement = flyingCard.GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = flyingCard.AddComponent<UnityEngine.UI.LayoutElement>();
        }
        layoutElement.ignoreLayout = true;

        // 禁用飞行卡牌的交互
        var canvasGroup = flyingCard.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = flyingCard.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;

        // 隐藏原始卡牌
        var originalCanvasGroup = cardGO.GetComponent<CanvasGroup>();
        if (originalCanvasGroup == null)
        {
            originalCanvasGroup = cardGO.AddComponent<CanvasGroup>();
        }
        originalCanvasGroup.alpha = 0f;


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

        // 等待一帧，让布局系统更新目标位置
        yield return null;
        // 获取目标位置和缩放
        Vector3 targetWorldPosition = cardGO.transform.position;
        Vector3 targetScale = new Vector3(0.8f, 0.8f, 1.0f);

        // 播放飞行动画
        PData.Instance.canOperate = false;
        // 使用协程实现飞行动画（不需要DOTween）
        float duration = 0.45f; // 飞行时间
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用缓动函数（EaseOutQuad）
            float easeT = 1f - (1f - t) * (1f - t);
            
            // 插值位置
            flyingCard.transform.position = Vector3.Lerp(startWorldPosition, targetWorldPosition, easeT);
            
            // 插值缩放
            flyingCard.transform.localScale = Vector3.Lerp(startScale, targetScale, easeT);
            
            yield return null;
        }
        // 确保最终位置准确
        flyingCard.transform.position = targetWorldPosition;
        flyingCard.transform.localScale = targetScale;
        // 销毁飞行卡牌，显示原始卡牌
        Object.Destroy(flyingCard);
        originalCanvasGroup.alpha = 1f;
        PData.Instance.canOperate = true;
    }
}
