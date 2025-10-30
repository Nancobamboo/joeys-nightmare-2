using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    public Text attackText;
    public Text defenceText;

    private static PlayerDisplay _instance;
    public static PlayerDisplay Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerDisplay>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        // Get active attack card
        int attackValue = 0;
        if (BattleManager.Instance != null && BattleManager.Instance.attackPanel != null)
        {
            // Check attackCardList first
            foreach (var cardGO in BattleManager.Instance.attackCardList)
            {
                if (cardGO != null && cardGO.activeInHierarchy)
                {
                    var cardDisplay = cardGO.GetComponent<CardDisplay>();
                    if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state == CardState.Active)
                    {
                        attackValue = cardDisplay.card.attack;
                        break;
                    }
                }
            }
            
            // Fallback to GetCardListOrderIndex0 if attackCardList is empty
            if (attackValue == 0)
            {
                GameObject activeAttackCard = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.attackPanel);
                if (activeAttackCard != null)
                {
                    var cardDisplay = activeAttackCard.GetComponent<CardDisplay>();
                    if (cardDisplay != null && cardDisplay.card != null)
                    {
                        // Check state, but also check if it's the top card
                        attackValue = cardDisplay.card.attack;
                    }
                }
            }
        }

        // Get active defence card
        int defenceValue = 0;
        if (BattleManager.Instance != null && BattleManager.Instance.defencePanel != null)
        {
            // Check defenceCardList first
            foreach (var cardGO in BattleManager.Instance.defenceCardList)
            {
                if (cardGO != null && cardGO.activeInHierarchy)
                {
                    var cardDisplay = cardGO.GetComponent<CardDisplay>();
                    if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state == CardState.Active)
                    {
                        defenceValue = cardDisplay.card.defence;
                        break;
                    }
                }
            }
            
            // Fallback to GetCardListOrderIndex0 if defenceCardList is empty
            if (defenceValue == 0)
            {
                GameObject activeDefenceCard = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.defencePanel);
                if (activeDefenceCard != null)
                {
                    var cardDisplay = activeDefenceCard.GetComponent<CardDisplay>();
                    if (cardDisplay != null && cardDisplay.card != null)
                    {
                        // Check state, but also check if it's the top card
                        defenceValue = cardDisplay.card.defence;
                    }
                }
            }
        }

        // Update UI
        if (attackText != null)
        {
            attackText.text = attackValue.ToString();
            Debug.Log($"[PlayerDisplay] Update attack: {attackValue}");
        }
        else
        {
            Debug.LogWarning("[PlayerDisplay] attackText is null!");
        }
        
        if (defenceText != null)
        {
            defenceText.text = defenceValue.ToString();
            Debug.Log($"[PlayerDisplay] Update defence: {defenceValue}");
        }
        else
        {
            Debug.LogWarning("[PlayerDisplay] defenceText is null!");
        }
    }
}

