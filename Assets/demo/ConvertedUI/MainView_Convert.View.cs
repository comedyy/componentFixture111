using UnityEngine;
public partial class MainView_Convert : BaseComponentScript
{
	[SerializeField] public UnityEngine.UI.Button button1; 
	[SerializeField] public UnityEngine.UI.Button button2; 
	[SerializeField] public UnityEngine.UI.Text _text; 
	[SerializeField] public MainViewItem_Convert mainViewItem; 
	[SerializeField] public MainViewItem_Convert[] mainViewItemList; 
	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "button1") button1 = oneFiledRecord.Object as UnityEngine.UI.Button; 
			if(oneFiledRecord.filedName == "button2") button2 = oneFiledRecord.Object as UnityEngine.UI.Button; 
			if(oneFiledRecord.filedName == "_text") _text = oneFiledRecord.Object as UnityEngine.UI.Text; 
			if(oneFiledRecord.filedName == "mainViewItem") mainViewItem = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as MainViewItem_Convert; 
			if(oneFiledRecord.filedName == "mainViewItemList") 
			{
				var allCompnents = ((ArrayContainerComponentFixture)oneFiledRecord.Object).components;
				mainViewItemList = new MainViewItem_Convert[allCompnents.Length];
				for(int i = 0; i < allCompnents.Length; i++) mainViewItemList[i] = allCompnents[i].CreateScript() as MainViewItem_Convert; 
			}
		}
		return true;
	}
}
