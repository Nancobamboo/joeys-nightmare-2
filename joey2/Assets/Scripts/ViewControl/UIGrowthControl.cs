using System.Collections.Generic;
using UnityEngine;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;
	private List<UIBtnControl> m_BtnControls = new List<UIBtnControl>();

	public static EResType GetResType()
	{
		return EResType.UIGrowth;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIGrowthView>();
		m_View.BtnSkip.onClick.AddListener(OnBtnSkipClick);

		InitButtons();
	}

	void OnBtnSkipClick()
	{
		Close();
	}

	private void InitButtons()
	{
		m_BtnControls.Clear();
		var trs = new List<RectTransform>
		{
			m_View.Btn, m_View.Btn1, m_View.Btn2, m_View.Btn3, m_View.Btn4,
			m_View.Btn5, m_View.Btn6, m_View.Btn7, m_View.Btn8, m_View.Btn9,
			m_View.Btn10, m_View.Btn11, m_View.Btn12, m_View.Btn13, m_View.Btn14,
			m_View.Btn15, m_View.Btn16, m_View.Btn17, m_View.Btn18, m_View.Btn19,
			m_View.Btn20
		};

		for (int i = 0; i < trs.Count; i++)
		{
			if (trs[i] == null) continue;

			UIBtnControl btnControl = Asset.OpenUI<UIBtnControl>(trs[i]);
			btnControl.InitWithTransform(i, OnGrowthBtnClick);
			m_BtnControls.Add(btnControl);
		}
	}

	public void SetData()
	{
		Refresh();
	}

	private void Refresh()
	{
		DataGrowth data = DataSystem.Instance.GetDataGrowth();

		if (m_View != null && m_View.TextCoins != null)
		{
			m_View.TextCoins.text = data.Points.ToString();
		}

		for (int i = 0; i < m_BtnControls.Count; i++)
		{
			UIBtnControl btnControl = m_BtnControls[i];
			GrowthInfo growthInfo = GData.Instance.GetGrowthInfo(i);

			if (growthInfo == null)
			{
				btnControl.SetData(false, false);
				continue;
			}

			bool isUnlocked = data.IsUnlocked(growthInfo.id);
			bool interactable = true;

			if (!isUnlocked)
			{
				if (growthInfo.depend != -1)
				{
					if (!data.IsUnlocked(growthInfo.depend))
					{
						interactable = false;
					}
				}
			}

			btnControl.SetData(isUnlocked, interactable);
		}
	}

	void OnGrowthBtnClick(int index)
	{
		GrowthInfo growthInfo = GData.Instance.GetGrowthInfo(index);
		if (growthInfo == null) return;

		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		if (growthInfo.depend != -1 && !data.IsUnlocked(growthInfo.depend)) return;
		if (data.IsUnlocked(growthInfo.id)) return;

		var window = Asset.OpenUI<UIGrowthWindowControl>();
		window.SetData(growthInfo.id, growthInfo.desc, growthInfo.price, () =>
		{
			DataSystem.Instance.GetDataGrowth().Unlock(growthInfo.id);
			DataSystem.Instance.SaveDataGrowth();
			Refresh();
		});
	}

	protected override void OnClose()
	{
		for (int i = 0; i < m_BtnControls.Count; i++)
		{
			if (m_BtnControls[i] != null)
			{
				m_BtnControls[i].Close();
			}
		}
		m_BtnControls.Clear();
		base.OnClose();
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}