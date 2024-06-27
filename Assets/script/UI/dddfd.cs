using UnityEngine;
public partial class dddfd : BaseComponentScript
{
	[SerializeField] public UnityEngine.RectTransform dsfsdf; 
	[SerializeField] public UnityEngine.Camera ccc; 
	[SerializeField] public UnityEngine.Transform aaa; 
	[SerializeField] public LoginViewDataItem ttttt; 
	[SerializeField] public UnityEngine.RectTransform[] componentArray; 
	[SerializeField] public LoginViewDataItem[] arrayFixture; 
	[SerializeField] public UnityEngine.RectTransform ffdfdfdf; 

	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "dsfsdf") dsfsdf = oneFiledRecord.Object as UnityEngine.RectTransform; 
			if(oneFiledRecord.filedName == "ccc") ccc = oneFiledRecord.Object as UnityEngine.Camera; 
			if(oneFiledRecord.filedName == "aaa") aaa = oneFiledRecord.Object as UnityEngine.Transform; 
			if(oneFiledRecord.filedName == "ttttt") ttttt = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as LoginViewDataItem; 
			if(oneFiledRecord.filedName == "componentArray") componentArray = ((ArrayContainerComponent)oneFiledRecord.Object).components as UnityEngine.RectTransform[]; 
			if(oneFiledRecord.filedName == "arrayFixture") 
			{
				var allCompnents = ((ArrayContainerComponentFixture)oneFiledRecord.Object).components;
				arrayFixture = new LoginViewDataItem[allCompnents.Length];
				for(int i = 0; i < allCompnents.Length; i++) arrayFixture[i] = allCompnents[i].CreateScript() as LoginViewDataItem; 
			}
			if(oneFiledRecord.filedName == "ffdfdfdf") ffdfdfdf = oneFiledRecord.Object as UnityEngine.RectTransform; 
		}
		return true;
	}
}
