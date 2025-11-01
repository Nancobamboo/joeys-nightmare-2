using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class VFXDamageHelper
{
    public static string damageUIPath = "prefab/UIDamage";


    public static IEnumerator PlayDamageVFX(Transform transform,Vector3 localPositionShift, int damage)
    {
        GameObject damageUIPrefab = Resources.Load<GameObject>(damageUIPath);
        GameObject damageInstance = null;
        if (damageUIPrefab != null)
        {
            // 实例化伤害UI
            damageInstance = Object.Instantiate(damageUIPrefab, transform);
            
            // 设置位置在图片上方
            damageInstance.transform.localPosition = localPositionShift;
            
            // 设置伤害数字文本
            if (damage > 0)
            {
                Transform damageTextTransform = damageInstance.transform.Find("Image/Damage");
                if (damageTextTransform != null)
                {
                    Text damageText = damageTextTransform.GetComponent<Text>();
                    if (damageText != null)
                    {
                        damageText.text = "-" + damage.ToString();
                        damageText.gameObject.SetActive(true);
                        // Debug.Log($"PlayDamageToPlayerVFX: Set damage text to {damageText.text}");
                    }
                }
            }
            else
            {
                Transform treatmentTextTransform = damageInstance.transform.Find("Image/Damage");
                if (treatmentTextTransform != null)
                {
                    Text treatmentText = treatmentTextTransform.GetComponent<Text>();
                    if (treatmentText != null)
                    {
                        treatmentText.text = "+" + damage.ToString();
                        treatmentText.gameObject.SetActive(true);
                    }
                }
            }
        }

        // 播放伤害数字动画
        if (damageInstance != null)
        {
            Animator damageAnimator = damageInstance.GetComponent<Animator>();
            if (damageAnimator != null)
            {
                damageAnimator.Play("UIDamage_kouxue");
                // Debug.Log("PlayDamageToPlayerVFX: Playing damage animation");
            }
        }
        yield return new WaitForSeconds(0.8f);
        if (damageInstance != null)
        {
            Object.Destroy(damageInstance);
        }
    }



}
