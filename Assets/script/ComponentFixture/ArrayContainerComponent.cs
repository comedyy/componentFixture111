using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 自动化赋值的数组。
public class ArrayContainerComponent : MonoBehaviour, ICheckError {
    [SerializeField]
    public Component[] components;
    [SerializeField]
    public string componentType;

    public bool HasError()
    {
        if(string.IsNullOrEmpty(componentType)) return true;

        return components != null && components.Length > 0;
    }
}