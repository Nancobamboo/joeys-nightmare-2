using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class CardDraw : PureSingleton<CardDraw>
{




    public List<List<string>> DrawCardEnv(int level)
    {
        return new List<List<string>>
        {
            new List<string> { "1001", "1002" },
            new List<string> { "2001", "2002" },
            new List<string> { "3001", "3002" },
            new List<string> { "4001", "4002" },
            new List<string> { "5001", "5002" }
        };
    }

}