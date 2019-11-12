//
//  DummyBrain.cs
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
 *   01/08/2019
 * Made it so that it ignores vertical height of target.
 * 
 ******************************************************************************/

using UnityEngine;
using ReachBeyond.VariableObjects;
using ReachBeyond.Characters.Bodies;

namespace ReachBeyond.Characters.Brains {

	public class DummyBrain : BaseBrainState {

		public Transform target;
		public float maxDist = 2f;

		private float walkingSpeed = 0f;

		protected virtual void Update() {
			IMoveX bodyMoveX = body as IMoveX;
			IMoveY bodyMoveY = body as IMoveY;
			IMoveZ bodyMoveZ = body as IMoveZ;
			ITwist bodyTwist = body as ITwist;
			IJump bodyJump = body as IJump;

			// For the sake of following the target, we need to see if they're
			// distant from us. This is going to be a pretty dumb follow
			// function and doesn't care how high-up they are. Thus, we will
			// project both our position and the target's position onto the
			// same plane. Then any heigh-difference gets ignored.
			//
			// If needed, we could also calculate the height pretty easily.
			// Just take the difference of the actual positions with their
			// respective projected positions, and these are their heights
			// relative to the plane. With this knowledge, we could calculate
			// the height relative to one another.
			Vector3 currentProjectedPos = Vector3.ProjectOnPlane(transform.position, transform.up);
			Vector3 targetProjectedPos = Vector3.ProjectOnPlane(target.position, transform.up);

			if ((targetProjectedPos - currentProjectedPos).sqrMagnitude > maxDist * maxDist) {
					if (bodyMoveZ != null) {
					walkingSpeed = Mathf.Clamp01(walkingSpeed + Time.deltaTime);
					bodyMoveZ.MoveZ(walkingSpeed);
				}

				if (bodyTwist != null) {
					bodyTwist.TwistToward(target.position);
				}
			}
			else {
				walkingSpeed = 0f;
			}
		}

	}

}