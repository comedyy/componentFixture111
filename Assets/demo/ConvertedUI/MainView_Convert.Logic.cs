using UnityEngine;
public partial class MainView_Convert : BaseComponentScript
{
    public override void OnEnable()
    {
        foreach(var x in allObjects)
        {
            x.SetActive(false);
        }

        foreach(var x in texts)
        {
            x.color = new Color(1, 1, 1, 0.2f);
        }

        for(int i = 0; i < mainViewItemList.Length; i++)
        {
            mainViewItemList[i]._text.text = i.ToString();
        }
    }
}
