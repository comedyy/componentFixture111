using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ConvertMonoToComponentFixture {
    
    [MenuItem("Tools/CreateComponentFixture")]
    public static void Do()
    {
        HashSet<string> generatedFiles = new HashSet<string>();
        GameObject[] objs = Selection.gameObjects;

        foreach(var x in objs)
        {
            var xx = x.GetComponent<BaseView>();
            if(xx == null) continue;

            Do1(xx.GetType(), generatedFiles);
        }
    }

    [MenuItem("Tools/Assign")]
    public static void xxxx()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach(var x in objs)
        {
            var xx = x.GetComponent<BaseView>();
            if(!xx) continue;

            Assign(xx);
        }
    }

    private static ComponentFixture1 Assign(MonoBehaviour baseview)
    {
        // 查找对应的类。
        var fileName = baseview.GetType().Name+ "_Convert";
        var typeName = fileName + ",Assembly-CSharp";
        var type = Type.GetType(typeName);
        if(type == null)
        {
            Debug.LogError($"not created {fileName}");
            return null;
        }

        var componet = baseview.GetComponent<ComponentFixture1>();
        if(componet)
        {
            Component.DestroyImmediate(componet);
        }

        componet = baseview.gameObject.AddComponent<ComponentFixture1>();
        componet.componentType = typeName;

        List<(string, Type, object)> list = GetAllValues(baseview);
        componet.records = new OneFiledRecord[list.Count];
        
        for(int i = 0; i < componet.records.Length; i++)
        {
            var name = list[i].Item1;
            var componentType = list[i].Item2;
            var data = list[i].Item3;

            componet.records[i] = new OneFiledRecord(){filedName = name};
            if(componentType.IsArray)
            {
                var childArray = data as UnityEngine.Object[];
                if(childArray.Length == 0) continue;

                var childType = componentType.GetElementType();
                var firstChild = childArray[0];
                if(firstChild is MonoBehaviour monoBehaviour && childType.Assembly.GetName().Name == "Assembly-CSharp")
                {
                    var parenetNode = monoBehaviour.transform.parent;
                    var array = parenetNode.gameObject.AddComponent<ArrayContainerComponentFixture>();
                    array.components = childArray.Select(m=>Assign(m as MonoBehaviour)).ToArray();
                }
                else if(firstChild is Component component)
                {
                    var parenetNode = component.transform.parent;
                    var array = parenetNode.gameObject.AddComponent<ArrayContainerComponent>();
                    array.components = data as UnityEngine.Component[];
                    array.componentType = ComponentFixture1Editor.GetSaveTypeName(childType);
                }
                else if(firstChild is GameObject go)
                {
                    var parenetNode = go.transform.parent;
                    var array = parenetNode.gameObject.AddComponent<ArrayContainerGameObject>();
                    array.gameObjects = data as UnityEngine.GameObject[];
                }
                else
                {
                    throw new Exception($"exception not here {childType}");
                }
            }
            else if(componentType.IsSubclassOf(typeof(MonoBehaviour)) && componentType.Assembly.GetName().Name == "Assembly-CSharp")
            {
                componet.records[i].Object = Assign(data as MonoBehaviour);
            }
            else
            {
                componet.records[i].Object = data as UnityEngine.Object;
            }
        }

        return componet;
    }

    private static void Do1(Type t, HashSet<string> generatedFiles)
    {
        if(t.Assembly.GetName().Name != "Assembly-CSharp")
        {
            return;
        }

        List<(string, Type)> list = GetAllTypes(t);

        GenerateCode(t, list, generatedFiles);

        foreach(var x in list)
        {
            if(x.Item2.IsSubclassOf(typeof(MonoBehaviour)))
            {
                Do1(x.Item2, generatedFiles);
            }
            else if(x.Item2.IsSubclassOf(typeof(MonoBehaviour[])))
            {
                var type = x.Item2.GetElementType();
                Do1(type, generatedFiles);
            }
        }
    }

    private static void GenerateCode(Type type, List<(string, Type)> list, HashSet<string> generatedFiles)
    {
        // 命名规则，为baseview 一样？还是怎么命名。Baseveiew1吧。  
        var fileName = type.Name + "_Convert";
        if(generatedFiles.Contains(fileName)) return;

        generatedFiles.Add(fileName);
        var path = Application.dataPath + $"/demo/ConvertedUI/{fileName}.View.cs";

        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(path);

        codeGeneratorBuilder.AppendLine("using UnityEngine;");
        using(codeGeneratorBuilder.StartFold($"public partial class {fileName} : BaseComponentScript"))
        {
            for(int i = 0; i < list.Count; i++)
            {
                var filedName = list[i].Item1;
                var fieldType = GetSaveType(list[i].Item2);

                codeGeneratorBuilder.AppendLine($"[SerializeField] public {fieldType} {filedName}; ");
            }
        }

        codeGeneratorBuilder.Save();
        AssetDatabase.Refresh();
    }

    private static string GetSaveType(Type item2)
    {
        Type t = item2;
        return t.FullName;
    }

    static List<(string, Type, object)> GetAllValues(object view)
    {
        var t = view.GetType();
        List<(string, Type, object)> list = new List<(string, Type, object)>();
        FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo fieldInfo = fields[i];
            if (fieldInfo.GetCustomAttribute<SerializeField>() != null || fieldInfo.IsPublic)
            {
                var isOkType = fieldInfo.FieldType.IsSubclassOf(typeof( UnityEngine.Object));
                isOkType |= fieldInfo.FieldType.IsArray && fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object));
                if(!isOkType)
                {
                    continue;
                }

                list.Add((fieldInfo.Name, fieldInfo.FieldType, fieldInfo.GetValue(view)));
            }
        }

        return list;
    }

    static List<(string, Type)> GetAllTypes(Type t)
    {
        List<(string, Type)> list = new List<(string, Type)>();
        FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo fieldInfo = fields[i];
            if (fieldInfo.GetCustomAttribute<SerializeField>() != null || fieldInfo.IsPublic)
            {
                var isOkType = fieldInfo.FieldType.IsSubclassOf(typeof( UnityEngine.Object));
                isOkType |= fieldInfo.FieldType.IsArray && fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object));
                if(!isOkType)
                {
                    continue;
                }

                list.Add((fieldInfo.Name, fieldInfo.FieldType));
            }
        }

        return list;
    }
}