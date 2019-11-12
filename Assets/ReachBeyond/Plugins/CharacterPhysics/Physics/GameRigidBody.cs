//
//  GameRigidBody.cs
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
 *   01/01/2019
 * Pulled AbsoluteCastAll into CoreCast; it's now the abstract function.
 * AbsoluteCastAll is now a proxy, and can be used for attempting optimizations,
 *    like caching.
 * Created AbsoluteMove function, which is now the heart of the Move function.
 *
 *   01/03/2019
 * Created Cast[From] functions, so we aren't locked in to casting from the same
 *    place all of the time.
 * 
/************************************************************   Notes   *******
 *   01/01/2019
 * It appears that caching the results of CoreCast doesn't do much good. Seems
 * like Unity or Mono is caching them for me, or the Unity profiler is wrong.
 * Either way, attempting to cache using a dictionary (which is the most
 * efficient option) is actually a little less efficient most of the time.
 * 
 ******************************************************************************/


using UnityEngine;
using ReachBeyond.EventObjects;
using System.Collections.Generic;

namespace ReachBeyond.GamePhysics {

	abstract public class GameRigidBody : MonoBehaviour {

		[Tooltip("This gets invoked when the object gets crushed.")]
		[SerializeField] EventObjectInvoker crushEvent;

		[Tooltip(
			"Enable this if this object is able to collide with its own layer."
			+ " This is slightly less efficient."
		)]
		[SerializeField] private bool ignoreSelfCollision;


		#region Public properties
		abstract public Vector3 FeetOffset {
			get;
		}

		abstract public Vector3 HeadOffset {
			get;
		}

		abstract public float Radius {
			get;
		}
		#endregion

		#region Caching tools
		/*
		private struct CastParameters {

			public CastParameters(Vector3 dir, float mag) {
				direction = dir;
				magnitude = mag;
			}

			public Vector3 direction;
			public float magnitude;
		}

		private float lastCastTime = -1;
		private Dictionary<CastParameters, RaycastHit[]> castCache;
		*/
		#endregion

		#region Unity events
		virtual protected void Awake() {
			// As a little optimization, we'll set the initial size to 16.
			// This way, no surprise resizes should happen, unless we make
			// more than 16 unique raycasts. Let's hope not!
			//castCache = new Dictionary<CastParameters, RaycastHit[]>(16);
		}
		#endregion


		#region Movement
		/// <summary>
		/// Move with the specified motion. In otherwords, attempt to apply the
		/// given offset to the object. This will only apply the motion as far
		/// as is available, and stops short if there is something in the way.
		/// 
		/// The object is garunteed to only move along the direction of the
		/// motion. It will stop short if ANYTHING is in the way.
		/// </summary>
		/// <returns>The actual motion moved we moved.</returns>
		///
		/// <param name="motion">
		/// Offset from current position. Note that this is relative to our
		/// current rotation.
		/// </param>
		public Vector3 Move(Vector3 motion) {
			Vector3 direction = motion.normalized;
			return direction * Move(direction, motion.magnitude);
		}

		/// <summary>
		/// Move with the specified motion. In otherwords, attempt to apply the
		/// given offset to the object. This will only apply the motion as far
		/// as is available, and stops short if there is something in the way.
		/// 
		/// The object is garunteed to only move along the direction of the
		/// motion. It will stop short if ANYTHING is in the way.
		/// </summary>
		/// <returns>
		/// The magnitude which we moved. Equal to what was passed in if there
		/// were no obstructions.
		/// </returns>
		/// <param name="direction">
		/// Direction to move. Must be normalized, and is relative to current
		/// rotation.
		/// </param>
		/// <param name="magnitude">
		/// Magnitude to move.
		/// </param>
		public float Move(Vector3 direction, float magnitude) {
			return AbsoluteMove(RelativeToAbsoluteRotation() * direction, magnitude);
		}


		/// <summary>
		/// Move with the specified motion. In otherwords, attempt to apply the
		/// given offset to the object. This will only apply the motion as far
		/// as is available, and stops short if there is something in the way.
		/// 
		/// The object is garunteed to only move along the direction of the
		/// motion. It will stop short if ANYTHING is in the way.
		/// 
		/// This takes an absolute direction, and does not move the object
		/// relative to its current rotation.
		/// </summary>
		/// <returns>
		/// The magnitude of the movement. Equal to what was passed in if there
		/// were no obstructions.
		/// </returns>
		/// <param name="direction">
		/// Direction to move in world space. Its magnitude is ignored.
		/// </param>
		/// <param name="magnitude">
		/// Magnitude to move.
		/// </param>
		/// <param name="allOrNothing">
		/// If true, only moves if there are no obstructions at all.
		/// Normally, this will move as far as possible.
		/// </param>
		public float AbsoluteMove(Vector3 direction, float magnitude, bool allOrNothing = false) {

			if(!Mathf.Approximately(direction.sqrMagnitude, 1f)) {
				// Only normalize if it's not already normalized.
				// (Sometimes previously normalized vectors are passed in.)
				direction = direction.normalized;
			}

			float originalMagnitude = magnitude;
			float raycastDist = magnitude;

			RaycastHit[] allHitInfo = AbsoluteCastAll(direction, raycastDist);

			foreach (RaycastHit hitInfo in allHitInfo) {

				if (Mathf.Approximately(hitInfo.distance, 0f)
				   && hitInfo.point == Vector3.zero
				  ) {

					if (crushEvent.HasEvents()) {
						crushEvent.Invoke();
					}
					else {
						Debug.LogError(
							gameObject.name
							+ " is stuck inside an object named "
							+ hitInfo.collider.gameObject.name + '\n'
							+ "No further movement will be made "
							+ "on the object until it's freed."
						);
					}

					magnitude = 0f;
				}
				else {
					// We have to account for negative distances here.
					if (Mathf.Abs(hitInfo.distance) < Mathf.Abs(magnitude)) {
						magnitude = hitInfo.distance;
					}

				}
			}

			if (System.Double.IsNaN(magnitude) || (allOrNothing && magnitude < originalMagnitude)) {
				magnitude = 0f;
			}
			else { 
				transform.position += (direction * magnitude);

				//return AbsoluteToRelativeRotation() * finalMotion;
			}

			return magnitude;
		}



		/// <summary>
		/// This causes us to move in a direction, but allows us to move off
		/// course if we hit something like a wall.
		/// </summary>
		/// 
		/// <param name="initDirection">
		/// Initial heading. Will never travel in a direction implied by
		/// this vector's negative. This is relative to current rotation.
		/// </param>
		/// 
		/// <param name="initMagnitude">
		/// Maximum travel distance.
		/// </param>
		/// 
		/// <param name="clampNormal">
		/// Normal of plane to clamp movement to. If equal to Vector3.zero,
		/// no clamp is used and we are free to move along any direction.
		/// This is relative to current rotation. MUST BE NORMALIZED.
		/// </param>
		/// 
		/// <param name="reduceSlideMagnitude">
		/// If true, the effective magnitude is reduced while pushing against a
		/// wall. The amount reduced is higher if we are travelling "into" the
		/// wall. Otherwise, we travel at full speed.
		/// </param>
		/// 
		/// <param name="skipInitialMovement">
		/// If true, we will NOT attempt to move before pushing up against
		/// the wall. This means that no movement will happen if we are not
		/// obstructed by anything. However, this is necessary if Move has
		/// already been called, and you need to "clean up" the rest of the
		/// unmoved magnitude. Otherwise, keep this false.
		/// </param>
		/// 
		/// <returns>
		/// Returns the magnitude after the move has been completed. If this is
		/// non-zero, we are stuck and are probably not going anywhere.
		/// </returns>
		public float SlipperyMove(
			Vector3 initDirection, float initMagnitude,
			Vector3 clampNormal = default(Vector3),
			bool reduceSlideMagnitude = true,
			bool skipInitialMovement = false
		) {
			// This value is used for deciding how much movement is
			// required to do a new cast. If magnitude changes less than this,
			// we keep using the old cast.
			//
			// With its current value, we can do up to 10 recasts. This is
			// pretty terrible, but it should almost never happen.
			float recastMinDelta = Mathf.Abs(initMagnitude) * 0.1f;

			// First thing's first, try to do a direct move in the target
			// direction. If this succeeds, we can skip all of the complex
			// stuff.
			float remainingMagnitude = initMagnitude;

			if(!skipInitialMovement) {
				remainingMagnitude -= Move(initDirection, initMagnitude);
			}

			// Used to track whether we've moved at all with a given set of
			// casts. If so, we'll move. We have to set this to true so that
			// we can step into the while loop, and it makes logical sense;
			// we have already attempted to move linearly, so we'll have to
			// try doing the slidey thing.
			bool hasMoved = true;

			// i.e. keep trying to move until we are stuck (haven't moved)
			//      or we've moved the full distance requested.
			while(remainingMagnitude > 0f && hasMoved) { // NOTE END CURLY

				// We haven't moved yet for this set of casts.
				hasMoved = false;

				float magnitudeAtCast = remainingMagnitude;
				RaycastHit[] hits = CastAll(initDirection, remainingMagnitude);

				int index = 0;


				// Stop if we run out of things we to cast against or if we
				// move. If we move, it means that we'll need to get a new set
				// of cast hits, since our position has changed.
				while(index < hits.Length && !hasMoved) { // NOTE END CURLY

					// The normal of the cast will be using the world's
					// rotation. Since our movements are according to our
					// rotation, we need to correct for this. The negation
					// causes the triangle maths to work out nicer.
					Vector3 normal =
						AbsoluteToRelativeRotation() * -hits[index].normal;

					// Kill any part of the plane which would drive us up/down.
					// We'll let slope handling deal with that.
					if(clampNormal != Vector3.zero) {
						normal = Vector3.ProjectOnPlane(normal, clampNormal).normalized;
					}

					// From https://forum.unity.com/threads/getting-perpendicular-direction-vector-from-surface-normal.250342/
					Vector3 surfaceParallel =
						initDirection - normal * Vector3.Dot(initDirection, normal);

					if(reduceSlideMagnitude) {
						// Of our triangle, the leftover magnitude is the
						// hypotenuse. The normal of the face we've bumped
						// is the "adjacent" side, while the vector we want to
						// travel along is the "opposite" side.
						remainingMagnitude =
							remainingMagnitude * Mathf.Sin(
								Mathf.Deg2Rad
								* Vector3.Angle(normal, initDirection)
							);
					}

					// Save the movement magnitude that wasn't spent on the
					// movement.
					remainingMagnitude -= Move(surfaceParallel, remainingMagnitude);


					// We have some amount of margine of error here. If we
					// used Mathf.Approximately, it would pick up really tiny
					// changes, which causes huge efficiency hits.
					if(magnitudeAtCast - remainingMagnitude < recastMinDelta) {
						// This cast hit hasn't let us move;
						// try next cast hit, assuming we have one.
						index++;
					}
					else {
						// This cast hit has let us move; we don't need to
						// keep trying cast hits.
						hasMoved = true;
					}
				} // End while(index < hits.Length && !hasMoved)

			} // End while(magnitude > 0f && hasMoved)


			return remainingMagnitude;
		} // End SlipperyMove
		#endregion


		#region Relative/absolute functions
		public Quaternion RelativeToAbsoluteRotation() {
			return transform.rotation;
		}

		public Quaternion AbsoluteToRelativeRotation() {
			return Quaternion.Inverse( RelativeToAbsoluteRotation() );
		}
		#endregion


		#region Relative ray casts
		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is relative to the current rotation. See CastAll if you need
		/// to cast relative to world coordinate system.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in. This is relative to the current
		/// rotation.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <param name="start">
		/// The point to fire the cast from. This is relative from
		/// transform.position, so doing transform.position + Vector3.up would
		/// fire the cast as if the object was one unit higher. This is NOT
		/// necessarily the center.
		/// </param>
		///
		/// <returns>
		/// The info of the nearest hit if something was hit; null otherwise.
		/// </returns>
		public RaycastHit? Cast(Vector3 direction, float distance, Vector3 start) {
			return AbsoluteCast(
				RelativeToAbsoluteRotation() * direction,
				distance,
				start
			);
		}

		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is relative to the current rotation. See CastAll if you need
		/// to cast relative to world coordinate system.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in. This is relative to the current
		/// rotation.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <returns>
		/// The info of the nearest hit if something was hit; null otherwise.
		/// </returns>
		public RaycastHit? Cast(Vector3 direction, float distance) {
			return AbsoluteCast(
				RelativeToAbsoluteRotation() * direction,
				distance,
				transform.position
			);
		}

		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is relative to the current rotation. See CastAll if you need
		/// to cast relative to world coordinate system.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in. This is relative to the current
		/// rotation.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <param name="start">
		/// The point to fire the cast from. This is relative from
		/// transform.position, so doing transform.position + Vector3.up would
		/// fire the cast as if the object was one unit higher. This is NOT
		/// necessarily the center.
		/// </param>
		///
		/// <returns>
		/// The info on all potential collisions. If no collisions are
		/// found, then an empty list is returned. Be wary that, for a
		/// given hit, if it has a distance of zero and a point of zero,
		/// then we have an overlap in our collision.
		/// </returns>
		public RaycastHit[] CastAll(Vector3 direction, float distance, Vector3 start) {
			return AbsoluteCastAll(
				RelativeToAbsoluteRotation() * direction,
				distance,
				start
			);
		}

		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is relative to the current rotation. See CastAll if you need
		/// to cast relative to world coordinate system.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in. This is relative to the current
		/// rotation.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <returns>
		/// The info on all potential collisions. If no collisions are
		/// found, then an empty list is returned. Be wary that, for a
		/// given hit, if it has a distance of zero and a point of zero,
		/// then we have an overlap in our collision.
		/// </returns>
		public RaycastHit[] CastAll(Vector3 direction, float distance) {
			return AbsoluteCastAll(
				RelativeToAbsoluteRotation() * direction,
				distance,
				transform.position
			);
		}

		#endregion


		#region Absolute ray casts

		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is absolute. See Cast if you want to cast
		/// relative to the object's rotation.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <param name="start">
		/// The point to fire the cast from. This is relative from
		/// transform.position, so doing transform.position + Vector3.up would
		/// fire the cast as if the object was one unit higher. This is NOT
		/// necessarily the center.
		/// </param>
		///
		/// <returns>
		/// The info of the nearest hit if something was hit; null otherwise.
		/// </returns>
		public RaycastHit? AbsoluteCast(Vector3 direction, float distance, Vector3 start) {
			RaycastHit[] hits = AbsoluteCastAll(direction, distance, start);

			if (hits.Length > 0) {
				//return hits[0];
				RaycastHit nearestHit = hits[0];

				// This function will get called often, so we need to be
				// very efficient with this.
				int hitCount = hits.Length;

				for (int i = 1; i < hitCount; i++) {
					if (hits[i].distance < nearestHit.distance) {
						nearestHit = hits[i];
					}
				}

				return nearestHit;
			}
			else {
				return null;
			}
		}

		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is absolute. See Cast if you want to cast
		/// relative to the object's rotation.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <returns>
		/// The info of the nearest hit if something was hit; null otherwise.
		/// </returns>
		public RaycastHit? AbsoluteCast(Vector3 direction, float distance) {
			return AbsoluteCast(direction, distance, transform.position);
		}


		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is absolute. See Cast if you want to cast
		/// relative to the object's rotation.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <param name="start">
		/// The point to fire the cast from. This is relative from
		/// transform.position, so doing transform.position + Vector3.up would
		/// fire the cast as if the object was one unit higher. This is NOT
		/// necessarily the center.
		/// </param>
		///
		/// <returns>
		/// The info on all potential collisions. If no collisions are
		/// found, then an empty list is returned. Be wary that, for a
		/// given hit, if it has a distance of zero and a point of zero,
		/// then we have an overlap in our collision.
		/// </returns>
		public RaycastHit[] AbsoluteCastAll(Vector3 direction, float distance, Vector3 start) {

			if(!ignoreSelfCollision) {
				return CoreCast(start, direction, distance);
			}
			else {
				List<RaycastHit> hits = new List<RaycastHit>(
					CoreCast(start, direction, distance)
				);

				hits.RemoveAll((hit) => hit.collider.gameObject == gameObject);

				return hits.ToArray();
			}
		}


		/// <summary>
		/// Project our object in the given direction and distance.
		///
		/// This is absolute. See Cast if you want to cast
		/// relative to the object's rotation.
		/// </summary>
		/// 
		/// <param name="direction">
		/// Direction to fire the cast in.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <returns>
		/// The info on all potential collisions. If no collisions are
		/// found, then an empty list is returned. Be wary that, for a
		/// given hit, if it has a distance of zero and a point of zero,
		/// then we have an overlap in our collision.
		/// </returns>
		public RaycastHit[] AbsoluteCastAll(Vector3 direction, float distance) {
			return AbsoluteCastAll(direction, distance, transform.position);
		}


		#endregion

		#region Protected abstracts

		/// <summary>
		/// Project our object in the given direction and distance.
		/// This returns the info for all possible hits. This is the core behind
		/// any object's collision detection. Overwrite this and add whatever
		/// casts are necessary. GameRigidBody will cache the results, and
		/// this only gets called if the time changes or if the object moved.
		/// </summary>
		///
		/// <param name="start">
		/// Center of the cast. Usually this should be transform.position,
		/// but can be anything depending on what is needed. This is not
		/// necessarily the center, but rather a point as if transform.position
		/// was there. If the collider isn't centered around transform.position,
		/// nor is this start point.
		/// </param>
		///
		/// <param name="direction">
		/// Direction to fire the cast in.
		/// This directoin is relative to the object's current facing.
		/// </param>
		///
		/// <param name="distance">
		/// Distance we want to project out to. This is the distance
		/// which we expect the object's center to travel as a whole.
		/// </param>
		///
		/// <returns>
		/// The info on all potential collisions. If no collisions are
		/// found, then an empty list is returned. Be wary that, for a
		/// given hit, if it has a distance of zero and a point of zero,
		/// then we have an overlap in our collision.
		/// </returns>
		abstract protected RaycastHit[] CoreCast(Vector3 start, Vector3 direction, float distance);

		#endregion



	} // End of namespace
} // End of namespace