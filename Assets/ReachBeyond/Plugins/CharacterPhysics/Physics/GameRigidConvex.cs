//
//  GameRigidConvex.cs
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReachBeyond.VariableObjects;

namespace ReachBeyond.GamePhysics {
	public class GameRigidConvex : GameRigidBody {

		#region Serialized fields
		[SerializeField] Rigidbody sweepSource;

		[SerializeField] protected float skinThickness = 0.05f;
		[SerializeField] protected BoolConstReference debugCasts;
		#endregion

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

		override protected RaycastHit[] CoreCast(Vector3 start, Vector3 direction, float distance) {

			RaycastHit[] hitInfoAr;

			// Make sure to travel the full distance, making up for the distance
			// which the skin thickness removed.
			distance += skinThickness;

			hitInfoAr = sweepSource.SweepTestAll(direction, distance);

			for(int i = 0; i < hitInfoAr.Length; i++) {
				// Remove the extra distance that the skinThickness added
				hitInfoAr[i].distance -= skinThickness;
			}

			if(debugCasts) {
				string debugMessage = "All hit info at " + Time.time + ":\n";

				foreach(RaycastHit hitInfo in hitInfoAr) {
					debugMessage += " " + hitInfo.distance + "\n";
				}

				Debug.Log(debugMessage);
			}

			return hitInfoAr;
		}

	} // End class
} // End namespace
