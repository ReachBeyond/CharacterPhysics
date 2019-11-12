using UnityEngine;

namespace ReachBeyond.VariableObjects {

	[CreateAssetMenu(menuName="Variable/LayerMask")]
	public class LayerMaskVariable : Base.StructVariable<LayerMask> {}

	[System.Serializable]
	public class LayerMaskReference : Base.Reference<LayerMask, LayerMaskVariable> {}

	[System.Serializable]
	public class LayerMaskConstReference : Base.ConstReference<LayerMask, LayerMaskVariable> {}

}



/* DO NOT REMOVE -- START VARIABLE OBJECT INFO -- DO NOT REMOVE **
{
    "name": "LayerMask",
    "type": "LayerMask",
    "referability": "Struct"
}
** DO NOT REMOVE --  END VARIABLE OBJECT INFO  -- DO NOT REMOVE */