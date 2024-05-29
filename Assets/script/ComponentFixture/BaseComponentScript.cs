using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseComponentScript
{
    public void OnAfterDeserializeSetFieldRecord(OneFiledRecord[] fieldRecords)
    {
        foreach(var fieldRecord in fieldRecords)
        {
            if(SetByCodeGen(fieldRecord)) return;

            if(fieldRecord.Object == null) return;

            // by reflect
            Type type = GetType();
            FieldInfo info = type.GetField(fieldRecord.filedName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (info == null)
            {
                continue;
            }

            var fieldType = info.FieldType;

            if(fieldType.IsArray)
            {
                var elementType = fieldType.GetElementType();
                var arrayContainer = (ArrayContainerMono)fieldRecord.Object;
                Array array = Array.CreateInstance(elementType, arrayContainer.Length);
                for(int i = 0; i < arrayContainer.Length; i++)
                {
                    if(elementType == typeof(GameObject))
                    {
                        array.SetValue(arrayContainer[i] , i);
                    }
                    else if(elementType.IsSubclassOf(typeof(BaseComponentScript)))
                    {
                        array.SetValue(arrayContainer[i].GetComponent<ComponentFixture1>().CreateScript() , i);
                    }
                    else
                    {
                        array.SetValue(arrayContainer[i].GetComponent(elementType) , i);
                    }
                }

                info.SetValue(this, array);
            }
            else if(fieldType.IsSubclassOf(typeof(BaseComponentScript)))
            {
                var x = ((ComponentFixture1)fieldRecord.Object).CreateScript();
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
