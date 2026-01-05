using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBtnControl : YViewControl
{
	private UIBtnView m_View;
	private Action<int> m_OnClick;
	private int m_Index;
	private const string RuntimeTitleName = "RuntimeTitle";
	private const string GrowthSpriteRoot = "Art/Img/growth/";
	private static readonly System.Collections.Generic.Dictionary<string, Sprite> s_GrowthSpriteCache =
		new System.Collections.Generic.Dictionary<string, Sprite>();

	public enum EBtnState
	{
		Start,
		Unlock,
		Lock,
		Unknow
	}

	/// <summary>
	/// 成长(技能树)节点的图标类型（用于 IconStart/IconLock/IconUnlock）。
	/// IconUnknow 始终使用 skilltree_icon_unknow。
	/// </summary>
	public enum EGrowthIconType
	{
		Hp,
		Gold,
		NewCard,
		StartRelicOrStartCard,
		NewRelic,
	}

	public static EResType GetResType()
	{
		return EResType.UIBtn;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIBtnView>();

		// 确保按钮文本存在（有些 prefab 可能缺少 Text 节点；这里做运行时兜底）
		EnsureTitleText();
		// 避免子节点(Icon/Line/Text等)抢射线导致 Button 无法点击
		DisableRaycastTargets();

		var colors = m_View.UIBtn.colors;
		if (colors.disabledColor.a < 0.1f)
		{
			colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			m_View.UIBtn.colors = colors;
		}

		m_View.UIBtn.onClick.RemoveAllListeners();
		m_View.UIBtn.onClick.AddListener(OnUIBtnClick);
	}

	public void Setup(int index, Action<int> onClick)
	{
		m_Index = index;
		m_OnClick = onClick;
	}

	private void OnUIBtnClick()
	{
		m_OnClick?.Invoke(m_Index);
	}

	public void SetTitle(string title)
	{
		EnsureTitleText();
		if (m_View.Text == null) return;

		m_View.Text.text = title ?? string.Empty;
		if (!m_View.Text.gameObject.activeSelf) m_View.Text.gameObject.SetActive(true);
	}

	public void SetState(EBtnState state)
	{
		// 1. uibtn这个节点激活了,就激活unlock
		// 2. 如果节点没激活,但是联通了激活的节点,就激活lock
		// 3. 没激活也没联通激活的,就激活unknow
		// 4. 最开始的节点激活start

		// Reset all states first
		SetActiveSafe(m_View.Start, false);
		SetActiveSafe(m_View.IconStart, false);
		SetActiveSafe(m_View.UnLock, false);
		SetActiveSafe(m_View.IconUnlock, false);
		SetActiveSafe(m_View.Lock, false);
		SetActiveSafe(m_View.IconLock, false);
		SetActiveSafe(m_View.Unknow, false);
		SetActiveSafe(m_View.IconUnknow, false);

		switch (state)
		{
			case EBtnState.Start:
				SetActiveSafe(m_View.Start, true);
				SetActiveSafe(m_View.IconStart, true);
				break;
			case EBtnState.Unlock:
				SetActiveSafe(m_View.UnLock, true);
				SetActiveSafe(m_View.IconUnlock, true);
				break;
			case EBtnState.Lock:
				SetActiveSafe(m_View.Lock, true);
				SetActiveSafe(m_View.IconLock, true);
				break;
			case EBtnState.Unknow:
				SetActiveSafe(m_View.Unknow, true);
				SetActiveSafe(m_View.IconUnknow, true);
				break;
		}
	}

	/// <summary>
	/// 给成长树节点按钮设置 icon（只改 IconStart/IconLock/IconUnlock/IconUnknow 四张图）。
	/// </summary>
	public void SetGrowthIcon(EGrowthIconType type)
	{
		if (m_View == null) return;

		Sprite typeSprite = LoadGrowthSprite(GetGrowthSpriteName(type));
		Sprite unknownSprite = LoadGrowthSprite("skilltree_icon_unknow");

		SetSpriteSafe(m_View.IconStart, typeSprite);
		SetSpriteSafe(m_View.IconUnlock, typeSprite);
		SetSpriteSafe(m_View.IconLock, typeSprite);
		SetSpriteSafe(m_View.IconUnknow, unknownSprite);
	}

	private static string GetGrowthSpriteName(EGrowthIconType type)
	{
		switch (type)
		{
			case EGrowthIconType.Hp:
				return "skilltree_icon_hp2";
			case EGrowthIconType.Gold:
				return "skilltree_icon_gold1";
			case EGrowthIconType.NewCard:
				return "skilltree_icon_newcard";
			case EGrowthIconType.StartRelicOrStartCard:
				return "skilltree_icon_relic1";
			case EGrowthIconType.NewRelic:
				return "skilltree_icon_relic2";
			default:
				return "skilltree_icon_hp2";
		}
	}

	private static Sprite LoadGrowthSprite(string spriteNameNoExt)
	{
		if (string.IsNullOrWhiteSpace(spriteNameNoExt)) return null;
		if (s_GrowthSpriteCache.TryGetValue(spriteNameNoExt, out var cached) && cached != null) return cached;

		var sp = Resources.Load<Sprite>(GrowthSpriteRoot + spriteNameNoExt);
		s_GrowthSpriteCache[spriteNameNoExt] = sp;
		return sp;
	}

	private void SetSpriteSafe(Image img, Sprite sprite)
	{
		if (img == null || sprite == null) return;
		img.sprite = sprite;
	}

	public void SetLine(bool show, bool isUnlock, float angle, float length)
	{
		if (!show)
		{
			SetActiveSafe(m_View.Rotation, false);
			return;
		}

		SetActiveSafe(m_View.Rotation, true);
		
		// 节点之间的连线不要自己画,用view里的line,已经激活的用unlock的,否则就用lock的,自己算rotation
		SetActiveSafe(m_View.LineUnlock, isUnlock);
		SetActiveSafe(m_View.LineLock, !isUnlock);

		if (m_View.Rotation != null)
		{
			m_View.Rotation.localRotation = Quaternion.Euler(0, 0, angle);

			// 线的 Rect 本身已经有一个起点偏移(anchoredPosition.x)，
			// 直接用“节点中心距离”会导致线段超出目标节点（看起来像露出来/穿过去）。
			// 因此这里用：width = centerDistance - startOffset
			var lineImg = isUnlock ? m_View.LineUnlock : m_View.LineLock;
			float startOffset = 0f;
			if (lineImg != null)
			{
				startOffset = lineImg.rectTransform.anchoredPosition.x;
			}
			float width = Mathf.Max(0f, length - startOffset);
			SetLineWidth(lineImg, width);
		}
	}

	private void SetLineWidth(Image lineImg, float length)
	{
		if (lineImg != null)
		{
			var rt = lineImg.rectTransform;
			Vector2 size = rt.sizeDelta;
			size.x = length;
			rt.sizeDelta = size;
		}
	}

	private void EnsureTitleText()
	{
		// 先尝试从 view 里拿（auto-generated view 可能绑定不到时这里也能补救）
		if (m_View != null && m_View.Text == null)
		{
			m_View.Text = GetComponentInChildren<Text>(true);
		}

		if (m_View == null || m_View.UIBtn == null) return;

		if (m_View.Text == null)
		{
			// 运行时创建一个 Text，放在按钮下面作为标题
			var go = new GameObject(RuntimeTitleName, typeof(RectTransform), typeof(Text));
			go.transform.SetParent(m_View.UIBtn.transform, false);
			go.transform.SetAsLastSibling();

			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = Vector2.zero;

			var t = go.GetComponent<Text>();
			t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			t.alignment = TextAnchor.MiddleCenter;
			t.color = Color.white;
			t.raycastTarget = false;
			t.supportRichText = false;
			t.text = string.Empty;

			m_View.Text = t;
		}

		// 文本不应该挡点击
		if (m_View.Text != null) m_View.Text.raycastTarget = false;
	}

	private void DisableRaycastTargets()
	{
		// 这些都是装饰/状态/连线，不应该挡住点击
		SetRaycastTarget(m_View.Lock, false);
		SetRaycastTarget(m_View.Start, false);
		SetRaycastTarget(m_View.UnLock, false);
		SetRaycastTarget(m_View.Unknow, false);
		SetRaycastTarget(m_View.IconStart, false);
		SetRaycastTarget(m_View.IconUnlock, false);
		SetRaycastTarget(m_View.IconLock, false);
		SetRaycastTarget(m_View.IconUnknow, false);
		SetRaycastTarget(m_View.LineLock, false);
		SetRaycastTarget(m_View.LineUnlock, false);
		SetRaycastTarget(m_View.Text, false);
	}

	private void SetRaycastTarget(Graphic g, bool enable)
	{
		if (g == null) return;
		// 如果某个 Graphic 恰好和 Button 在同一个对象上，别去动它，避免把按钮本体也禁用了
		if (g.GetComponent<Button>() != null) return;
		g.raycastTarget = enable;
	}

	public void SetInteractable(bool interactable)
	{
		if (m_View.UIBtn != null)
		{
			m_View.UIBtn.interactable = interactable;
		}
	}

	private void SetActiveSafe(Component comp, bool active)
	{
		if (comp != null) comp.gameObject.SetActive(active);
	}
	
	private void SetActiveSafe(GameObject go, bool active)
	{
		if (go != null) go.SetActive(active);
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}
