using UnityEngine;
using ReachBeyond.Characters.Bodies;

namespace ModelF.Behaviours {

	[CreateAssetMenu(fileName = "DummyFire", menuName = "FireBehaviour/Dummy")]
	public class DummyFire : BaseFireBehaviour {

		public override void Reload() {
			Debug.Log("Reloading");
		}

		public override void FireBegin(Gun gun) {
			Debug.Log("Fire begin " + gun.ToString());
		}
		public override void FireHold(Gun gun) {
			Debug.Log("Fire hold " + gun.ToString());
		}
		public override void FireEnd(Gun gun) {
			Debug.Log("Fire end " + gun.ToString());
		}

		public override void AirborneFireBegin(Gun gun) {
			Debug.Log("Airborne fire begin " + gun.ToString());
		}
		public override void AirborneFireHold(Gun gun) {
			Debug.Log("Airborne fire hold " + gun.ToString());
		}
		public override void AirborneFireEnd(Gun gun) {
			Debug.Log("Airborne fire end " + gun.ToString());
		}
	}
}
