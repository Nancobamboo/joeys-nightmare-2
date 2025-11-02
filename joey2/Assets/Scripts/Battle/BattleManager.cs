using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;  // 添加这一行

public class BattleManager : MonoSingleton<BattleManager>
{
    public int level = 1;// 关卡等级
    public int mode = 0; // 模式 : 0-教学, 1-解密, 2-肉鸽,3-测试
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

    public Image joeyImage;


    // 弃牌列表
    public List<GameObject> usedCardList = new List<GameObject>();

    void Start()
    {
        // If PData.currentLevel is greater than editor level, use PData (for level progression)
        // Otherwise, use editor level value and sync to PData
        if (PData.Instance.currentLevel > level)
        {
            // Game progression takes priority
            level = PData.Instance.currentLevel;
        }
        else if (level > 0)
        {
            // Editor value takes priority, sync to PData
            PData.Instance.currentLevel = level;
        }
        else if (PData.Instance.currentLevel > 0)
        {
            // Use PData value if editor value is 0 or not set
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
        GameEvents.OnHPChanged += OnHPChanged;

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
        GameEvents.OnHPChanged -= OnHPChanged;
    }

    public void OnHPChanged(int hp)
    {
        StartCoroutine(VFXStackHelper.ChangeJoeyImage(joeyImage: joeyImage));
    }
    public void OnMonsterAttackPreFinish(GameObject monsterCardGO)
    {
        monsterCardGO.GetComponent<CardDisplay>().card.state = CardState.Active;
    }
    public void OnCardFinished(GameObject cardGO,float delay=0f)
    {
        StartCoroutine(VFXStackHelper.FinshCardVFX(cardGO: cardGO,delay:delay));
    }

    public GameObject GetRandomEnemy()
    {
        return EnemyManager.GetRandomEnemy(envPanels);
    }
    public List<GameObject> GetAllEnemies()
    {
        return EnemyManager.GetAllEnemies(envPanels);
    }


    public void UseAttack(GameObject attakCardGameObject, GameObject targetCardGameObject)
    {
        var attakCard = attakCardGameObject.GetComponent<CardDisplay>();
        if (attakCard == null)
        {
            Debug.LogError("UseAttack: attakCard 为空");
            return;
        }
        EffectRunner.Instance.Raise(CardTrigger.OnPlay, source: attakCardGameObject);
        // GameEvents.RaiseAttackInitiated(attakCardGameObject, targetCardGameObject);
        StartCoroutine(VFX.PlayHit(attakCardGameObject, targetCardGameObject, -1, true));

    }

    public void OnAttackPre(GameObject attackerCardGO, GameObject targetCardGO, int damage, bool monsterAttack, Dictionary<string, object> extra = null)
    {
        ApplyDamageToEnemy(enemy: targetCardGO, damage: damage, monsterAttack: monsterAttack, attackerCardGO: attackerCardGO, extra: extra);
    }
    public void ApplyDamageToEnemy(GameObject enemy, int damage, bool monsterAttack = false, GameObject attackerCardGO = null, Dictionary<string, object> extra = null)
    {
        var targetCard = enemy.GetComponent<CardDisplay>();
        if (targetCard == null || targetCard.card == null) return;

        targetCard.card.health -= damage;
        if (targetCard.card.health < 0) targetCard.card.health = 0;
        targetCard.ShowCard();
        StartCoroutine(VFXStackHelper.PlayDamageVFX(cardGO: enemy, damage: damage, monsterAttack: monsterAttack, attackerCardGO: attackerCardGO, extra: extra));
    }

    public void OnDamageComplete(GameObject enemyCardGO, bool monsterAttack = false, int damage = 0, GameObject attackerCardGO = null, Dictionary<string, object> extra = null)
    {
        SettlementEnemy(enemyCardGO, monsterAttack, damage, attackerCardGO, extra);
    }

    public void SettlementEnemy(GameObject enemyCardGO, bool monsterAttack = false, int damage = 0, GameObject attackerCardGO = null, Dictionary<string, object> extra = null)
    {
        var enemyCard = enemyCardGO.GetComponent<CardDisplay>();
        if (enemyCard.card.health <= 0)
        {
            EffectRunner.Instance.Raise(CardTrigger.OnKill, source: enemyCardGO, target: attackerCardGO);
            GameEvents.RaiseCardFinished(cardGO: enemyCardGO,delay:0.4f);
            
        }
        EffectRunner.Instance.Raise(CardTrigger.OnDealDamage, source: attackerCardGO, target: enemyCardGO, value: damage, extra: extra);

        if (monsterAttack && enemyCard.card.health > 0)
        {
            StartCoroutine(VFX.PlayMonsterHit(cardGO: enemyCardGO));

        }
    }

    public void OnAttackPreFinish(GameObject attackerCardGO)
    {
        // 正常情况销毁这个卡,如果这张卡是回旋镖,并且当前回旋镖没打完,就先别销毁
        // 如果是双次攻击卡牌，且还没完成第二次攻击，也先别销毁
        if (attackerCardGO != null)
        {
            var attackerCard = attackerCardGO.GetComponent<CardDisplay>();
            if (attackerCard.card.effectIds.Any(id => id.Contains("BounceToRandomEnemy_OnDealDamage")))
            {
                Debug.Log("OnAttackPreFinish: 回旋镖，不销毁");
                return;
            }

            // Check if this is a double attack card
            var tracker = attackerCardGO.GetComponent<DoubleAttackTracker>();
            if (tracker != null)
            {
                // If double attack is completed (isDoubleAttack == false and attackCount >= 2), destroy it
                // Or if it's still in progress, don't destroy yet
                if (tracker.isDoubleAttack && tracker.attackCount < 2)
                {
                    Debug.Log("OnAttackPreFinish: 双次攻击卡，还未完成第二次攻击，不销毁");
                    return;
                }
                // If attackCount >= 2, the DoubleAttack_OnDealDamage will handle destruction
                // So we don't destroy here to avoid double destruction
                else if (tracker.attackCount >= 2)
                {
                    Debug.Log("OnAttackPreFinish: 双次攻击卡已完成，由OnDealDamage处理销毁");
                    return;
                }
            }

            // Normal card or double attack not applicable, destroy it
            // Debug.Log("OnAttackPreFinish: 正常销毁");
            GameEvents.RaiseCardFinished(attackerCardGO);
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
        if (cd.card.state != CardState.Active)
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
        EffectRunner.Instance.Raise(CardTrigger.OnPlay, source: cardGameObject);
        cardGameObject.GetComponent<CardDisplay>().card.state = CardState.Inactive;
        if (cardGameObject.GetComponent<CardDisplay>().card.type == "skill")
        {
            OnBagSkillClicked(cardGameObject);
        }
        else if (cardGameObject.GetComponent<CardDisplay>().card.type == "item")
        {
            OnBagItemClicked(cardGameObject);
        }
        else
        {
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
        // Trigger OnPlay effect for skill cards
        StartCoroutine(VFX.PlayAnimator(cardGameObject, "UI_Carditem_dunpai"));
        // EffectRunner.Instance.Raise(CardTrigger.OnPlay, cardGameObject);

        // Don't move card here - let FinshCardVFX handle the animation and move
        // CardHelper.MoveCard(cardGO:cardGameObject, fromCardList:skillCardList, toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
        // UIGridHelper.RefreshPanel(skillPanel);

        // Trigger card finished animation - FinshCardVFX will handle moving the card after animation
        GameEvents.RaiseCardFinished(cardGO: cardGameObject,delay:0.4f);
    }

    public void OnBagItemClicked(GameObject cardGameObject)
    {
        StartCoroutine(VFX.PlayAnimator(cardGameObject, "UI_Carditem_dunpai"));
        // Trigger OnPlay effect for item cards
        // EffectRunner.Instance.Raise(CardTrigger.OnPlay, cardGameObject);

        // Don't move card here - let FinshCardVFX handle the animation and move
        // CardHelper.MoveCard(cardGO:cardGameObject, fromCardList:itemCardList, toCardList:usedCardList, state:CardState.Used, position:CardPosition.Used);
        // UIGridHelper.RefreshPanel(itemPanel);

        // Trigger card finished animation - FinshCardVFX will handle moving the card after animation
        GameEvents.RaiseCardFinished(cardGO: cardGameObject,delay:0.4f);
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
        int envListIndex = UIGridHelper.FindEnvListIndexByCardGO(cardGO: cardGO, envCardListList: envCardListList);

        if (envListIndex < 0 || envListIndex >= envCardListList.Count)
        {
            Debug.LogError("OnEnvCardClicked: envListIndex 超出范围");
            return;
        }
        // 从 env 的 list 中移除卡
        StartCoroutine(SFX.PlayAudioCoroutine(audioPath:"Audio/SFX/deal_cards",startTime:0f));
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
        UseDefence(attackGO: monsterCardGO);
    }
    public void UseDefence(GameObject attackGO)
    {
        GameObject defenceGO = UIGridHelper.GetCardListOrderIndex0(defencePanel);
        int defenceValue = 0;
        if (defenceGO != null)
        {
            SFX.Instance.StartCoroutine(SFX.PlayAudioCoroutine(audioPath:"Audio/SFX/Battle/Defence",startTime:0f));
            defenceValue = defenceGO.GetComponent<CardDisplay>().card.defence;
        }
        EffectRunner.Instance.Raise(CardTrigger.UseDefence, source: defenceGO, target: attackGO);

        int attackValue = attackGO.GetComponent<CardDisplay>().card.attack;
        int damage = 0;
        if (defenceValue < attackValue)
        {
            damage = attackValue - defenceValue;
        }

        PData.Instance.SetPlayerHP(PData.Instance.playerHealth - damage);
        StartCoroutine(VFXStackHelper.PlayDamageToPlayerVFX(defenceCardGO: defenceGO, damage: damage));

    }

    // public void monsterattack(gameobject monstercardgo)
    // {
    //     startcoroutine(vfx.playmonsterhit(cardgo:monstercardgo));
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
        StartCoroutine(SFX.PlayAudioCoroutine(audioPath:"Audio/SFX/shuffle_cards",startTime:0f));

        // Set player health for tutorial levels (1-3) from CSV config
        if (level >= 1 && level <= 4)
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
                CardHelper.CreateCardToTransform(cardPrefab: cardPrefab, parent: envPanels[i].transform, cardId: cardId, state: state, position: CardPosition.Env, attachList: oneEnvCardList);
            }
            envCardListList.Add(oneEnvCardList);
        }

        // Tutorial levels (1-3) use CSV config equipment deck
        Dictionary<string, List<string>> equipmentDeck;
        if (level >= 1 && level <= 4)
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
                CardHelper.CreateCardToTransform(cardPrefab: cardPrefab, parent: panel, cardId: cardId, state: state, position: CardPosition.Bag, attachList: list);
            }

            // Refresh panel after creating cards to ensure active state is set correctly
            UIGridHelper.RefreshPanel(panel);
        }
        Debug.Log($"attackCardList: {attackCardList.Count}, defenceCardList: {defenceCardList.Count}, skillCardList: {skillCardList.Count}, itemCardList: {itemCardList.Count}");

        // Update player attack and defence after game start
        UpdatePlayerAttackAndDefence();
    }




}