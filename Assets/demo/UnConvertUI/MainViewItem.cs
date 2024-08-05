using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainViewItem : MonoBehaviour
{
    public Text _text;

    public void OnOpenView(int x)
    {
        _text.text = "KKKK" + x;
    }
}
