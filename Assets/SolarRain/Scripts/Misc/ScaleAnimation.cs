using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour, IAnimate {

	[SerializeField] private Vector3 startScale;
	[SerializeField] private Vector3 finalScale;

	[Range(0, 1)]
	[SerializeField] private float progress;

	void IAnimate.UpdateAnimation(float newProgress) {
		progress = Mathf.Clamp01(newProgress);
	}

	void OnEnable() {
		transform.localScale = startScale;		
	}

	void Update() {
		transform.localScale =
			Vector3.Lerp(startScale, finalScale, progress);
	}

} // End class
