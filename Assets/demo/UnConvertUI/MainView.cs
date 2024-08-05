using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainView : BaseView
{
    public Button button1;
    public Button button2;
    public Text _text;

    public MainViewItem mainViewItem;
    public MainViewItem[] mainViewItemList;

    protected override void OnOpenView(object param)
    {
        base.OnOpenView(param);
        _text.text = "ABCDEFG";


        mainViewItem.OnOpenView(12121231);

        for (int i = 0; i < mainViewItemList.Length; i++)
        {
            mainViewItemList[i].OnOpenView(i);
        }
    }
}
