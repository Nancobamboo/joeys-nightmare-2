using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CardDisplay : MonoBehaviour
{

    public Image cardImage;
    public Image cardFrame;
    public Text cardName;
    public Image star1;
    public Image star2;
    public Image star3;
    public Image iconType;
    public Text description;
    public Image attack;
    public Text attackText;
    public Image monster;
    public Text monsterText;
    public Image attaction;
    public Image defence;
    public Text defenceText;
    public Image other;
    public Text otherText;

    public Card card;

    // Start is called before the first frame update
    void Start()
    {
        if (card != null)
        {
            ShowCard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowCard()
    {
        if (card == null)
        {
            Debug.LogWarning("CardDisplay.ShowCard 被调用时 card 为空");
            return;
        }
        if (card.cardName != null)
        {
            cardName.text = card.cardName;
        }
        if (!string.IsNullOrEmpty(card.description))
        {
            description.text = card.description;
        }
        if (!string.IsNullOrEmpty(card.type))
        {
            iconType.sprite = Resources.Load<Sprite>(card.iconType);
        }
        if (card.type == "attack")
        {
            attack.gameObject.SetActive(true);
            defence.gameObject.SetActive(false);
            monster.gameObject.SetActive(false);
            other.gameObject.SetActive(false);
            attackText.text = card.attack.ToString();
            attackText.gameObject.SetActive(true);
        }
        else if (card.type == "defence")
        {
            defence.gameObject.SetActive(true);
            attack.gameObject.SetActive(false);
            monster.gameObject.SetActive(false);
            other.gameObject.SetActive(false);
            defenceText.text = card.defence.ToString();
            defenceText.gameObject.SetActive(true);
        }
        else if (card.type == "monster")
        {
            monster.gameObject.SetActive(true);
            attack.gameObject.SetActive(false);
            defence.gameObject.SetActive(false);
            other.gameObject.SetActive(false);
            monsterText.text = card.health.ToString();
            monsterText.gameObject.SetActive(true);
            attaction.gameObject.SetActive(true);
        }
        else
        {
            other.gameObject.SetActive(true);
            attack.gameObject.SetActive(false);
            defence.gameObject.SetActive(false);
            monster.gameObject.SetActive(false);
            otherText.text = card.description.ToString();
            otherText.gameObject.SetActive(true);
        }

        if (card.stars <= 0){
            star1.gameObject.SetActive(false);
            star2.gameObject.SetActive(false);
            star3.gameObject.SetActive(false);
        }
        if (card.stars == 1)
        {
            star1.gameObject.SetActive(true);
            star2.gameObject.SetActive(false);
            star3.gameObject.SetActive(false);
        }
        else if (card.stars == 2)
        {
            star1.gameObject.SetActive(true);
            star2.gameObject.SetActive(true);
            star3.gameObject.SetActive(false);
        }
        else if (card.stars >= 3)
        {
            star1.gameObject.SetActive(true);
            star2.gameObject.SetActive(true);
            star3.gameObject.SetActive(true);
        }
    }








}
