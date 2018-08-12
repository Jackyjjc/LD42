using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchChild : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.tag.Equals("Player")) {
			GetComponentInParent<LightSwitch>().OnTrigger(gameObject.name.Equals("On"));
		}
	}
}
