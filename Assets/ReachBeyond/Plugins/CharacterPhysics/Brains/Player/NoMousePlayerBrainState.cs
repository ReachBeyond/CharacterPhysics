using UnityEngine;
using ReachBeyond.VariableObjects;
using ReachBeyond.Characters.Bodies;

namespace ReachBeyond.Characters.Brains.Players {

	public class NoMousePlayerBrainState : BaseBrainState {

		[SerializeField] private StringConstReference xAxisName;
		[SerializeField] private StringConstReference yAxisName;
		[SerializeField] private StringConstReference zAxisName;
		[SerializeField] private StringConstReference twistAxisName;
		[SerializeField] private StringConstReference jumpButtonName;

		[Tooltip("This is the degrees per second if a value of 1 or -1 is given")]
		[SerializeField] private FloatConstReference maxTwistSpeed;

		protected virtual void Update() {
			IMoveX bodyMoveX = body as IMoveX;
			IMoveY bodyMoveY = body as IMoveY;
			IMoveZ bodyMoveZ = body as IMoveZ;
			ITwist bodyTwist = body as ITwist;
			IJump bodyJump = body as IJump;

			if(bodyJump != null) {
				if(Input.GetButtonDown(jumpButtonName)) {
					bodyJump.JumpBegin();
				}
				else if(Input.GetButtonUp(jumpButtonName)) {
					bodyJump.JumpEnd();
				}
			}

			if(bodyMoveX != null) {
				bodyMoveX.MoveX(Input.GetAxis(xAxisName));
			}

			if(bodyMoveY != null) {
				bodyMoveY.MoveY(Input.GetAxis(yAxisName));
			}

			if(bodyMoveZ != null) {
				if(Input.GetAxis(zAxisName) < 0f) {
					int x = 1;
					x += 1;
				}

				bodyMoveZ.MoveZ(Input.GetAxis(zAxisName));
			}

			if(bodyTwist != null) {
				bodyTwist.Twist(Input.GetAxis(twistAxisName) * Time.deltaTime * maxTwistSpeed);
			}
		}

	}

}