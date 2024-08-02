using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

// 界面信息
public class ViewInfo 
{
    public string path;
    public bool is_main;
}

public abstract class BaseView : MonoBehaviour
{
    private Dictionary<int, Action> m_dicAttrib = new Dictionary<int,Action>();
    private List<(Type, Delegate)> _eventListeners = new List<(Type, Delegate)>();

    public void Init(UIManager uIManger) {
        OnAwake();
        InitCallback();
        uIManger.AddOpenCallBack(this, OpenOrClose);
    }

    private void OpenOrClose(bool isOpen, object param) {
        gameObject.SetActive(isOpen);

        if (isOpen)
        {
            OnOpenView(param);
            
            foreach (int attrib in m_dicAttrib.Keys)
            {
                Action action = m_dicAttrib[attrib];
                // UserInfo.Instance.AddCallBack(this, attrib, action);
                action();
            }
        }
        else {
            // foreach (int attrib in m_dicAttrib.Keys)
            // {
            //     UserInfo.Instance.RemoveCallBack(this, attrib);
            // }

            // foreach(var x in _eventListeners)
            // {
            //     GameEventMgr.Instance.UnsubscribeWithDelegate(x.Item1, x.Item2);
            // }

            OnCloseView();
        }
    }

    protected void AddAttribAction(int attrib, Action action){
        m_dicAttrib[attrib] = action;
    }

#region callback
    protected virtual void InitCallback(){}
    protected virtual void OnAwake() { }
    protected virtual void OnOpenView(object param) { }
    protected virtual void OnCloseView() {}
#endregion
    
    #if UNITY_EDITOR
    [ContextMenu("自动赋值")]
    public void CopyComponent()
    {
        // 找出所有的compnent 跟 路径
        var fieldInfos = GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        foreach(var info in fieldInfos)
        {
            var filedType = info.FieldType;
            if(filedType.IsArray)
            {
                var subType = filedType.GetElementType();
                if(!subType.IsSubclassOf(typeof(Component)))
                {
                    continue;
                }

                var array = (Array)info.GetValue(this);
                var x = gameObject.GetComponentsInChildren(subType);
                if(x.Length == array.Length)
                {
                    for(int i = 0; i < x.Length; i++)
                    {
                        array.SetValue(x[i], i);
                    }
                }
            }
            else 
            {
                FieldItem(filedType, info);
            }
        }

        UnityEditor.EditorUtility.SetDirty(transform.root);
    }

    void FieldItem(Type filedType, FieldInfo info)
    {
        if(filedType == typeof(GameObject) || filedType == typeof(Transform) )
        {
            var name = info.Name.Split('_').Last().ToLower();
            var xs = gameObject.GetComponentsInChildren<Transform>().ToList();
            var index = xs.FindIndex(m=>m.name == name);
            if(index >= 0)
            {
                if(filedType == typeof(GameObject))
                {
                    info.SetValue(this, xs[index].gameObject);
                }
                else
                {
                    info.SetValue(this, xs[index]);
                }
            }
        }
        else if(filedType.IsSubclassOf(typeof(Component)))
        {
            var x = gameObject.GetComponentsInChildren(filedType);
            if(x.Length == 1)
            {
                info.SetValue(this, x[0]);
            }
        }
    }
#endif
}
