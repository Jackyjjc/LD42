using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour {

	public Light[] onLight;
	public Light[] offLights;

	public void OnTrigger(bool on) {
		foreach (var light in onLight)
		{
			light.gameObject.SetActive(on);
		}
		foreach (var light in offLights)
		{
			light.gameObject.SetActive(!on);
		}
	}
}
