using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReachBeyond.GamePhysics;

public class MoveGameRigidBody : MonoBehaviour {

	[SerializeField] private GameRigidBody rb;

	public void Update() {
		rb.Move(new Vector3(0, -0.1f, 0));
	}

}
