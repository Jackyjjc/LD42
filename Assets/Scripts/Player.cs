using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	private MemoryList memoryList;
	private DialogDisplay dialogBox;
	private bool canMove;
	private bool finishedTips;
	private Rigidbody2D body;
	private Vector3 facingDir;

	private float speed = 50f;

	public void Setup(MemoryList memoryList, DialogDisplay dialogBox) {
		this.memoryList = memoryList;
		this.dialogBox = dialogBox;
		if (canMove) {
			throw new System.Exception("player is allowed to move around before Setup is called");
		}

		memoryList.onMemoryLost += (_1, _2) => {
			if (finishedTips) {
				return;
			}
			canMove = false;
			finishedTips = true;
			StartCoroutine(dialogBox.DisplayText(new string[] {
				"(I feel like I haven just forgotten something)",
				"(Damn it, this headache just won't go way!)",
				"<Due to the condition, you can only remember the last 10 topics>"
			},
			() => {
				canMove = true;
			}));
		};
	}

	public void SetCanMove(bool value) {
		this.canMove = value;
	}

	void Start () {
		this.finishedTips = false;
		this.canMove = false;
		this.body = GetComponent<Rigidbody2D>();
		this.facingDir = new Vector3(0, -1, 0).normalized;
	}
	
	void Update() {
		if (!canMove) return;
		if (Input.GetKeyUp("space")) {
			// raycast to see if player can talk to someone
			RaycastHit2D hits = Physics2D.Raycast(gameObject.transform.position, facingDir, 1f);
			if (hits && hits.collider.tag.Equals("NPC")) {
				SetCanMove(false);
				StartCoroutine(hits.collider.GetComponent<NPC>().Talk(memoryList, dialogBox, () => {
					SetCanMove(true);
				}));
			}
		}
	}

	void FixedUpdate () {
		if (!canMove) return;

 		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");
		if (Mathf.Abs(inputX) > 0 || Mathf.Abs(inputY) > 0) {
			body.AddForce(new Vector2(inputX * speed, inputY * speed), ForceMode2D.Force);
			facingDir = new Vector3(inputX, inputY, 0).normalized;
		}
	}
}
