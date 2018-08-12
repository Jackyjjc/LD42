using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MemoryList : MonoBehaviour {
	
	public event EventHandler onMemoryLost;
	
	private delegate void OnAddItemAnimationFinish();
	private readonly float listAnimationDuration = 1f;
	private readonly float itemAnimationDuration = 0.2f;

	public GameObject itemPrefab;

	private GameObject itemContainer;

	private int size;

	public List<string> items;

 	void Start() {
		this.itemContainer = transform.Find("MemoryList").gameObject;
		this.size = 10;
		this.items = new List<string>(size + 1);
		this.toAdd = new List<string>(size + 1);
		this.canAdd = false;
	}

	public void ReEnforce(int i) {
		AddItem(items[i]);
	}

	public bool Contains(string item) {
		return items.Contains(item);
	}

	public bool Contains(string[] itemsToCheck) {
		bool result = true;
		foreach (string toCheck in itemsToCheck) {
			result &= items.Contains(toCheck);
		}
		return result;
	}

	private List<string> toAdd;
	private bool canAdd;
	void Update() {
		if (toAdd.Count == 0) {
			return;
		}

		if (!canAdd) {
			return;
		}

		canAdd = false;
		string item = toAdd[0];
		toAdd.RemoveAt(0);
		StartCoroutine(AnimateAddItem(item, () => { canAdd = true; }));
	}

	private IEnumerator AnimateAddItem(string item, OnAddItemAnimationFinish onAddItemAnimationFinish) {
		items.Insert(0, item);
		GameObject newItem = GameObject.Instantiate(itemPrefab, itemContainer.transform);
		newItem.transform.SetAsFirstSibling();
		Text newItemText = newItem.GetComponent<Text>();
		newItemText.text = item;

		float time = 0;
		while(time < itemAnimationDuration) {
			newItemText.transform.localScale = new Vector3(1, time / itemAnimationDuration , 1);
			newItemText.color = new Color(newItemText.color.r, newItemText.color.g, newItemText.color.b, time / itemAnimationDuration);
			time += Time.deltaTime;
			yield return null;
		}
		newItemText.transform.localScale = new Vector3(1, 1, 1);
		newItemText.color = new Color(newItemText.color.r, newItemText.color.g, newItemText.color.b, 1f);
		
		// remove the last item
		if (items.Count > size) {
			items.RemoveAt(size);
			Destroy(itemContainer.transform.GetChild(size).gameObject);
			onMemoryLost.Invoke(this, EventArgs.Empty);
		}

		// Apply the alpha changes
		int childCount = itemContainer.transform.childCount;
		for (int i = 0; i < childCount; i++) {
			Text text = itemContainer.transform.GetChild(i).GetComponent<Text>();
			text.color = new Color(text.color.r, text.color.g, text.color.b, ((size - i) / (float)size) * 1f );
		}

		onAddItemAnimationFinish();
	}

	public void AddItem(string item) {
		toAdd.Add(item);
	}

	public IEnumerator PlayAppearAnimation() {
		CanvasGroup cg = GetComponent<CanvasGroup>();
		float time = 0;
		while (time < listAnimationDuration) {
			cg.alpha = time / listAnimationDuration;
			time += Time.deltaTime;
			yield return null;
		}
		cg.alpha = 1;
		canAdd = true;
	}
}
