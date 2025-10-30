using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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


    public static IEnumerator PlayDamageVFX(GameObject cardGO, int damage)
    {
        if (cardGO == null) yield break;
        // 获取Canvas
        Canvas canvas = cardGO.GetComponentInParent<Canvas>();
        // 1. 播放卡牌受击动画（如果有animator）
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        Debug.Log($"PlayDamageVFX: Animator found = {animator != null}, CardGO = {cardGO.name}");
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayDamageVFX: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                animator.SetTrigger("UI_Carditem_shouji");
                yield return null;
            }
            
        }

        // 2. 展示伤害数字
        GameObject damageUI = Resources.Load<GameObject>("prefab/UIDamage");
        GameObject damageInstance = null;
        if (damageUI != null)
        {
            if (canvas != null)
            {
                // 实例化伤害数字UI
                damageInstance = Object.Instantiate(damageUI, canvas.transform);
                
                // 设置位置在卡牌上方
                damageInstance.transform.position = cardGO.transform.position + new Vector3(0, 3f, 0);
                // 设置伤害数字
                UnityEngine.UI.Text damageText = damageInstance.GetComponentInChildren<UnityEngine.UI.Text>();
                if (damageText != null)
                {
                    damageText.text = damage.ToString();
                }
            }
        }

        // 3. 播放受击特效
        GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_Shouji");
        GameObject vfxInstance = Object.Instantiate(vfxPrefab, canvas.transform);
        vfxInstance.transform.position = cardGO.transform.position; // 使用相同的坐标设置方式
        
        // 4. 播放震动特效
        if (CameraShake.Instance != null)
        {
            Debug.Log("PlayDamageVFX: Triggering camera shake");
            yield return CameraShake.Instance.ShakeLight();
        }
        else
        {
            Debug.LogWarning("PlayDamageVFX: CameraShake.Instance is null");
        }
        
        
        yield return new WaitForSeconds(0.5f);
        if (vfxInstance != null)
        {
            Object.Destroy(vfxInstance);
        }
        if (damageInstance != null)
        {
            Object.Destroy(damageInstance);
        }


    }


}
