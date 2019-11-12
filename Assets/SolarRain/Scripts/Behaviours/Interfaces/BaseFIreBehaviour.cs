using UnityEngine;
using ReachBeyond.Characters.Bodies;

namespace ModelF.Behaviours {
	public abstract class BaseFireBehaviour : ScriptableObject {

		protected ModelFBody parent;

		public virtual void Prepare(ModelFBody parent) {
			this.parent = parent;
		}

		public abstract void Reload();

		public virtual void FireBegin(Gun gun) { }
		public virtual void FireHold(Gun gun) { }
		public virtual void FireEnd(Gun gun) { }

		public virtual void AirborneFireBegin(Gun gun) { }
		public virtual void AirborneFireHold(Gun gun) { }
		public virtual void AirborneFireEnd(Gun gun) { }
	}
}
