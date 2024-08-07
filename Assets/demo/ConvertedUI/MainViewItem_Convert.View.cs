using UnityEngine;
using System.Linq;
public partial class MainViewItem_Convert : BaseComponentScript
{
	public static ViewInfo Info = new ViewInfo(){path = "Mainview"};
	[SerializeField] public UnityEngine.UI.Text _text; 
	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "_text") _text = oneFiledRecord.Object as UnityEngine.UI.Text; 
		}
		return true;
	}
}
