using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GamePhase
{
	battleStart, playerStart, playerAction, playerEnd, enemyStart, enemyAction, enemyEnd, battleEnd
}

public class BattleManager : MonoSingleton<BattleManager>
{
    public int level =1 ;// 关卡等级
    public GamePhase gamePhase = GamePhase.battleStart;

    public List<Transform> envPanels = new List<Transform>();
    public List<List<GameObject>> envCardListList = new List<List<GameObject>>();
    public Transform attackPanel;
    public List<GameObject> attackCardList = new List<GameObject>();
    public Transform defencePanel;
    public List<GameObject> defenceCardList = new List<GameObject>();
    public Transform skillPanel;
    public List<GameObject> skillCardList = new List<GameObject>();
    public Transform itemPanel;
    public List<GameObject> itemCardList = new List<GameObject>();
    public GameObject cardPrefab;

    // 弃牌列表
    public List<string> usedCardIdList = new List<string>();

	void Start()
	{
		// 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
		GameStart();
	}

    void OnEnable()
    {
        GameEvents.OnCardClicked += OnCardClicked;
    }
    void OnDisable()
    {
        GameEvents.OnCardClicked -= OnCardClicked;
    }

    private void OnCardClicked(GameObject cardGameObject)
    {
        var cd = cardGameObject.GetComponent<CardDisplay>();
        if (cd == null || cd.card == null)
        {
            Debug.LogError("OnCardClicked: CardDisplay 或 card 为空");
            return;
        }
        Debug.Log($"OnCardClicked: {cd.card.id}, {cd.card.state}");
    }

    private GameObject CreateCard(Transform parent, string cardId,CardState state, List<GameObject> attachList = null)
    {
        if (parent == null || string.IsNullOrEmpty(cardId)) {
            Debug.LogError($"CreateCard: parent is null or cardId is empty, cardId: {cardId}");
            return null;
        }
        if (!GData.Instance.CardDict.ContainsKey(cardId)) {
            Debug.LogError($"CreateCard: cardId not found in CardDict, cardId: {cardId}");
            return null;
        }

        var go = Instantiate(cardPrefab, parent);
        // 视觉倒序：新卡插到最前
        go.transform.SetSiblingIndex(0);
        if (attachList != null) attachList.Add(go);

        var cd = go.GetComponent<CardDisplay>();
        var baseCard = GData.Instance.CardDict[cardId];
        cd.card = baseCard.Clone();
        cd.card.state = state;
        cd.ShowCard();
        return go;
    }

    private bool TryGetPanelAndList(string cardType, out Transform panel, out List<GameObject> list)
    {
        switch (cardType)
        {
            case "attack": panel = attackPanel; list = attackCardList; return true;
            case "defence": panel = defencePanel; list = defenceCardList; return true;
            case "skill": panel = skillPanel; list = skillCardList; return true;
            case "item": panel = itemPanel; list = itemCardList; return true;
            default: panel = null; list = null; return false;
        }
    }

    public void GameStart()
    {
        // 加载所有卡牌数据
        GData.Instance.LoadAll();
        // 抽取环境卡牌
        List<List<string>> cardIdListEnv = CardDraw.Instance.DrawCardEnv(level);
        for (int i = 0; i < cardIdListEnv.Count; i++)
        {
            List<string> cardIdList = cardIdListEnv[i];
            List<GameObject> oneEnvCardList = new List<GameObject>();
            for (int j = 0; j < cardIdList.Count; j++)
            {
                string cardId = cardIdList[j];
                CardState state = CardState.EnvInactive;
                if (j == 0) state = CardState.EnvActive;
                CreateCard(envPanels[i].transform, cardId, state, oneEnvCardList);
            }
            envCardListList.Add(oneEnvCardList);
        }

        // 遍历玩家牌组，根据卡牌类型分配到对应面板
        foreach (var kv in GData.Instance.DeckItemDict)
        {
            string cardType = kv.Key;
            List<string> cardIds = kv.Value;
            
            if (cardIds == null || cardIds.Count == 0) continue;
            if (!TryGetPanelAndList(cardType, out var panel, out var list)) continue;

            for (int j = 0; j < cardIds.Count; j++)
            {
                string cardId = cardIds[j];
                CardState state = CardState.Default;
                if (j == 0) state = CardState.Deck;
                CreateCard(panel, cardId, state, list);
            }
        }
        Debug.Log($"attackCardList: {attackCardList.Count}, defenceCardList: {defenceCardList.Count}, skillCardList: {skillCardList.Count}, itemCardList: {itemCardList.Count}");

    }




}