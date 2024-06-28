using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ConvertMonoToComponentFixture {
    
    [MenuItem("Tools/xx111")]
    public static void Do()
    {
        GameObject[] objs = Selection.gameObjects;

        foreach(var x in objs)
        {
            Do1(x);
        }
    }

    private static void Do1(GameObject x)
    {
        var t = x.GetComponent<BaseView>();

        if(!t) return;

        List<(string, Type, UnityEngine.Object)> list = GetAllValues(t);

        // generate type;
        GenerateCode(t, list);

        // add component to obj;

        // fill fields
    }

    private static void GenerateCode(BaseView t, List<(string, Type, UnityEngine.Object)> list)
    {
        // 命名规则，为baseview 一样？还是怎么命名。Baseveiew1吧。  
        var fileName = t.GetType().Name;
        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(Application.dataPath + $"/script/UI/{fileName}.View.cs");

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

    static List<(string, Type, UnityEngine.Object)> GetAllValues(BaseView view)
    {
        var t = view.GetType();
        List<(string, Type, UnityEngine.Object)> list = new List<(string, Type, UnityEngine.Object)>();
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

                list.Add((fieldInfo.Name, fieldInfo.FieldType, fieldInfo.GetValue(view) as UnityEngine.Object));
            }
        }

        return list;
    }
}