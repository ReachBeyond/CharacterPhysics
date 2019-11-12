using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReachBeyond.Characters.Bodies {

	#region Basic interfaces 

	public interface IStand {
		/// <summary>
		/// If we're standing, then this returns true. Otherwise, we're airborne or something.
		/// </summary>
		/// <returns><c>true</c> if the body is standing; otherwise, <c>false</c>.</returns>
		bool IsStanding();
	}

	public interface IBumpHead {
		/// <summary>
		/// Determines whether this instance is bumping head. (This is the default description, and it's glorious.)
		/// </summary>
		/// <returns><c>true</c> if this instance is bumping head; otherwise, <c>false</c>.</returns>
		bool IsBumpingHead();
	}

	public interface IJump {
		/// <summary>
		/// Initiate a jump.
		/// </summary>
		void JumpBegin();

		/// <summary>
		/// Ends a jump, usually prematurely. This might get called long after the jump is finished, or maybe never at
		/// all. Because of this, don't depend on this getting called.
		/// </summary>
		void JumpEnd();

		float JumpMaxHeight { get; }
		float JumpPeekDist  { get; }
		float JumpMaxDist   { get; }
	}

	public interface IMoveX {
		/// <summary>
		/// Moves the body in the X direction.
		/// </summary>
		/// <param name="magnitude">Magnitude of the movement; can be between -1 and 1, inclusive.</param>
		void MoveX(float magnitude);
	}

	public interface IMoveY {
		/// <summary>
		/// Moves the body in the Y direction.
		/// </summary>
		/// <param name="magnitude">Magnitude of the movement; can be between -1 and 1, inclusive.</param>
		void MoveY(float magnitude);
	}

	public interface IMoveZ {
		void MoveZ(float magnitude);
	}

	public interface ITwist {
		void Twist(float magnitude);
		void TwistToward(Vector3 lookTarget);
	}

	public interface IMoveOnPathTowards {
		/// <summary>
		/// Move towards some point, using this body's own methods of getting there.
		/// </summary>
		/// <returns>True if we predict that we can reach the location; false otherwise.</returns>
		/// <param name="target">Target position.</param>
		bool MoveOnPathTowards(Vector3 target);

		/// <summary>
		/// Move towards some point, using this body's own methods of getting there.
		/// </summary>
		/// <returns>True if we predict that we can reach the location; false otherwise.</returns>
		/// <param name="target">Target transform.</param>
		bool MoveOnPathTowards(Transform target);
	}

	public interface IHaveDirections {
		Vector3 Forward { get; }
		Vector3 Right   { get; }
		Vector3 Up      { get; }
	}

	#endregion

	#region Complex interfaces

	public interface IPlatformer2D : IStand, IMoveX, IJump
	{ }

	public interface IPlatformer3D : IPlatformer2D, IMoveZ, ITwist
	{ }

	#endregion

} // End namespace
