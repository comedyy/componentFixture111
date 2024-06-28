using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CodeGenerator
{
    public static void SaveCSFile(SerializedProperty _script_name_property, SerializedProperty _recordArray)
    {
        var fileName = _script_name_property.stringValue.Split(',')[0];

        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(Application.dataPath + $"/script/UI/{fileName}.View.cs");

        codeGeneratorBuilder.AppendLine("using UnityEngine;");
        using(codeGeneratorBuilder.StartFold($"public partial class {fileName} : BaseComponentScript"))
        {
            for(int i = 0; i < _recordArray.arraySize; i++)
            {
                var item = _recordArray.GetArrayElementAtIndex(i);
                var filedName = item.FindPropertyRelative("filedName").stringValue;
                var obj = item.FindPropertyRelative("Object").objectReferenceValue;

                if(obj == null) continue;
                var fieldType = GetSaveType(obj );

                codeGeneratorBuilder.AppendLine($"[SerializeField] public {fieldType} {filedName}; ");
            }

            codeGeneratorBuilder.NewLine();

            using(codeGeneratorBuilder.StartFold($"protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)")){
                using(codeGeneratorBuilder.StartFold($"foreach(var oneFiledRecord in oneFiledRecords)")){
                    for(int i = 0; i < _recordArray.arraySize; i++)
                    {
                        var item = _recordArray.GetArrayElementAtIndex(i);
                        var filedName = item.FindPropertyRelative("filedName").stringValue;
                        var obj = item.FindPropertyRelative("Object").objectReferenceValue;

                        if(obj == null) continue;
                        var fieldType = GetSaveType(obj);

                        if(obj is ComponentFixture1)
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as {fieldType}; ");
                        }
                        else if(obj is ArrayContainerComponent)
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ArrayContainerComponent)oneFiledRecord.Object).components as {fieldType}; ");
                        }
                        else if(obj is ArrayContainerComponentFixture arrayFixture)
                        {
                            using(codeGeneratorBuilder.StartFold($"if(oneFiledRecord.filedName == \"{filedName}\") "))
                            {
                                codeGeneratorBuilder.AppendLine($"var allCompnents = ((ArrayContainerComponentFixture)oneFiledRecord.Object).components;");
                                codeGeneratorBuilder.AppendLine($"{filedName} = new LoginViewDataItem[allCompnents.Length];");
                                codeGeneratorBuilder.AppendLine($"for(int i = 0; i < allCompnents.Length; i++) {filedName}[i] = allCompnents[i].CreateScript() as {GetSaveType(arrayFixture.components[0])}; ");
                            }
                        }
                        else if(obj is ArrayContainerMono)
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ArrayContainerMono)(oneFiledRecord.Object)).gameObjects; ");
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

        codeGeneratorBuilder.Save();

        // save custom file
        SaveCustomCSFile(_script_name_property, _recordArray);

        AssetDatabase.Refresh();
    }

    private static void SaveCustomCSFile(SerializedProperty script_name_property, SerializedProperty recordArray)
    {
        var fileName = script_name_property.stringValue.Split(',')[0];
        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(Application.dataPath + $"/script/UI/{fileName}.cs");
        using(codeGeneratorBuilder.StartFold($"public partial class {fileName} : BaseComponentScript"))
        {
        }
        codeGeneratorBuilder.Save();
    }

    public static string GetSaveType(Object o)
    {
        var c = o as Component;
        var type = o.GetType();
        if(type == typeof(ComponentFixture1))
        {
            var x = c.GetComponent<ComponentFixture1>().componentType;
            return x.Split(',')[0];
        }
        else if(c is ArrayContainerComponent arrayContainerComponent)
        {
            var x = arrayContainerComponent.componentType;
            var y = x.Split(',')[0];
            return y + "[]";
        }
        else if(c is ArrayContainerComponentFixture fixture)
        {
            if(fixture.components.Length == 0) throw new System.Exception("ArrayContainerComponentFixture not fill data");

            var x = fixture.components[0].componentType;
            var y = x.Split(',')[0];
            return y + "[]";
        }
        else if(c is ArrayContainerMono)
        {
            return typeof(GameObject[]).FullName;
        }
        else
        {
            return type.FullName;
        }
    }
}
