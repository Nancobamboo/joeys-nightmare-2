using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GamePhase
{
	battleStart, playerStart, playerAction, playerEnd, enemyStart, enemyAction, enemyEnd, battleEnd
}

public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;
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


	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		// 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
		GameStart();
	}

    public void GameStart()
    {
        GData.Instance.LoadAll();
        List<List<string>> cardIdListEnv = CardDraw.Instance.DrawCardEnv(level);
        for (int i = 0; i < cardIdListEnv.Count; i++)
        {
            List<string> cardIdList = cardIdListEnv[i];
            List<GameObject> oneEnvCardList = new List<GameObject>();
            foreach (string cardId in cardIdList)
            {
                GameObject cardGO = Instantiate(cardPrefab, envPanels[i].transform);
                oneEnvCardList.Add(cardGO);
                CardDisplay cd = cardGO.GetComponent<CardDisplay>();
                Debug.Log($"cardId: {cardId}, cd: {cd}");
                cd.card = GData.Instance.CardDict[cardId];
                cd.ShowCard();
            }
            envCardListList.Add(oneEnvCardList);
        }

        // 遍历玩家牌组，根据卡牌类型分配到对应面板
        foreach (var kv in GData.Instance.DeckItemDict)
        {
            string cardType = kv.Key;
            List<string> cardIds = kv.Value;
            
            if (cardIds == null || cardIds.Count == 0) continue;
            
            foreach (string cardId in cardIds)
            {
                if (string.IsNullOrEmpty(cardId)) continue;
                if (!GData.Instance.CardDict.ContainsKey(cardId)) continue;
                
                GameObject cardGO = null;
                
                if (cardType == "attack")
                {
                    cardGO = Instantiate(cardPrefab, attackPanel);
                    attackCardList.Add(cardGO);
                }
                else if (cardType == "defence")
                {
                    cardGO = Instantiate(cardPrefab, defencePanel);
                    defenceCardList.Add(cardGO);
                }
                else if (cardType == "skill")
                {
                    cardGO = Instantiate(cardPrefab, skillPanel);
                    skillCardList.Add(cardGO);
                }
                else if (cardType == "item")
                {
                    cardGO = Instantiate(cardPrefab, itemPanel);
                    itemCardList.Add(cardGO);
                }
                
                if (cardGO != null)
                {
                    cardGO.GetComponent<CardDisplay>().card = GData.Instance.CardDict[cardId];
                    cardGO.GetComponent<CardDisplay>().ShowCard();
                }
            }
        }




    }
}