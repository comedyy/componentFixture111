using UnityEngine;
public partial class dddfd : BaseComponentScript
{
	[SerializeField] UnityEngine.RectTransform dsfsdf; 
	[SerializeField] UnityEngine.Camera ccc; 
	[SerializeField] UnityEngine.Transform aaa; 
	[SerializeField] LoginViewDataItem ttttt; 
	[SerializeField] UnityEngine.Transform zzz; 
	[SerializeField] UnityEngine.Light ffff; 
	[SerializeField] LoginViewDataItem fgfg; 

	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "dsfsdf") dsfsdf = oneFiledRecord.Object as UnityEngine.RectTransform; 
			if(oneFiledRecord.filedName == "ccc") ccc = oneFiledRecord.Object as UnityEngine.Camera; 
			if(oneFiledRecord.filedName == "aaa") aaa = oneFiledRecord.Object as UnityEngine.Transform; 
			if(oneFiledRecord.filedName == "ttttt") ttttt = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as LoginViewDataItem; 
			if(oneFiledRecord.filedName == "zzz") zzz = oneFiledRecord.Object as UnityEngine.Transform; 
			if(oneFiledRecord.filedName == "ffff") ffff = oneFiledRecord.Object as UnityEngine.Light; 
			if(oneFiledRecord.filedName == "fgfg") fgfg = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as LoginViewDataItem; 
		}
		return true;
	}
}
