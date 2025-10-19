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
        ShowCard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowCard()
    {
        nameText.text = card.name;
        descriptionText.text = card.desc;
        if (card is EnemyCard)
        {
            var enemy = card as EnemyCard;
            leftText.text = enemy.current_attack.ToString();
            rightText.text = enemy.current_hp.ToString();
        }
        else if (card is ItemCard)
        {
            var item = card as ItemCard;
            if (item.attack <=0)
            {
                leftText.gameObject.SetActive(false);
            }
            else
            {
                leftText.text = item.current_attack.ToString();
            }
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
