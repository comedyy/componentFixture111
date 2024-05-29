using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;

public class FiledInfo
{
    public string filedName;
    public Type type;
}

[CustomEditor(typeof(ComponentFixture1), true)]
public class ComponentFixture1Editor : Editor
{
    static Dictionary<string, Type> _allTypeByString = new Dictionary<string, Type>();
    private ComponentFixture1 _target_object;
    SerializedProperty _script_name_property;
    SerializedProperty _recordArray;
    Dictionary<string, Type> _allDataFields = new Dictionary<string, Type>();

    static string[] _allBaseComponentType;

    internal void OnEnable()
    {
        if(_allBaseComponentType == null)
        {
            _allBaseComponentType = FindAllBaseComponets().ToArray();
        }

        this._target_object = (ComponentFixture1)this.target;
        if(_target_object.records == null) _target_object.records = new OneFiledRecord[0];
        _script_name_property = serializedObject.FindProperty("componentType");
        _recordArray = serializedObject.FindProperty("records");
        GetTypeStruct(_script_name_property.stringValue);
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
                components.Add(GetTypeName(type));
            }
        }

        return components;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        var index = Array.FindIndex(_allBaseComponentType, m=>m == _script_name_property.stringValue);
        if(index <= 0)
        {
            index = 0;
            index = EditorGUILayout.Popup(index, _allBaseComponentType);
            if(index > 0)
            {
                _script_name_property.stringValue = _allBaseComponentType[index];
                GetTypeStruct(_script_name_property.stringValue);
            }
        }
        else
        {
            EditorGUILayout.LabelField(_script_name_property.stringValue);
            EditorGUILayout.LabelField("");
        }

        if(!string.IsNullOrEmpty(_script_name_property.stringValue))
        {
             for(int i = 0; i < _recordArray.arraySize; i++)
            {
                var item = _recordArray.GetArrayElementAtIndex(i);

                var match = CheckFieldMatch(item.FindPropertyRelative("filedName").stringValue, item.FindPropertyRelative("Object").objectReferenceValue);
                if(!match)
                {
                    GUI.color = Color.red;
                }
                item.FindPropertyRelative("Object").objectReferenceValue = EditorGUILayout.ObjectField(GetDisplayName(item.FindPropertyRelative("filedName").stringValue), 
                                                            item.FindPropertyRelative("Object").objectReferenceValue,
                                                            GetTypeByStr(item.FindPropertyRelative("filedType").stringValue), true);
                GUI.color = Color.white;
            }
        }
       
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private bool CheckFieldMatch(string stringValue, Object objectReferenceValue)
    {
        if(!_allDataFields.TryGetValue(stringValue, out var type))
        {
            return true;
        }

        if(objectReferenceValue == null)
        {
            return true;
        }

        if(objectReferenceValue is ArrayContainerMono arrayMono)
        {
            var elementType = type.GetElementType();
            if(elementType == typeof(GameObject))
            {
                return true;
            }
            else
            {
                var subType = GetMonoType(elementType);
                if(subType == typeof(ComponentFixture1))
                {
                    return arrayMono.gameObjects.All(m=>{
                        var comp = m.GetComponent<ComponentFixture1>();
                        if(comp == null) return false;

                        return comp.componentType == GetTypeName(elementType);
                    });
                }
                else if(subType == typeof(ArrayContainerMono))
                {
                    Debug.LogError("sub type 不能为ArrayContainerMono");
                    return false;
                }
                else if(subType.IsSubclassOf(typeof(Object)))
                {
                    return arrayMono.gameObjects.All(m=>{
                        return m.GetComponent(subType) != null;
                    });
                }
                else
                {
                    return false;
                }
            }
        }
        else if(objectReferenceValue is ComponentFixture1 componentFixture)
        {
            return componentFixture.componentType == GetTypeName(type);
        }
        else
        {
            return type.IsAssignableFrom(objectReferenceValue.GetType());
        }
    }


    private string GetDisplayName(string stringValue)
    {
        string x = "Object";
        if(_allDataFields.TryGetValue(stringValue, out var type))
        {
            x = type.Name;
        }

        return $"【{stringValue}】-{x}";
    }


    private Type GetTypeByStr(string stringValue)
    {
        if(_allTypeByString.TryGetValue(stringValue, out var type))
        {
            return type;
        }

        type = Type.GetType(stringValue);
        if(type == null)
        {
            type = typeof(Object);
        }

        _allTypeByString.Add(stringValue, type);

        return type;
    }

    private bool GetTypeStruct(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        var t = Type.GetType(name);
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

        FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo fieldInfo = fields[i];
            if (fieldInfo.GetCustomAttribute<SerializeField>() != null)
            {
                var findIndex = Array.FindIndex(_target_object.records, m=>m.filedName == fieldInfo.Name);
                var fieldMonoType = GetMonoType(fieldInfo.FieldType);
                if(fieldMonoType == null)
                {
                    continue;
                }

                var fieldMonoTypeStr = GetTypeName(fieldMonoType);

                _allDataFields.Add(fieldInfo.Name, fieldInfo.FieldType);
                if(findIndex < 0)
                {
                    var lastIndex = _recordArray.arraySize;
                    _recordArray.InsertArrayElementAtIndex(lastIndex);
                    var obj = _recordArray.GetArrayElementAtIndex(lastIndex);
                    obj.FindPropertyRelative("filedName").stringValue = fieldInfo.Name;
                    obj.FindPropertyRelative("filedType").stringValue = fieldMonoTypeStr;
                    obj.FindPropertyRelative("Object").objectReferenceValue = null;
                }
            }
        }

        // _recordArray.arraySize = _info.typeComponents.Count;
        // for(int i = 0; i < _info.typeComponents.Count; i++)
        // {
        //     var obj = _recordArray.GetArrayElementAtIndex(i);
        //     obj.FindPropertyRelative("filedName").stringValue = _info.typeComponents[i].field_name;
        //     obj.FindPropertyRelative("Object").objectReferenceValue = _info.typeComponents[i].Object;
        // }

        return true;
    }

    static string GetTypeName(Type type)
    {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    Type GetMonoType(Type type)
    {
        if(type.IsArray)
        {
            var subType = type.GetElementType();
            if(subType.IsArray)
            {
                return null;
            }

            if(GetMonoType(subType) != null)
            {
                return typeof(ArrayContainerMono);
            }
            else
            {
                return null;
            }
        }
        else if(type.IsSubclassOf(typeof(BaseComponentScript)))
        {
            return typeof(ComponentFixture1);
        }
        else if(type.IsSubclassOf(typeof(Object)))
        {
            return type;
        }
        
        return null;
    }
}
