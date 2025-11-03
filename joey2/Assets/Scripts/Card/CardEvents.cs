using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardEvents : MonoBehaviour, IPointerDownHandler,IPointerEnterHandler, IPointerExitHandler
{
    public float zoomSize = 1.1f;
    public float enterDelay = 0.2f;         // 悬停延迟时间
    public float spacingOnEnterEnv = -490;
    public float spacingOnEnterBag = -460;
    public bool pointerIn = false;
    // 添加特效相关变量
    private GameObject vfxInstance;
    private string glowPath = "VFX/base/VFX_glow";
    private VerticalLayoutGroup parentVLG;
    private Coroutine enterCoroutine;
    public CardDisplay GetCardDisplay()
    {
        var cd = this.GetComponent<CardDisplay>();
        if (cd == null || cd.card == null)
        {
            Debug.LogError("ClickCard: CardDisplay 或 card 为空");
            return null;
        }
        return cd;
    }

    public void Start()
    {
        parentVLG = GetComponentInParent<VerticalLayoutGroup>();
        if (parentVLG == null)
        {
            Debug.LogError("CardEvents: VerticalLayoutGroup 为空");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 只允许鼠标左键
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (PData.Instance.canOperate == false)
        {
            return;
        }

        var cd = GetCardDisplay();
        // Debug.Log($"CardClick: {id}");
        if (cd.card.state != CardState.Active)
        {
            return;
            
        }
        else
        {
            GameEvents.RaiseCardClicked(cd.gameObject); // 广播全局事件
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var cd = GetCardDisplay();
        if (cd.card.position == CardPosition.Deck || cd.card.state == CardState.Active)
        {
            pointerIn = true;
            transform.localScale = new Vector3(zoomSize, zoomSize, 1.0f);
            // PlayGlowVFX();
        }
        // // 如果之前有延迟协程，先取消
        // if (enterCoroutine != null)
        // {
        //     StopCoroutine(enterCoroutine);
        //     enterCoroutine = null;
        // }
        // if (cd.card.state == CardState.Inactive)
        // {
        //     if (cd.card.position == CardPosition.Env)
        //     {
        //         enterCoroutine = StartCoroutine(DelayedSetSpacing(enterDelay, -400));
        //     }
        //     else
        //     {
        //         enterCoroutine = StartCoroutine(DelayedSetSpacing(enterDelay, -400));
        //     }
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var cd = GetCardDisplay();
        if (pointerIn)
        {
            pointerIn = false;
            transform.localScale = Vector3.one;
        }
        // if (enterCoroutine == null)
        // {
        //     return ;
        // }
        // if (enterCoroutine != null)
        // {
        //     StopCoroutine(enterCoroutine);
        //     enterCoroutine = null;
        // }
        // // 离开时立即恢复 spacing（或你也可以延迟恢复）
        // if (cd.card.position == CardPosition.Env)
        // {
        //     SetSpacing(spacingOnEnterEnv);
        // }
        // else
        // {
        //     SetSpacing(spacingOnEnterBag);
        // }
    }

    // private IEnumerator DelayedSetSpacing(float delay, float targetSpacing)
    // {
    //     yield return new WaitForSeconds(delay);
    //     SetSpacing(targetSpacing);
    //     enterCoroutine = null;
    // }

    // private void SetSpacing(float spacing)
    // {
    //     parentVLG.spacing = spacing;
    //     // //强制刷新布局，否则可能看不出 spacing 修改效果
    //     // LayoutRebuilder.ForceRebuildLayoutImmediate(
    //     //     parentVLG.GetComponent<RectTransform>()
    //     // );
    // }



    private void PlayGlowVFX()
    {
        // Debug.Log("PlayGlowVFX");
        // 如果已经有特效实例，先销毁
        DestroyGlowVFX();
        
        // 从Resources加载特效预制体
        GameObject vfxPrefab = Resources.Load<GameObject>(glowPath);
        if (vfxPrefab != null)
        {
            // 在卡牌位置实例化特效
            vfxInstance = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
            // 设置为卡牌的子对象，跟随卡牌移动
            vfxInstance.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogError($"无法加载特效: {glowPath}");
        }
    }

    private void DestroyGlowVFX()
    {
        // 销毁特效实例
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }
    }

    // 在对象销毁时也清理特效
    private void OnDestroy()
    {
        DestroyGlowVFX();
    }

}
