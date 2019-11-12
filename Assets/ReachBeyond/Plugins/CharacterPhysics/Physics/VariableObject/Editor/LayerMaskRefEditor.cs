using UnityEngine;
using UnityEditor;

namespace ReachBeyond.VariableObjects.Editor {

	[CustomPropertyDrawer(typeof(LayerMaskReference))]
	public class LayerMaskRefEditor : Base.Editor.RefEditor { }

	[CustomPropertyDrawer(typeof(LayerMaskConstReference))]
	public class LayerMaskConstRefEditor : Base.Editor.ConstRefEditor { }
}



/* DO NOT REMOVE -- START VARIABLE OBJECT INFO -- DO NOT REMOVE **
{
    "name": "LayerMask",
    "type": "LayerMask",
    "referability": "Struct"
}
** DO NOT REMOVE --  END VARIABLE OBJECT INFO  -- DO NOT REMOVE */
