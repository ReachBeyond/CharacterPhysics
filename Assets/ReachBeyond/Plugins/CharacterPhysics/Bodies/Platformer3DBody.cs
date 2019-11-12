//
//  Platformer3DBody.cs
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

/************************************************************ Changelog *******
 *   01/02/2019
 * Rather than having a system for moving on the ground and a system for moving
 *     in the air, we now just use RelativeVelocity variable. This vastly
 *     simplifies things like walking off edges.
 * Stopped using base.Walk; It doesn't do what we need for applying velocities.
 * Added air movement and horizontal drag.
 *
 *   01/05/2019
 * Added the stair-climbing stuff.
 * Removed Range attributes from many of the fields. (They were unnecessary.)
 * Made it so that slipping can't get you stuck.
 * Added a check for slope steepness when attempting to climb stairs.
 *
 *   01/08/2019
 * Fixed TwistToward so that it respects positive/negative angles.
 * Changed Twist so that it takes an angle instead of a magnitude.
 * Set the defaults to be in line with the current prefab config.
 * 
 ******************************************************************************/


using UnityEngine;

namespace ReachBeyond.Characters.Bodies {
	public class Platformer3DBody : PlatformerBody, IPlatformer3D {

		#region Serialized fields

		[Header("3D Body setup")]
		[Tooltip(
			"Transform of the renderer. Used mainly for rotation stuff."
		)]
		[SerializeField] private Transform rendererTransform;


		[Header("Airborne Parameters")]

		[Tooltip("Used for moving while airborne.")]
		[SerializeField] private float airAcceleration = 6f;

		[SerializeField] private float airHorizDrag = 0.8f;


		[Header("Edges")]

		[Tooltip(
			"This is used for 'edge slipping,' which stops us from getting "
			+ "stuck on edges because of our round underside. If this is too "
			+ "short, it will not trigger. If it is too long, we won't find "
			+ "anything and we'll get false positives."
		)]
		[SerializeField] private float edgeSlipCastDist = 0.9f;

		[Tooltip(
			"This increases the slip amount by a fixed amount. "
			+ "The algorithm needs a little 'help' to get things lined up."
		)]
		[SerializeField] private float edgeOverslip = 0.05f;

		[SerializeField] private bool debugSlip = false;


		[Header("Misc")]

		[SerializeField] private float maxStepHeight = 0.25f;

		[Tooltip(
			"If true, the player's character's speed is reduced while walking "
			+ "against a wall, depending on how much their motion is parallel "
			+ "to the wall. If this is off, the character will move at top "
			+ "speed when going against the wall, but will still move parallel "
			+ "to it."
		)]
		[SerializeField] private bool reduceParallelWallSpeed = true;

		[Tooltip(
			"We can only climb slopes that are this steep relative to our "
			+ "current orientation."
		)]
		[Range(0f, 90f)]
		[SerializeField] private float maxSlopeAngle = 45f;

		[Tooltip(
			"In degrees per seconds. "
			+ "This is the max speed which the renderer can turn."
		)]
		[SerializeField] private float rendererMaxTwistSpeed = 520f;

		[SerializeField] private float horizontalGroundDrag;

		#endregion

		#region Private fields
		private Vector2 targetMovement = Vector2.zero;
		private bool wasStanding = true;

		/// <summary>
		/// This is the distance FROM THE BOTTOM to fire the raycast to check
		/// if we should slip. If actually firing the ray, from the bottom,
		/// you'll want to add in gBody.FeetOffset.magnitude.
		/// </summary>
		/// <value>The slip cast dist.</value>
		//private float SlipCastDist {
		//	get { return footBoxDepth * edgeSlipCastScale; }
		//}
		#endregion
		//private float targetTwist = 0f;


		virtual public void MoveX(float magnitude) {
			targetMovement[0] = magnitude;
		}

		virtual public void MoveZ(float magnitude) {
			targetMovement[1] = magnitude;
		}

		virtual public void Twist(float angle) {
			//Transform targetTransform =
			//	(facingTransform != null ? facingTransform : transform);

			/*
			magnitude = Mathf.Clamp(magnitude, -1f, 1f);

			if(Mathf.Abs(magnitude) > 0f) {

				Quaternion oldRot = rendererTransform.rotation;

				// Yes this is local rotation. No, I don't really know why
				// this is local. Stop asking questions.
				transform.Rotate(
					0f,
					magnitude * twistSpeed * Time.deltaTime,
					0f
				);

				rendererTransform.rotation = oldRot;
			}
*/
			if(!Mathf.Approximately(angle, 0f)) {
				Quaternion oldRot = rendererTransform.rotation;

				transform.Rotate(transform.up, angle);

				rendererTransform.rotation = oldRot;
			}

		}

		virtual public void TwistToward(Vector3 lookTarget) {
			Quaternion oldRot = rendererTransform.rotation;

			Vector3 targetForward = Vector3.ProjectOnPlane(
				lookTarget - transform.position, transform.up
			);

			// Needs to be signed since we can turn left OR right
			float angle = Vector3.SignedAngle(
				transform.forward, targetForward, transform.up
			);

			transform.Rotate(transform.up, angle);

			rendererTransform.rotation = oldRot;
		}

		public override void JumpBegin() {
			base.JumpBegin();
			//attemptedJump = true;
		}


		#region Unity events
		override protected void Awake() {
			base.Awake();

			//airborneVelocity = Vector2.zero;
		}

		override protected void Start() {
			base.Start();

			/*
			if(facingTransform != null) {
				facingTransform.localRotation = Quaternion.identity;
			}
			*/
		}

		override protected void LateUpdate() {

			base.LateUpdate();

			HandleHorizontalMovement();

			HandleEdgeSlip();

			bool isStanding = IsStanding();    // Cache the result

			// We're standing right now, but we were airborne, so we should
			// axe the airborne velocity.
			if (isStanding && !wasStanding) {
				//Debug.Log(Time.time + ": clearing airborne velocity");
				//airborneVelocity = Vector2.zero;
				RelativeVelocity = new Vector3(0f, RelativeVelocity.y, 0f);
			}

			// Since this should hopefully be the last thing which runs on any
			// given frame.
			wasStanding = isStanding;

			// Reset so that we don't continue moving next frame.
			// If the intention is to continue moving into next frame,
			// then MoveX/MoveZ/JumpBegin needs to be called during next Update
			// step.
			targetMovement = Vector2.zero;
			//attemptedJump = false;
		}
		#endregion

		#region Late Update steps

		/// <summary>
		/// Deals with everything that happens when the body is trying to move
		/// along its X (right/left) and Z (forward/backward) directions. Also
		/// handles the jump button.
		///
		/// This movement also handles stairs, slopes, and moving along walls.
		/// </summary>
		private void HandleHorizontalMovement() {

			Vector2 clampedTargetMovement = Vector2.zero;

			if (targetMovement != Vector2.zero) {

				float rawMagnitude = targetMovement.magnitude;
				float clampedMagnitude = Mathf.Clamp01(rawMagnitude);

				// Already know the rawMagnitude can never be zero.
				// This just makes sure that the movement we want to make is
				// not going to the max speed we can actually walk.
				clampedTargetMovement =
					targetMovement * (clampedMagnitude / rawMagnitude);
			}

			if (IsStanding()) {

				if (targetMovement != Vector2.zero) {

					// We need to get the direction we want to go, not the
					// direction we are facing. This way, we can move our
					// renderer appropriately.
					Vector3 targetDir = new Vector3(
						targetMovement[0], 0f, targetMovement[1]
					);
					Quaternion targetRot = Quaternion.LookRotation(
						gBody.RelativeToAbsoluteRotation() * targetDir,
						transform.up
					);

					// This'll move the renderer towards the direction
					// we're walking.
					rendererTransform.rotation = Quaternion.RotateTowards(
						rendererTransform.rotation,
						targetRot,
						rendererMaxTwistSpeed * Time.deltaTime
					);
				}

				RelativeVelocity = new Vector3(
					clampedTargetMovement[0] * walkingSpeed,
					RelativeVelocity.y,
					clampedTargetMovement[1] * walkingSpeed
				);
			}
			else {

				Vector3 curRelVel = RelativeVelocity;
				float drag = Mathf.Clamp01(1 - airHorizDrag * Time.deltaTime);

				// We're airborne so we'll need to apply the airborne speed.
				//Debug.Log("In air");
				RelativeVelocity = new Vector3(
					drag * (curRelVel.x + clampedTargetMovement[0] * airAcceleration * Time.deltaTime),
					curRelVel.y,
					drag * (curRelVel.z + clampedTargetMovement[1] * airAcceleration * Time.deltaTime)
				);

				//Debug.Log("Would use " + curRelVel.ToString() + " but with drag " + RelativeVelocity.ToString());

			}


			// TODO Let's test this in some other games, and see how they
			//      handle this. If you jump into a wall, do you maintain
			//      your speed? Or does it get chopped?
			Vector3 finalRelVel = RelativeVelocity;

			HorizontalMove(
				finalRelVel.x,
				finalRelVel.z,
				Time.deltaTime,
				IsStanding()
			);


		}

		/// <summary>
		/// Causes the body to "slip" off the edge if they are only partially on
		/// on the edge. This only makes sense for rounded game bodies, but
		/// this object should never be working with boxes so it shouldn't
		/// ever be an issue.
		/// </summary>
		private void HandleEdgeSlip() {
			Vector3 relativeDown = -transform.up;

			float maxDist = gBody.FeetOffset.magnitude + edgeSlipCastDist; //SlipCastDist;

			Ray slipCheckRay = new Ray(transform.position, relativeDown);
			RaycastHit[] floorHits = IsStandingOn();

			/*
			foreach(RaycastHit hitInfo in Physics.RaycastAll(slipCheckRay, maxDist)) {
				Debug.Log(hitInfo.collider.name);
			}
			*/

			if(!Physics.Raycast(slipCheckRay, maxDist) && floorHits.Length > 0) {
				foreach(RaycastHit hitInfo in floorHits) {

					Vector3 slipDirection = transform.position - hitInfo.point;
					slipDirection = gBody.AbsoluteToRelativeRotation() * slipDirection;
					slipDirection = Vector3.ProjectOnPlane(slipDirection, Vector3.up);

					float slipMagnitude = gBody.Radius - slipDirection.magnitude + edgeOverslip;

					// Only move if there are no hits over in the direction
					// we want to push. We're extending the check just to
					// add a little extra wiggle room; sometimes, many slips are
					// needed.
					if (!gBody.Cast(slipDirection, slipMagnitude * 1.5f).HasValue) {
						gBody.Move(slipDirection, slipMagnitude);
					}

#if UNITY_EDITOR
					if(debugSlip) {
						Debug.Log(
							gameObject.name + " is slipping\n"
							+ slipDirection.normalized + " * " + slipMagnitude
							+ " = " + slipDirection.normalized * slipMagnitude
						);
					}
#endif
				} // End foreach
			} // End if

		} // End HandleEdgeSlip

		#endregion

		/// <summary>
		/// Moves along the horizontal plane. The magnitude of xMove and zMove
		/// is multiplied against timeStep.
		/// </summary>
		/// 
		/// <param name="xVelocity">
		/// Movement along the X axis. This is relative.
		/// </param>
		///
		/// <param name="zVelocity">
		/// Movement along the Z axis. This is relative.
		/// </param>
		/// 
		/// <param name="timeStep">Time step of the move.</param>
		///
		/// <param name="shouldClimbStairs">
		/// If true, will check if it's possible to climb a stair before
		/// starting to slide.
		/// </param>
		private void HorizontalMove(float xVelocity, float zVelocity, float timeStep, bool shouldClimbStairs = false) {

			Vector3 direction = new Vector3(
				xVelocity, 0f, zVelocity
			);

			// Save the targetMagnitude so we don't keep
			// having to recalculate it. Also we want to multiply/divide
			// by timeStep here.
			float targetMagnitude = direction.magnitude;
			float resultMagnitude = gBody.Move(
				direction, targetMagnitude * timeStep
			) / timeStep;

			float leftoverMagnitude = targetMagnitude - resultMagnitude;

			//if (!Mathf.Approximately(resultMagnitude, targetMagnitude)) {
			if (leftoverMagnitude > 0.001) {

				//bool willSlide = true;

				if (shouldClimbStairs) {
					leftoverMagnitude = ClimbStairs(
						direction.normalized,
						leftoverMagnitude * Time.deltaTime
					) / Time.deltaTime;
				}

				// Don't bother sliding if we've already spent all of the
				// distance on climbing stairs or moving forward. And this check
				// is a little frivolus if shouldClimbStairs is false, but it's
				// easier to do it this way.
				if(leftoverMagnitude > Mathf.Epsilon) {

					gBody.SlipperyMove(
						direction, leftoverMagnitude * timeStep,
						reduceSlideMagnitude: reduceParallelWallSpeed,
						skipInitialMovement: true,
						clampNormal: Vector3.up
					);
				}
			} // End resultMagnitude != targetMagnitude
		} // End HorizontalMove

		/// <summary>
		/// Attempts to climb stairs, if they exist.
		/// </summary>
		/// 
		/// <returns>
		/// The distance yet to move. If 0, moved by magnitude and am done.
		/// If this is equal to magnitude, we didn't move at all.
		/// </returns>
		/// 
		/// <param name="direction">
		/// Relative direction to travel in. For efficiency, this NEEDS to be
		/// normalized but we do not normalize it. Please normalize it!
		/// </param>
		///
		/// <param name="magnitude">
		/// Distance to travel in units.
		/// </param>
		private float ClimbStairs(Vector3 direction, float magnitude) {

			// When we get called, we've just bumped into something. We have no
			// idea what the nature of this thing is, so we'll have to figure
			// out what we can do. Here's our plan of attack:
			// 
			// 1.) Attempt to step up, using the distance of the slip raycast.
			//     (We use this becase this is the highest we can step without
			//     slipping back down on the following frame.)
			//   a.) If we hit nothing, we can step up the highest possible.
			//   b.) If we hit something, we can only go up as far as it would
			//       allow us. This is fine, and we can still continue.
			//   c.) If we hardly are able to move up, abort the process.
			//
			// 2.) Calculate how far we can walk forward.
			//   a.) Ideally, we can go forward by the collider radius.
			//   b.) If not, then delete
			//
			// 3.) Walk forward; as far as the radius of the collider.
			//   a.) If we bump nothing, we can step forward the whole distance.
			//   b.) If we bump something and traveled a non-trivial distance,
			//       it's also fine and we can continue.
			//   c.) If we bump something and hardly traveled at all, then we
			//       need to abort.
			//
			// 4.) Cast down again by the same amount we moved up in step 1.
			//     This distance will be the amount we can step down.
			//   a.) If the surface isn't too steep, continue.
			//   b.) If it is too steep (e.g. we'd slide off), don't climb up.
			// 
			// 5.) Teleport the character to the final position in step 3.
			//     Subtract step forward amount from the magnitude.
			//
			// 6.) Call the move function using the same direction and the
			//     remaining magnitude. (CURRENTLY SKIPPING)
			//   a.) If we hit nothing, we're done.
			//   b.) If we hit something, call this function again with the
			//       new remaining magnitude.
			//
			// TODO We need to worry about slopes. We shouldn't attempt to climb
			//      steep slopes.
			// TODO How to we deal with stepping down?

			const float minStepUpDist = 0.1f;
			//const float minRestepDist = 0.3f;

			Vector3 imaginedPosition = transform.position;

			//string debugMessage = "";

			////////////
			// Step 1 //
			////////////
			float stepUpDist = maxStepHeight;

			// We're using AbsoluteCast and transform.up simply
			// because it's more efficient; no conversions are needed between
			// absolute and relative, nor do we care.
			RaycastHit? hitInfo = gBody.AbsoluteCast(
				transform.up, stepUpDist, imaginedPosition
			);

			if(hitInfo.HasValue) {
				stepUpDist = hitInfo.Value.distance;
			}

			//debugMessage += "Step up dist: " + stepUpDist.ToString();

			if(stepUpDist > minStepUpDist) {
				imaginedPosition += transform.up * stepUpDist;

				////////////
				// Step 2 //
				////////////
				float stepForwardDist = Mathf.Min(gBody.Radius, magnitude);
				//float initStepForwardDist = stepForwardDist;

				////////////
				// Step 3 //
				////////////
				hitInfo = gBody.Cast(
					direction, stepForwardDist, imaginedPosition
				);

				if(hitInfo.HasValue) {
					stepForwardDist = hitInfo.Value.distance;
				}

				//debugMessage += "\nStep forward dist: " + stepForwardDist.ToString();

				if (stepForwardDist > Mathf.Epsilon) {
					imaginedPosition += gBody.RelativeToAbsoluteRotation() * (direction * stepForwardDist);

					////////////
					// Step 4 //
					////////////
					float stepDownDist = stepUpDist;

					hitInfo = gBody.AbsoluteCast(
						-transform.up, stepDownDist, imaginedPosition
					);

					// In case our cast down doesn't hit anything, we'll
					// assume that we're walking to a floor which shares our
					// normal. This way, it is garunteed to have an angle
					// which matches our current angle.
					Vector3 floorNormal = transform.up;

					if(hitInfo.HasValue) {
						stepDownDist = hitInfo.Value.distance;
						floorNormal = hitInfo.Value.normal;
					}
					//debugMessage += "\nStep down dist: " + stepDownDist.ToString();

					imaginedPosition += (-transform.up) * stepDownDist;

					if(Vector3.Angle(transform.up, floorNormal) < maxSlopeAngle) {

						////////////
						// Step 5 //
						////////////
						transform.position = imaginedPosition;
						magnitude -= stepForwardDist;


						////////////
						// Step 6 //
						////////////
						magnitude -= gBody.Move(direction, magnitude);

						// TODO This works as long as our steps aren't too large.
						//      But we need to wrap this in a loop.
						/*
						//if(magnitude > minRestepDist) {
						// If we moved the FULL distance, it means we can continue to walk.
						// If we bumped something but still went forward, well... 
						if (Mathf.Approximately(stepForwardDist, initStepForwardDist)) {
							Debug.Log("Running again");
							magnitude = ClimbStairs(direction, magnitude);
						}
						*/
					}
				}
			}

			//Debug.Log(debugMessage);

			return magnitude;
		}

	} // End class

} // End namespace
