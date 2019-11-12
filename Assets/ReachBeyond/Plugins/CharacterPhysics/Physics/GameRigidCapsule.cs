//
//  GameRigidBox.cs
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
using ReachBeyond.VariableObjects;

namespace ReachBeyond.GamePhysics {

	public class GameRigidCapsule : GameRigidBody {

		#region Serialized fields
		[SerializeField] LayerMaskConstReference collisionMask;
		[SerializeField] CapsuleCollider boundingCapsule;
		/// <summary>
		/// The "skin" thickness. When doing a raycast, we make out
		/// cast bigger by this scale. That way, we have a little extra pad
		/// and avoid going through things.
		///
		/// This will never become a Vector3 because otherwise we couldn't
		/// easily revert that distance. It needs to be a flat scaler.
		/// </summary>
		[SerializeField] protected float skinThickness = 0.05f;

		/*
		[Space(10)]
		[SerializeField] BoolConstReference debugCasts;
		[Range(0, 10)]
		[SerializeField] float debugCastLifetime = 0.1f;
		[SerializeField] Color debugCastAttempt  = Color.yellow;
		[SerializeField] Color debugCastMiss     = Color.red;
		[SerializeField] Color debugCastHit      = Color.cyan;
*/
		#endregion


		#region Public properties
		//public Vector3 halfBoxSize {
		//	get { return boundingCapsule.size * 0.5f; }
		//}

		//public Vector3 castBoxSize {
		//	get { return halfBoxSize - Vector3.one * skinThickness; }
		//}


		public override Vector3 FeetOffset {
			get {
				float halfHeight = (boundingCapsule.height * 0.5f);

				return boundingCapsule.center - CapsuleDirection * halfHeight;
			}
		}

		public override Vector3 HeadOffset {
			get {
				float halfHeight = (boundingCapsule.height * 0.5f);

				return boundingCapsule.center + CapsuleDirection * halfHeight;
			}
		}

		public override float Radius {
			get {
				return boundingCapsule.radius;
			}
		}

		/// <summary>
		/// Gets the distance of a point from the capsule's center.
		/// </summary>
		/// <value>The point dinstance.</value>
		public float PointDistance {
			get { return boundingCapsule.height - boundingCapsule.radius; }
		}

		public Vector3 CapsuleDirection {
			get {
				switch(boundingCapsule.direction) {
					case 0:
						// X direction
						return transform.right;
					case 1:
						// Y direction
						return transform.up;
					case 2:
						// Z direction
						return transform.forward;
					default:
						return transform.up;
				}
			}
		}
		#endregion

		override protected RaycastHit[] CoreCast(Vector3 start, Vector3 direction, float distance) {

			//Debug.Log(Time.time.ToString() + " (CoreCast)");

			bool wasNegative = false;;

			if(distance < 0f) {
				direction *= -1f;
				distance *= -1f;
				wasNegative = true;
			}

			// Make sure to travel the full distance, making up for the distance
			// which the skin thickness removed.
			distance += skinThickness; //+ boundingCapsule.radius;

			RaycastHit[] hitInfoAr;

			Vector3 center = start + boundingCapsule.center;
			float pointDistance = -boundingCapsule.height * 0.5f + boundingCapsule.radius;

			// Note that this is the point where we take into account the
			// capsule's current rotation; these two points come right out of
			// the capsule itself.
			Vector3 p1 = center + CapsuleDirection * pointDistance;
			Vector3 p2 = center + CapsuleDirection * -pointDistance;

			hitInfoAr = Physics.CapsuleCastAll(
				p1, p2, boundingCapsule.radius,
				direction, distance,
				collisionMask.ConstValue,
				QueryTriggerInteraction.Ignore
			);

			for(int i = 0; i < hitInfoAr.Length; i++) {
				// Remove the extra distance that the skinThickness added. This
				// is necessary because our object's skin thickness is... man,
				// I don't remember. I should probably experiment with this.
				hitInfoAr[i].distance = Mathf.Max(
					hitInfoAr[i].distance - skinThickness,
					0f
				);// - boundingCapsule.radius;

				if(wasNegative) {
					hitInfoAr[i].distance *= -1;
				}
			}

			//Debug.Log(hitInfoAr.Length + " hit(s) in direction " + direction);

			return hitInfoAr;
		} // End CastAll


	} // End of class
} // End of namespace