using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class BattleManager : MonoSingleton<BattleManager>
{
    public int level =1 ;// 关卡等级
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
    public List<GameObject> usedCardList = new List<GameObject>();

	void Start()
	{
		// 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
        PhaseManager.Instance.SetGamePhase(GamePhase.battleStart);
        GameEvents.RaiseHPChanged(PData.Instance.playerHealth);
	}

    void OnEnable()
    {
        GameEvents.OnCardClicked += OnCardClicked;
    }
    void OnDisable()
    {
        GameEvents.OnCardClicked -= OnCardClicked;
    }


    public GameObject GetRandomEnemy()
    {
        return EnemyManager.GetRandomEnemy(envPanels);
    }

    // public void Handle

    public void ApplyDamageToEnemy(GameObject enemy, int damage)
    {
        var targetCard = enemy.GetComponent<CardDisplay>();
        if (targetCard == null || targetCard.card == null) return;

        targetCard.card.health -= damage;
        if (targetCard.card.health < 0) targetCard.card.health = 0;
        targetCard.ShowCard();

        if (targetCard.card.health <= 0)
        {
            // 移动到已使用堆
            int envListIndex = UIGridHelper.FindEnvListIndexByCardGO(cardGO:enemy, envCardListList:envCardListList);
            if (envListIndex != -1)
            {
                CardHelper.MoveCard(cardGO:enemy, fromCardList:envCardListList[envListIndex], toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
                UIGridHelper.RefreshPanel(envPanels[envListIndex]);
            }
            // 触发击杀
            EffectRunner.Instance.Raise(CardTrigger.OnKill, source: null, target: enemy, value: damage);
        }
    }

    public void OnCardClicked(GameObject cardGameObject)
    {
        var cd = cardGameObject.GetComponent<CardDisplay>();
        if (cd == null || cd.card == null)
        {
            Debug.LogError("OnCardClicked: CardDisplay 或 card 为空");
            return;
        }
        if(cd.card.state != CardState.Active)
        {
            Debug.LogError("OnCardClicked: 卡牌状态不是Active");
            return;
        }
        if (cd.card.position == CardPosition.Env)
        {
            OnEnvClicked(cardGameObject);
        }
        else if (cd.card.position == CardPosition.Bag)
        {
            OnBagClicked(cardGameObject);
        }
        else 
        {
            Debug.LogError("OnCardClicked: 未知的位置");
            return;
        }
    }


    public void OnEnvClicked(GameObject cardGameObject)
    {
        if (cardGameObject.GetComponent<CardDisplay>().card.type == "monster")
        {
            OnEnvMonsterClicked(cardGameObject);
        }
        else if (cardGameObject.GetComponent<CardDisplay>().card.type == "other")
        {
            Debug.Log($"OnEnvClicked: {cardGameObject.name}");
        }
        else
        {
            OnEnvCardClicked(cardGameObject);
        }
    }

    public void OnBagClicked(GameObject cardGameObject)
    {
        if (cardGameObject.GetComponent<CardDisplay>().card.type == "attack")
        {
            OnBagAttackClicked(cardGameObject);
        }
        else if (cardGameObject.GetComponent<CardDisplay>().card.type == "defence")
        {
            OnBagDefenceClicked(cardGameObject);
        }
        else if (cardGameObject.GetComponent<CardDisplay>().card.type == "skill")
        {
            OnBagSkillClicked(cardGameObject);
        }
        else if (cardGameObject.GetComponent<CardDisplay>().card.type == "item")
        {
            OnBagItemClicked(cardGameObject);
        }
        else
        {
            Debug.LogError("OnBagClicked: 未知的位置");
            return;
        }
    }

    public void OnBagAttackClicked(GameObject cardGameObject)
    {
        Debug.Log($"OnBagAttackClicked: {cardGameObject.name}");
    }

    public void OnBagDefenceClicked(GameObject cardGameObject)
    {
        Debug.Log($"OnBagDefenceClicked: {cardGameObject.name}");
    }

    public void OnBagSkillClicked(GameObject cardGameObject)
    {
        Debug.Log($"OnBagSkillClicked: {cardGameObject.name}");
    }

    public void OnBagItemClicked(GameObject cardGameObject)
    {
        Debug.Log($"OnBagItemClicked: {cardGameObject.name}");
    }

    public void OnEnvMonsterClicked(GameObject cardGameObject)
    {
        if (attackCardList.Count == 0)
        {
            CardHelper.CreateCardToTransform(cardPrefab:cardPrefab, parent:attackPanel, cardId:"1005", state:CardState.Active, position:CardPosition.Bag, attachList:attackCardList);
        }
        GameObject attackCardGameObject = UIGridHelper.GetCardListOrderIndex0(attackPanel);
        UseAttack(attackCardGameObject, cardGameObject);
    }

    public void OnEnvCardClicked(GameObject cardGO)
    {
        int envListIndex = UIGridHelper.FindEnvListIndexByCardGO(cardGO:cardGO, envCardListList:envCardListList);
        
        if (envListIndex < 0 || envListIndex >= envCardListList.Count)
        {
            Debug.LogError("OnEnvCardClicked: envListIndex 超出范围");
            return;
        }

        // 从 env 的 list 中移除卡
        
        switch (cardGO.GetComponent<CardDisplay>().card.type)
        {
            case "attack":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:envCardListList[envListIndex], toCardList:attackCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(attackPanel);
                UIGridHelper.RefreshPanel(attackPanel);
                break;
            case "defence":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:envCardListList[envListIndex], toCardList:defenceCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(defencePanel);
                UIGridHelper.RefreshPanel(defencePanel);
                break;
            case "skill":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:envCardListList[envListIndex], toCardList:skillCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(skillPanel);
                UIGridHelper.RefreshPanel(skillPanel);
                break;
            case "item":
                CardHelper.MoveCard(cardGO:cardGO, fromCardList:envCardListList[envListIndex], toCardList:itemCardList, state:CardState.Active, position:CardPosition.Bag);
                cardGO.transform.SetParent(itemPanel);
                UIGridHelper.RefreshPanel(itemPanel);
                break;
            default:
                Debug.LogError("OnEnvCardClicked: 未知的位置");
                return;
        }
        UIGridHelper.RefreshPanel(envPanels[envListIndex]);
        PhaseManager.Instance.SetGamePhase(GamePhase.playerStart);
    }

    public void UseAttack(GameObject attakCardGameObject,GameObject targetCardGameObject)
    {
        var attakCard = attakCardGameObject.GetComponent<CardDisplay>();
        var targetCard = targetCardGameObject.GetComponent<CardDisplay>();
        if (attakCard == null || targetCard == null)
        {
            Debug.LogError("UseAttack: attakCard 或 targetCard 为空");
        }
        int attackValue = attakCard.card.attack;
        targetCard.card.health -= attackValue;
        if (targetCard.card.health <= 0)
        {
            targetCard.card.health = 0;
        }
        targetCard.ShowCard();
        CardHelper.MoveCard(cardGO:attakCardGameObject, fromCardList:attackCardList, toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
        UIGridHelper.RefreshPanel(attackPanel);

        if (targetCard.card.health <= 0)
        {
            int envListIndex = UIGridHelper.FindEnvListIndexByCardGO(cardGO:targetCardGameObject, envCardListList:envCardListList);
            if (envListIndex == -1)
            {
                Debug.LogError("MonsterAttack: envListIndex 为空");
                return;
            }
            CardHelper.MoveCard(cardGO:targetCardGameObject, fromCardList:envCardListList[envListIndex], toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
            UIGridHelper.RefreshPanel(envPanels[envListIndex]);
        }
        else
        {
            MonsterAttack(monsterCardGO:targetCardGameObject);
        }
    }

    public void UseDefence(GameObject attackGO,GameObject defenceGO=null)
    {
        int defenceValue = 0;
        if (defenceGO != null)
        {
            defenceValue = defenceGO.GetComponent<CardDisplay>().card.defence;
        }

        int attackValue = attackGO.GetComponent<CardDisplay>().card.attack;
        int attackRealValue = 0;
        if (defenceValue < attackValue)
        {
            attackRealValue = attackValue - defenceValue;
        }
        PData.Instance.SetPlayerHP(PData.Instance.playerHealth - attackRealValue);
        CardHelper.MoveCard(cardGO:defenceGO, fromCardList:defenceCardList, toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
        UIGridHelper.RefreshPanel(defencePanel);

        

    }

    public void MonsterAttack(GameObject monsterCardGO)
    {
        var monsterCard = monsterCardGO.GetComponent<CardDisplay>();
        if (monsterCard == null || monsterCard.card == null)
        {
            Debug.LogError("MonsterAttack: monsterCard 或 monsterCard.card 为空");
            return;
        }
        if (defenceCardList.Count == 0)
        {
            UseDefence(attackGO:monsterCardGO,defenceGO:null);
        }
        else
        {
            UseDefence(attackGO:monsterCardGO,defenceGO:defenceCardList[0]);
        }
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
                CardState state = CardState.Inactive;
                if (j == 0) state = CardState.Active;
                CardHelper.CreateCardToTransform(cardPrefab:cardPrefab, parent:envPanels[i].transform, cardId:cardId, state:state, position:CardPosition.Env, attachList:oneEnvCardList);
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
                CardState state = CardState.Inactive;
                if (j == 0) state = CardState.Active;
                CardHelper.CreateCardToTransform(cardPrefab:cardPrefab, parent:panel, cardId:cardId, state:state, position:CardPosition.Bag, attachList:list);
            }
        }
        Debug.Log($"attackCardList: {attackCardList.Count}, defenceCardList: {defenceCardList.Count}, skillCardList: {skillCardList.Count}, itemCardList: {itemCardList.Count}");
    }




}