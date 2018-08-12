using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	private CameraController cameraController;
	public DialogDisplay dialogBox;
	public GameObject memoryPanel;

	public DialogSystem dialogSystem;
	public NPC[] npcs;

	private readonly float endGameAnimationDuration = 0.5f;
	public GameObject endGamePanel;
	private bool gameEnded;

	public Player player;

	void Start () {
		this.gameEnded = false;
		this.cameraController = Camera.main.GetComponent<CameraController>();
		this.dialogSystem = DialogSystem.Create();
		foreach(NPC npc in npcs) {
			npc.Setup(dialogSystem[npc.name]);
		}

		StartCoroutine(ShowIntroSequence());
	}

	void Update() {
		if (Input.GetKeyUp("r")) {
			SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
		}
	}
	
	private IEnumerator ShowIntroSequence() {
		yield return dialogBox.DisplayText(new string[] {
			"(What happened?)",
			"(I don't remember anything.)",
			"(...)",
		});
		yield return cameraController.ShakeCamera(0.8f);
		yield return dialogBox.DisplayText(new string[] {
			"\"Ouch!\"",
			"(My head...It hurts...)",
			"(...)",
			"\"Who am I?\"",
			"(...)",
			"(I should talk to someone.)",
			"<Use arrow keys to move around and space key to interact with people>\n<If you are stuck, press R to restart>\n<Topics will show up on the top-left memory list, they can be used to open up different conversations>",
		}, () => {
			player.Setup(memoryPanel.GetComponent<MemoryList>(), dialogBox);
			player.SetCanMove(true);
			StartCoroutine(memoryPanel.GetComponent<MemoryList>().PlayAppearAnimation());
			memoryPanel.GetComponent<MemoryList>().AddItem("Wake up in a room");
		});
	}

	public void GameEnd() {
		endGamePanel.SetActive(true);
		player.SetCanMove(false);
		gameEnded = true;
		StartCoroutine(ShowEndGameSequence());
	}

	private IEnumerator ShowEndGameSequence() {
		CanvasGroup cg = endGamePanel.GetComponent<CanvasGroup>();
		float time = 0;
		while (time < endGameAnimationDuration) {
			cg.alpha = time / endGameAnimationDuration;
			time += Time.deltaTime;
			yield return null;
		}
		cg.alpha = 1;
	}
}
