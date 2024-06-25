using UnityEngine;
using UnityEngine.UI;

public class LoginViewData : BaseComponentScript
{
    [SerializeField] Text[] arrayContainer;
    [SerializeField] LoginViewDataItem loginViewData;
    [SerializeField] LoginViewDataItem[] loginViewDatas;
    [SerializeField] GameObject[] hides;

    public override void OnEnable()
    {
        base.OnEnable();
        int i = 0;
        foreach(var x in arrayContainer)
        {
            x.text = (i++).ToString();
        }

        loginViewData.img.color = Color.red;

        foreach(var x in loginViewDatas)
        {
            x.img.color = Color.blue;
        }

        foreach(var x in hides)
        {
            x.gameObject.SetActive(false);
        }
    }
}

