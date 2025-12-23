using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LotteryEditor : EditorWindow
{
    private List<string> m_NameList = new List<string>()
    {
        "宋晓帆",
        "顾彬",
        "赵炫耀",
        "邓鑫敏",
        "段能鑫",
        "宋胤宏",
        "凡新蕾",
        "周国瑾",
        "郑炫之",
        "瞿俐雯",
        "蔡雨涵",
        "夏冰",
        "胡文轩",
        "吴婧😘",
        "金雯杰",
        "吴杨",
        "林珑鹏",
        "岑仕俊",
        "廉爽",
        "陈圆圆",
        "毛燕隆",
        "陈佳君",
        "吉佳慧",
        "郑蕾",
        "张琪",
        "唐安苗",
        "缪文杰",
        "李安",
        "宋诗卉",
        "李春燕",
        "季妍",
        "孙如意",
        "谢瞳🔥",
        "郑哲",
        "严仕麟",
        "李昕蔚",
        "汤胜男",
        "王堃",
        "周怡汶",
        "史凯",
        "Angela Li",
        "冯莹洁",
        "祝佳奇",
        "马昕宇",
        "张菁",
        "曹旭洋",
        "董怡然",
        "林锦鸿",
        "白帆",
        "朱艺佳",
        "严政",
        "施洲轶",
        "高守良",
        "陈崔亮🔺",
        "卢珊珊",
        "邵蕾",
        "王嘉伟",
        "喻策",
        "黄亚军",
        "鲍栋",
        "张硕",
        "幸遥路",
        "张丙卫",
        "李昊宸",
        "程思聪",
        "姜义杰",
        "陈子涵",
        "王晨",
        "郑伊煊🔥",
        "潘清",
        "刘声彬",
        "胡欣",
        "王昆元",
        "潘燕军",
        "高笑",
        "龚伟业",
        "杨梅",
        "曹正",
        "陈琦",
        "姚兴虎",
        "杨爱玲",
        "张欣华",
        "陈嘉铭",
        "黄兆天😘",
        "孙浩",
        "许潇杨",
        "朱冬玥",
        "朱哲宇",
        "赵昌宇",
        "王赟",
        "朱铭",
        "徐溯",
        "方笑",
        "郭宝",
        "倪梓",
        "赵传松🔥",
        "葛明宇",
        "毛彩连",
        "薛毅",
        "张孜正",
        "吴浩",
        "石煜冬",
        "王子恺",
        "张红",
        "喻杰",
        "郭牧辉",
        "王晨",
        "蒋致远",
        "陈益盈",
        "刘世达",
        "贾晓旭",
        "陈思辰",
        "韩弘毅",
        "洪美婧",
        "陈茜",
        "周国文"
    };

    private List<string> m_ShuffledList = new List<string>();
    private int m_CurrentIndex = 0;
    private bool m_IsShuffled = false;
    private string m_CurrentName = "";

    [MenuItem("Tools/抽奖")]
    public static void ShowWindow()
    {
        GetWindow<LotteryEditor>("抽奖工具");
    }

    void OnEnable()
    {
        InitNameList();
    }

    void InitNameList()
    {
        m_CurrentIndex = 0;
        m_IsShuffled = false;
        m_CurrentName = "";
        m_ShuffledList.Clear();
    }

    List<string> SampleUsersWithSeed(List<string> users, int k, int seed)
    {
        Random.InitState(seed);

        List<string> tempList = new List<string>(users);
        List<string> result = new List<string>();

        int sampleCount = Mathf.Min(k, tempList.Count);

        for (int i = 0; i < sampleCount; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        return result;
    }

    [MenuItem("Tools/测试抽奖")]
    public static void DebugTestLottery()
    {
        List<string> testList = new List<string>();
        for (int i = 1; i <= 11; i++)
        {
            testList.Add(i.ToString());
        }

        LotteryEditor editor = new LotteryEditor();
        List<string> result = editor.SampleUsersWithSeed(testList, 10, 1234);

        Debug.Log("========== 测试抽奖结果 ==========");
        Debug.Log($"原始列表: {string.Join(", ", testList)}");
        Debug.Log($"抽取数量: 10, 种子: 1234");
        Debug.Log($"抽取结果: {string.Join(", ", result)}");
        Debug.Log("============================");
    }

    void OnGUI()
    {
        GUILayout.Space(20);

        GUILayout.Label($"参与人数: {m_NameList.Count}", EditorStyles.boldLabel);
        GUILayout.Space(10);

        int columnSize = 20;
        int columnCount = (m_NameList.Count + columnSize - 1) / columnSize;

        GUILayout.BeginHorizontal();
        for (int col = 0; col < columnCount; col++)
        {
            GUILayout.BeginVertical();
            int startIndex = col * columnSize;
            int endIndex = Mathf.Min(startIndex + columnSize, m_NameList.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                GUILayout.Label($"{i + 1}. {m_NameList[i]}");
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("开始抽奖", GUILayout.Height(50)))
        {
            m_ShuffledList = SampleUsersWithSeed(m_NameList, 10, 8976243);
            m_IsShuffled = true;
        }

        if (m_IsShuffled)
        {
            GUILayout.Space(20);
            GUILayout.Label("========== 中奖名单 ==========", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.normal.textColor = Color.green;

            for (int i = 0; i < m_ShuffledList.Count; i++)
            {
                GUILayout.Label($"{i + 1}. {m_ShuffledList[i]}", style);
            }

            GUILayout.Space(10);
            GUILayout.Label("============================", EditorStyles.boldLabel);
        }
    }
}

