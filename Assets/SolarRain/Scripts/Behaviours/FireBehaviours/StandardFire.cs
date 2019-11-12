using UnityEngine;
using ReachBeyond.Characters.Bodies;
using System.Collections.Generic;

namespace ModelF.Behaviours {

	[CreateAssetMenu(fileName = "StandardFire", menuName = "FireBehaviour/Standard")]
	public class StandardFire : BaseFireBehaviour {

		[SerializeField] private float airShotRecoilVelocity = 8f;
		[SerializeField] private GameObject projectilePrefab;

		private bool[] gunChambers;
		private int gunCount;

		public StandardFire(PlatformerBody parent) {
		}

		public override void Prepare(ModelFBody parent) {
			base.Prepare(parent);

			gunCount = System.Enum.GetValues(typeof(Gun)).Length;
			//gunChamber = new List<bool>(gunCount);
			gunChambers = new bool[gunCount];

			for (int i = 0; i < gunCount; i++) {
				//gunChamber.Add(false);
				gunChambers[i] = false;
			}
		}

		public override void Reload() {
			for(int i = 0; i < gunCount; i++) {
				gunChambers[i] = true;
			}
		}

		public override void FireBegin(Gun gun) {
			Instantiate(
				projectilePrefab,
				parent.GetFirePos(gun),
				parent.transform.rotation,
				null
			);
		}


		public override void AirborneFireBegin(Gun gun) {

			if(gunChambers[(int) gun]) {
				Instantiate(
					projectilePrefab,
					parent.GetFirePos(gun),
					parent.transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down),
					null
				);

				Vector3 curRelVel = parent.RelativeVelocity;
				parent.RelativeVelocity = new Vector3(
					curRelVel.x,
					airShotRecoilVelocity,
					curRelVel.z
				);

				parent.IsJumping = true;

				gunChambers[(int)gun] = false;
			}

		}

	} // End class

} // End namespace