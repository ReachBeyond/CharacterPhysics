//
//  PlatformerBody.cs
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

	/// <summary>
	/// This is a very basic platformer body. It ONLY implements
	/// jumping. Even though it has a "walkingSpeed" variable, this
	/// is only used by this object to determine jumping parameters.
	///
	/// That said, it does have a basic Move function. This makes use
	/// of the walkingSpeed variable, but there are no interfaces which
	/// uses it.
	/// </summary>
	public class PlatformerBody : BaseBody, IStand, IJump {

		[SerializeField] protected GameRigidBody gBody;

		[Space(20)]
		[Tooltip(
			"How far down to check if we are standing. " +
			"If any object is found within the box beneath the object, " +
			"it is considered to be standing and may jump again."
		)]
		[Range(0f, 1f)]
		[SerializeField] protected float footBoxDepth = 0.05f;

		[Space(10)]
		[Tooltip(
			"Walking speed in units per second. " +
			"Keep in mind that this is the same even in the air."
		)]
		[Range(0f, 20f)]
		[SerializeField] protected float walkingSpeed = 6f;

		[Tooltip(
			"How high the character goes at the peek of a full jump. " +
			"Making this zero disables jumps."
		)]
		[Range(0f, 10f)]
		[SerializeField] float _jumpHeight = 3.45f;

		[Tooltip(
			"How far the character goes at the peek of a full jump. " +
			"Making this zero disables jumps. " +
			"Higher values make jumps feel more snappy."
		)]
		[Range(0f, 10f)]
		[SerializeField] float _jumpPeekDist = 4f;

		[Tooltip(
			"This determines how fast the character falls " +
			"either after they fall off an edge or reach the peek of a jump. " +
			"This is effectively a scaler of the 'jump gravity' which gets " +
			"calculated based on the values above."
		)]
		[Range(0f, 100f)]
		[SerializeField] float normalGravityScale = 2f;

		[Space(10)]
		[Range(0f, 0.9999f)]
		[SerializeField] float fallingDrag = 0.85f;


		#region Public properties

		public float JumpMaxHeight {
			get { return _jumpHeight; }
		}

		public float JumpPeekDist {
			get { return _jumpPeekDist; }
		}

		public float JumpMaxDist {
			get {
				Debug.LogWarning(this.GetType().Name + " has not fully implemented jumpMaxDist; using jumpPeekDist instead");
				return _jumpPeekDist - (0);
			}
		}

		/// <summary>
		/// Gets the magnitude of the jump velocity.
		/// </summary>
		/// <value>The jump velocity.</value>
		public float JumpVelocity {
			get {
				// See https://www.youtube.com/watch?v=hG9SzQxaCm8
				return 2 * JumpMaxHeight * walkingSpeed / JumpPeekDist;
			}
		}

		/// <summary>
		/// Gets the magnitude of the jump gravity.
		/// </summary>
		/// <value>The jump gravity.</value>
		public float JumpGravity {
			get {
				// See https://www.youtube.com/watch?v=hG9SzQxaCm8
				return -2 * JumpMaxHeight * walkingSpeed * walkingSpeed
					   / (JumpPeekDist * JumpPeekDist);
			}
		}

		/// <summary>
		/// Gravity which the character experiences normally.
		/// </summary>
		/// <value>The normal gravity.</value>
		public float NormalGravity {
			get { return normalGravityScale * JumpGravity; }
		}

		/// <summary>
		/// While true, jumping properties (such as gravity) is used instead
		/// of standard gravity. This gets set to false whenever the character
		/// begins to fall.
		/// </summary>
		public bool IsJumping { get; set; }

		public float CurrentGravity {
			get {
				if(IsJumping) {
					return JumpGravity;
				}
				else {
					return NormalGravity;
				}
			}
		}

		public float CurrentDrag {
			get {
				if(IsJumping) {
					return 0f;
				}
				else {
					return fallingDrag;
				}
			}
		}


		public Vector3 AbsoluteVelocity {
			get; set;
		}

		public Vector3 RelativeVelocity {
			get {
				return gBody.AbsoluteToRelativeRotation() * AbsoluteVelocity;
			}
			set {
				AbsoluteVelocity = gBody.RelativeToAbsoluteRotation() * value;
			}
		}
		#endregion

		#region Interface implenetations
		virtual public void JumpBegin() {
			if(IsStanding()) {
				// ApplyJumpingGravity();
				IsJumping = true;

				//YVelocity = JumpVelocity;
				RelativeVelocity += Vector3.up * JumpVelocity;
			}
		}

		virtual public void JumpEnd() {
			//ApplyNormalGravity();
			IsJumping = false;
		}


		private const float StandCheckInterval = 0.25f;
		private const float StandForceRefreshDist = 0.2f;

		private Vector3 _posOfLastStandCheck = Vector3.zero;
		private float _timeSinceLastStandCheck = -100;

		private RaycastHit[] _isStandingOn;
		virtual public RaycastHit[] IsStandingOn() {
			if(Time.time > StandCheckInterval + _timeSinceLastStandCheck ||
				Vector3.Distance(_posOfLastStandCheck, transform.position) > StandForceRefreshDist
			) {
				/*Debug.Log(
					"Refreshing\n" +
					"Delta: " + (Time.time - _timeSinceLastStandCheck).ToString() + "\n" +
					"Time:  " + Time.time.ToString()
				);*/

				_isStandingOn = gBody.CastAll(Vector3.up * -1, footBoxDepth);
				_timeSinceLastStandCheck = Time.time;
				_posOfLastStandCheck = transform.position;
			}

			return _isStandingOn;
		}

		virtual public bool IsStanding() {
			return (IsStandingOn().Length > 0);
		}
		#endregion

		#region Unity events
		override protected void Awake() {
			base.Awake();

			Assert.IsNotNull(gBody);

			//ApplyNormalGravity();
			IsJumping = false;
			//YVelocity = 0f;
			AbsoluteVelocity = Vector3.zero;
		}


		override protected void OnEnable() {
			base.OnEnable();

			//ApplyNormalGravity();
			IsJumping = false;
		}

		override protected void OnDisable() {
			base.OnDisable();
		}

		virtual protected void Update() {

			if (RelativeVelocity.y <= 0f && IsJumping) {
				IsJumping = false;
			}
		}

		/// <summary>
		/// Calls HandleGravity and then HandleVelocity
		/// </summary>
		virtual protected void LateUpdate() {

			// TODO Apply drag

			HandleGravity();
			HandleVelocity();
		}

		#endregion

		#region Late update steps
		virtual protected void HandleGravity() {
			// Cache this; RelativeVelocity is a property, so we don't
			// want to call it a bunch of times if we can help it.
			Vector3 currentRelVel = RelativeVelocity;
			float drag = Mathf.Clamp01(1 - CurrentDrag * Time.deltaTime);

			RelativeVelocity = new Vector3(
				currentRelVel.x,
				drag * (currentRelVel.y + CurrentGravity * Time.deltaTime),
				currentRelVel.z
			);

			//Debug.Log("Would use " + (currentRelVel.y + CurrentGravity * Time.deltaTime).ToString() + " but with drag " + RelativeVelocity.y.ToString());
		}

		virtual protected void HandleVelocity() {
			// Cache this; RelativeVelocity is a property, so we don't
			// want to call it a bunch of times if we can help it.
			Vector3 currentRelVel = RelativeVelocity;

			RelativeVelocity = new Vector3(
				currentRelVel.x,
				gBody.Move(Vector3.up, currentRelVel.y * Time.deltaTime) / Time.deltaTime,
				currentRelVel.z
			);
		}
		#endregion


		#region Misc private/protected functions

		[System.Obsolete("Use Walk instead")]
		protected float Move(Vector3 direction, float magnitude) {
			// TODO Maybe mark this as obsolete
			return Walk(direction, magnitude, Time.deltaTime);
		}

		// Cause us to 'walk' in a given direction, using the given timestep.
		// The the direction vector will be normalized, and the given magnitude
		// is used to decide how much of the 'maxSpeed' to move our object.
		// If maxSpeed is not given, walkingSpeed is used instead.
		virtual protected float Walk(Vector3 direction, float magnitude, float timeStep, float maxSpeed = -1f) {

			if(maxSpeed < 0) {
				maxSpeed = walkingSpeed;
			}

			magnitude = Mathf.Clamp(magnitude, -1f, 1f);

			float maxMoveDist = walkingSpeed * timeStep;

			float moveDist = gBody.Move(
				direction, magnitude * maxMoveDist
			);

			return moveDist / maxMoveDist;

		}
		#endregion


	} // End class

} // End namespace
