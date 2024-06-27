using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(ArrayContainerComponent), true)]
public class ArrayContainerComponentEditor : Editor
{
    SerializedProperty _componentType;
    SerializedProperty _recordArray;
    bool fold = false;

    private void OnEnable() {
        _componentType = serializedObject.FindProperty("componentType");
        _recordArray = serializedObject.FindProperty("components");

        AutoRefresh();
        serializedObject.ApplyModifiedProperties();
    }

    void AutoRefresh()
    {
        if(string.IsNullOrEmpty(_componentType.stringValue))
        {
            _componentType.stringValue = "UnityEngine.Transform, UnityEngine.CoreModule";
        }

        var type = Type.GetType(_componentType.stringValue);
        var go = (target as ArrayContainerComponent).gameObject;
        int childCount = go.transform.childCount;
        _recordArray.arraySize = childCount;

        for(int i = 0; i < childCount; i++)
        {
            var x = go.transform.GetChild(i).GetComponent(type);
            _recordArray.GetArrayElementAtIndex(i).objectReferenceValue = x;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        fold = EditorGUILayout.Foldout(fold, "修改类型");
        if(fold)
        {   
            var go = (target as ArrayContainerComponent).gameObject;
            if(go.transform.childCount != 0)
            {
                var components = go.transform.GetChild(0).GetComponents<Component>();
                foreach(var x in components)
                {
                    if(GUILayout.Button($"{x.GetType()}"))
                    {
                        _componentType.stringValue = ComponentFixture1Editor.GetSaveTypeName(x.GetType());
                        AutoRefresh();
                    }
                }
            }
        }

        GUI.enabled = false;
        EditorGUILayout.LabelField($"TYPE: {_componentType.stringValue}, count：{_recordArray.arraySize}");

        for(int i = 0; i < _recordArray.arraySize; i++)
        {
            // Debug.LogError(_recordArray.GetArrayElementAtIndex(i).objectReferenceValue);
            EditorGUILayout.ObjectField(_recordArray.GetArrayElementAtIndex(i).objectReferenceValue, Type.GetType(_componentType.stringValue), true);
        }
        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
