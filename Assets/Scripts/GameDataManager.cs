using System;
using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;

public class GameDataManager : Singleton<GameDataManager>
{
    private const string currentTable = "gde_data";

    private Dictionary<string, GDEFlashCardsData> flashCardsTableData = new();
    public GDEFlashCardsData GetFlashCardsDat(string key)
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

        foreach (var one in GDEDataManager.GetAllItems<GDEFlashCardsData>())
        {
            flashCardsTableData.Add(one.Key, one);
        }

    }

    public void ClearGameData()
    {

    }

}