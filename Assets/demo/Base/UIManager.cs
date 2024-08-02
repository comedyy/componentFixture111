using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public enum CloseType 
{ 
    None,
    One,
    All,
}

public class UIManager : Singleton<UIManager> {
    [UnityEngine.Scripting.Preserve]
    public UIManager(){}

    GameObject _obj = null;
    GameObject _obj_main = null;
    bool _isUIInScene;
    public Transform UIRoot => _obj.transform;
    public bool IsInited{get; private set;}

    Dictionary<ViewInfo, BaseView> m_dicView = new Dictionary<ViewInfo, BaseView>();
    List<BaseView> m_stackView = new List<BaseView>();
    Dictionary<BaseView, Action<bool, object>> m_dicAction = new Dictionary<BaseView, Action<bool, object>>();

    public void AddOpenCallBack(BaseView view, Action<bool, object> action)
    {
        m_dicAction[view] = action;
    }

    public void Init(bool isUIInScene){
        _isUIInScene = isUIInScene;
        _obj = GameObject.Find("/Canvas");
        _obj_main = GameObject.Find("/CanvasMain");
        GameObject.DontDestroyOnLoad(_obj);

        if(_obj_main)
        {
            GameObject.DontDestroyOnLoad(_obj_main);
        }

        EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
        GameObject.DontDestroyOnLoad(eventSystem.gameObject);
        IsInited = true;
    }

    public void OpenView(ViewInfo type, CloseType closeType = CloseType.One, object param = null)
    {
        if(!UIManager.Instance.IsInited)
        {
            return;
        }

        CloseView(closeType);
        OpenViewEx(type, param);
    }

    public void CloseTop(ViewInfo type = null, bool ignoreError = false)
    {
        if (m_stackView.Count == 0)
        {
           return; 
        }

        if (type != null)
        {
            if (!m_dicView.ContainsKey(type))
            {
                if(!ignoreError)
                {
                    Debug.LogError($"CloseTop Error not open [{type.path}]" );
                }
                return;
            }

            if (m_stackView.Last() != m_dicView[type] && !ignoreError)
            {
                if(!ignoreError)
                {
                    Debug.LogError($"CloseTop Error expect [{type.path}] but [{m_stackView.Last()}]" );
                }
                return;
            }
        }

        CloseView(CloseType.One);
    }

    private BaseView OpenViewEx(ViewInfo type, object param)
    {
		if (!m_dicView.ContainsKey(type)) {
			LoadView (type);
		}

        BaseView view = m_dicView[type];
        view.transform.SetAsLastSibling();
        m_stackView.Add(view);

        m_dicAction[view](true, param);
        return view;
    }

    List<BaseView> m_lstCloseView = new List<BaseView>();
    public void CloseView(CloseType type)
    {
        m_lstCloseView.Clear();
        if (type == CloseType.All)
        {
            m_lstCloseView.AddRange(m_stackView);
            m_stackView.Clear();
        }
        else if (type == CloseType.One)
        {
            if (m_stackView.Count > 0)
            {
                var x = m_stackView.Last();
                m_stackView.RemoveAt(m_stackView.Count - 1);
                m_lstCloseView.Add(x);
            }
        }

        // 关闭界面
        m_lstCloseView.ForEach((m) =>
        {
            m_dicAction[m](false, null);
        });
    }

	void LoadView(ViewInfo type)
    {        
        if(type == null)
        {
            throw new Exception("viewinfo == null");
        }

        Transform parent = type.is_main ? _obj_main.transform : _obj.transform;
        if(parent ==  null)
        {
            throw new Exception("ui parent not found");
        }

        GameObject obj = null;
        if (_isUIInScene)
        {
            var trans = parent.Find(type.path);
            if(trans == null)
            {
                throw new Exception($"LoadView not found :{type.path} in scene");
            }

            obj = trans.gameObject;
        }
        else{
            GameObject objPrefab = Resources.Load<GameObject> (string.Format("UI/{0}", type.path));
            if(objPrefab == null)
            {
                throw new Exception($"LoadView not found :{type.path} Load");
            }

            obj = GameObject.Instantiate (objPrefab, parent, false) as GameObject;
        }
        
        BaseView view = obj.GetComponent<BaseView>();
        if(view == null)
        {
            throw new Exception($"cannot found baseView in :{type.path}");
        }

        view.Init(this);
        m_dicView[type] = view;
    }

    public bool IsLoadedView(ViewInfo type)
    {
        return m_dicView.ContainsKey(type);
    }

    public bool IsOpenView(ViewInfo type)
    {
        if(!m_dicView.TryGetValue(type, out var view))
        {
            return false;
        }

        foreach(var x in m_stackView)
        {
            if(x == view)
            {
                return true;
            }
        }

        return false;
    }
}
