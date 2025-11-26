using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIShopControl : YViewControl
{
	private UIShopView m_View;
	private List<UIShopCardControl> m_ShopCardList = new List<UIShopCardControl>();
	private List<ShopCardData> m_CurrentShopCards = new List<ShopCardData>();
	private DataJoeyPlayer m_PlayerData;
	private int m_RefreshCost = 50;
	private UIBuildControl m_BuildControl;

	private class ShopCardData
	{
		public Card card;
		public int shopPrice;

		public ShopCardData(Card card, int shopPrice)
		{
			this.card = card;
			this.shopPrice = shopPrice;
		}
	}

	public static EResType GetResType()
	{
		return EResType.UIShop;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIShopView>();
		m_View.Control.onClick.AddListener(OnControlClick);
		m_View.BtnClose.onClick.AddListener(OnBtnCloseClick);
		m_View.BtnRefresh.onClick.AddListener(OnBtnRefreshClick);
		m_PlayerData = DataSystem.Instance.GetDataJoeyPlayer();
	}

	void OnControlClick()
	{
		Close();
	}

	void OnBtnCloseClick()
	{
		m_BuildControl.SaveBuild(m_BuildControl.m_CurrentCardType);

		Close();
	}

	public new void Close()
	{
		gameObject.SetActive(false);
	}

	void OnBtnBuyClick()
	{
	}

	void OnBtnSaleClick()
	{
	}

	void OnBtnRefreshClick()
	{
		if (m_PlayerData.Coin < m_RefreshCost)
		{
			Debug.Log("金币不足，无法刷新！需要 " + m_RefreshCost + " 金币");
			return;
		}

		m_PlayerData.Coin -= m_RefreshCost;

		GenerateShopCards();
		RefreshShopDisplay();

		DataSystem.Instance.SaveDataJoeyPlayer();
	}

	public void SetData()
	{
		GenerateShopCards();
		RefreshShopDisplay();

		if (m_BuildControl == null)
		{
			m_BuildControl = Asset.OpenUI<UIBuildControl>(transform);
		}
		else
		{
			m_BuildControl.gameObject.SetActive(true);
		}
		m_BuildControl.SetShopData();
	}

	void GenerateShopCards()
	{
		m_CurrentShopCards.Clear();

		GData.Instance.LoadCards();

		List<Card> allCards = GData.Instance.CardDict.Values.ToList();
		List<Card> availableCards = allCards.Where(c =>
		{
			ECardType cardType = c.GetCardType();
			return (cardType == ECardType.attack ||
				cardType == ECardType.defence ||
				cardType == ECardType.skill ||
				cardType == ECardType.item) &&
			c.price > 0;
		}).ToList();

		int shopCardCount = 8;
		List<Card> shuffledCards = availableCards.OrderBy(x => Random.value).ToList();

		// Randomly select one card for 50% discount
		int halfPriceIndex = Random.Range(0, Mathf.Min(shopCardCount, shuffledCards.Count));

		for (int i = 0; i < shopCardCount && i < shuffledCards.Count; i++)
		{
			Card card = shuffledCards[i];
			int shopPrice;

			if (i == halfPriceIndex)
			{
				// First selected card: 50% discount
				shopPrice = Mathf.RoundToInt(card.price * 0.5f);
			}
			else
			{
				// Other cards: 90%-100% smooth random
				float discountRate = Random.Range(0.9f, 1.0f);
				shopPrice = Mathf.RoundToInt(card.price * discountRate);
			}

			if (shopPrice < 1)
			{
				shopPrice = 1;
			}

			m_CurrentShopCards.Add(new ShopCardData(card, shopPrice));
		}
	}

	void RefreshShopDisplay()
	{
		while (m_ShopCardList.Count < m_CurrentShopCards.Count)
		{
			UIShopCardControl cardControl = Asset.OpenUI<UIShopCardControl>(null);
			cardControl.CacheTrans.SetParent(m_View.Content);
			cardControl.CacheTrans.localScale = Vector3.one * 0.7f;
			cardControl.CacheTrans.localPosition = Vector3.zero;
			cardControl.CacheTrans.localEulerAngles = Vector3.zero;
			m_ShopCardList.Add(cardControl);
		}

		for (int i = 0; i < m_ShopCardList.Count; i++)
		{
			UIShopCardControl control = m_ShopCardList[i];
			if (i < m_CurrentShopCards.Count)
			{
				ShopCardData shopCardData = m_CurrentShopCards[i];
				control.gameObject.SetActive(true);
				control.SetData(shopCardData.card, shopCardData.shopPrice);
				control.ShopClickHandler = OnShopCardClick;
			}
			else
			{
				control.gameObject.SetActive(false);
				control.ShopClickHandler = null;
			}
		}
	}

	void OnShopCardClick(UIShopCardControl cardControl)
	{
		ShopCardData shopCardData = null;
		for (int i = 0; i < m_CurrentShopCards.Count; i++)
		{
			ShopCardData data = m_CurrentShopCards[i];
			if (data.card.id == cardControl.CardData.id)
			{
				shopCardData = data;
				break;
			}
		}

		if (shopCardData == null)
		{
			Debug.LogWarning("找不到对应的商店卡牌数据！");
			return;
		}

		int price = shopCardData.shopPrice;

		if (m_PlayerData.Coin < price)
		{
			Debug.Log("金币不足！需要 " + price + " 金币，当前只有 " + m_PlayerData.Coin + " 金币");
			return;
		}

		m_PlayerData.Coin -= price;

		Card newCard = DataSystem.Instance.CreateCard(shopCardData.card.id);
		m_PlayerData.AddSelfCardDictData(newCard);

		m_CurrentShopCards.Remove(shopCardData);

		DataSystem.Instance.SaveDataJoeyPlayer();

		RefreshShopDisplay();

		Debug.Log("购买成功！花费 " + price + " 金币购买了 " + newCard.cardName);
	}

	protected override void OnReturn()
	{
		m_ShopCardList.Clear();
		m_CurrentShopCards.Clear();
		base.OnReturn();
	}
}