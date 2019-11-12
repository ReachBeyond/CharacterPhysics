using UnityEngine;

public class ExplosionController : MonoBehaviour {

	[SerializeField] private Component animationComponent;
	[SerializeField] private float animationTime;

	private IAnimate progressAnimation;
	float spawnTime;

	void Awake () {
		progressAnimation = animationComponent as IAnimate;

		UnityEngine.Assertions.Assert.IsNotNull(progressAnimation);
	}

	private void OnEnable() {
		spawnTime = Time.time;
	}

	void Update () {

		// TODO show bobby
		float timeSinceSpawn = Time.time - spawnTime;

		if(timeSinceSpawn < animationTime) {
			progressAnimation.UpdateAnimation(timeSinceSpawn / animationTime);
		}
		else {
			Destroy(gameObject);
		}
	}
}
