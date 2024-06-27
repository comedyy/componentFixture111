using UnityEngine;
public partial class LoginViewDataItem : BaseComponentScript
{
	[SerializeField] public UnityEngine.UI.Image img; 
	[SerializeField] public UnityEngine.EventSystems.EventSystem d111; 
	[SerializeField] public UnityEngine.Transform sssdf; 

	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "img") img = oneFiledRecord.Object as UnityEngine.UI.Image; 
			if(oneFiledRecord.filedName == "d111") d111 = oneFiledRecord.Object as UnityEngine.EventSystems.EventSystem; 
			if(oneFiledRecord.filedName == "sssdf") sssdf = oneFiledRecord.Object as UnityEngine.Transform; 
		}
		return true;
	}
}
