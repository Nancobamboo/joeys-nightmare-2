using UnityEngine;

public class UILobbyControl : YViewControl
{
	private UILobbyView m_View;
	private UIShopControl m_ShopControl;

	public static EResType GetResType()
	{
		return EResType.UILobby;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UILobbyView>();
		m_View.BtnBuild.onClick.AddListener(OnBtnBuildClick);
		m_View.BtnShop.onClick.AddListener(OnBtnShopClick);
		m_View.BtnGame.onClick.AddListener(OnBtnGameClick);
		m_View.BtnMerge.onClick.AddListener(OnBtnMergeClick);
	}

	void OnBtnBuildClick()
	{
	}

	void OnBtnShopClick()
	{
		if (m_ShopControl == null)
		{
			m_ShopControl = Asset.OpenUI<UIShopControl>();
			m_ShopControl.SetData();

		}
		else
		{
			m_ShopControl.gameObject.SetActive(true);
		}
	}

	void OnBtnGameClick()
	{
		if (m_ShopControl != null)
		{
			m_ShopControl.Close();
		}
		Close();
		JoeyGameControl.Instance.LoadNextLevel();
	}

	void OnBtnMergeClick()
	{
	}

	public void SetData()
	{
		;
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}