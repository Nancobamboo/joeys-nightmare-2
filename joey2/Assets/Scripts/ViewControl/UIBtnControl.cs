using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBtnControl : YViewControl
{
	private UIBtnView m_View;
	private Action<int> m_OnClick;
	private int m_Index;

	public static EResType GetResType()
	{
		return EResType.UIBtn;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIBtnView>();

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
		m_View.Text.text = title ?? string.Empty;
	}

	public void SetData(bool isUnlocked, bool interactable)
	{
		if (isUnlocked)
		{
			m_View.Text.gameObject.SetActive(false);
			m_View.Sold.SetActive(true);
			m_View.UIBtn.interactable = false;
		}
		else
		{
			m_View.Sold.SetActive(false);
			m_View.UIBtn.interactable = interactable;
		}
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}