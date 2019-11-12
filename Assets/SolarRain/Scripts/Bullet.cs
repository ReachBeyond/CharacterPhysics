//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReachBeyond.EventObjects;

public class Bullet : MonoBehaviour {

	[SerializeField] private GameObject explosionPrefab;
	[SerializeField] private float speed;

	//public Action onHitAction;
	// TODO This should be in the explosion
	//[SerializeField] private EventObjectInvoker onHitEnemyEvent;

	private void Update() {
		transform.position = transform.position + transform.forward * speed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other) {
		//onHitEnemyEvent.Raise();
		Instantiate(explosionPrefab, transform.position, transform.rotation, transform.parent);
		Destroy(gameObject);
	}
}
