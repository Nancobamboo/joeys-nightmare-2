using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CardDisplay : MonoBehaviour
{

    public Text nameText;
    public Text descriptionText;
    public Text leftText;
    public Text rightText;

    public Image backgroundImage;

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
        nameText.text = card.name;
        if (descriptionText != null)
        {
            descriptionText.text = card.desc;
        }
        

        if (card is EnemyCard)
        {
            var enemy = card as EnemyCard;
            if (leftText != null)
            {
                leftText.text = enemy.current_attack.ToString();
            }
            if (rightText != null)
            {
                rightText.text = enemy.current_hp.ToString();
            }
        }
        else if (card is ItemCard)
        {
            var item = card as ItemCard;
            if (leftText != null)
            {
                if (item.attack <=0)
                {
                    leftText.gameObject.SetActive(false);
                }
                else
                {
                    leftText.text = item.current_attack.ToString();
                }
            }
            if (rightText != null)
            {
                if (item.heal <=0)
                {
                    rightText.gameObject.SetActive(false);
                }
                else
                {
                    rightText.text = item.current_heal.ToString();
                }
            }
        }
    }








}
