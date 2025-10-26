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

        if (!string.IsNullOrEmpty(card.cardName)) cardName.text = card.cardName;
        if (!string.IsNullOrEmpty(card.cardImage)) cardImage.sprite = LoadSprite(card.cardImage);
        if (!string.IsNullOrEmpty(card.description)) description.text = card.description;
        // 注意：使用 iconType 的非空判断再赋值，避免原有用 type 判断的误用
        if (!string.IsNullOrEmpty(card.iconType)) iconType.sprite = LoadSprite(card.iconType);

        SetTypeUI(card.type);
        SetStars(card.stars, card.cardFrame);
    }
    
    private void SetTypeUI(string type)
    {
        // 统一先关掉所有，再按类型开启，避免到处 SetActive(false)
        attack.gameObject.SetActive(false);
        defence.gameObject.SetActive(false);
        monster.gameObject.SetActive(false);
        other.gameObject.SetActive(false);

        attackText.gameObject.SetActive(false);
        defenceText.gameObject.SetActive(false);
        monsterText.gameObject.SetActive(false);
        otherText.gameObject.SetActive(false);

        attaction.gameObject.SetActive(false);

        switch (type)
        {
            case "attack":
                attack.gameObject.SetActive(true);
                attackText.text = card.attack.ToString();
                attackText.gameObject.SetActive(true);
                break;

            case "defence":
                defence.gameObject.SetActive(true);
                defenceText.text = card.defence.ToString();
                defenceText.gameObject.SetActive(true);
                break;

            case "monster":
                monster.gameObject.SetActive(true);
                monsterText.text = card.health.ToString();
                monsterText.gameObject.SetActive(true);
                attaction.gameObject.SetActive(true);
                break;

            default:
                other.gameObject.SetActive(true);
                otherText.text = string.IsNullOrEmpty(card.description) ? "" : card.description;
                otherText.gameObject.SetActive(true);
                break;
        }
    }

    private void SetStars(int stars, string framePath)
    {
        // 压到 0~3，统一控制显隐
        stars = Mathf.Clamp(stars, 0, 3);
        star1.gameObject.SetActive(stars >= 1);
        star2.gameObject.SetActive(stars >= 2);
        star3.gameObject.SetActive(stars >= 3);

        if (!string.IsNullOrEmpty(framePath))
        {
            var sp = LoadSprite(framePath);
            if (sp != null) cardFrame.sprite = sp;
        }
    }

    private Sprite LoadSprite(string path)
    {
        return string.IsNullOrEmpty(path) ? null : Resources.Load<Sprite>(path);
    }







}
