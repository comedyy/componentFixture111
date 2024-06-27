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
                var fieldType = ComponentFixture1Editor.GetSaveType(obj as Component);

                codeGeneratorBuilder.AppendLine($"[SerializeField] {fieldType.Split(',')[0]} {filedName}; ");
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
                        var fieldType = ComponentFixture1Editor.GetSaveType(obj as Component);

                        if(obj is ComponentFixture1)
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as {fieldType.Split(',')[0]}; ");
                        }
                        else
                        {
                            codeGeneratorBuilder.AppendLine($"if(oneFiledRecord.filedName == \"{filedName}\") {filedName} = oneFiledRecord.Object as {fieldType.Split(',')[0]}; ");
                        }
                    }
                }

                codeGeneratorBuilder.AppendLine("return true;");
            }
        }

        codeGeneratorBuilder.Save();
        AssetDatabase.Refresh();
    }
}
