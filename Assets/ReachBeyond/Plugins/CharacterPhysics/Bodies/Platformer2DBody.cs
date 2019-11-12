//
//  Platformer2DBody.cs
//
//  Author:
//       Autofire <http://www.reach-beyond.pro/>
//
//  Copyright (c) 2018 ReachBeyond
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using UnityEngine.Assertions;
using ReachBeyond.GamePhysics;

namespace ReachBeyond.Characters.Bodies {

	[RequireComponent(typeof(GameRigidBody))]
	public class Platformer2DBody : PlatformerBody, IPlatformer2D, IHaveDirections {

		[Header("Animation")]
		[SerializeField] Transform rotationTarget;
		[SerializeField] Vector3 rightRotation;
		[SerializeField] Vector3 leftRotation;
		[Tooltip("If true, the reported 'forward' vector is inverted.")]
		[SerializeField] bool invertForward;
		[Tooltip("If true, the reported 'right' vector is inverted.")]
		[SerializeField] bool invertRight;
		[Tooltip("If true, the reported 'up' vector is inverted.")]
		[SerializeField] bool invertUp;

		[Space(10)]
		[SerializeField] Animator animator;
		[SerializeField] string moveFloatName = "Forward";
		[SerializeField] string groundBoolName = "OnGround";
		[SerializeField] string verticalFloatName = "Vertical";
		[SerializeField] string jumpTriggerName = "Jumped";
		[Range(0f, 1f)]
		[SerializeField] float jumpAnimScale = 1f;
		[Tooltip("If true, the will appear to walk even if something stops us from walking.")]
		[SerializeField] bool animateByIntendedSpeed = true;

		#region Properties

		public Vector3 Forward {
			get {
				if(!invertForward) {
					return rotationTarget.forward;
				}
				else {
					return -rotationTarget.forward;
				}
			}
		}

		public Vector3 Right {
			get {
				if(!invertRight) {
					return rotationTarget.right;
				}
				else {
					return -rotationTarget.right;
				}
			}
		}

		public Vector3 Up {
			get {
				if(!invertUp) {
					return rotationTarget.up;
				}
				else {
					return -rotationTarget.up;
				}
			}
		}

		#endregion

		#region Unity events
		override protected void Update() {
			base.Update();			

			if(animator != null) {
				animator.SetFloat(
					verticalFloatName,
					RelativeVelocity.y * jumpAnimScale
				);
				animator.SetBool(groundBoolName, IsStanding());
			}
		}
		#endregion


		#region Interface implenetations
		virtual public void MoveX(float magnitude) {
			Walk(Vector3.right, magnitude, Time.deltaTime);
		}

		override public void JumpBegin() {
			base.JumpBegin();

			if(animator != null) {
				animator.SetTrigger(jumpTriggerName);
			}
		}

		override public void JumpEnd() {
			base.JumpEnd();

			if(animator != null) {
				animator.ResetTrigger(jumpTriggerName);
			}
		}
		#endregion

		#region Misc private/protected functions
		override protected float Walk(Vector3 direction, float magnitude, float timeStep, float maxSpeed = -1) {
			magnitude = base.Walk(direction, magnitude, timeStep, maxSpeed);

			if(animator != null) {
				animator.SetFloat(
					moveFloatName,
					animateByIntendedSpeed
						? Mathf.Abs(magnitude)
						: Mathf.Abs(direction.normalized.x * magnitude) / Time.deltaTime
				);
			}

			if(rotationTarget != null) {
				if(magnitude > 0f) {
					rotationTarget.localRotation = Quaternion.Euler(rightRotation);
				}
				else if(magnitude < 0f) {   // Yes we make this check too; otherwise we'd flip when we hold still.
					rotationTarget.localRotation = Quaternion.Euler(leftRotation);
				}
			}

			return magnitude;
		}
		#endregion

	}

}