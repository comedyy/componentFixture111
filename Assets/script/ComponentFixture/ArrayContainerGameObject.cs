using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayContainerGameObject : MonoBehaviour, ICheckError {
    [SerializeField]
    public GameObject[] gameObjects;

    public int Length => gameObjects.Length;

    public GameObject this[int index]
    {
        get
        {
            return gameObjects[index];
        }
    }

    public T Get<T>(int index)
    {
        return gameObjects[index].GetComponent<T>();
    }   

    #if UNITY_EDITOR
    void OnValidate() {
        FindChildRen();
    }


    [ContextMenu("自动赋值")]
    public void CopyComponent()
    {
        FindChildRen();

        UnityEditor.EditorUtility.SetDirty(transform.root);
    }

    void FindChildRen()
    {
        var childCount = transform.childCount;
        gameObjects = new GameObject[childCount];

        for(int i = 0; i < childCount; i++)
        {
            gameObjects[i] = transform.GetChild(i).gameObject;
        }
    }

    public bool HasError()
    {
        return gameObjects == null || gameObjects.Length == 0;
    }

#endif
}