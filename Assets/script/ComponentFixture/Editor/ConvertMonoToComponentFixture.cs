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
        if(baseview == null) throw new Exception("Assign found null");

        // 查找对应的类。
        var fileName = baseview.GetType().Name+ "_Convert";
        var typeName = fileName + ", Assembly-CSharp";
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

            if(data == null) throw new Exception($"null found {name} {componentType}");

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
                    componet.records[i].Object = array;
                }
                else if(firstChild is Component component)
                {
                    var parenetNode = component.transform.parent;
                    var array = parenetNode.gameObject.AddComponent<ArrayContainerComponent>();
                    array.components = data as UnityEngine.Component[];
                    array.componentType = ComponentFixture1Editor.GetSaveTypeName(childType);
                    componet.records[i].Object = array;
                }
                else if(firstChild is GameObject go)
                {
                    var parenetNode = go.transform.parent;
                    var array = parenetNode.gameObject.AddComponent<ArrayContainerGameObject>();
                    array.gameObjects = data as UnityEngine.GameObject[];
                    componet.records[i].Object = array;
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
                var fieldType = GetSaveType(list[i].Item2, out var x);

                codeGeneratorBuilder.AppendLine($"[SerializeField] public {fieldType} {filedName}; ");
            }

            using(codeGeneratorBuilder.StartFold($"protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)")){
                using(codeGeneratorBuilder.StartFold($"foreach(var oneFiledRecord in oneFiledRecords)")){
                    for(int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        var filedName = item.Item1;
                        var type1 = item.Item2;
                        var fieldType = GetSaveType(item.Item2, out var isComponentFixture);

                        if(isComponentFixture)
                        {
                            if(type1.IsArray)
                            {
                                var elementType = GetSaveType(type1.GetElementType(), out var _);
                                using(codeGeneratorBuilder.StartFold($"if(oneFiledRecord.filedName == \"{filedName}\") "))
                                {
                                    codeGeneratorBuilder.AppendLine($"var allCompnents = ((ArrayContainerComponentFixture)oneFiledRecord.Object).components;");
                                    codeGeneratorBuilder.AppendLine($"{filedName} = new {elementType}[allCompnents.Length];");
                                    codeGeneratorBuilder.AppendLine($"for(int i = 0; i < allCompnents.Length; i++) {filedName}[i] = allCompnents[i].CreateScript() as {elementType}; ");
                                }
                            }
                            else
                            {
                                codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as {fieldType}; ");
                            }
                        }
                        else if(type1.IsArray)
                        {
                            if(type1.GetElementType() != typeof(GameObject))
                            {
                                codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ArrayContainerMono)(oneFiledRecord.Object)).gameObjects; ");
                            }
                            else
                            {
                                codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ArrayContainerComponent)oneFiledRecord.Object).components as {fieldType}; ");
                            }
                        }
                        else
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = oneFiledRecord.Object as {fieldType}; ");
                        }
                    }
                }

                codeGeneratorBuilder.AppendLine("return true;");
            }
        }

        SaveCustomCSFile(fileName);

        codeGeneratorBuilder.Save();
        AssetDatabase.Refresh();
    }

    private static void SaveCustomCSFile(string fileName)
    {
        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(Application.dataPath + $"/demo/ConvertedUI/{fileName}.Logic.cs");
        codeGeneratorBuilder.AppendLine("using UnityEngine;");
        using(codeGeneratorBuilder.StartFold($"public partial class {fileName} : BaseComponentScript"))
        {
        }
        codeGeneratorBuilder.Save();
    }

    private static string GetSaveType(Type item2, out bool isComponentFixture)
    {
        Type t = item2;
        var x = t.FullName;
        

        if(t.Assembly.GetName().Name != "Assembly-CSharp")
        {
            isComponentFixture = false;
            return x;
        }

        if(t.IsSubclassOf(typeof(MonoBehaviour)))
        {
            isComponentFixture = true;
            return x + "_Convert";
        }

        if(t.IsArray)
        {
            isComponentFixture = true;
            var tt = t.GetElementType();
            return tt.FullName + "_Convert[]";
        }

        isComponentFixture = false;
        return x;
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