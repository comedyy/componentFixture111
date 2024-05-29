using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorUtil
{   
    [MenuItem("tools/xxx")]
    public static void Open()
    {
        OpenScriptOfType(typeof(LoginViewData));
    }

    ///Get MonoScript reference from type if able
    public static MonoScript MonoScriptFromType(System.Type targetType) {
        if ( targetType == null ) return null;
        var typeName = targetType.Name;
        if ( targetType.IsGenericType ) {
            targetType = targetType.GetGenericTypeDefinition();
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
        }
        return AssetDatabase.FindAssets(string.Format("{0} t:MonoScript", typeName))
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
            .FirstOrDefault(m => m != null && m.GetClass() == targetType);
    }

    ///Opens the MonoScript of a type if able
    public static bool OpenScriptOfType(System.Type type) {
        var mono = MonoScriptFromType(type);
        if ( mono != null ) {
            AssetDatabase.OpenAsset(mono);
            return true;
        }
        Debug.LogError(string.Format("Can't open script of type '{0}', because a script with the same name does not exist.", type));
        return false;
    }
}
