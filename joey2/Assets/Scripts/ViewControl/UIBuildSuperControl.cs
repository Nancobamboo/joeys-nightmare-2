using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuildSuperControl : YViewControl
{
    private UIBuildNewView m_View;
    private List<UIBuildCardNewControl> EquipedCardList = new List<UIBuildCardNewControl>();
    private RectTransform[] m_EquipedItemArray;
    private DataJoeyPlayer m_PlayerData;
    public ECardType m_CurrentCardType;
    private int m_SellCardCount = 0;

    public static EResType GetResType()
    {
        return EResType.UIBuildNew;
    }

    protected override void OnInit()
    {
        base.OnInit();
        m_View = CreateView<UIBuildNewView>();
        m_View.BtnAttack.onClick.AddListener(OnBtnAttackClick);
        m_View.BtnDefence.onClick.AddListener(OnBtnDefenceClick);
        m_View.BtnItem.onClick.AddListener(OnBtnItemClick);
        m_View.BtnSkill.onClick.AddListener(OnBtnSkillClick);
        m_EquipedItemArray = new RectTransform[] { m_View.item7, m_View.item6, m_View.item5, m_View.item4, m_View.item3, m_View.item2, m_View.item1 };
        m_PlayerData = DataSystem.Instance.GetDataJoeyPlayer();
        m_CurrentCardType = ECardType.other;
        RegistAction(EActionId.OnCoinChange, OnCoinChange);
    }
    void OnBtnAttackClick()
    {
        RefreshEquipedCardsByType(ECardType.attack);
    }
    void OnBtnDefenceClick()
    {
        RefreshEquipedCardsByType(ECardType.defence);
    }
    void OnBtnItemClick()
    {
        RefreshEquipedCardsByType(ECardType.item);
    }
    void OnBtnSkillClick()
    {
        RefreshEquipedCardsByType(ECardType.skill);
    }

    public void RefreshEquipedCardsByType(ECardType cardType, bool isSaveBuild = true)
    {
        m_CurrentCardType = cardType;
        m_View.SelectAttack.SetActive(false);
        m_View.SelectDefence.SetActive(false);
        m_View.SelectItem.SetActive(false);
        m_View.SelectSkill.SetActive(false);

        switch (cardType)
        {
            case ECardType.attack:
                m_View.SelectAttack.SetActive(true);
                break;
            case ECardType.defence:
                m_View.SelectDefence.SetActive(true);
                break;
            case ECardType.item:
                m_View.SelectItem.SetActive(true);
                break;
            case ECardType.skill:
                m_View.SelectSkill.SetActive(true);
                break;
            default:
                return;
        }

        if (EquipedCardList.Count < m_EquipedItemArray.Length)
        {
            for (int i = EquipedCardList.Count; i < m_EquipedItemArray.Length; i++)
            {
                UIBuildCardNewControl cardControl = Asset.OpenUI<UIBuildCardNewControl>(m_View.Content);
                RectTransform itemRect = m_EquipedItemArray[i];
                cardControl.CacheTrans.localPosition = itemRect.localPosition;
                cardControl.CacheTrans.localScale = Vector3.one;
                cardControl.EquipIndex = i;
                cardControl.OnDragEndHandler = OnCardDragEnd;
                EquipedCardList.Add(cardControl);
            }
        }

        List<string> cardIds = new List<string>();
        for (int i = 0; i < m_PlayerData.EnvCardPool.Count; i++)
        {
            string cardId = m_PlayerData.EnvCardPool[i];
            Card cardConfig = GData.Instance.GetCardConfigById(cardId);
            if (cardConfig != null && cardConfig.GetCardType() == cardType)
            {
                cardIds.Add(cardId);
            }
        }

        for (int i = 0; i < m_EquipedItemArray.Length; i++)
        {
            UIBuildCardNewControl cardControl = EquipedCardList[i];
            if (i < cardIds.Count)
            {
                string cardId = cardIds[i];
                Card cardConfig = GData.Instance.GetCardConfigById(cardId);
                if (cardConfig != null && cardConfig.GetCardType() == cardType)
                {
                    cardControl.gameObject.SetActive(true);
                    cardControl.SetData(cardConfig);
                }
                else
                {
                    cardControl.gameObject.SetActive(false);
                }
            }
            else
            {
                cardControl.gameObject.SetActive(false);
            }
        }
    }

    public void SetShopData()
    {
        for (int i = 0; i < m_EquipedItemArray.Length; i++)
        {
            UIBuildCardNewControl cardControl = Asset.OpenUI<UIBuildCardNewControl>(m_View.Content);
            RectTransform itemRect = m_EquipedItemArray[i];
            cardControl.CacheTrans.localPosition = itemRect.localPosition;
            cardControl.CacheTrans.localScale = Vector3.one;
            cardControl.EquipIndex = i;
            cardControl.OnDragEndHandler = OnCardDragEnd;
            EquipedCardList.Add(cardControl);
        }

        m_View.TxtCoin.text = "0";
        m_View.ImgSell.SetActive(false);
        m_View.ImgDel.SetActive(true);
        RefreshEquipedCardsByType(ECardType.attack);
        UpdateTxtCoin();
    }

    void OnCoinChange(object[] paraArray)
    {
        int coin = (int)paraArray[0];
        UpdateTxtCoin();
    }

    void UpdateTxtCoin()
    {
        int sellCost = GetSellCardCost();
        m_View.TxtCoin.text = "-" + sellCost.ToString();
    }

    public void SaveBuild(ECardType cardType)
    {
    }

    private void OnCardDragEnd(UIBuildCardNewControl draggedCard, PointerEventData eventData)
    {
        if (CheckCardInDeleteArea(draggedCard, eventData))
        {
            DeleteCard(draggedCard);
            return;
        }

        for (int i = 0; i < m_EquipedItemArray.Length; i++)
        {
            if (m_EquipedItemArray[i] != null)
            {
                RectTransform itemRect = m_EquipedItemArray[i];
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    itemRect,
                    eventData.position,
                    eventData.pressEventCamera))
                {
                    OnCardDragEndSwap(draggedCard, i);
                    return;
                }
            }
        }

        RectTransform rectTransform = draggedCard.CacheTrans as RectTransform;
        if (rectTransform != null)
        {
            if (draggedCard.EquipIndex < m_EquipedItemArray.Length)
            {
                rectTransform.localPosition = m_EquipedItemArray[draggedCard.EquipIndex].localPosition;
            }
        }
    }

    private bool CheckCardInDeleteArea(UIBuildCardNewControl draggedCard, PointerEventData eventData)
    {
        if (m_View.BtnDelete == null || draggedCard.CardData == null)
        {
            return false;
        }

        RectTransform deleteRect = m_View.BtnDelete.rectTransform;
        RectTransform cardRect = draggedCard.CacheTrans as RectTransform;

        if (deleteRect == null || cardRect == null)
        {
            return false;
        }

        Vector3[] deleteCorners = new Vector3[4];
        Vector3[] cardCorners = new Vector3[4];
        deleteRect.GetWorldCorners(deleteCorners);
        cardRect.GetWorldCorners(cardCorners);

        Rect deleteRectWorld = new Rect(deleteCorners[0].x, deleteCorners[0].y,
            deleteCorners[2].x - deleteCorners[0].x,
            deleteCorners[2].y - deleteCorners[0].y);

        Rect cardRectWorld = new Rect(cardCorners[0].x, cardCorners[0].y,
            cardCorners[2].x - cardCorners[0].x,
            cardCorners[2].y - cardCorners[0].y);

        return deleteRectWorld.Overlaps(cardRectWorld);
    }

    private void DeleteCard(UIBuildCardNewControl draggedCard)
    {
        if (draggedCard.CardData == null)
        {
            return;
        }

        Card card = draggedCard.CardData;
        string cardId = card.id;

        int sellCost = GetSellCardCost();
        if (m_PlayerData.Coin < sellCost)
        {
            Debug.Log($"金币不足！卖卡需要 {sellCost} 金币，当前只有 {m_PlayerData.Coin} 金币");
            RectTransform rectTransform = draggedCard.CacheTrans as RectTransform;
            if (rectTransform != null)
            {
                if (draggedCard.EquipIndex < m_EquipedItemArray.Length)
                {
                    rectTransform.localPosition = m_EquipedItemArray[draggedCard.EquipIndex].localPosition;
                }
            }
            return;
        }

        m_PlayerData.RemoveEnvCardPoolData(cardId);
        DataSystem.Instance.AddCoin(-sellCost);
        m_SellCardCount++;
        Debug.Log($"卖卡成功！花费 {sellCost} 金币，当前卖卡次数: {m_SellCardCount}");

        UpdateTxtCoin();
        DataSystem.Instance.SaveDataJoeyPlayer();

        RectTransform rectTransform2 = draggedCard.CacheTrans as RectTransform;
        if (rectTransform2 != null)
        {
            if (draggedCard.EquipIndex < m_EquipedItemArray.Length)
            {
                rectTransform2.localPosition = m_EquipedItemArray[draggedCard.EquipIndex].localPosition;
            }
        }

        draggedCard.gameObject.SetActive(false);

        RefreshEquipedCardsByType(m_CurrentCardType);
    }

    private int GetSellCardCost()
    {
        return 50 * (m_SellCardCount + 1);
    }

    private void OnCardDragEndSwap(UIBuildCardNewControl draggedCard, int targetItemIndex)
    {
        UIBuildCardNewControl targetCard = null;
        int draggedIndex = -1;
        int targetIndex = -1;

        for (int i = 0; i < EquipedCardList.Count; i++)
        {
            if (EquipedCardList[i] == draggedCard)
            {
                draggedIndex = i;
            }
            if (EquipedCardList[i].EquipIndex == targetItemIndex && EquipedCardList[i] != draggedCard)
            {
                targetCard = EquipedCardList[i];
                targetIndex = i;
            }
        }

        if (targetCard != null)
        {
            UIBuildCardNewControl tempCard = EquipedCardList[draggedIndex];
            EquipedCardList[draggedIndex] = EquipedCardList[targetIndex];
            EquipedCardList[targetIndex] = tempCard;

            int tempEquipIndex = draggedCard.EquipIndex;
            draggedCard.EquipIndex = targetCard.EquipIndex;
            targetCard.EquipIndex = tempEquipIndex;

            if (draggedCard.EquipIndex < m_EquipedItemArray.Length)
            {
                draggedCard.CacheTrans.localPosition = m_EquipedItemArray[draggedCard.EquipIndex].localPosition;
            }

            if (targetCard.EquipIndex < m_EquipedItemArray.Length)
            {
                targetCard.CacheTrans.localPosition = m_EquipedItemArray[targetCard.EquipIndex].localPosition;
            }
        }
        else
        {
            draggedCard.EquipIndex = targetItemIndex;
            if (targetItemIndex < m_EquipedItemArray.Length)
            {
                draggedCard.CacheTrans.localPosition = m_EquipedItemArray[targetItemIndex].localPosition;
            }
        }
    }

    protected override void OnReturn()
    {
        EquipedCardList.Clear();
        base.OnReturn();
    }
}

