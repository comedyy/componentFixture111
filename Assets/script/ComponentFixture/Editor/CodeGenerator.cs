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

        CodeGeneratorBuilder codeGeneratorBuilder = new CodeGeneratorBuilder(Application.dataPath + $"/script/UI/{fileName}.cs");

        codeGeneratorBuilder.AppendLine("using UnityEngine;");
        using(codeGeneratorBuilder.StartFold($"public partial class {fileName} : BaseComponentScript"))
        {
            for(int i = 0; i < _recordArray.arraySize; i++)
            {
                var item = _recordArray.GetArrayElementAtIndex(i);
                var filedName = item.FindPropertyRelative("filedName").stringValue;
                var obj = item.FindPropertyRelative("Object").objectReferenceValue;

                if(obj == null) continue;
                var fieldType = GetSaveType(obj as Component);

                codeGeneratorBuilder.AppendLine($"[SerializeField] {fieldType} {filedName}; ");
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
                        var fieldType = GetSaveType(obj as Component);

                        if(obj is ComponentFixture1)
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as {fieldType}; ");
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
        AssetDatabase.Refresh();
    }

    
    public static string GetSaveType(Component c)
    {
        var type = c.GetType();
        if(type == typeof(ComponentFixture1))
        {
            var x = c.GetComponent<ComponentFixture1>().componentType;
            return x.Split(',')[0];
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
