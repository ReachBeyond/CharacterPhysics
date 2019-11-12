using UnityEngine;

namespace ReachBeyond.VariableObjects {

	[CreateAssetMenu(menuName="Variable/String")]
	public class StringVariable : Base.ClassVariable<string> {}

	[System.Serializable]
	public class StringReference : Base.Reference<string, StringVariable> {}

	[System.Serializable]
	public class StringConstReference : Base.ConstReference<string, StringVariable> {}

}



/* DO NOT REMOVE -- START VARIABLE OBJECT INFO -- DO NOT REMOVE **
{
    "name": "String",
    "type": "string",
    "referability": "Class"
}
** DO NOT REMOVE --  END VARIABLE OBJECT INFO  -- DO NOT REMOVE */