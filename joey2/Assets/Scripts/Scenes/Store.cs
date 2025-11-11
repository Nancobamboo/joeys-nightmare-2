using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;  // 添加这一行

public class Store : MonoSingleton<Store>
{
    public Text goldText;
    public List<Transform> itemList = new List<Transform>();


    void Start()
    {
        if (PData.Instance.currentLevel > 0)
        {
            level = PData.Instance.currentLevel;
        }

        // 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
        PhaseManager.Instance.SetGamePhase(GamePhase.battleStart);
        PData.Instance.SetPlayerHP(PData.Instance.playerHealth);
    }


    public void OnHPChanged(int hp)
    {
        StartCoroutine(VFXStackHelper.ChangeJoeyImage(joeyImage: joeyImage));
    }




}