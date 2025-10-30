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
    public Image monsterAttackIcon;
    public Image defence;
    public Text defenceText;
    public Image other;
    public Text otherText;

    public Card card;

    public Dictionary<string, List<GameObject>> TriggerEffectDict = new Dictionary<string, List<GameObject>>();
    public Dictionary<string, string> TriggerAnimDict = new Dictionary<string, string>();
    public Animator CardAnimator;

    void Start()
    {
        if (card != null)
        {
            ShowCard();
        }
        CardAnimator = GetComponentInChildren<Animator>(true);
    }

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
        if (!string.IsNullOrEmpty(card.iconType)) iconType.sprite = LoadSprite(card.iconType);

        SetTypeUI(card.type);
        SetStars(card.stars, card.cardFrame);
    }

    private void SetTypeUI(string type)
    {
        attack.gameObject.SetActive(false);
        defence.gameObject.SetActive(false);
        monster.gameObject.SetActive(false);
        other.gameObject.SetActive(false);

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
                attack.gameObject.SetActive(true);
                attackText.text = card.attack.ToString();
                monster.gameObject.SetActive(true);
                monsterText.text = card.health.ToString();
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

    public void PlayVFX(string cardTrigger = null, List<string> cardEffects = null, string animName = null)
    {

        if (!string.IsNullOrEmpty(animName))
        {
            CardAnimator.CrossFade(animName, 0.1f, 0);
        }

        if (cardEffects != null && cardEffects.Count > 0)
        {
            if (!TriggerEffectDict.ContainsKey(cardTrigger))
            {
                TriggerEffectDict[cardTrigger] = new List<GameObject>();
            }
            var list = TriggerEffectDict[cardTrigger];

            for (int i = 0; i < cardEffects.Count; i++)
            {
                string effectName = cardEffects[i];
                if (string.IsNullOrEmpty(effectName)) continue;
                var vfxPrefab = Resources.Load<GameObject>("VFX/" + effectName);
                if (vfxPrefab == null)
                {
                    Debug.LogWarning($"PlayVFX: 资源未找到 VFX/{effectName}");
                    continue;
                }
                var instance = Instantiate(vfxPrefab);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                list.Add(instance);
            }
        }
    }

    public void StopVFX(string cardTrigger)
    {
        CardAnimator.CrossFade("Idle", 0.1f, 0);

        if (TriggerEffectDict != null && TriggerEffectDict.TryGetValue(cardTrigger, out var list) && list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go != null)
                {
                    Destroy(go);
                }
            }
            list.Clear();
        }
    }

    private void OnDestroy()
    {
        if (TriggerEffectDict != null)
        {
            foreach (var kv in TriggerEffectDict)
            {
                var list = kv.Value;
                if (list == null) continue;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        Destroy(list[i]);
                    }
                }
                list.Clear();
            }
            TriggerEffectDict.Clear();
        }
    }







}
