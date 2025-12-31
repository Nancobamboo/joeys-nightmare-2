using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIShopSuperControl : YViewControl
{
    private UIShopView m_View;
    private List<UIShopCardControl> m_ShopCardList = new List<UIShopCardControl>();
    private List<ShopCardData> m_CurrentShopCards = new List<ShopCardData>();
    private DataJoeyPlayer m_PlayerData;
    private UIBuildSuperControl m_BuildControl;
    private bool m_IsNew;
    private MonoBehaviourPool<UIDamageTextControl> m_DamageTextPool;

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
        m_View.BtnClose.onClick.AddListener(OnBtnCloseClick);
        m_View.BtnRefresh.onClick.AddListener(OnBtnRefreshClick);
        m_PlayerData = DataSystem.Instance.GetDataJoeyPlayer();
        RegistAction(EActionId.OnCoinChange, OnCoinChange);

        m_DamageTextPool = new MonoBehaviourPool<UIDamageTextControl>(() =>
        {
            return this.Asset.OpenUI<UIDamageTextControl>(null);
        });
    }

    void OnControlClick()
    {
        for (int i = 0; i < m_ShopCardList.Count; i++)
        {
            if (m_ShopCardList[i] != null)
            {
                m_ShopCardList[i].CleanupDescExt();
            }
        }
        Close();
    }

    void OnBtnCloseClick()
    {
        if (m_BuildControl != null)
        {
            m_BuildControl.SaveBuild(m_BuildControl.m_CurrentCardType);
        }

        DataSystem.Instance.SaveDataJoeyPlayer();
        for (int i = 0; i < m_ShopCardList.Count; i++)
        {
            if (m_ShopCardList[i] != null)
            {
                m_ShopCardList[i].CleanupDescExt();
            }
        }
        Close();
        JoeyGameControl.Instance.LoadNextLevel();
    }

    void OnBtnBuyClick()
    {
    }

    void OnBtnSaleClick()
    {
    }

    void OnBtnRefreshClick()
    {
        int refreshCost = GetRefreshCost();
        if (m_PlayerData.Coin < refreshCost)
        {
            Debug.Log("金币不足，无法刷新！需要 " + refreshCost + " 金币");
            return;
        }

        DataSystem.Instance.AddCoin(-refreshCost);

        GenerateShopCards();
        RefreshShopDisplay();

        DataSystem.Instance.SaveDataJoeyPlayer();
    }

    private int GetRefreshCost()
    {
        int baseCost = 50;
        
        // Apply difficulty price multiplier
        float difficultyMultiplier = GData.Instance.GetShopPriceMultiplier();
        int costWithDifficulty = Mathf.RoundToInt(baseCost * difficultyMultiplier);
        
        // Apply shop discount relic
        if (DataSystem.Instance.HasRelic(ERelicType.ShopDiscount))
        {
            costWithDifficulty = Mathf.RoundToInt(costWithDifficulty * 0.8f);
        }
        
        // Minimum cost is 1
        if (costWithDifficulty < 1)
        {
            costWithDifficulty = 1;
        }
        
        return costWithDifficulty;
    }

    public void SetData(bool isNew = false)
    {
        m_IsNew = isNew;
        GenerateShopCards();
        RefreshShopDisplay();
        m_View.TxtCoin.text = m_PlayerData.Coin.ToString();

        if (m_BuildControl == null)
        {
            m_BuildControl = Asset.OpenUI<UIBuildSuperControl>(m_View.BuildRoot);
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

        List<Card> allCards = GData.Instance.CardDict.Values.ToList();
        List<Card> availableCards = allCards.Where(c =>
        {
            ECardType cardType = c.GetCardType();
            // Exclude curse cards (1025: Pain Blade, 2016: Pain Shield)
            return (cardType == ECardType.attack ||
                cardType == ECardType.defence ||
                cardType == ECardType.skill ||
                cardType == ECardType.item) &&
            c.price > 0 &&
            c.id != "1025" && c.id != "2016";
        }).ToList();

        int shopCardCount = 8;

        // Use default shop star rates (similar to normal stage: 60% 1-star, 30% 2-star, 10% 3-star)
        Dictionary<int, int> shopStarRates = new Dictionary<int, int>
        {
            { 1, 60 },
            { 2, 30 },
            { 3, 10 }
        };

        // Select cards with difficulty-adjusted star probabilities
        List<Card> selectedCards = GData.Instance.SelectCardsWithStarProbability(availableCards, shopCardCount, shopStarRates);

        int halfPriceIndex = Random.Range(0, Mathf.Min(shopCardCount, selectedCards.Count));

        // Get difficulty price multiplier
        float difficultyPriceMultiplier = GData.Instance.GetShopPriceMultiplier();

        for (int i = 0; i < selectedCards.Count; i++)
        {
            Card card = selectedCards[i];
            int shopPrice;

            if (i == halfPriceIndex)
            {
                shopPrice = Mathf.RoundToInt(card.price * 0.5f);
            }
            else
            {
                float discountRate = Random.Range(0.8f, 1.0f);
                shopPrice = Mathf.RoundToInt(card.price * discountRate);
            }

            // Apply difficulty price multiplier
            shopPrice = Mathf.RoundToInt(shopPrice * difficultyPriceMultiplier);

            // Apply shop discount relic on top of existing discount
            if (DataSystem.Instance.HasRelic(ERelicType.ShopDiscount))
            {
                shopPrice = Mathf.RoundToInt(shopPrice * 0.8f);
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

        m_PlayerData.AddEnvCardPoolData(shopCardData.card.id);
        DataSystem.Instance.AddCoin(-price);
        m_CurrentShopCards.Remove(shopCardData);
        DataSystem.Instance.SaveDataJoeyPlayer();
        RefreshShopDisplay();
        YActionSystem.Instance.DispatchAction(EActionId.RefreshCardLimitDebuff);

        ECardType cardType = shopCardData.card.GetCardType();
        if (m_BuildControl != null)
        {
            m_BuildControl.RefreshEquipedCardsByType(cardType, false);
        }

        Debug.Log("购买成功！花费 " + price + " 金币，卡牌 " + shopCardData.card.cardName + " 已加入卡牌池");
    }

    void OnCoinChange(object[] paraArray)
    {
        int coin = (int)paraArray[0];
        int delta = paraArray.Length > 1 && paraArray[1] is int ? (int)paraArray[1] : 0;
        m_View.TxtCoin.text = coin.ToString();
        if (delta != 0)
        {
            UIDamageTextControl damageTextControl = m_DamageTextPool.Get();
            if (delta > 0)
            {
                damageTextControl.SetCoinData(delta, Asset.UIRoot, Vector3.zero);
            }
            else
            {
                damageTextControl.SetData(-delta, Asset.UIRoot, Vector3.zero, true);
            }
            damageTextControl.transform.position = m_View.TxtCoin.transform.position - new Vector3(1f, 1f, 0f);
        }
    }

    protected override void OnClose()
    {
        if (m_BuildControl != null)
        {
            m_BuildControl.Close();
        }
        for (int i = 0; i < m_ShopCardList.Count; i++)
        {
            if (m_ShopCardList[i] != null)
            {
                m_ShopCardList[i].Close();
            }
        }
        m_ShopCardList.Clear();
        m_CurrentShopCards.Clear();
        if (m_DamageTextPool != null)
        {
            m_DamageTextPool.ReleaseAll();
        }
        base.OnClose();
    }

    private void OnDestroy()
    {
        if (m_DamageTextPool != null)
        {
            m_DamageTextPool.DestroyAll();
            m_DamageTextPool = null;
        }
    }
}

