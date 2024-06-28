using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 自动化赋值的数组。
public class ArrayContainerComponentFixture : MonoBehaviour, ICheckError
{
    [SerializeField]
    public ComponentFixture1[] components;

    public bool HasError()
    {
        return components == null || components.Length == 0;
    }
}