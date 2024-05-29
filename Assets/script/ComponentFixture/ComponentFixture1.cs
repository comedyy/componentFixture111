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

    #if UNITY_EDITOR
    public string filedType;
    #endif
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
