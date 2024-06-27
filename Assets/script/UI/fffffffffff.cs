using UnityEngine;
public partial class fffffffffff : BaseComponentScript
{
	[SerializeField] public UnityEngine.GameObject[] ddddddddddd; 
	[SerializeField] public UnityEngine.GameObject tttt; 

	protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords)
	{
		foreach(var oneFiledRecord in oneFiledRecords)
		{
			if(oneFiledRecord.filedName == "ddddddddddd") ddddddddddd = ((ArrayContainerMono)(oneFiledRecord.Object)).gameObjects; 
			if(oneFiledRecord.filedName == "tttt") tttt = oneFiledRecord.Object as UnityEngine.GameObject; 
		}
		return true;
	}
}
