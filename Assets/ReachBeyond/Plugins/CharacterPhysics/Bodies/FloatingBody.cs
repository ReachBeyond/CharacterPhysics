using UnityEngine;
using UnityEngine.Assertions;
using ReachBeyond.VariableObjects;

namespace ReachBeyond.Characters.Bodies {

	public class FloatingBody : BaseBody, IMoveX, IMoveY {

		[SerializeField] GamePhysics.GameRigidBody gBody;

		[Space(10)]
		[SerializeField] FloatConstReference dampingScale;
		[SerializeField] FloatConstReference moveSpeedX;
		[SerializeField] FloatConstReference moveSpeedY;


		#region Unity events

		override protected void Awake() {
			base.Awake();

			Assert.IsNotNull(gBody);
		}

		override protected void OnEnable() {
			//gBody.AbsoluteVelocity = Vector2.zero;
		}

		virtual protected void Update() {
			if(!Mathf.Approximately(dampingScale, 0f)) {
				/*gBody.AbsoluteVelocity = new Vector3(
					Mathf.MoveTowards(gBody.AbsoluteVelocity.x, 0, dampingScale * Time.deltaTime),
					Mathf.MoveTowards(gBody.AbsoluteVelocity.y, 0, dampingScale * Time.deltaTime),
					Mathf.MoveTowards(gBody.AbsoluteVelocity.z, 0, dampingScale * Time.deltaTime)
				);*/
				Debug.Log("This doesn't do anything in Update right now");
			}

			//Debug.Log(gBody.AbsoluteVelocity);
		}

		#endregion

		#region IMoveX implementation

		public void MoveX (float magnitude) {
			//gBody.velocity = gBody.velocity + (Vector3.right * magnitude * moveSpeedX.constValue);
			/*
			gBody.AbsoluteVelocity = new Vector3(
				magnitude * moveSpeedX,
				gBody.AbsoluteVelocity.y,
				gBody.AbsoluteVelocity.z
			);
			*/

		}

		#endregion


		#region IMoveY implementation

		public void MoveY (float magnitude) {
			//gBody.velocity = gBody.velocity + (Vector3.up * magnitude * moveSpeedY.constValue);
			/*
			gBody.AbsoluteVelocity = new Vector3(
				gBody.AbsoluteVelocity.x,
				magnitude * moveSpeedY,
				gBody.AbsoluteVelocity.z
			);
			*/
		}

		#endregion



	}
}
