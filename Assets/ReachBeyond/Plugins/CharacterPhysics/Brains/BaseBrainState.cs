using UnityEngine;
using ReachBeyond.Characters.Bodies;
using UnityEngine.Assertions;

namespace ReachBeyond.Characters.Brains {

	public abstract class BaseBrainState : MonoBehaviour {

		[SerializeField] protected BaseBody body;

		protected virtual void Awake() {
			Assert.IsNotNull(body);
		}

	}

} // End namespace
