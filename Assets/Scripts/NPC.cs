using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPC : MonoBehaviour {

	private DialogNode node;

	public void Setup(DialogNode node) {
		this.node = node;
	}

	public IEnumerator Talk(MemoryList memoryList, DialogDisplay dialogBox, OnDialogFinish onDialogFinish) {
		bool keepTalking = true;
		while (keepTalking) {
			if (node == null) {
				keepTalking = false;
			} else if (node is DialogTextNode) {
				DialogTextNode textNode = (DialogTextNode)node;
				if (textNode.requireItems == null || memoryList.Contains(textNode.requireItems)) {
					yield return dialogBox.DisplayText(textNode.dialogs);
					if (textNode.addItems != null) {
						foreach(string item in textNode.addItems) {
							memoryList.AddItem(item);
						}
					}
					if (node.children != null) {
						node = node.children[0];
						if (node.id == 999) {
							GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().GameEnd();
							onDialogFinish();
							yield break;
						}
					} else {
						node = null;
					}
				} else {
					yield return dialogBox.DisplayText(new string[]{"Hi Ben!"});
				}
				keepTalking = false;
			} else if (node is DialogOptionNode) {
				DialogOptionNode optionNode = (DialogOptionNode)node;
				string[] availableOptions = optionNode.options
					.Where((option) => !option.selected && (option.requireItems == null || memoryList.Contains(option.requireItems)))
					.Select((option) => option.text).ToArray();

				if (availableOptions.Length > 0) {
					yield return dialogBox.DisplayOptionList(availableOptions, (selection) => {
						node = optionNode.Select(selection);
					});
					if (optionNode == node) {
						keepTalking = false;
					}
				} else {
					yield return dialogBox.DisplayText(new string[]{"Hi Ben!"});
					keepTalking = false;
				}
			}
			yield return null;
		}
		onDialogFinish();
	}
}
