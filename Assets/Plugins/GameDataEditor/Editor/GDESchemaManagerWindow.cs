using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDataEditor;

public class GDESchemaManagerWindow : GDEManagerWindowBase {

    string newSchemaName = string.Empty;
    Dictionary<string, int> basicFieldTypeSelectedDict = new Dictionary<string, int>();
    Dictionary<string, int> customSchemaTypeSelectedDict = new Dictionary<string, int>();
    Dictionary<string, int> customEnumTypeSelectedDict = new Dictionary<string, int>();

    Dictionary<string, string> newBasicFieldName = new Dictionary<string, string>();
    HashSet<string> isBasicList = new HashSet<string>();
    HashSet<string> isBasic2DList = new HashSet<string>();

    Dictionary<string, string> newCustomFieldName = new Dictionary<string, string>();
    HashSet<string> isCustomList = new HashSet<string>();
    HashSet<string> isCustom2DList = new HashSet<string>();

    Dictionary<string, string> newCustomEnumFieldName = new Dictionary<string, string>();
    HashSet<string> isCustomEnumList = new HashSet<string>();
    HashSet<string> isCustomEnum2DList = new HashSet<string>();

    List<string> deletedFields = new List<string>();
    Dictionary<List<string>, Dictionary<string, object>> renamedFields = new Dictionary<List<string>, Dictionary<string, object>>();

    Dictionary<string, string> renamedSchemas = new Dictionary<string, string>();

    #region OnGUI/Header Methods
    protected override void OnGUI()
    {
        mainHeaderText = GDEConstants.DefineDataHeader;

        // Set the header color
        headerColor = GDESettings.Instance.DefineDataColor.ToColor();
        headerColor.a = 1f;

        if (shouldRebuildEntriesList || entriesToDraw == null || GDEItemManager.ShouldReloadSchemas)
        {
            entriesToDraw = GetEntriesToDraw(GDEItemManager.AllSchemas);

            shouldRebuildEntriesList = false;
            shouldRecalculateHeights = true;
            GDEItemManager.ShouldReloadSchemas = false;
        }

        base.OnGUI();

        DrawExpandCollapseAllFoldout(GDEItemManager.AllSchemas.Keys.ToArray(), GDEConstants.SchemaListHeader);

        float currentGroupHeightTotal = CalculateGroupHeightsTotal();
        scrollViewHeight = drawHelper.HeightToBottomOfWindow();
        scrollViewY = drawHelper.TopOfLine();
        verticalScrollbarPosition = GUI.BeginScrollView(new Rect(drawHelper.CurrentLinePosition, scrollViewY, drawHelper.FullWindowWidth(), scrollViewHeight),
                                    verticalScrollbarPosition,
                                    new Rect(drawHelper.CurrentLinePosition, scrollViewY, drawHelper.ScrollViewWidth(), currentGroupHeightTotal));

        foreach (KeyValuePair<string, Dictionary<string, object>> schema in entriesToDraw)
        {
            float currentGroupHeight;
            if (!groupHeights.TryGetValue(schema.Key, out currentGroupHeight))
                currentGroupHeight = GDEConstants.LineHeight;

            if (drawHelper.IsVisible(verticalScrollbarPosition, scrollViewHeight, scrollViewY, currentGroupHeight) <= 0)
                DrawEntry(schema.Key, schema.Value);
            else
            {
                drawHelper.NewLine(currentGroupHeight / GDEConstants.LineHeight);
            }
        }
        GUI.EndScrollView();

        // Remove any schemas that were deleted
        foreach (string deletedSchemaKey in deleteEntries)
            Remove(deletedSchemaKey);
        deleteEntries.Clear();

        // Rename any schemas that were renamed
        string error;
        foreach (KeyValuePair<string, string> pair in renamedSchemas)
        {
            if (!GDEItemManager.RenameSchema(pair.Key, pair.Value, out error))
                EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, string.Format(GDEConstants.CouldNotRenameFormat, pair.Key, pair.Value, error), GDEConstants.OkLbl);
        }
        renamedSchemas.Clear();

        // Clone any schemas
        foreach (string schemaKey in cloneEntries)
            Clone(schemaKey);
        cloneEntries.Clear();
    }
    #endregion

    #region Draw Methods
    protected override void DrawCreateSection()
    {
        float topOfSection = drawHelper.TopOfLine() + 4f;
        float bottomOfSection = 0;
        float leftBoundary = 0;

        drawHelper.DrawSubHeader(GDEConstants.CreateNewSchemaHeader, headerColor, GDEConstants.SizeCreateSubHeaderKey);

        size.x = 100;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.SchemaNameLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        size.x = 120;
        newSchemaName = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newSchemaName);
        drawHelper.CurrentLinePosition += (size.x + 2);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        content.text = GDEConstants.CreateNewSchemaBtn;
        drawHelper.TryGetCachedSize(GDEConstants.SizeCreateNewSchemaBtnKey, content, GUI.skin.button, out size);
        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
        {
            if (Create(newSchemaName))
            {
                newSchemaName = string.Empty;
                GUI.FocusControl(string.Empty);
            }
        }
        drawHelper.CurrentLinePosition += (size.x + 6f);
        if (drawHelper.CurrentLinePosition > leftBoundary)
            leftBoundary = drawHelper.CurrentLinePosition;

        drawHelper.NewLine();

        bottomOfSection = drawHelper.TopOfLine();

        drawHelper.DrawSectionSeparator();

        leftBoundary += 5f;
    }

    void DrawAddFieldSection(string schemaKey, Dictionary<string, object> schemaData)
    {
        drawHelper.CurrentLinePosition += GDEConstants.Indent;

        content.text = GDEConstants.NewFieldHeader;
        drawHelper.TryGetCachedSize(GDEConstants.SizeNewFieldHeaderKey, content, labelStyle, out size);
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);

        drawHelper.NewLine();

        drawHelper.CurrentLinePosition += GDEConstants.Indent;

        // ***** Basic Field Type Group ***** //
        size.x = 120;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.BasicFieldTypeLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        // Basic field type selected
        int basicFieldTypeIndex;
        if (!basicFieldTypeSelectedDict.TryGetValue(schemaKey, out basicFieldTypeIndex))
        {
            basicFieldTypeIndex = 0;
            basicFieldTypeSelectedDict.TryAddValue(schemaKey, basicFieldTypeIndex);
        }

        size.x = 80;
        int newBasicFieldTypeIndex = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), basicFieldTypeIndex, GDEItemManager.BasicFieldTypeStringArray);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (newBasicFieldTypeIndex != basicFieldTypeIndex && GDEItemManager.BasicFieldTypeStringArray.IsValidIndex(newBasicFieldTypeIndex))
        {
            basicFieldTypeIndex = newBasicFieldTypeIndex;
            basicFieldTypeSelectedDict.TryAddOrUpdateValue(schemaKey, basicFieldTypeIndex);
        }

        // Basic field type name field
        string newBasicFieldNameText = string.Empty;
        if (!newBasicFieldName.TryGetValue(schemaKey, out newBasicFieldNameText))
        {
            newBasicFieldName.Add(schemaKey, string.Empty);
            newBasicFieldNameText = string.Empty;
        }

        size.x = 70;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.FieldNameLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 120;
        newBasicFieldNameText = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newBasicFieldNameText);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (!newBasicFieldNameText.Equals(newBasicFieldName[schemaKey]))
            newBasicFieldName[schemaKey] = newBasicFieldNameText;


        // Basic field type isList checkbox
        bool isBasicListTemp = isBasicList.Contains(schemaKey);
        content.text = GDEConstants.IsListLbl;
        drawHelper.TryGetCachedSize(GDEConstants.SizeIsListLblKey, content, EditorStyles.label, out size);
        EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isBasicListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isBasicListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isBasicListTemp && !isBasicList.Contains(schemaKey))
        {
            isBasicList.Add(schemaKey);

            // Turn 2D List off
            isBasic2DList.Remove(schemaKey);
        }
        else if (!isBasicListTemp && isBasicList.Contains(schemaKey))
            isBasicList.Remove(schemaKey);


        // Basic field type is2DList checkbox
        bool isBasic2DListTemp = isBasic2DList.Contains(schemaKey);
        content.text = GDEConstants.Is2DListLbl;
        drawHelper.TryGetCachedSize(GDEConstants.SizeIs2DListLblKey, content, EditorStyles.label, out size);
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), content);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isBasic2DListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isBasic2DListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isBasic2DListTemp && !isBasic2DList.Contains(schemaKey))
        {
            isBasic2DList.Add(schemaKey);

            // Turn off 1D List
            isBasicList.Remove(schemaKey);
        }
        else if (!isBasic2DListTemp && isBasic2DList.Contains(schemaKey))
            isBasic2DList.Remove(schemaKey);


        content.text = GDEConstants.AddFieldBtn;
        drawHelper.TryGetCachedSize(GDEConstants.SizeAddFieldBtnKey, content, GUI.skin.button, out size);
        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
        {
            if (AddBasicField(GDEItemManager.BasicFieldTypes[basicFieldTypeIndex], schemaKey, schemaData, newBasicFieldNameText, isBasicListTemp, isBasic2DListTemp))
            {
                isBasicList.Remove(schemaKey);
                newBasicFieldName.TryAddOrUpdateValue(schemaKey, string.Empty);

                newBasicFieldNameText = string.Empty;
                GUI.FocusControl(string.Empty);
            }
        }
        drawHelper.NewLine();


        // ****** Custom Field Type Group ****** //
        drawHelper.CurrentLinePosition += GDEConstants.Indent;

        size.x = 120;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.CustomFieldTypeLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        List<string> customTypeList = GDEItemManager.AllSchemas.Keys.ToList();
        customTypeList.Remove(schemaKey);

        string[] customTypes = customTypeList.ToArray();

        int customSchemaTypeIndex;
        if (!customSchemaTypeSelectedDict.TryGetValue(schemaKey, out customSchemaTypeIndex))
        {
            customSchemaTypeIndex = 0;
            customSchemaTypeSelectedDict.TryAddValue(schemaKey, customSchemaTypeIndex);
        }

        // Custom schema type selected
        size.x = 80;
        int newCustomSchemaTypeSelected = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), customSchemaTypeIndex, customTypes);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (newCustomSchemaTypeSelected != customSchemaTypeIndex && customTypes.IsValidIndex(newCustomSchemaTypeSelected))
        {
            customSchemaTypeIndex = newCustomSchemaTypeSelected;
            customSchemaTypeSelectedDict.TryAddOrUpdateValue(schemaKey, customSchemaTypeIndex);
        }


        // Custom field type name field
        string newCustomFieldNameText = string.Empty;
        if (!newCustomFieldName.TryGetValue(schemaKey, out newCustomFieldNameText))
        {
            newCustomFieldName.Add(schemaKey, string.Empty);
            newCustomFieldNameText = string.Empty;
        }

        size.x = 70;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.FieldNameLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 120;
        newCustomFieldNameText = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newCustomFieldNameText);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (!newCustomFieldNameText.Equals(newCustomFieldName[schemaKey]))
            newCustomFieldName[schemaKey] = newCustomFieldNameText;


        // Custom field type isList checkbox
        bool isCustomListTemp = isCustomList.Contains(schemaKey);

        size.x = 38;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.IsListLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isCustomListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isCustomListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isCustomListTemp && !isCustomList.Contains(schemaKey))
        {
            isCustomList.Add(schemaKey);

            // Turn off 2D List
            isCustom2DList.Remove(schemaKey);
        }
        else if (!isCustomListTemp && isCustomList.Contains(schemaKey))
            isCustomList.Remove(schemaKey);


        // Custom field type is2DList checkbox
        bool isCustom2DListTemp = isCustom2DList.Contains(schemaKey);
        content.text = GDEConstants.Is2DListLbl;
        drawHelper.TryGetCachedSize(GDEConstants.SizeIs2DListLblKey, content, EditorStyles.label, out size);
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isCustom2DListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isCustom2DListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isCustom2DListTemp && !isCustom2DList.Contains(schemaKey))
        {
            isCustom2DList.Add(schemaKey);

            // Turn off 1D List
            isCustomList.Remove(schemaKey);
        }
        else if (!isCustom2DListTemp && isCustom2DList.Contains(schemaKey))
            isCustom2DList.Remove(schemaKey);


        content.text = GDEConstants.AddCustomFieldBtn;
        drawHelper.TryGetCachedSize(GDEConstants.SizeAddCustomFieldBtnKey, content, GUI.skin.button, out size);
        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
        {
            if (!customTypes.IsValidIndex(customSchemaTypeIndex) || customTypes.Length.Equals(0))
            {
                EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, GDEConstants.InvalidCustomFieldType, GDEConstants.OkLbl);
            }
            else if (AddCustomField(customTypes[customSchemaTypeIndex], schemaKey, schemaData, newCustomFieldNameText, isCustomListTemp, isCustom2DListTemp))
            {
                isCustomList.Remove(schemaKey);
                newCustomFieldName.TryAddOrUpdateValue(schemaKey, string.Empty);
                newCustomFieldNameText = string.Empty;
                GUI.FocusControl(string.Empty);
            }
        }
        drawHelper.NewLine();


        // ****** Custom Enum Group ****** //
        drawHelper.CurrentLinePosition += GDEConstants.Indent;

        size.x = 120;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.CustomEnumFieldTypeLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        int customEnumTypeIndex;
        if (!customEnumTypeSelectedDict.TryGetValue(schemaKey, out customEnumTypeIndex))
        {
            customEnumTypeIndex = 0;
            customEnumTypeSelectedDict.TryAddValue(schemaKey, customEnumTypeIndex);
        }


        // Custom Enum dropdown
        size.x = 80;
        int newCustomEnumTypeSelected = EditorGUI.Popup(new Rect(drawHelper.CurrentLinePosition, drawHelper.PopupTop(), size.x, drawHelper.StandardHeight()), customEnumTypeIndex, GDEItemManager.EnumTypes);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (newCustomEnumTypeSelected != customEnumTypeIndex && GDEItemManager.EnumTypes.IsValidIndex(newCustomEnumTypeSelected))
        {
            customEnumTypeIndex = newCustomEnumTypeSelected;
            customEnumTypeSelectedDict.TryAddOrUpdateValue(schemaKey, customEnumTypeIndex);
        }


        // Custom Enum type name field
        string newCustomEnumFieldNameText = string.Empty;
        if (!newCustomEnumFieldName.TryGetValue(schemaKey, out newCustomEnumFieldNameText))
        {
            newCustomEnumFieldName.Add(schemaKey, string.Empty);
            newCustomEnumFieldNameText = string.Empty;
        }

        size.x = 70;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.FieldNameLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 120;
        newCustomEnumFieldNameText = EditorGUI.TextField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newCustomEnumFieldNameText);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (!newCustomEnumFieldNameText.Equals(newCustomEnumFieldName[schemaKey]))
            newCustomEnumFieldName[schemaKey] = newCustomEnumFieldNameText;


        // Custom enum field type isList checkbox
        bool isCustomEnumListTemp = isCustomEnumList.Contains(schemaKey);

        size.x = 38;
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), GDEConstants.IsListLbl);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isCustomEnumListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isCustomEnumListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isCustomEnumListTemp && !isCustomEnumList.Contains(schemaKey))
        {
            isCustomEnumList.Add(schemaKey);

            // Turn off 2D List
            isCustomEnum2DList.Remove(schemaKey);
        }
        else if (!isCustomEnumListTemp && isCustomEnumList.Contains(schemaKey))
            isCustomEnumList.Remove(schemaKey);


        // Custom enum field type is2DList checkbox
        bool isCustomEnum2DListTemp = isCustomEnum2DList.Contains(schemaKey);
        content.text = GDEConstants.Is2DListLbl;
        drawHelper.TryGetCachedSize(GDEConstants.SizeIs2DListLblKey, content, EditorStyles.label, out size);
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
        drawHelper.CurrentLinePosition += (size.x + 2);

        size.x = 15;
        isCustomEnum2DListTemp = EditorGUI.Toggle(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), isCustomEnum2DListTemp);
        drawHelper.CurrentLinePosition += (size.x + 6);

        if (isCustomEnum2DListTemp && !isCustomEnum2DList.Contains(schemaKey))
        {
            isCustomEnum2DList.Add(schemaKey);

            // Turn off 1D List
            isCustomEnumList.Remove(schemaKey);
        }
        else if (!isCustomEnum2DListTemp && isCustomEnum2DList.Contains(schemaKey))
            isCustomEnum2DList.Remove(schemaKey);


        // Add Custom Enum button
        content.text = GDEConstants.AddCustomEnumFieldBtn;
        drawHelper.TryGetCachedSize(GDEConstants.SizeAddCustomEnumFieldBtn, content, GUI.skin.button, out size);
        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
        {
            if (!GDEItemManager.EnumTypes.IsValidIndex(customEnumTypeIndex) || GDEItemManager.EnumTypes.Length.Equals(0))
            {
                EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, GDEConstants.InvalidCustomFieldType, GDEConstants.OkLbl);
            }
            else if (AddCustomField(GDEItemManager.EnumTypes[customEnumTypeIndex], schemaKey, schemaData, newCustomEnumFieldNameText, isCustomEnumListTemp, isCustomEnum2DListTemp))
            {
                isCustomEnumList.Remove(schemaKey);
                newCustomEnumFieldName.TryAddOrUpdateValue(schemaKey, string.Empty);
                newCustomEnumFieldNameText = string.Empty;
                GUI.FocusControl(string.Empty);
            }
        }
    }

    void DrawEntry(string schemaKey, Dictionary<string, object> schemaData)
    {
        float beginningHeight = drawHelper.CurrentHeight();

        // Start drawing below
        bool isOpen = DrawFoldout(GDEConstants.SchemaLbl + " ", schemaKey, schemaKey, schemaKey, RenameSchema);
        drawHelper.NewLine();

        if (isOpen)
        {
            bool shouldDrawSpace = false;
            bool didDrawSpaceForSection = false;
            bool isFirstSection = true;
            int listDimension = 0;

            // Draw the basic types
            foreach (BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
            {
                List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.EnumToString(), 0);
                foreach (string fieldKey in fieldKeys)
                {
                    drawHelper.CurrentLinePosition += GDEConstants.Indent;
                    DrawSingleField(schemaKey, fieldKey, schemaData);
                    shouldDrawSpace = true;
                    isFirstSection = false;
                }
            }

            // Draw the custom types
            foreach (string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, 0))
            {
                if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
                {
                    drawHelper.NewLine(0.5f);
                }

                drawHelper.CurrentLinePosition += GDEConstants.Indent;
                DrawSingleField(schemaKey, fieldKey, schemaData);

                shouldDrawSpace = true;
                isFirstSection = false;
                didDrawSpaceForSection = true;
            }
            didDrawSpaceForSection = false;

            // Draw the lists
            for (int dimension = 1;  dimension <= 2;  dimension++)
            {
                foreach (BasicFieldType fieldType in GDEItemManager.BasicFieldTypes)
                {
                    List<string> fieldKeys = GDEItemManager.SchemaFieldKeysOfType(schemaKey, fieldType.EnumToString(), dimension);
                    foreach (string fieldKey in fieldKeys)
                    {
                        string isListKey = fieldKey.ListKey();
                        schemaData.TryGetInt(isListKey, out listDimension);

                        if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
                        {
                            drawHelper.NewLine(0.5f);
                        }

                        drawHelper.CurrentLinePosition += GDEConstants.Indent;
                        if (listDimension == 1)
                            DrawListField(schemaKey, schemaData, fieldKey);
                        else
                            Draw2DListField(schemaKey, schemaData, fieldKey);

                        shouldDrawSpace = true;
                        didDrawSpaceForSection = true;
                        isFirstSection = false;
                    }
                }
                didDrawSpaceForSection = false;
            }

            // Draw the custom lists
            for (int dimension = 1;  dimension <= 2;  dimension++)
            {
                foreach (string fieldKey in GDEItemManager.SchemaCustomFieldKeys(schemaKey, dimension))
                {
                    if (shouldDrawSpace && !didDrawSpaceForSection && !isFirstSection)
                    {
                        drawHelper.NewLine(0.5f);
                    }

                    drawHelper.CurrentLinePosition += GDEConstants.Indent;
                    if (dimension == 1)
                        DrawListField(schemaKey, schemaData, fieldKey);
                    else
                        Draw2DListField(schemaKey, schemaData, fieldKey);

                    shouldDrawSpace = true;
                    didDrawSpaceForSection = true;
                    isFirstSection = false;
                }
            }
            didDrawSpaceForSection = false;

            drawHelper.NewLine();

            DrawAddFieldSection(schemaKey, schemaData);

            drawHelper.NewLine(2f);

            DrawEntryFooter(GDEConstants.CloneSchema, GDEConstants.SizeCloneSchemaKey, schemaKey);

            // Remove any fields that were deleted above
            foreach (string deletedKey in deletedFields)
                RemoveField(schemaKey, schemaData, deletedKey);
            deletedFields.Clear();

            // Rename any fields that were renamed
            string error;
            string oldFieldKey;
            string newFieldKey;
            foreach (KeyValuePair<List<string>, Dictionary<string, object>> pair in renamedFields)
            {
                oldFieldKey = pair.Key[0];
                newFieldKey = pair.Key[1];
                EditorStringExtensions.ClearCaches();
                if (!GDEItemManager.RenameSchemaField(oldFieldKey, newFieldKey, schemaKey, pair.Value, out error))
                    EditorUtility.DisplayDialog(GDEConstants.ErrorLbl, string.Format(GDEConstants.CouldNotRenameFormat, oldFieldKey, newFieldKey, error), GDEConstants.OkLbl);
            }
            renamedFields.Clear();
        }

        float newGroupHeight = drawHelper.CurrentHeight() - beginningHeight;
        float currentGroupHeight;

        groupHeights.TryGetValue(schemaKey, out currentGroupHeight);

        if (!newGroupHeight.NearlyEqual(currentGroupHeight))
        {
            currentGroupHeightTotal -= currentGroupHeight;
            currentGroupHeightTotal += newGroupHeight;
        }

        SetGroupHeight(schemaKey, newGroupHeight);
    }

    void DrawSingleField(string schemaKey, string fieldKey, Dictionary<string, object> schemaData)
    {
        string fieldPreviewKey = schemaKey.PreviewKey(fieldKey, string.Empty);
        string fieldType;
        schemaData.TryGetString(fieldKey.TypeKey(), out fieldType);

        BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
        if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
        {
            fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
            fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
        }

        content.text = fieldType;
        drawHelper.TryGetCachedSize(schemaKey + fieldKey + GDEConstants.TypeSuffix, content, labelStyle, out size);

        size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
        GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content, labelStyle);
        drawHelper.CurrentLinePosition += (size.x + 2);

        string editFieldKey = schemaKey.EditFieldKey(fieldKey);
        DrawEditableLabel(fieldKey, editFieldKey, RenameSchemaField, schemaData);

        switch (fieldTypeEnum)
        {
        case BasicFieldType.Bool:
            DrawBool(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.Int:
            DrawInt(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.Float:
            DrawFloat(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.String:
            DrawString(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.Vector2:
            DrawVector2(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.Vector3:
            DrawVector3(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.Vector4:
            DrawVector4(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.Color:
            DrawColor(fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.GameObject:
            DrawObject<GameObject>(fieldPreviewKey, fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.Texture2D:
            DrawObject<Texture2D>(fieldPreviewKey, fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.Material:
            DrawObject<Material>(fieldPreviewKey, fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;
        case BasicFieldType.VideoClip:
            DrawObject<VideoClip>(fieldPreviewKey, fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        case BasicFieldType.AudioClip:
            DrawAudio(fieldPreviewKey, fieldKey, schemaData, GDEConstants.DefaultValueLbl);
            break;

        default:
            DrawCustom(fieldKey, schemaData, false);
            break;
        }

        content.text = GDEConstants.DeleteBtn;
        drawHelper.TryGetCachedSize(GDEConstants.SizeDeleteBtnKey, content, GUI.skin.button, out size);
        if (fieldTypeEnum.Equals(BasicFieldType.Vector2) ||
                fieldTypeEnum.Equals(BasicFieldType.Vector3) ||
                fieldTypeEnum.Equals(BasicFieldType.Vector4))
        {
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.VerticalMiddleOfLine(), size.x, size.y), content))
                deletedFields.Add(fieldKey);

            drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
        }
        else
        {
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                deletedFields.Add(fieldKey);

            drawHelper.NewLine();
        }
    }

    void DrawListField(string schemaKey, Dictionary<string, object> schemaData, string fieldKey)
    {
        try
        {
            string foldoutKey = schemaKey.FoldoutKey(fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);

            string fieldType;
            schemaData.TryGetString(fieldKey.TypeKey(), out fieldType);

            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            {
                fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
                fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
            }

            content.text = fieldType.ListDeclaration();
            drawHelper.TryGetCachedSize(schemaKey + fieldKey + GDEConstants.TypeSuffix, content, EditorStyles.foldout, out size);
            size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
            newFoldoutState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentFoldoutState, content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            DrawEditableLabel(fieldKey, schemaKey.EditFieldKey(fieldKey), RenameSchemaField, schemaData);

            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }

            object temp = null;
            IList list = null;

            if (schemaData.TryGetValue(fieldKey, out temp))
                list = temp as IList;

            content.text = GDEConstants.DefaultSizeLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeDefaultSizeLblKey, content, EditorStyles.label, out size);
            GUI.Label(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            int newListCount;
            string listCountKey = schemaKey.ListCountKey(fieldKey);
            if (newListCountDict.ContainsKey(listCountKey))
            {
                newListCount = newListCountDict[listCountKey];
            }
            else
            {
                newListCount = list.Count;
                newListCountDict.Add(listCountKey, newListCount);
            }

            size.x = 40;
            newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
            drawHelper.CurrentLinePosition += (size.x + 2);

            newListCountDict[listCountKey] = newListCount;

            content.text = GDEConstants.ResizeBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
            if (newListCount != list.Count)
            {
                if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                {
                    object defaultResizeValue = GDEItemManager.GetDefaultValueForType(fieldTypeEnum);
                    if (ResizeList(list, newListCount, defaultResizeValue))
                        SetNeedToSave(true);
                }
                drawHelper.CurrentLinePosition += (size.x + 2);
            }

            content.text = GDEConstants.DeleteBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeDeleteBtnKey, content, GUI.skin.button, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                deletedFields.Add(fieldKey);

            drawHelper.NewLine();

            if (newFoldoutState)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    drawHelper.CurrentLinePosition += GDEConstants.Indent * 2;
                    content.text = i.OneDLbl();

                    switch (fieldTypeEnum) {
                    case BasicFieldType.Bool:
                    {
                        DrawListBool(content, i, Convert.ToBoolean((object)list[i]), list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.Int:
                    {
                        DrawListInt(content, i, Convert.ToInt32((object)list[i]), list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.Float:
                    {
                        DrawListFloat(content, i, Convert.ToSingle((object)list[i]), list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.String:
                    {
                        DrawListString(content, i, list[i] as string, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.Vector2:
                    {
                        DrawListVector2(content, i, list[i] as Dictionary<string, object>, list);
                        drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                        break;
                    }
                    case BasicFieldType.Vector3:
                    {
                        DrawListVector3(content, i, list[i] as Dictionary<string, object>, list);
                        drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                        break;
                    }
                    case BasicFieldType.Vector4:
                    {
                        DrawListVector4(content, i, list[i] as Dictionary<string, object>, list);
                        drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                        break;
                    }
                    case BasicFieldType.Color:
                    {
                        DrawListColor(content, i, list[i] as Dictionary<string, object>, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.GameObject:
                    {
                        DrawListObject<GameObject>(foldoutKey + i, content, i, list[i] as GameObject, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.Texture2D:
                    {
                        DrawListObject<Texture2D>(foldoutKey + i, content, i, list[i] as Texture2D, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.Material:
                    {
                        DrawListObject<Material>(foldoutKey + i, content, i, list[i] as Material, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.VideoClip:
                    {
                        DrawListObject<VideoClip>(foldoutKey + i, content, i, list[i] as VideoClip, list);
                        drawHelper.NewLine();
                        break;
                    }
                    case BasicFieldType.AudioClip:
                    {
                        DrawListAudio(foldoutKey + i, content, i, list[i] as AudioClip, list);
                        drawHelper.NewLine();
                        break;
                    }
                    default:
                    {
                        DrawListCustom(content, i, list[i] as string, list, false);
                        drawHelper.NewLine();
                        break;
                    }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    void Draw2DListField(string schemaKey, Dictionary<string, object> schemaData, string fieldKey)
    {
        try
        {
            string foldoutKey = schemaKey.FoldoutKey(fieldKey);
            bool newFoldoutState;
            bool currentFoldoutState = listFieldFoldoutState.Contains(foldoutKey);

            string fieldType;
            schemaData.TryGetString(fieldKey.TypeKey(), out fieldType);

            BasicFieldType fieldTypeEnum = BasicFieldType.Undefined;
            if (Enum.IsDefined(typeof(BasicFieldType), fieldType))
            {
                fieldTypeEnum = (BasicFieldType)Enum.Parse(typeof(BasicFieldType), fieldType);
                fieldType = GDEItemManager.GetVariableTypeFor(fieldTypeEnum);
            }

            content.text = fieldType.ListTwoDDeclaration();
            drawHelper.TryGetCachedSize(schemaKey + fieldKey + GDEConstants.TypeSuffix, content, EditorStyles.foldout, out size);
            size.x = Math.Max(size.x, GDEConstants.MinLabelWidth);
            newFoldoutState = EditorGUI.Foldout(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), currentFoldoutState, content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            DrawEditableLabel(fieldKey, schemaKey.EditFieldKey(fieldKey), RenameSchemaField, schemaData);

            if (newFoldoutState != currentFoldoutState)
            {
                if (newFoldoutState)
                    listFieldFoldoutState.Add(foldoutKey);
                else
                    listFieldFoldoutState.Remove(foldoutKey);
            }

            object temp = null;
            IList list = null;

            if (schemaData.TryGetValue(fieldKey, out temp))
                list = temp as IList;

            content.text = GDEConstants.DefaultSizeLbl;
            drawHelper.TryGetCachedSize(GDEConstants.SizeDefaultSizeLblKey, content, EditorStyles.label, out size);
            EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
            drawHelper.CurrentLinePosition += (size.x + 2);

            int newListCount;
            string listCountKey = schemaKey.ListCountKey(fieldKey);
            if (newListCountDict.ContainsKey(listCountKey))
            {
                newListCount = newListCountDict[listCountKey];
            }
            else
            {
                newListCount = list.Count;
                newListCountDict.Add(listCountKey, newListCount);
            }

            size.x = 40;
            newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
            drawHelper.CurrentLinePosition += (size.x + 2);

            newListCountDict[listCountKey] = newListCount;

            content.text = GDEConstants.ResizeBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
            if (newListCount != list.Count)
            {
                if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                {
                    object defaultResizeValue = null;
                    if (GDEItemManager.IsUnityType(fieldTypeEnum))
                        defaultResizeValue = Activator.CreateInstance(list.GetType().GetGenericArguments()[0]);
                    else
                        defaultResizeValue = new List<object>();

                    if (ResizeList(list, newListCount, defaultResizeValue))
                        SetNeedToSave(true);
                }
                drawHelper.CurrentLinePosition += (size.x + 2);
            }

            content.text = GDEConstants.DeleteBtn;
            drawHelper.TryGetCachedSize(GDEConstants.SizeDeleteBtnKey, content, GUI.skin.button, out size);
            if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                deletedFields.Add(fieldKey);

            drawHelper.NewLine();

            if (newFoldoutState)
            {
                for (int index = 0; index < list.Count; index++)
                {
                    IList subList = list[index] as IList;

                    drawHelper.CurrentLinePosition += GDEConstants.Indent * 2;
                    content.text = fieldType.TwoDSublistDeclaration(index);

                    bool isOpen = DrawFoldout(content.text, foldoutKey + "_" + index, string.Empty, string.Empty, null);

                    // Draw resize
                    content.text = GDEConstants.DefaultSizeLbl;
                    drawHelper.TryGetCachedSize(GDEConstants.SizeDefaultSizeLblKey, content, EditorStyles.label, out size);
                    EditorGUI.LabelField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content);
                    drawHelper.CurrentLinePosition += (size.x + 2);

                    listCountKey = schemaKey.ListCountKey(fieldKey) + "_" + index;
                    if (newListCountDict.ContainsKey(listCountKey))
                    {
                        newListCount = newListCountDict[listCountKey];
                    }
                    else
                    {
                        newListCount = subList.Count;
                        newListCountDict.Add(listCountKey, newListCount);
                    }

                    size.x = 40;
                    newListCount = EditorGUI.IntField(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, drawHelper.StandardHeight()), newListCount);
                    drawHelper.CurrentLinePosition += (size.x + 2);

                    newListCountDict[listCountKey] = newListCount;

                    content.text = GDEConstants.ResizeBtn;
                    drawHelper.TryGetCachedSize(GDEConstants.SizeResizeBtnKey, content, GUI.skin.button, out size);
                    if (newListCount != subList.Count)
                    {
                        if (GUI.Button(new Rect(drawHelper.CurrentLinePosition, drawHelper.TopOfLine(), size.x, size.y), content))
                        {
                            object defaultResizeValue = GDEItemManager.GetDefaultValueForType(fieldTypeEnum);
                            if (ResizeList(subList, newListCount, defaultResizeValue))
                                SetNeedToSave(true);
                        }
                        drawHelper.CurrentLinePosition += (size.x + 2);
                    }
                    drawHelper.NewLine();

                    if (isOpen)
                    {
                        for (int x = 0; x < subList.Count; x++)
                        {
                            drawHelper.CurrentLinePosition += GDEConstants.Indent * 3;
                            content.text = index.TwoDSublistLbl(x);

                            switch (fieldTypeEnum)
                            {
                            case BasicFieldType.Bool:
                            {
                                DrawListBool(content, x, Convert.ToBoolean((object)subList[x]), subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.Int:
                            {
                                DrawListInt(content, x, Convert.ToInt32((object)subList[x]), subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.Float:
                            {
                                DrawListFloat(content, x, Convert.ToSingle((object)subList[x]), subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.String:
                            {
                                DrawListString(content, x, subList[x] as string, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.Vector2:
                            {
                                DrawListVector2(content, x, subList[x] as Dictionary<string, object>, subList);
                                drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                                break;
                            }
                            case BasicFieldType.Vector3:
                            {
                                DrawListVector3(content, x, subList[x] as Dictionary<string, object>, subList);
                                drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                                break;
                            }
                            case BasicFieldType.Vector4:
                            {
                                DrawListVector4(content, x, subList[x] as Dictionary<string, object>, subList);
                                drawHelper.NewLine(GDEConstants.VectorFieldBuffer + 1);
                                break;
                            }
                            case BasicFieldType.Color:
                            {
                                DrawListColor(content, x, subList[x] as Dictionary<string, object>, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.GameObject:
                            {
                                DrawListObject<GameObject>(foldoutKey + index + "_" + x, content, x, subList[x] as GameObject, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.Texture2D:
                            {
                                DrawListObject<Texture2D>(foldoutKey + index + "_" + x, content, x, subList[x] as Texture2D, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.Material:
                            {
                                DrawListObject<Material>(foldoutKey + index + "_" + x, content, x, subList[x] as Material, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.VideoClip:
                            {
                                DrawListObject<VideoClip>(foldoutKey + index + "_" + x, content, x, subList[x] as VideoClip, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            case BasicFieldType.AudioClip:
                            {
                                DrawListAudio(foldoutKey + index + "_" + x, content, x, subList[x] as AudioClip, subList);
                                drawHelper.NewLine();
                                break;
                            }
                            default:
                            {
                                DrawListCustom(content, x, subList[x] as string, subList, false);
                                drawHelper.NewLine();
                                break;
                            }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    #endregion

    #region Filter Methods
    protected override bool ShouldFilter(string schemaKey, Dictionary<string, object> schemaData)
    {
        bool schemaKeyMatch = schemaKey.Lower().Contains(filterText.Lower());
        bool fieldKeyMatch = !GDEItemManager.ShouldFilterByField(schemaKey, filterText);

        // Return if the schema keys don't contain the filter text or
        // if the schema fields don't contain the filter text
        if (!schemaKeyMatch && !fieldKeyMatch)
            return true;

        return false;
    }

    protected override bool DrawFilterSection()
    {
        bool clearSearch = base.DrawFilterSection();

        size.x = 200;

        int totalItems = GDEItemManager.AllSchemas.Count;
        string itemText = totalItems != 1 ? GDEConstants.ItemsLbl : GDEConstants.ItemLbl;
        if (!string.IsNullOrEmpty(filterText))
        {
            float pos = drawHelper.TopOfLine() + drawHelper.LineHeight * .1f;
            string resultText = string.Format(GDEConstants.SearchResultFormat, NumberOfItemsBeingShown(), totalItems, itemText);
            GUI.Label(new Rect(drawHelper.CurrentLinePosition, pos, size.x, drawHelper.StandardHeight()), resultText);
            drawHelper.CurrentLinePosition += (size.x + 2);
        }

        drawHelper.NewLine(1.25f);

        return clearSearch;
    }
    #endregion

    #region Add/Remove Field Methods
    bool AddBasicField(BasicFieldType type, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList, bool is2DList)
    {
        bool result = true;
        object defaultValue = GDEItemManager.GetDefaultValueForType(type);
        string error;

        if (GDEItemManager.AddBasicFieldToSchema(type, schemaKey, schemaData, newFieldName, out error, isList, is2DList, defaultValue))
        {
            SetNeedToSave(true);
            HighlightNew(newFieldName);
        }
        else
        {
            EditorUtility.DisplayDialog(GDEConstants.ErrorCreatingField, error, GDEConstants.OkLbl);
            result = false;
        }

        return result;
    }

    bool AddCustomField(string customType, string schemaKey, Dictionary<string, object> schemaData, string newFieldName, bool isList, bool is2DList)
    {
        bool result = true;
        string error;

        if (GDEItemManager.AddCustomFieldToSchema(customType, schemaKey, schemaData, newFieldName, isList, is2DList, out error))
        {
            SetNeedToSave(true);
            HighlightNew(newFieldName);
        }
        else
        {
            EditorUtility.DisplayDialog(GDEConstants.ErrorCreatingField, error, GDEConstants.OkLbl);
            result = false;
        }

        return result;
    }

    void RemoveField(string schemaKey, Dictionary<string, object> schemaData, string deletedFieldKey)
    {
        newListCountDict.Remove(schemaKey.FoldoutKey(deletedFieldKey));
        GDEItemManager.RemoveFieldFromSchema(schemaKey, schemaData, deletedFieldKey);

        SetNeedToSave(true);
    }
    #endregion

    #region Load/Save Schema Methods
    protected override void Load()
    {
        base.Load();

        newSchemaName = string.Empty;
        basicFieldTypeSelectedDict.Clear();
        customSchemaTypeSelectedDict.Clear();
        newBasicFieldName.Clear();
        isBasicList.Clear();
        newCustomFieldName.Clear();
        isCustomList.Clear();
        deletedFields.Clear();
        renamedFields.Clear();
        renamedSchemas.Clear();
    }

    protected override bool NeedToSave()
    {
        return GDEItemManager.SchemasNeedSave;
    }

    protected override void SetNeedToSave(bool shouldSave)
    {
        GDEItemManager.SchemasNeedSave = shouldSave;
    }
    #endregion

    #region Create/Remove Schema Methods
    protected override bool Create(object data)
    {
        bool result = true;
        string key = data as string;
        string error;

        result = GDEItemManager.AddSchema(key, new Dictionary<string, object>(), out error);
        if (result)
        {
            SetNeedToSave(true);
            SetFoldout(true, key);

            HighlightNew(key);
        }
        else
        {
            EditorUtility.DisplayDialog(GDEConstants.ErrorCreatingSchema, error, GDEConstants.OkLbl);
            result = false;
        }

        return result;
    }

    protected override void Remove(string key)
    {
        // Show a warning if we have items using this schema
        List<string> items = GDEItemManager.GetItemsOfSchemaType(key);
        bool shouldDelete = true;

        if (items != null && items.Count > 0)
        {
            string itemWord = items.Count == 1 ? GDEConstants.ItemLbl : GDEConstants.ItemsLbl;
            shouldDelete = EditorUtility.DisplayDialog(string.Format(GDEConstants.DeleteWarningFormat, items.Count, itemWord), GDEConstants.SureDeleteSchema, GDEConstants.DeleteSchemaBtn, GDEConstants.CancelBtn);
        }

        if (shouldDelete)
        {
            GDEItemManager.RemoveSchema(key, true);
            SetNeedToSave(true);
        }
    }

    protected override bool Clone(string key)
    {
        bool result = true;
        string error;
        string newKey;

        result = GDEItemManager.CloneSchema(key, out newKey, out error);
        if (result)
        {
            SetNeedToSave(true);
            SetFoldout(true, newKey);

            HighlightNew(newKey);
        }
        else
        {
            EditorUtility.DisplayDialog(GDEConstants.ErrorCloningSchema, error, GDEConstants.OkLbl);
            result = false;
        }

        return result;
    }
    #endregion

    #region Helper Methods
    protected override float CalculateGroupHeightsTotal()
    {
        if (!shouldRecalculateHeights)
            return currentGroupHeightTotal;

        currentGroupHeightTotal = 0;
        float entryHeight = 0;

        foreach (var entry in entriesToDraw)
        {
            groupHeights.TryGetValue(entry.Key, out entryHeight);
            if (entryHeight < GDEConstants.LineHeight)
            {
                entryHeight = GDEConstants.LineHeight;
                SetGroupHeight(entry.Key, entryHeight);
            }

            currentGroupHeightTotal += entryHeight;
        }

        shouldRecalculateHeights = false;

        return currentGroupHeightTotal;
    }

    protected override string FilePath()
    {
        return GDEItemManager.DataFilePath;
    }
    #endregion

    #region Rename Methods
    protected bool RenameSchema(string oldSchemaKey, string newSchemaKey, Dictionary<string, object> data, out string error)
    {
        error = string.Empty;
        renamedSchemas.Add(oldSchemaKey, newSchemaKey);
        return true;
    }

    protected bool RenameSchemaField(string oldFieldKey, string newFieldKey, Dictionary<string, object> schemaData, out string error)
    {
        error = string.Empty;
        List<string> fieldKeys = new List<string>() {oldFieldKey, newFieldKey};
        renamedFields.Add(fieldKeys, schemaData);
        return true;
    }
    #endregion
}
