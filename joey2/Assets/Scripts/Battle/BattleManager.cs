using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class BattleManager : MonoSingleton<BattleManager>
{
    public int level =1 ;//   关卡等级
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

    public Image joeyImage ;


    // 弃牌列表
    public List<GameObject> usedCardList = new List<GameObject>();

	void Start()
	{
		// Load level from PData if exists, otherwise use default
		if (PData.Instance.currentLevel > 0)
		{
			level = PData.Instance.currentLevel;
		}
		
		// 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
        PhaseManager.Instance.SetGamePhase(GamePhase.battleStart);
        PData.Instance.SetPlayerHP(PData.Instance.playerHealth);
	}

    void OnEnable()
    {
        GameEvents.OnCardClicked += OnCardClicked;
        GameEvents.OnDamageComplete += OnDamageComplete;
        GameEvents.OnDamageToPlayerComplete += OnDamageToPlayerComplete;
        GameEvents.OnAttackPre += OnAttackPre;
        GameEvents.OnAttackPreFinish += OnAttackPreFinish;
        GameEvents.OnMonsterAttackPre += OnMonsterAttackPre;
        GameEvents.OnMonsterAttackPreFinish += OnMonsterAttackPreFinish;
        GameEvents.OnNextLevelRequested += OnNextLevelRequested;
        GameEvents.OnCardFinished += OnCardFinished;
    }
    void OnDisable()
    {
        GameEvents.OnCardClicked -= OnCardClicked;
        GameEvents.OnDamageComplete -= OnDamageComplete;
        GameEvents.OnDamageToPlayerComplete -= OnDamageToPlayerComplete;
        GameEvents.OnAttackPre -= OnAttackPre;
        GameEvents.OnAttackPreFinish -= OnAttackPreFinish;
        GameEvents.OnMonsterAttackPre -= OnMonsterAttackPre;
        GameEvents.OnMonsterAttackPreFinish -= OnMonsterAttackPreFinish;
        GameEvents.OnNextLevelRequested -= OnNextLevelRequested;
        GameEvents.OnCardFinished -= OnCardFinished;
    }


    public void OnMonsterAttackPreFinish(GameObject monsterCardGO)
    {
        monsterCardGO.GetComponent<CardDisplay>().card.state = CardState.Active;
    }
    public void OnCardFinished(GameObject cardGO)
    {
        StartCoroutine(VFXStackHelper.FinshCardVFX(cardGO:cardGO));
    }

    public GameObject GetRandomEnemy()
    {
        return EnemyManager.GetRandomEnemy(envPanels);
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
        StartCoroutine(VFX.PlayHit(attakCardGameObject,targetCardGameObject,attackValue,true));
    }
    public void OnAttackPreFinish(GameObject attackerCardGO)
    {
        GameEvents.RaiseCardFinished(cardGO:attackerCardGO);
    }
    // public void Handle
    public void OnAttackPre(GameObject attackerCardGO,GameObject targetCardGO,int damage,bool monsterAttack)
    {
        ApplyDamageToEnemy(enemy:targetCardGO, damage:damage,monsterAttack:monsterAttack);
    }
    public void ApplyDamageToEnemy(GameObject enemy, int damage,bool monsterAttack=false)
    {
        var targetCard = enemy.GetComponent<CardDisplay>();
        if (targetCard == null || targetCard.card == null) return;

        targetCard.card.health -= damage;
        if (targetCard.card.health < 0) targetCard.card.health = 0;
        targetCard.ShowCard();
        StartCoroutine(VFXStackHelper.PlayDamageVFX(cardGO:enemy, damage:damage, monsterAttack:monsterAttack));
    }

    public void OnDamageComplete(GameObject enemy,bool monsterAttack=false)
    {
        SettlementEnemy(enemy,monsterAttack);
    }

    public void SettlementEnemy(GameObject enemy,bool monsterAttack=false)
    {
        var enemyCard = enemy.GetComponent<CardDisplay>();
        if (enemyCard.card.health <= 0)
        {
            // 移动到已使用堆
            GameEvents.RaiseCardFinished(cardGO:enemy);
            // 触发击杀
            EffectRunner.Instance.Raise(CardTrigger.OnKill, source: null, target: enemy);
        }
        else 
        {
            if (monsterAttack)
            {
                StartCoroutine(VFX.PlayMonsterHit(cardGO:enemy));
                // MonsterAttack(monsterCardGO:enemy);
            }
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
        
        // Check if card id is 6001 (next level card)
        if (cd.card.id == "6001")
        {
            GameEvents.RaiseNextLevelRequested();
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
            // Debug.Log($"OnEnvClicked: {cardGameObject.name}");
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
        // Debug.Log($"OnBagAttackClicked: {cardGameObject.name}");
    }

    public void OnBagDefenceClicked(GameObject cardGameObject)
    {
        // Debug.Log($"OnBagDefenceClicked: {cardGameObject.name}");
    }

    public void OnBagSkillClicked(GameObject cardGameObject)
    {
        // Debug.Log($"OnBagSkillClicked: {cardGameObject.name}");
    }

    public void OnBagItemClicked(GameObject cardGameObject)
    {
        // Debug.Log($"OnBagItemClicked: {cardGameObject.name}");
    }

    public void OnEnvMonsterClicked(GameObject cardGameObject)
    {
        if (attackCardList.Count == 0)
        {
            // CardHelper.CreateCardToTransform(cardPrefab:cardPrefab, parent:attackPanel, cardId:"1005", state:CardState.Active, position:CardPosition.Bag, attachList:attackCardList);
            return;
        }
        cardGameObject.GetComponent<CardDisplay>().card.state = CardState.Inactive;
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
        StartCoroutine(VFXStackHelper.PlayAppearDisappearVFX(cardGO:cardGO, envListIndex:envListIndex));
        PhaseManager.Instance.SetGamePhase(GamePhase.playerEnd);
    }




    public void OnDamageToPlayerComplete()
    {
        // GameEvents.RaiseCardFinished(cardGO:defenceGO);
    }

    public void SettlementPlayer()
    {

        // GameEvents.RaiseCardFinished(cardGO:defenceGO);
    }


    public void OnMonsterAttackPre(GameObject monsterCardGO)
    {
        UseDefence(attackGO:monsterCardGO);
    }
    public void UseDefence(GameObject attackGO)
    {
        GameObject defenceGO = UIGridHelper.GetCardListOrderIndex0(defencePanel);
        int defenceValue = 0;
        if (defenceGO != null)
        {
            defenceValue = defenceGO.GetComponent<CardDisplay>().card.defence;
        }

        int attackValue = attackGO.GetComponent<CardDisplay>().card.attack;
        int damage = 0;
        if (defenceValue < attackValue)
        {
            damage = attackValue - defenceValue;
        }

        PData.Instance.SetPlayerHP(PData.Instance.playerHealth - damage);
        StartCoroutine(VFXStackHelper.PlayDamageToPlayerVFX(joeyImage:joeyImage,defenceCardGO:defenceGO,damage:damage));

    }

    // public void MonsterAttack(GameObject monsterCardGO)
    // {
    //     StartCoroutine(VFX.PlayMonsterHit(cardGO:monsterCardGO));
    // }






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

    // Update player attack and defence based on active cards
    public void UpdatePlayerAttackAndDefence()
    {
        // Get active attack card
        int attackValue = 0;
        if (attackPanel != null)
        {
            GameObject activeAttackCard = UIGridHelper.GetCardListOrderIndex0(attackPanel);
            if (activeAttackCard != null)
            {
                var cardDisplay = activeAttackCard.GetComponent<CardDisplay>();
                if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state == CardState.Active)
                {
                    attackValue = cardDisplay.card.attack;
                }
            }
        }

        // Get active defence card
        int defenceValue = 0;
        if (defencePanel != null)
        {
            GameObject activeDefenceCard = UIGridHelper.GetCardListOrderIndex0(defencePanel);
            if (activeDefenceCard != null)
            {
                var cardDisplay = activeDefenceCard.GetComponent<CardDisplay>();
                if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state == CardState.Active)
                {
                    defenceValue = cardDisplay.card.defence;
                }
            }
        }

        // Update PData
        // Debug.Log($"[BattleManager] UpdatePlayerAttackAndDefence: attack={attackValue}, defence={defenceValue}");
        PData.Instance.SetPlayerAttack(attackValue);
        PData.Instance.SetPlayerDefence(defenceValue);
    }

    /// <summary>
    /// Handle next level requested event
    /// </summary>
    private void OnNextLevelRequested()
    {
        LoadNextLevel();
    }

    /// <summary>
    /// Load next level scene based on current level
    /// </summary>
    private void LoadNextLevel()
    {
        int nextLevel = level + 1;
        
        // Save next level to PData before reloading scene
        PData.Instance.currentLevel = nextLevel;
        
        Debug.Log($"Loading next level: {nextLevel}, reloading Battle scene");
        
        // Reload Battle scene with new level
        SceneLoader.Instance.LoadScene("Battle");
    }

    public void GameStart()
    {
        // 加载所有卡牌数据
        GData.Instance.LoadAll();
        
        // Set player health for tutorial levels (1-3) from CSV config
        if (level >= 1 && level <= 3)
        {
            var playerData = GData.Instance.GetTutorialPlayerData(level);
            if (playerData.HasValue)
            {
                PData.Instance.playerHealth = playerData.Value.health;
                PData.Instance.playerMaxHealth = playerData.Value.maxHealth;
                PData.Instance.SetPlayerHP(playerData.Value.health);
            }
        }
        
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

        // Tutorial levels (1-3) use CSV config equipment deck
        Dictionary<string, List<string>> equipmentDeck;
        if (level >= 1 && level <= 3)
        {
            // Load tutorial deck from CSV
            equipmentDeck = GData.Instance.GetTutorialEquipmentDeck(level);
            if (equipmentDeck.Count == 0)
            {
                Debug.LogError($"Tutorial equipment deck for level {level} not found in CSV!");
                return;
            }
        }
        else
        {
            // Normal levels use deck from GData
            equipmentDeck = GData.Instance.DeckItemDict;
        }

        // 遍历玩家牌组，根据卡牌类型分配到对应面板
        foreach (var kv in equipmentDeck)
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
            
            // Refresh panel after creating cards to ensure active state is set correctly
            UIGridHelper.RefreshPanel(panel);
        }
        Debug.Log($"attackCardList: {attackCardList.Count}, defenceCardList: {defenceCardList.Count}, skillCardList: {skillCardList.Count}, itemCardList: {itemCardList.Count}");
        
        // Update player attack and defence after game start
        UpdatePlayerAttackAndDefence();
    }




}