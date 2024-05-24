using UnityEngine;
using UnityEngine.UI;

public class LoginViewData : BaseComponentScript
{
    [SerializeField] Text[] arrayContainer;
    [SerializeField] LoginViewDataItem loginViewData;

    public override void OnEnable()
    {
        base.OnEnable();
        int i = 0;
        foreach(var x in arrayContainer)
        {
            x.text = (i++).ToString();
        }

        loginViewData.img.color = Color.red;
    }
}

public class LoginViewDataItem : BaseViewItem
{
    [SerializeField]public  Image img;
}