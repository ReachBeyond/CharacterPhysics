using UnityEngine;

public class Transformer : MonoBehaviour {

	[SerializeField] private Vector3 moveSpeed;
	[SerializeField] private Vector3 rotationSpeed;
	[SerializeField] private Vector3 scaleSpeed;

	[SerializeField] private float randomRotationMagnitude;

	private Vector3 extraRotationSpeed;

	private void Start() {
		Vector3 randomizedDirection = new Vector3(
			Random.Range(-1f, 1f),
			Random.Range(-1f, 1f),
			Random.Range(-1f, 1f)
		).normalized;

		extraRotationSpeed = randomizedDirection * randomRotationMagnitude;
	}

	private void Update () {
		transform.position += moveSpeed * Time.deltaTime;
		//transform.Rotate(rotationSpeed.constValue * Time.deltaTime);
		transform.localRotation = transform.localRotation * Quaternion.Euler((rotationSpeed + extraRotationSpeed) * Time.deltaTime);
		transform.localScale += scaleSpeed * Time.deltaTime;
	}
}
