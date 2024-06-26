using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;
using System.IO;

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
    string _newFiledName = "";
    string _errorMsg = "";
    string folderFiledName = "";

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
        GetTypeStruct(_script_name_property.stringValue, false);
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
                components.Add(GetSaveTypeName(type));
            }
        }

        return components;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        if(string.IsNullOrEmpty(_script_name_property.stringValue)) // 完全empty
        {
            var index = Array.FindIndex(_allBaseComponentType, m=>m == _script_name_property.stringValue);
            if(index <= 0)
            {
                index = 0;
                index = EditorGUILayout.Popup(index, _allBaseComponentType);
                if(index > 0)
                {
                    _script_name_property.stringValue = _allBaseComponentType[index];
                    GetTypeStruct(_script_name_property.stringValue, true);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("新增类型：");
            _newFiledName = EditorGUILayout.TextField(_newFiledName);
            if(GUILayout.Button("+"))
            {
                if(Type.GetType(_newFiledName) != null)
                {
                    _errorMsg = $"{_newFiledName} exist, can not create";
                    _newFiledName = "";
                }
                else
                {
                    _script_name_property.stringValue = $"{_newFiledName},Assembly-CSharp";
                    _newFiledName = "";
                }
            }

            EditorGUILayout.EndHorizontal();

            if(string.IsNullOrEmpty(_script_name_property.stringValue))
            {
                EditorGUI.EndChangeCheck();
                return;
            }
        }
        else
        {
            DrawScriptTitle();
        }

        for(int i = 0; i < _recordArray.arraySize; i++)
        {
            var item = _recordArray.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            var filedName = item.FindPropertyRelative("filedName").stringValue;
            var isNew = !_allDataFields.ContainsKey(filedName);
            if(isNew)
            {
                GUI.color = Color.yellow;
            }

            var match = CheckFieldMatch(filedName, item.FindPropertyRelative("Object").objectReferenceValue);
            if(!match)
            {
                GUI.color = Color.red;
            }

            item.FindPropertyRelative("Object").objectReferenceValue = EditorGUILayout.ObjectField(GetDisplayName(item.FindPropertyRelative("filedName").stringValue), 
                                                        item.FindPropertyRelative("Object").objectReferenceValue,
                                                        GetTypeByStr(item.FindPropertyRelative("filedType").stringValue), true);
            GUI.color = Color.white;

           
            if(GUILayout.Button("x"))
            {
                _recordArray.DeleteArrayElementAtIndex(i);
                i--;
                continue;
            }
            
            EditorGUILayout.EndHorizontal();

             if(isNew && item.FindPropertyRelative("Object").objectReferenceValue != null) // 可以选择类型
            {
                var obj = item.FindPropertyRelative("Object").objectReferenceValue;
                var isFold = EditorGUILayout.Foldout(folderFiledName == filedName, "选择组件");
                if(isFold)
                {
                    folderFiledName = filedName;

                    var components = (obj as Component).GetComponents<Component>();
                    foreach(var x in components)
                    {
                        if(GUILayout.Button($"{x.GetType()}"))
                        {
                            item.FindPropertyRelative("Object").objectReferenceValue = x;
                            item.FindPropertyRelative("filedType").stringValue = GetSaveType(x);
                            folderFiledName = "";
                            break;
                        }
                    }
                }
                else
                {
                    folderFiledName = "";
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("新增字段：");
        _newFiledName = EditorGUILayout.TextField(_newFiledName);
        if(GUILayout.Button("+"))
        {
            bool exist = false;
            for(int i = 0; i < _recordArray.arraySize; i++)
            {
                var item = _recordArray.GetArrayElementAtIndex(i);
                if(item.FindPropertyRelative("filedName").stringValue == _newFiledName)
                {
                    exist = true;
                }
            }

            if(exist || string.IsNullOrWhiteSpace(_newFiledName))
            {
                _errorMsg = $"{_newFiledName} exist, can not create filed";
                _newFiledName = "";
            }
            else
            {
                _recordArray.arraySize = _recordArray.arraySize + 1;
                var item = _recordArray.GetArrayElementAtIndex(_recordArray.arraySize - 1);
                item.FindPropertyRelative("filedName").stringValue = _newFiledName;
                item.FindPropertyRelative("filedType").stringValue = GetSaveTypeName(typeof(Component));
                _newFiledName = "";
            }
        }
        EditorGUILayout.EndHorizontal();        

        if(HasThingSave())
        {
            if(GUILayout.Button("save cs file"))
            {
                CodeGenerator.SaveCSFile(_script_name_property, _recordArray);
            }
        }
       
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawScriptTitle()
    {
        var isExist = Type.GetType(_script_name_property.stringValue) != null;

        if(isExist)
        {
            GUI.enabled = false;
            var fileName = _script_name_property.stringValue.Split(',')[0];
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>($"Assets/script/UI/{fileName}.cs");
            EditorGUILayout.ObjectField("", monoScript, typeof(MonoScript), false);
            GUI.enabled = true;
        }
        else
        {
            GUI.color = Color.red;
            var title = $"未保存对象 【{_script_name_property.stringValue}】";
            EditorGUILayout.LabelField(title);
            GUI.color = Color.white;
        }
    }

    private bool HasThingSave()
    {
        if(Type.GetType(_script_name_property.stringValue) == null)
        {
            return true;
        } 

        for(int i = 0; i < _recordArray.arraySize; i++)
        {
            var item = _recordArray.GetArrayElementAtIndex(i);
            var filedName = item.FindPropertyRelative("filedName").stringValue;
            var fieldType = item.FindPropertyRelative("filedType").stringValue;

            var isNewField = !_allDataFields.ContainsKey(filedName);
            var value = item.FindPropertyRelative("Object").objectReferenceValue;
            if(value == null && isNewField)
            {
                return false;
            }

            if(!_allDataFields.TryGetValue(filedName, out var type) || GetSaveTypeName(type) != fieldType)
            {
                return true;
            }
        }

        var toutalCount = _allDataFields.Count;
        if(toutalCount != _recordArray.arraySize)
        {
            return true;
        } 

        return false;
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

                        return comp.componentType == GetSaveTypeName(elementType);
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
            return componentFixture.componentType == GetSaveTypeName(type);
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

    private bool GetTypeStruct(string name, bool validateType)
    {
        if (string.IsNullOrEmpty(name)) return false;

        var t = Type.GetType(name);
        if (t == null)
        {
            if(validateType)
            {
                Debug.LogErrorFormat("not find type {0}", name);
            }
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

                var fieldMonoTypeStr = GetSaveTypeName(fieldMonoType);

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

    static string GetSaveTypeName(Type type)
    {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    string GetSaveType(Component c)
    {
        var type = c.GetType();
        if(type == typeof(ComponentFixture1))
        {
            return c.GetComponent<ComponentFixture1>().componentType;
        }
        else if(c is ArrayContainerMono)
        {
            return GetSaveTypeName(typeof(GameObject[]));
        }
        else
        {
            return GetSaveTypeName(type);
        }
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
