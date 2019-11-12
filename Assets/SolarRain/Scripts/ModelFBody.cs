using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReachBeyond.Characters.Bodies;
using ModelF.Behaviours;
using ModelF.Behaviours.Interfaces;

namespace ModelF {

	public enum Gun {
		Left, Right
	}

	public class ModelFBody : Platformer3DBody {

		[Space(5)]
		[Header("Model F: Setup")]
		[SerializeField] private Transform leftFirePoint;
		[SerializeField] private Transform rightFirePoint;

		[Header("Model F: Shadow")]
		[SerializeField] private Transform shadowCastObject;
		[SerializeField] private LayerMask shadowCastMask;
		[SerializeField] private float shadowCastObjectSpeed;


		public Vector3 leftFirePos {
			get { return leftFirePoint.position; }
		}
		public Vector3 rightFirePos {
			get { return rightFirePoint.position; }
		}

		public Vector3 GetFirePos(Gun target) {
			switch(target) {
				case Gun.Left:
			        return leftFirePos;
			    case Gun.Right:
			        return rightFirePos;
				default:
					return transform.position;
			}
		}

		#region Behaviours
		[SerializeField] private BaseFireBehaviour fireBehaviour;
		#endregion


		protected override void Awake() {
			base.Awake();

			//fireBehaviour = new DummyFire(this);
			fireBehaviour.Prepare(this);
		}

		protected override void Update() {
			base.Update();


			if (IsStanding()) {
				Reload();
			}

		}

		public virtual void Reload() {
			fireBehaviour.Reload();
		}

		public virtual void FireBegin(Gun firedGun) {

			if (IsStanding()) {
				fireBehaviour.FireBegin(firedGun);
			}
			else {
				fireBehaviour.AirborneFireBegin(firedGun);
			}
		}

		public virtual void FireHold(Gun firedGun) {

			if (IsStanding()) {
				fireBehaviour.FireHold(firedGun);
			}
			else {
				fireBehaviour.AirborneFireHold(firedGun);
			}
		}

		public virtual void FireEnd(Gun firedGun) {
			if(IsStanding()) {
				fireBehaviour.FireEnd(firedGun);
			}
			else {
				fireBehaviour.AirborneFireEnd(firedGun);
			}
		}


		protected override void LateUpdate() {
			base.LateUpdate();

			Ray shadowRay = new Ray(transform.position, -transform.up);
			RaycastHit hitInfo;

			if (Physics.Raycast(shadowRay, out hitInfo, Mathf.Infinity, shadowCastMask.value)) {
				//Debug.Log("Updating shadow point " + hitInfo.point.ToString());
				//shadowCastObject.position = hitInfo.point;
				shadowCastObject.position = Vector3.MoveTowards(
					shadowCastObject.position,
					hitInfo.point,
					shadowCastObjectSpeed * Time.deltaTime
				);
			}
		}
	}

}