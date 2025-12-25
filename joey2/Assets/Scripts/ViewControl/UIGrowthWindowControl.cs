using System;
using UnityEngine;

public class UIGrowthWindowControl : YViewControl
{
	private UIGrowthWindowView m_View;
	private Action m_OnBuy;
	private int m_Price;
	private int m_Id;

	public static EResType GetResType()
	{
		return EResType.UIGrowthWindow;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIGrowthWindowView>();
		m_View.BtnBuy.onClick.AddListener(OnBtnBuyClick);
		m_View.BtnClose.onClick.AddListener(Close);
	}

	public void SetData(int id, string desc, int price, Action onBuy)
	{
		m_Id = id;
		m_Price = price;
		m_OnBuy = onBuy;
		m_View.TxtDesc.text = desc;
		m_View.TxtPrice.text = "-" + price.ToString();

		int currentPoints = DataSystem.Instance.GetDataGrowth().Points;
		m_View.BtnBuy.interactable = currentPoints >= price;
	}

	private void OnBtnBuyClick()
	{
		int currentPoints = DataSystem.Instance.GetDataGrowth().Points;
		if (currentPoints >= m_Price)
		{
			DataSystem.Instance.AddGrowthPoints(-m_Price);
			m_OnBuy?.Invoke();
			Close();
		}
	}
}