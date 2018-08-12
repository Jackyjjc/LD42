using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnDialogFinish();
public delegate void OnOptionSelected(string item);

public class DialogDisplay : MonoBehaviour {
	private float secondsBetweenChar = 0.03f;
	private GameObject dialogTextPanel;
	private GameObject dialogOptionPanel;
	
	public Text dialogText;
	private string[] lines;
	private int currentLineIndex;
	private int currentCharIndex;

	void Start() {
		dialogTextPanel = transform.Find("DialogPanelText").gameObject;
		dialogOptionPanel = transform.Find("DialogPanelOptions").gameObject;
		dialogTextPanel.SetActive(false);
		dialogOptionPanel.SetActive(false);
	}

	public IEnumerator DisplayText(string[] lines, OnDialogFinish onFinish = null) {
		gameObject.SetActive(true);
		dialogTextPanel.SetActive(true);
		Text dialogText = dialogTextPanel.GetComponentInChildren<Text>();
		int lineIndex = 0;
		while (lineIndex < lines.Length) {
			int charIndex = 0;
			while (charIndex < lines[lineIndex].Length) {
				dialogText.text = lines[lineIndex].Substring(0, charIndex + 1);
				charIndex++;
				yield return new WaitForSeconds(secondsBetweenChar);
			}
			// done a line, wait for user input to continue
			bool next = false;
			while (!next) {
				if (Input.GetKeyUp("space")) {
					next = true;
				}
				yield return null;
			}
			lineIndex++;
		}
		dialogTextPanel.SetActive(false);
		gameObject.SetActive(false);
		if (onFinish != null) {
			onFinish();
		}
	}

	public GameObject optionPrefab;

	public IEnumerator DisplayOptionList(string[] options, OnOptionSelected onOptionSelected = null) {
		int index = 0;
		gameObject.SetActive(true);
		dialogOptionPanel.SetActive(true);
		
		foreach (Transform child in dialogOptionPanel.transform)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < options.Length; i++)
		{
			GameObject optionObj = GameObject.Instantiate(optionPrefab, dialogOptionPanel.transform);
			optionObj.GetComponentInChildren<Text>().text = options[i];
			if (i == 0) {
				optionObj.GetComponent<Image>().enabled = true;
			}
		}

		// Wait for the next frame to reset the Input
		yield return null;

		int selectedIndex = -1;
		while (selectedIndex < 0) {
			if (Input.GetKeyUp("up")) {
				dialogOptionPanel.transform.GetChild(index).GetComponent<Image>().enabled = false;
				index = Mathf.Max(index - 1, 0);
				dialogOptionPanel.transform.GetChild(index).GetComponent<Image>().enabled = true;
			} else if (Input.GetKeyUp("down")) {
				dialogOptionPanel.transform.GetChild(index).GetComponent<Image>().enabled = false;
				index = Mathf.Min(index + 1, options.Length - 1);
				dialogOptionPanel.transform.GetChild(index).GetComponent<Image>().enabled = true;
			} else if (Input.GetKeyUp("space")) {
				selectedIndex = index;
			} else if (Input.GetKeyUp("escape")) {
				dialogOptionPanel.SetActive(false);
				gameObject.SetActive(false);
				yield break;
			}
			yield return null;
		}

		dialogOptionPanel.SetActive(false);
		gameObject.SetActive(false);

		if (onOptionSelected != null) {
			onOptionSelected(options[index]);
		}
	}
}
