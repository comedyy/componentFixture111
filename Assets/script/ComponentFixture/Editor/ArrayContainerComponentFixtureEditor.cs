using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(ArrayContainerComponentFixture), true)]
public class ArrayContainerComponentFixtureEditor : Editor
{
    SerializedProperty _recordArray;

    private void OnEnable() {
        _recordArray = serializedObject.FindProperty("components");
    }

    void AutoRefresh()
    {
        var go = (target as ArrayContainerComponentFixture).gameObject;
        int childCount = go.transform.childCount;
        _recordArray.arraySize = childCount;

        for(int i = 0; i < childCount; i++)
        {
            var x = go.transform.GetChild(i).GetComponent<ComponentFixture1>();
            _recordArray.GetArrayElementAtIndex(i).objectReferenceValue = x;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AutoRefresh();

        GUI.enabled = false;
        GUILayout.Label($"count:{_recordArray.arraySize}");
        for(int i = 0; i < _recordArray.arraySize; i++)
        {
            EditorGUILayout.ObjectField(_recordArray.GetArrayElementAtIndex(i).objectReferenceValue, typeof(ComponentFixture1), true);
        }
        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
