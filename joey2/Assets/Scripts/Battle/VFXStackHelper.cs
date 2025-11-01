using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class VFXStackHelper
{
    public static string appearVFXPath = "VFX/base/VFX_appear";
    public static string disappearVFXPath = "VFX/base/VFX_disappear";

    public static Sprite playerDamageSprite;
    public static Sprite playerSleepSprite;

    static VFXStackHelper()
    {
        playerDamageSprite = Resources.Load<Sprite>("Art/Img/img_card_merge");
        playerSleepSprite = Resources.Load<Sprite>("Art/Img/img_sleep");
    }


    public static IEnumerator PlayMoveCardVFX(GameObject cardGO, Vector3 targetWorldPosition)
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
        // 播放飞行动画
        PData.Instance.canOperate = false;
        // 使用协程实现飞行动画（不需要DOTween）
        float duration = 0.4f; // 飞行时间
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用缓动函数（EaseOutQuad）
            float easeT = 1f - (1f - t) * (1f - t);
            
            // 插值位置
            flyingCard.transform.position = Vector3.Lerp(startWorldPosition, targetWorldPosition, easeT);
            
            yield return null;
        }
        // 确保最终位置准确
        flyingCard.transform.position = targetWorldPosition;
        // 销毁飞行卡牌，显示原始卡牌
        Object.Destroy(flyingCard);
        PData.Instance.canOperate = true;

    }



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


    public static IEnumerator PlayDamageVFX(GameObject cardGO, int damage,bool monsterAttack=false)
    {
        if (cardGO == null) yield break;
        // 获取Canvas
        Canvas canvas = cardGO.GetComponentInParent<Canvas>();
        // 1. 播放卡牌受击动画（如果有animator）
        Animator animator = cardGO.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"PlayDamageVFX: No AnimatorController assigned to {cardGO.name}");
            }
            else
            {
                // Reset animator state to ensure animation can play even if previous animation is still playing
                animator.Rebind();
                animator.Update(0f);
                animator.Play("UI_Carditem_shouji", 0, 0f);
                yield return null;
            }
        }

        // 2. 展示伤害数字
        GameObject damageUIPrefab = Resources.Load<GameObject>("prefab/UIDamage");
        GameObject damageInstance = null;
        if (damageUIPrefab != null)
        {
            // 实例化伤害UI
            damageInstance = Object.Instantiate(damageUIPrefab, cardGO.transform);
            
            // 设置位置在卡牌上方
            damageInstance.transform.localPosition = new Vector3(0f, 180f, 0);
            
            // 设置伤害数字文本
            Transform damageTextTransform = damageInstance.transform.Find("Image/Damage");
            if (damageTextTransform != null)
            {
                Text damageText = damageTextTransform.GetComponent<Text>();
                if (damageText != null)
                {
                    damageText.text = "-" + damage.ToString();
                    damageText.gameObject.SetActive(true);
                    // Debug.Log($"PlayDamageVFX: Set damage text to {damageText.text}");
                }
                else
                {
                    Debug.LogError("PlayDamageVFX: Damage Text component not found");
                }
            }
            else
            {
                // Debug.LogError("PlayDamageVFX: Damage transform not found");
            }
            
            // 3. 播放伤害数字动画
            Animator damageAnimator = damageInstance.GetComponent<Animator>();
            if (damageAnimator != null)
            {
                damageAnimator.Play("UIDamage_kouxue");
                // Debug.Log("PlayDamageVFX: Playing damage animation");
            }
        }
        else
        {
            Debug.LogError("PlayDamageVFX: Failed to load UIDamage prefab");
        }

        // 4. 播放受击特效
        GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_Shouji");
        GameObject vfxInstance = Object.Instantiate(vfxPrefab, canvas.transform);
        vfxInstance.transform.position = cardGO.transform.position; // 使用相同的坐标设置方式
        
        // 5. 播放震动特效
        if (CameraShake.Instance != null)
        {
            // Debug.Log("PlayDamageVFX: Triggering camera shake");
            yield return CameraShake.Instance.ShakeLight();
        }
        else
        {
            Debug.LogWarning("PlayDamageVFX: CameraShake.Instance is null");
        }
        
        
        yield return new WaitForSeconds(1f);
        if (vfxInstance != null)
        {
            Object.Destroy(vfxInstance);
        }
        if (damageInstance != null)
        {
            Object.Destroy(damageInstance);
        }
        GameEvents.RaiseDamageComplete(cardGO,monsterAttack);


    }


    public static IEnumerator PlayDamageToPlayerVFX(Image joeyImage,GameObject defenceCardGO, int damage)
    {
        PData.Instance.canOperate = false;
        // 获取Canvas
        Canvas canvas = joeyImage.GetComponentInParent<Canvas>();
        GameObject vfxInstance=null;

        if (damage > 0)
        {
            // 1. 盾牌受击特效
            if (defenceCardGO != null)
            {
                GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_Dunsui");
                vfxInstance = Object.Instantiate(vfxPrefab, canvas.transform);
                vfxInstance.transform.position = defenceCardGO.transform.position; // 使用相同的坐标设置方式

                // 1.5 盾牌受击动画
                Animator animator = defenceCardGO.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                    if (animator.runtimeAnimatorController == null)
                    {
                        Debug.LogError($"PlayDamageVFX: No AnimatorController");
                    }
                    else
                    {
                        animator.Play("UI_Carditem_dunpai");
                    }
                }

            }



            // 2. joey受击图片
            joeyImage.sprite = playerDamageSprite;
            // 3. 展示伤害数字
            BattleManager.Instance.StartCoroutine(VFXDamageHelper.PlayDamageVFX(transform:joeyImage.transform,localPositionShift:new Vector3(100f, 190f, 0),damage:damage));
            // 4. 播放屏幕震动
            BattleManager.Instance.StartCoroutine(CameraShake.Instance.ShakeLight());
            yield return new WaitForSeconds(0.6f);
            // 切回原始图片
            joeyImage.sprite = playerSleepSprite;

        }
        else
        {
            // 1. 盾牌受击特效
            GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_Dun");
            vfxInstance = Object.Instantiate(vfxPrefab, canvas.transform);
            vfxInstance.transform.position = defenceCardGO.transform.position; // 使用相同的坐标设置方式

            // 1.5 盾牌受击动画
            Animator animator = defenceCardGO.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError($"PlayDamageVFX: No AnimatorController");
                }
                else
                {
                    animator.Play("UI_Carditem_dunpai");
                }
            
            BattleManager.Instance.StartCoroutine(VFXDamageHelper.PlayDamageVFX(transform:joeyImage.transform,localPositionShift:new Vector3(100f, 190f, 0),damage:damage));
            yield return new WaitForSeconds(0.5f);
            }

        }
        
        // 5. 移走盾牌
        if (defenceCardGO != null)
        {
            Vector3 targetWorldPosition = defenceCardGO.transform.position + new Vector3(0,-500f,0);
            yield return PlayMoveCardVFX(cardGO:defenceCardGO, targetWorldPosition:targetWorldPosition);
        }

        PData.Instance.canOperate = true;

        // 触发伤害完成事件
        GameEvents.RaiseDamageToPlayerComplete();
        
        // Check if player is dead and trigger game over event
        if (PData.Instance.playerHealth <= 0)
        {
            GameEvents.RaiseGameOver();
        }
    }


}
