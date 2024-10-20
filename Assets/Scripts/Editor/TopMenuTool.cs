using UnityEngine;
using UnityEditor;
using GameDataEditor;

public class TopMenuTool : MonoBehaviour
{
    [MenuItem("QuickCommand/Import Cards Data")]
    static void ImportSheetData()
    {
        GDEExcelManager.DoImport();

        GDEItemManager.Load();

        GDECodeGen.GenStaticSchemaKeysClass(GDEItemManager.AllSchemas);
        GDECodeGen.GenStaticKeysClass(GDEItemManager.AllSchemas);
        GDECodeGen.GenClasses(GDEItemManager.AllSchemas);

        AssetDatabase.Refresh();
    }
}
