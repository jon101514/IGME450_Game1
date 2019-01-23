﻿/** Jonathan So, jds7523@rit.edu
 * This cactus object was used in an animated preview I had made a couple years ago. It may serve as the basis for our spring's movement.
 * The cactus will charge its jump when the spacebar is held; it will jump upon release.
 */
using UnityEngine;
using System.Collections;

public class Cactus : MonoBehaviour {

	public KeyCode cactus; // The key to hit for jumping.
    public int facingRight = 1;//used for changing directions, should always start facing right. +1 is right, -1 is left. Possible values are +1 and -1.

	private float jumpFX = 0f; // Horizontal Force to apply to make the cactus jump.
	private float jumpFY = 0f; // Vertictal Force to apply to make the cactus jump.

	// Forces for maximum jump strength.
	private const float MAX_JUMP_FX = 600f; 
	private const float MAX_JUMP_FY = 1200f; 
	private const float HOLD_TIME = 2f; // Amount of time it takes to hold the button for a maximum jump.

	// Keeps track of the current coroutine that's running.
	private Coroutine currCoroutine = null;

	private Animator anim;
	private Rigidbody2D rb;
	private SpriteRenderer sr;

	void Awake() {
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
	}

	// Input detection
	void Update () {
        Vector2 direction = GetMousePos();

        if (Input.GetKeyDown(cactus) && !anim.GetBool("InAir")) { // Charge our jump.
			currCoroutine = StartCoroutine(ChargeJump());
			anim.SetBool("Charging", true);
		} 
		if (Input.GetKeyUp(cactus) && !anim.GetBool("InAir")) { // Release our jump.
			StopCoroutine(currCoroutine);
			sr.color = Color.white;
			if (direction.y < 0.1f) { // Handle the case where the mouse is pointing below our character. This is a short hop.
				rb.AddForce(new Vector2((direction.x / 2f) * jumpFX, 200f));
			} else {
				rb.AddForce(new Vector2(direction.x * jumpFX, direction.y * jumpFY));
			}
			anim.SetBool("Charging", false);
			anim.SetBool("InAir", true);
		}

        //changing directions before jump
        if ((direction.x <= 0 && (facingRight == 1)) || (direction.x > 0 && facingRight == -1))  {
			Facing();
        }
    }
		
	// Charges the power of our jump. Jump strength is indicated by our color.
	private IEnumerator ChargeJump() {
		float timer;
		// Default "tap-jump" is a tiny hop that gets us off the ground. 
		jumpFX = 100f;
		jumpFY = 200f;
		Color tempColor = Color.white;
		// Oscillating jump
		while (true) {
			// Our jump strength increases...
			timer = 0f;
			while (jumpFX < MAX_JUMP_FX || jumpFY < MAX_JUMP_FY) { 
				jumpFX = Mathf.Lerp(100, MAX_JUMP_FX, (timer / HOLD_TIME));
				jumpFY = Mathf.Lerp(200, MAX_JUMP_FY, (timer / HOLD_TIME));
				tempColor.r = Mathf.Lerp(Color.white.r, Color.red.r, (timer / HOLD_TIME));
				tempColor.g = Mathf.Lerp(Color.white.g, Color.red.g, (timer / HOLD_TIME));
				tempColor.b = Mathf.Lerp(Color.white.b, Color.red.b, (timer / HOLD_TIME));
				sr.color = tempColor;
				timer += Time.deltaTime;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			yield return new WaitForSeconds(Time.deltaTime);
			// Then, it decreases.
			timer = 0f;
			while (jumpFX > 100 || jumpFY > 200) { 
				jumpFX = Mathf.Lerp(MAX_JUMP_FX, 100, (timer / HOLD_TIME));
				jumpFY = Mathf.Lerp(MAX_JUMP_FY, 200, (timer / HOLD_TIME));
				tempColor.r = Mathf.Lerp(Color.red.r, Color.white.r, (timer / HOLD_TIME));
				tempColor.g = Mathf.Lerp(Color.red.g, Color.white.g, (timer / HOLD_TIME));
				tempColor.b = Mathf.Lerp(Color.red.b, Color.white.b, (timer / HOLD_TIME));
				sr.color = tempColor;
				timer += Time.deltaTime;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}

		/* 
		// The following code is for an implementation where, upon reaching the maximum strength, 
		// the charge resets directly to the weakest strength, starting over from the beginning. We can choose whether we want this or an oscillating jump.
		while (jumpFX < MAX_JUMP_FX || jumpFY < MAX_JUMP_FY) {
			jumpFX = Mathf.Lerp(100, MAX_JUMP_FX, (timer / HOLD_TIME));
            jumpFY = Mathf.Lerp(200, MAX_JUMP_FY, (timer / HOLD_TIME));
			tempColor.r = Mathf.Lerp(Color.white.r, Color.red.r, (timer / HOLD_TIME));
			tempColor.g = Mathf.Lerp(Color.white.g, Color.red.g, (timer / HOLD_TIME));
			tempColor.b = Mathf.Lerp(Color.white.b, Color.red.b, (timer / HOLD_TIME));
			sr.color = tempColor;
			timer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return new WaitForSeconds(1/4f);
		StopCoroutine(currCoroutine);
		currCoroutine = StartCoroutine(ChargeJump());
		*/
	}

    /* Returns the normalized 2d vector that points from the player to the mouse position
    */
    private Vector2 GetMousePos()
    {
        float distanceToCamera = 10; //this is used because ScreenToWorldPoint creates the point on a X/Y plane Z units away from the camera

        Vector3 playerWorldPos = gameObject.transform.position; //the players position in the world
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera)); //mouses postion on the same plane as the player
        Vector2 direction = (mouseWorldPos - playerWorldPos).normalized; //normalized direction vector

        Debug.DrawRay(playerWorldPos,direction,Color.red);
        return direction;
    }

    /* Changes the direction that this object is facing.
	 * We take our current scale and save a temporary copy; we then modify individual components
	 * of it and then save it back into our current scale.
	 */
    private void Facing() {
		Vector3 tempScale = transform.localScale;
		tempScale.x *= -1;
		facingRight *= -1;
		transform.localScale = tempScale;
	}


	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			anim.SetBool("InAir", false);
		}
	}
}
