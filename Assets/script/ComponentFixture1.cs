using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class OneFiledRecord
{
    public string filedName;
    public UnityEngine.Object Object;
}

public class BaseComponentScript
{
    public void OnAfterDeserializeSetFieldRecord(OneFiledRecord[] fieldRecords)
    {
        foreach(var fieldRecord in fieldRecords)
        {
            if(SetByCodeGen(fieldRecord)) return;

            // by reflect
            Type type = GetType();
            FieldInfo info = type.GetField(fieldRecord.filedName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (info == null)
            {
                continue;
            }

            var fieldType = info.FieldType;
            Debug.LogError(fieldType + " " + fieldType.IsSubclassOf(typeof(BaseComponentScript)));

            if(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ArrayContainer<>))
            {
                Type genericType = typeof(ArrayContainer<>);
                Type[] typeArgs = fieldType.GetGenericArguments();
                Type constructedType = genericType.MakeGenericType(typeArgs);

                object instance = Activator.CreateInstance(constructedType, new object[]{(ArrayContainerMono)fieldRecord.Object});
                info.SetValue(this, instance);
            }
            else if(fieldType.IsSubclassOf(typeof(BaseComponentScript)))
            {
                var x = ((ComponentFixture1)fieldRecord.Object).CreateScript();
                Debug.LogError(x);
                info.SetValue(this, x);
            }
            else
            {
                info.SetValue(this, fieldRecord.Object);
            }
        }
    }

    protected virtual bool SetByCodeGen(OneFiledRecord oneFiledRecord){ return false;}
    public virtual void OnEnable(){}
    public virtual void Awake(){}
    public virtual void OnDisable(){}
    public virtual void OnDestroy(){}
}

public class ComponentFixture1 : MonoBehaviour
{
    public string componentType;
    public OneFiledRecord[] records;
    BaseComponentScript baseComponentScript;

    void Awake()
    {
        CreateScript();
    }

    public BaseComponentScript CreateScript()
    {
        if(baseComponentScript != null) return baseComponentScript;

        baseComponentScript = (BaseComponentScript)Activator.CreateInstance(Type.GetType(componentType), true);
        Assert.IsTrue(baseComponentScript != null);
        baseComponentScript.OnAfterDeserializeSetFieldRecord(records);
        baseComponentScript.Awake();

        return baseComponentScript;
    }

    private void OnEnable() {
        baseComponentScript.OnEnable();
    }

    private void OnDisable() {
        baseComponentScript.OnDisable();
    }

    void OnDestroy()
    {
        baseComponentScript.OnDisable();
    }
}
