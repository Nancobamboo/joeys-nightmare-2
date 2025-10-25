using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// HoverCardPreview（无边界裁剪版+细调）
/// - 悬停 hoverDelay 秒后，在源卡相反象限显示半透明的大卡预览
/// - 鼠标移出或脚本禁用时销毁预览
/// - 不做边界裁剪；提供 padding 和 offsetFineTune 控制距离与视觉细调
/// </summary>
public class HoverCardPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Preview")]
    public GameObject previewPrefab;           // 需要实例化的“预览大卡”预制体
    public Canvas targetCanvas;                // 预览挂载的 Canvas；为空则自动查找
    public float hoverDelay = 1.0f;            // 悬停多久后显示（秒）

    // 更近的默认间距（原来是 16,16）
    public Vector2 padding = new Vector2(6f, 6f); 

    [Tooltip("沿象限外扩方向的微调。比如设为(-4,-2)，会让预览比 padding 更靠近源卡一点。")]
    public Vector2 offsetFineTune = Vector2.zero;

    [Range(0f, 1f)]
    public float previewAlpha = 0.93f;          // 预览透明度

    private Coroutine _hoverCo;
    private GameObject _previewInstance;
    private RectTransform _previewRect;
    private CardDisplay _sourceDisplay;
    private RectTransform _sourceRect;

    void Awake()
    {
        _sourceDisplay = GetComponent<CardDisplay>();
        _sourceRect = GetComponent<RectTransform>();
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
            if (targetCanvas == null) targetCanvas = GameObject.FindObjectOfType<Canvas>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverCo == null) _hoverCo = StartCoroutine(Co_ShowAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverCo != null) { StopCoroutine(_hoverCo); _hoverCo = null; }
        DestroyPreview();
    }

    void OnDisable()
    {
        if (_hoverCo != null) { StopCoroutine(_hoverCo); _hoverCo = null; }
        DestroyPreview();
    }

    private IEnumerator Co_ShowAfterDelay()
    {
        float t = 0f;
        while (t < hoverDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        _hoverCo = null;
        ShowPreview();
    }

    private void ShowPreview()
    {
        if (_previewInstance != null) return;
        if (previewPrefab == null)
        {
            Debug.LogWarning("HoverCardPreview: 未设置 previewPrefab");
            return;
        }
        if (_sourceDisplay == null || _sourceDisplay.card == null)
        {
            Debug.LogWarning("HoverCardPreview: 源 CardDisplay 或 card 为空");
            return;
        }
        if (targetCanvas == null)
        {
            Debug.LogWarning("HoverCardPreview: 未找到 Canvas");
            return;
        }

        _previewInstance = Instantiate(previewPrefab, targetCanvas.transform);
        _previewRect = _previewInstance.GetComponent<RectTransform>();

        var cg = _previewInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = _previewInstance.AddComponent<CanvasGroup>();
        cg.alpha = previewAlpha;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var display = _previewInstance.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.card = _sourceDisplay.card;
            display.ShowCard();
        }

        PlaceAroundSource();
        // 无 Clamp
    }

	private void PlaceAroundSource()
	{
		if (_previewRect == null || targetCanvas == null) return;

		RectTransform canvasRect = targetCanvas.transform as RectTransform;

		// 将鼠标屏幕坐标转换为 Canvas 本地坐标
		Vector2 mouseLocal;
		Camera cam = (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : targetCanvas.worldCamera;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out mouseLocal))
			return;

		// 正确逻辑：右半边 -> 右中点贴住；左半边 -> 左中点贴住
		bool rightHalf = mouseLocal.x >= 0f;

		Vector2 pivot;
		float signX;
		if (rightHalf)
		{
			// 右半边：用卡的右中点贴住鼠标，并向左留出 padding
			pivot = new Vector2(1f, 0.5f);
			signX = -1f;
		}
		else
		{
			// 左半边：用卡的左中点贴住鼠标，并向右留出 padding
			pivot = new Vector2(0f, 0.5f);
			signX = 1f;
		}

		_previewRect.pivot = pivot;

		// 与鼠标的基础左右距离 + 细调（上下仅用 fineTune 控）
		Vector2 baseOffset = new Vector2(padding.x * signX, 0f);
		Vector2 fineOffset = new Vector2(offsetFineTune.x * signX, offsetFineTune.y);

		_previewRect.anchoredPosition = mouseLocal + baseOffset + fineOffset;
	}

    private void DestroyPreview()
    {
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
            _previewRect = null;
        }
    }
}