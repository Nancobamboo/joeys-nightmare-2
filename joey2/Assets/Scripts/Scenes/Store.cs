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




}