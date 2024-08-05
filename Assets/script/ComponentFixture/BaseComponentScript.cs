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
        if(SetByCodeGen(fieldRecords)) return;

        throw new Exception("not implement SetByCodeGen" + this);
    }

    protected virtual bool SetByCodeGen(OneFiledRecord[] oneFiledRecords){ return false;}
    public virtual void OnEnable(){}
    public virtual void Awake(){}
    public virtual void OnDisable(){}
    public virtual void OnDestroy(){}
}
