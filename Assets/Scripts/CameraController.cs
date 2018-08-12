using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	private readonly float startPower = 10f;

	private Vector3 followDistance;
	public Player follow;

	private bool shaking;

	void Start() {
		this.shaking = false;
		this.followDistance = transform.position - followDistance;
	}

	public void LateUpdate() {
		if (this.shaking) {
			return;
		}
		transform.position = follow.transform.position + followDistance;
	}

	public IEnumerator ShakeCamera(float shakeDuration) {
		this.shaking = true;
		Vector3 originalPos = transform.position;

		float shakePower = startPower;
		Vector2 dir = Random.insideUnitCircle.normalized;
		while (shakeDuration > 0) {
			Vector2 shakePos = dir * shakePower;
			transform.position = originalPos + new Vector3(shakePos.x, shakePos.y, transform.position.z);
			shakeDuration -= Time.deltaTime;
			shakePower = Mathf.Lerp(shakeDuration, 0, 1-shakeDuration);
			dir = -dir;
			yield return null;
		}
		transform.position = originalPos;
		this.shaking = false;
	}
}
