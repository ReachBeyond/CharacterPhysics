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

	public class GameRigidBox : GameRigidBody {

		#region Serialized fields
		[SerializeField] LayerMaskConstReference collisionMask;
		[SerializeField] BoxCollider boundingBox;
		/// <summary>
		/// The "skin" thickness. When doing a raycast, we make out
		/// cast bigger by this scale. That way, we have a little extra pad
		/// and avoid going through things.
		///
		/// This will never become a Vector3 because otherwise we couldn't
		/// easily revert that distance. It needs to be a flat scaler.
		/// </summary>
		[SerializeField] protected float skinThickness = 0.05f;

		[Space(10)]
		[SerializeField] BoolConstReference debugCasts;
		[Range(0, 10)]
		[SerializeField] float debugCastLifetime = 0.1f;
		[SerializeField] Color debugCastAttempt  = Color.yellow;
		[SerializeField] Color debugCastMiss     = Color.red;
		[SerializeField] Color debugCastHit      = Color.cyan;
		#endregion


		#region Public properties
		public Vector3 halfBoxSize {
			get { return boundingBox.size * 0.5f; }
		}

		public Vector3 castBoxSize {
			get { return halfBoxSize - Vector3.one * skinThickness; }
		}

		public override float Radius {
			get {
				throw new System.NotImplementedException();
			}
		}

		public override Vector3 HeadOffset {
			get {
				throw new System.NotImplementedException();
			}
		}

		public override Vector3 FeetOffset {
			get {
				throw new System.NotImplementedException();
			}
		}
		#endregion

		override protected RaycastHit[] CoreCast(Vector3 start, Vector3 direction, float distance) {

			// Make sure to travel the full distance, making up for the distance
			// which the skin thickness removed.
			distance += skinThickness;

			RaycastHit[] hitInfoAr;

			hitInfoAr = Physics.BoxCastAll(
				center:       start,                halfExtents:             castBoxSize,
				direction:    direction,            layerMask:               collisionMask.ConstValue,
				orientation:  transform.rotation,   queryTriggerInteraction: QueryTriggerInteraction.Ignore,
				maxDistance:  distance
			);

			for(int i = 0; i < hitInfoAr.Length; i++) {
				// Remove the extra distance that the skinThickness added
				hitInfoAr[i].distance -= skinThickness;
			}


			#if UNITY_EDITOR
			if(debugCasts.ConstValue) {
				ExtDebug.DrawBoxCastBox(
					origin:      start,              halfExtents: castBoxSize,
					orientation: transform.rotation, direction:   direction,
					distance:    distance,           

					duration:    debugCastLifetime,
					color:       (hitInfoAr.Length > 0 ? debugCastAttempt : debugCastMiss)
				);

				foreach(RaycastHit hitInfo in hitInfoAr) {
					ExtDebug.DrawBoxCastOnHit(
						origin:          start,              halfExtents: castBoxSize,
						orientation:     transform.rotation, direction:   direction,
						hitInfoDistance: hitInfo.distance,

						duration:        debugCastLifetime,
						color:           debugCastHit
					);
				}
			}
			#endif

			return hitInfoAr;
		} // End CastAll


	} // End of class
} // End of namespace