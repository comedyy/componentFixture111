﻿using System;
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

        var type = Type.GetType(componentType);
        if(type == null)
        {
            throw new Exception($"not found Type {type}");
        }

        baseComponentScript = (BaseComponentScript)Activator.CreateInstance(type, true);
        Assert.IsTrue(baseComponentScript != null);
        baseComponentScript.OnAfterDeserializeSetFieldRecord(records);
        baseComponentScript.Awake();

        return baseComponentScript;
    }

    private void OnEnable() {
        baseComponentScript.OnEnable();
    }

    private void OnDisable() {
        baseComponentScript?.OnDisable();
    }

    void OnDestroy()
    {
        baseComponentScript?.OnDisable();
    }
}
