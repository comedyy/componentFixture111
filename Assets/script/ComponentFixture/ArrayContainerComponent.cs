using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 自动化赋值的数组。
public class ArrayContainerComponent : MonoBehaviour {
    [SerializeField]
    public Component[] components;
    [SerializeField]
    public string componentType;
}