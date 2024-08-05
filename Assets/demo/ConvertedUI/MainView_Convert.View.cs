using UnityEngine;
using System.Linq;
public partial class MainView_Convert : BaseComponentScript
{
	public static ViewInfo Info = new ViewInfo(){path = "Mainview"};
	[SerializeField] public UnityEngine.UI.Button button1; 
	[SerializeField] public UnityEngine.UI.Button button2; 
	[SerializeField] public UnityEngine.UI.Text _text; 
	[SerializeField] public MainViewItem_Convert mainViewItem; 
	[SerializeField] public MainViewItem_Convert[] mainViewItemList; 
	[SerializeField] public UnityEngine.GameObject[] allObjects; 
	[SerializeField] public UnityEngine.UI.Text[] texts; 
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
				mainViewItemList = ((ArrayContainerComponentFixture)oneFiledRecord.Object).components.Select(m=>m.CreateScript() as MainViewItem_Convert).ToArray();
			}
			if(oneFiledRecord.filedName == "allObjects") allObjects = ((ArrayContainerGameObject)oneFiledRecord.Object).gameObjects; 
			if(oneFiledRecord.filedName == "texts") texts = ((ArrayContainerComponent)oneFiledRecord.Object).components.Select(m=>m as UnityEngine.UI.Text).ToArray(); 
		}
		return true;
	}
}
