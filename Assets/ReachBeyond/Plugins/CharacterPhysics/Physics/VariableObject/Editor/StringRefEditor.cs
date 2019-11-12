using UnityEngine;
using UnityEditor;

namespace ReachBeyond.VariableObjects.Editor {

	[CustomPropertyDrawer(typeof(StringReference))]
	public class StringRefEditor : Base.Editor.RefEditor { }

	[CustomPropertyDrawer(typeof(StringConstReference))]
	public class StringConstRefEditor : Base.Editor.ConstRefEditor { }
}



/* DO NOT REMOVE -- START VARIABLE OBJECT INFO -- DO NOT REMOVE **
{
    "name": "String",
    "type": "string",
    "referability": "Class"
}
** DO NOT REMOVE --  END VARIABLE OBJECT INFO  -- DO NOT REMOVE */
