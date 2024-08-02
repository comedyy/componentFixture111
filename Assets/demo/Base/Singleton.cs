// Singleton pattern -- Structural example  
using System;
using System.Collections.Generic;

public interface IExitCleanup
{
    void CleanUp();
} 

public static class SingletonCleanUp
{
    public static List<IExitCleanup> allCleanUp = new List<IExitCleanup>();

    public static void CleanUp()
    {
        foreach (var cleanAction in allCleanUp)
        {
            cleanAction.CleanUp();
        }

        allCleanUp.Clear();
    }
}

// "Singleton"
public abstract class Singleton<T> : IExitCleanup where T : Singleton<T>
{
    private static T s_Instance = null;
    public static T Instance
    {
        get
        {
            if(s_Instance == null)
            {
                s_Instance = Activator.CreateInstance(typeof(T), true) as T;
                s_Instance.OnInitSingleton();
                SingletonCleanUp.allCleanUp.Add(s_Instance);
            }
            return s_Instance;
        }
    }

    protected virtual void OnInitSingleton()
    {

    }

    protected virtual void OnDisposeSingleton()
    {
    }

    public void CleanUp()
    {
        OnDisposeSingleton();
        s_Instance = null;
    }
}
