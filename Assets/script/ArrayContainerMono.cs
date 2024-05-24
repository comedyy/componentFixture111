using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayContainer<T> : IEnumerable<T> where T : UnityEngine.Object
{
    public List<T> Values;

    public ArrayContainer(ArrayContainerMono container)
    {
        Values = new List<T>();
        for(int i = 0; i < container.Length; i++)
        {
            Values.Add(container.Get<T>(i));
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }
}

public class ArrayContainerMono : MonoBehaviour {
    [SerializeField]
    GameObject[] gameObjects;

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
#endif
}