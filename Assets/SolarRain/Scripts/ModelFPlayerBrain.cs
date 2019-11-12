using UnityEngine;
using ReachBeyond.VariableObjects;
using ReachBeyond.Characters.Brains.Players;

namespace ModelF {
	[RequireComponent(typeof(ModelFBody))]
	public class ModelFPlayerBrain : NoMousePlayerBrainState {

		[Space(5)]
		[SerializeField] private StringConstReference fireLeftButtonName;
		[SerializeField] private StringConstReference fireRightButtonName;

		protected override void Awake() {
			base.Awake();

			UnityEngine.Assertions.Assert.IsNotNull(body as ModelFBody);
		}

		protected override void Update() {
			base.Update();

			Cursor.lockState = CursorLockMode.Locked;

			ModelFBody fBody = body as ModelFBody;

			if (Input.GetButtonDown(fireLeftButtonName)) {
				fBody.FireBegin(Gun.Left);
			}
			if (Input.GetButton(fireLeftButtonName)) {
				fBody.FireHold(Gun.Left);
			}
			if (Input.GetButtonUp(fireLeftButtonName)) {
				fBody.FireEnd(Gun.Left);
			}

			if (Input.GetButtonDown(fireRightButtonName)) {
				fBody.FireBegin(Gun.Right);
			}
			if (Input.GetButton(fireRightButtonName)) {
				fBody.FireHold(Gun.Right);
			}
			if (Input.GetButtonUp(fireRightButtonName)) {
				fBody.FireEnd(Gun.Right);
			}
		}
	}

}