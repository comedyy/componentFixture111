﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;

public struct EditorFiledInfo1
{
    public string field_name;
    public Type type;
}

struct ComponentFixture1EditorInfo
{
    public string componentTypeName;
    public List<(string, Type, Object)> typeComponents;
}


[CustomEditor(typeof(ComponentFixture1), true)]
public class ComponentFixture1Editor : Editor
{
    ComponentFixture1EditorInfo _info;
    private ComponentFixture1 _target_object;
    SerializedProperty _script_name_property;
    SerializedProperty _recordArray;

    static string[] _allBaseComponentType;

    internal void OnEnable()
    {
        if(_allBaseComponentType == null)
        {
            _allBaseComponentType = FindAllBaseComponets().ToArray();
        }

        this._target_object = (ComponentFixture1)this.target;
        _script_name_property = serializedObject.FindProperty("componentType");
        _recordArray = serializedObject.FindProperty("records");
        _info.componentTypeName = _script_name_property.stringValue;
        _info.typeComponents = new List<(string, Type, Object)>();
        GetTypeStruct(_info.componentTypeName);
        serializedObject.ApplyModifiedProperties();
    }

    private static List<string> FindAllBaseComponets()
    {
        var components = new List<string>(){""};

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = Array.Find(assemblies, m=>m.GetName().Name == "Assembly-CSharp");
        if(assembly == null) 
        {
            Debug.LogError("not found Assembly-CSharp");
            Debug.LogError(string.Join("|", assemblies.Select(m=>m.GetName().Name)));
            return components;
        }

        Type objectType = typeof(object);

        string x = "";
        // 遍历程序集中的所有类型
        foreach (Type type in assembly.GetTypes())
        {
            // 检查类型的基类是否为 Object
            if (type.IsSubclassOf(typeof(BaseComponentScript)))
            {
                components.Add(type.ToString());
            }
        }

        return components;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var index = Array.FindIndex(_allBaseComponentType, m=>m == _info.componentTypeName);
        if(index <= 0)
        {
            index = 0;
            index = EditorGUILayout.Popup(index, _allBaseComponentType);
            if(index > 0)
            {
                _script_name_property.stringValue = _allBaseComponentType[index];
            }
        }
        else
        {
            EditorGUILayout.LabelField(_info.componentTypeName);
            EditorGUILayout.LabelField("");
        }

        if(string.IsNullOrEmpty(_script_name_property.stringValue))
        {
            _info.componentTypeName = "";
            return;
        }

        var changeType = _script_name_property.stringValue != _info.componentTypeName;
        if(changeType)
        {
            if(!GetTypeStruct(_script_name_property.stringValue))
            {
                _script_name_property.stringValue = _info.componentTypeName;
            }
        }

        EditorGUI.BeginChangeCheck();
        for(int i = 0; i < _info.typeComponents.Count; i++)
        {
            var item = _info.typeComponents[i];
            var targetObj = EditorGUILayout.ObjectField(item.Item1, item.Item3, item.Item2, true);
            if(item.Item3 != targetObj)
            {
                item.Item3 = targetObj;
                var obj = _recordArray.GetArrayElementAtIndex(i);
                obj.FindPropertyRelative("filedName").stringValue = _info.typeComponents[i].Item1;
                obj.FindPropertyRelative("Object").objectReferenceValue = targetObj;
            }

            _info.typeComponents[i] = item;
        }

        if (EditorGUI.EndChangeCheck() || changeType)
        {
            serializedObject.ApplyModifiedProperties();
        }

    }

    private bool GetTypeStruct(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        var t = Type.GetType(string.Format("{0},Assembly-CSharp", name));
        if (t == null)
        {
            Debug.LogErrorFormat("not find type {0}", name);
            return false;
        }

        if(!typeof(BaseComponentScript).IsAssignableFrom(t))
        {
            Debug.LogErrorFormat("not BaseComponentScript {0}", name);
            return false;
        }

        if(_target_object.records == null) _target_object.records = new OneFiledRecord[0];
        _info.componentTypeName = name;
        _info.typeComponents.Clear();
        FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo info = fields[i];
            if (info.GetCustomAttribute<SerializeField>() != null)
            {
                var findIndex = Array.FindIndex(_target_object.records, m=>m.filedName == info.Name);
                var target = findIndex < 0 ? null : _target_object.records[findIndex].Object;
                _info.typeComponents.Add((info.Name, GetMonoType(info.FieldType), target));
            }
        }

        _recordArray.arraySize = _info.typeComponents.Count;
        for(int i = 0; i < _info.typeComponents.Count; i++)
        {
            var obj = _recordArray.GetArrayElementAtIndex(i);
            obj.FindPropertyRelative("filedName").stringValue = _info.typeComponents[i].Item1;
            obj.FindPropertyRelative("Object").objectReferenceValue = _info.typeComponents[i].Item3;
        }

        return true;
    }

    Type GetMonoType(Type type)
    {
        if(type.IsArray)
        {
            return typeof(ArrayContainerMono);
        }
        else if(type.IsSubclassOf(typeof(BaseComponentScript)))
        {
            return typeof(ComponentFixture1);
        }
        else
        {
            return type;
        }
    }
}
