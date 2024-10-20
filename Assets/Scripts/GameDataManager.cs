using System;
using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

public class GameDataManager : Singleton<GameDataManager>
{
    private const string currentTable = "gde_data";

    List<string> allCardKeys = new List<string>();

    private Dictionary<string, GDEFlashCardData> flashCardsTableData = new();
    public GDEFlashCardData GetFlashCardsData(string key)
    {
        if (key != null && flashCardsTableData.ContainsKey(key))
        {
            return flashCardsTableData[key];
        }
        else
        {
            Debug.LogWarning("Empty key: " + key);

            return null;
        }
    }

    private void Awake()
    {
        InitGDEData();
    }

    private void OnDestroy()
    {
        flashCardsTableData.Clear();
    }

    public void InitGDEData()
    {
        if (!GDEDataManager.Init(currentTable))
        {
            return;
        }

        foreach (var one in GDEDataManager.GetAllItems<GDEFlashCardData>())
        {
            flashCardsTableData.Add(one.Key, one);

            allCardKeys.Add(one.Key);
        }

    }

    public string GetRandomQuestionKey()
    {
        return allCardKeys.Random();
    }

}