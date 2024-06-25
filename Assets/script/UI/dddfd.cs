using UnityEngine;
public partial class dddfd : BaseComponentScript{ 
[SerializeField] UnityEngine.Transform dsfsdf; 
[SerializeField] UnityEngine.Camera ccc; 
[SerializeField] UnityEngine.Transform aaa; 
[SerializeField] LoginViewDataItem ttttt; 
[SerializeField] UnityEngine.Component zzz; 
protected override bool SetByCodeGen(OneFiledRecord[] oneFiledRecords){ 
foreach(var oneFiledRecord in oneFiledRecords) {
if(oneFiledRecord.filedName == "dsfsdf") dsfsdf = oneFiledRecord.Object as UnityEngine.Transform; 
if(oneFiledRecord.filedName == "ccc") ccc = oneFiledRecord.Object as UnityEngine.Camera; 
if(oneFiledRecord.filedName == "aaa") aaa = oneFiledRecord.Object as UnityEngine.Transform; 
if(oneFiledRecord.filedName == "ttttt") ttttt = ((ComponentFixture1)oneFiledRecord.Object).CreateScript() as LoginViewDataItem; 
if(oneFiledRecord.filedName == "zzz") zzz = oneFiledRecord.Object as UnityEngine.Component; 
}
return true;
}

 public override void OnEnable()
{
    Debug.LogError(aaa);
    Debug.LogError(zzz);
    Debug.LogError(ttttt.img);
    Debug.LogError(ccc);
    Debug.LogError(dsfsdf);
}
}
