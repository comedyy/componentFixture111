using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainView : BaseView
{
    public Button button1;
    public Button button2;
    public Text _text;

    protected override void OnOpenView(object param)
    {
        base.OnOpenView(param);
        _text.text = "ABCDEFG";
    }
}
