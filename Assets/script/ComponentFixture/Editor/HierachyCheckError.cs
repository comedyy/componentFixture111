

using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class HierachyCheckError 
{
    static HierachyCheckError()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItem;
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItem;
    }

    private static void OnHierarchyItem(int instanceID, Rect selectionRect)
    {
        if (EditorApplication.isUpdating)
        {
            return;
        }

        GameObject instance = EditorUtility.InstanceIDToObject (instanceID) as GameObject;
        
        if (instance == null)
        {
            return;
        }

        // #if UNITY_2019_1_OR_NEWER
        //     selectionRect.height = 16f;
        // #endif

        var x = instance.GetComponent<ICheckError>();
        if(x != null && x.HasError())
        {
            var xMax = selectionRect.xMax - 16;
            var rectNew = new Rect(xMax, selectionRect.y, 16, selectionRect.height - 2);
            EditorGUI.DrawRect (rectNew, Color.red);
        }
    }
}