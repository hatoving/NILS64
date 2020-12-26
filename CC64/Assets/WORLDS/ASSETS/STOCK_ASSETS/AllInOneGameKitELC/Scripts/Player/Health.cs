/// All in One Game Kit - Easy Ledge Climb Character System
/// Health.cs
///
/// This script allows the player to:
/// 1. Have a set health.
/// 2. Receive damage from enemies, items, and falls.
/// 3. Regain health over time or from items.
/// 4. Respawn.
/// 5. Set UI images for the health.
/// 6. Position the health UI anywhere on the screen, and limit the number of UI images (hearts) that can be on each row of health.
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Health : MonoBehaviour {
	
	//health
	[System.Serializable]
	public class PlayerHealth {
		
		//hearts
		public int numberOfHearts = 3; //the number of full hearts the player has
		public int maxHeartsPerRow = 8; //the maximum number of hearts per row
		public bool regainHealthOverTime = false; //allows the player to regain his health over a certain amount of time
		public int quartersOfHealthToRegain = 2; //the quarters of health that the player regains after a certain amount of time
		public float timeNeededToRegainHealth = 7; //the amount of time before the player regains health
		//health UI
		[System.Serializable]
		public class HealthUI {
			public Sprite[] heartSprites = new Sprite[5]; //sprites of the hearts being used to represent the player's health; in order from no health (0 quarters) to full health (4 quarters)
			public bool overlayHearts = true; //makes each new heart overlay the last one
			public Vector2 uISpacing = new Vector2(1, 1); //the horizontal and vertical spacing between each heart image/UI
			public Vector3 uIPosition = new Vector3(8, 188, 0); //the position of the heart sprites/UI
			public Vector2 uIScale = new Vector2(73, 73); //the width and height of the heart sprites/UI
		}
		public HealthUI healthUI = new HealthUI(); //variables that control the player's health UI
		[HideInInspector]
		public bool debugHealth = false; //allows the user to test the functions of the health script (regain health, take damage, add heart, remove heart)
		
	}
	public PlayerHealth health = new PlayerHealth(); //variables that control the player's health
	
	//damage
	[System.Serializable]
	public class PlayerDamage {
		
		//damage from enemies
		[System.Serializable]
		public class EnemyDamage {
			public string enemyTag = "Enemy"; //the tag of the enemies in the scene
			public float secondsToStayInvincibleAfterAttacking = 0.7f; //the amount of time the player stays invincible after attacking an enemy
			public float secondsToStayInvincibleAfterBeingHurt = 1.0f; //the amount of time the player stays invincible after being hurt
		}
		public EnemyDamage enemyDamage = new EnemyDamage(); //variables that control the player's reactions to damage received from or delivered to an enemy
		
		//fall damage
		[System.Serializable]
		public class FallDamage {
			public bool receiveFallDamage = true; //allows the player to receive damage from falls
			public float minimumFallSpeedToReceiveDamage = 4; //the minimum speed the player must fall in order to receive fall damage
			public int minimumReceivableDamage = 2; //the minimum damage (in quarters of hearts) the player receives from a fall
			public int speedDamageMultiple = 6; //the amount of damage the player receives increased/multiplied by the fall's speed
			public float maxGroundedDistance = 0.2f; //the maximum distance you can be from the ground to be considered grounded
			public LayerMask collisionLayers = ~((1 << 2) | (1 << 4)); //the layers that the grounded detectors (raycasts/linecasts) will collide with
		}
		public FallDamage fallDamage = new FallDamage(); //variables that control the player's fall damage
		
		public float damageBlinkSpeed = 0.1f; //the speed at which the player blinks (becomes invisible and visible again) after being hurt
	}
	public PlayerDamage damage = new PlayerDamage(); //variables that control the player's damage
	
	//respawn
	[System.Serializable]
	public class Respawn {
		public Vector3 respawnLocation; //the location at which the player respawns
		public Vector3 respawnRotation; //the rotation at which the player respawns
		public bool respawnAtStartLocationAndRotation = true; //enables the player to respawn at the same location and rotation he started with
	}
	public Respawn respawn = new Respawn(); //variables that control the player's respawn
	
	//health items
	[System.Serializable]
	public class HealthItems {
		public string healthItemTag = "Heart"; //the tag of an item that gives the player health
		public int quartersOfHealthToRegain = 4; //the amount of health you regain from the health item (measured in quarter hearts)
	}
	public HealthItems[] healthItems = new HealthItems[1]; //items that give the player health
	
	//damage items
	[System.Serializable]
	public class DamageItems {
		public string damageItemTag = "Hurt"; //the tag of an item that takes the player's health
		public int quartersOfHealthToLose = 2; //the amount of health you lose from the damage item (measured in quarter hearts)
	}
	public DamageItems[] damageItems = new DamageItems[1]; //items that take the player's health
	
	//private health variables
	[HideInInspector]
	public int currentHealth; //the current health of the player
	private GameObject healthHolder; //the gameObject that holds the health UI
	private GameObject canvas; //the canvas and parent of the UI
	private List<GameObject> hearts = new List<GameObject>(); //the health UI images
	private int newNumberOfHearts = 3; //the player's new and current number of hearts
	private int lastNumberOfHearts = 3; //the player's last number of hearts
	[HideInInspector]
	public bool canDamage = true; //detects whether the player can be damaged
	[HideInInspector]
	public bool canDamage2 = true; //detects whether the player cannot be damaged because he has just become invincible
	private float damageTimer = 0.0f; //the amount of time since the player was last damaged
	private bool invincible = false; //determines if the player is currently invincible
	private float blinkTimer; //the amount of time between the character becoming visible and invisible (blinking) while the player is damaged
	private float damageTimeoutTimer; //the amount of time since the player was last damaged (used to check if we can still be invincible)
	private Renderer[] renderers; //the renderers attached to the player and the player's children
	[HideInInspector]
	public bool applyDamage = false; //determines whether the player is currently being damaged
	[HideInInspector]
	public bool blink = false; //determines whether the player can blink while damaged
	[HideInInspector]
	public int enemyAttackPower = 2; //the amount of damage you take from enemies (measured in quarter hearts)
	private bool disableCollider; //disables the player's collider while respawning (so that we don't take any objects with us)
	private float regainHealthTimer; //the amount of time since we were last damaged (used to regain health)
	private bool grounded; //determines whether the player is grounded or not
	private float lastYPos; //the player's y position from the last update
	private int fallDamageToReceive; //the current amount of fall damage to receive
	
	//private health item variables
	private int quartersOfHealthToRegain; //the amount of health you regain from health items (measured in quarter hearts)
	//private damage item variables
	private int quartersOfHealthToLose; //the amount of damage you take from damage items (measured in quarter hearts)
	
	//respawn variables
	[HideInInspector]
	public Vector3 respawnLocation;
	[HideInInspector]
	public Quaternion respawnRotation;
	
	//UI spacing
	private float spacingX;
	private float spacingY;
	
	//distance and angle measurements
	private float dist; //distance (on the x and z-axis) between the player and the closest enemy
	private float distY; //distance (on the y-axis) between the player and the closest enemy
	private float angle; //angle between the player and the closest enemy
	private GameObject closest; //the current, closest enemy to the player
	
	void Start () {
		
		//initializing hearts
		newNumberOfHearts = health.numberOfHearts;
		lastNumberOfHearts = health.numberOfHearts;
		spacingX = health.healthUI.uIScale.x;
		spacingY = -health.healthUI.uIScale.y;
		
		//creating hearts
		AddHearts(newNumberOfHearts);
		
		//setting damage timers
		blinkTimer = damage.damageBlinkSpeed + 0.02f;
		damageTimeoutTimer = damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt + 0.02f;
		
		//setting respawn location and rotation
		if (respawn.respawnAtStartLocationAndRotation){
			respawnLocation = transform.position;
			respawnRotation = transform.rotation;
		}
		else {
			respawnLocation = respawn.respawnLocation;
			respawnRotation = Quaternion.Euler(respawn.respawnRotation.x, respawn.respawnRotation.y, respawn.respawnRotation.z);
		}
		
		lastYPos = transform.position.y;
		
	}
	
	void Update() {
		
		renderers = transform.GetComponentsInChildren<Renderer>(true);
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag(damage.enemyDamage.enemyTag); 
		float distance = Mathf.Infinity; 
		Vector3 position = transform.position; 
		// Iterate through them and find the closest one
		foreach (GameObject go in gos)  { 
			Vector3 diff = (go.transform.position - position);
			float curDistance = diff.sqrMagnitude; 
			if (curDistance < distance) { 
				closest = go; 
				distance = curDistance; 
			} 
		}
		if (closest == null){
			return;
		}
		Vector3 dir = (closest.transform.position - transform.position);
		angle = Vector3.Angle(dir, transform.forward);
		dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(closest.transform.position.x - transform.position.x),2) + Mathf.Pow(Mathf.Abs(closest.transform.position.z - transform.position.z),2));
		distY = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(closest.transform.position.y - transform.position.y),2));
		if (angle <= 60 && dist <= 3 && distY <= 5){
			invincible = true;
		}
		else {
			invincible = false;
		}

		if (damageTimeoutTimer > damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt){
			foreach (Renderer r in renderers) {
				r.enabled = true;
				canDamage = true;
				canDamage2 = true;
			}
		}
		
		//keeping the player from being damaged if he just attacked
		if (invincible){
			if (GetComponent<PlayerController>() && GetComponent<PlayerController>().attackTimer <= damage.enemyDamage.secondsToStayInvincibleAfterAttacking){
				canDamage2 = false;
			}
		}
		else {
			canDamage2 = true;
		}
		
		//applying damage to player
		if (applyDamage && canDamage && enemyAttackPower != 0){
			ApplyDamage();
			applyDamage = false;
		}
		damageTimer += 0.02f;
		if (damageTimer >= damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt){
			canDamage = true;
		}
		
		//blinking after being damaged
		if (blink && damageTimeoutTimer >= damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt && enemyAttackPower != 0){
			BlinkOut();
			blink = false;
		}
		if (blinkTimer > damage.damageBlinkSpeed){
			blinkTimer = 0;
		}
		if (damage.damageBlinkSpeed > 0){
			if (blinkTimer == 0 && damageTimeoutTimer <= damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt - 0.2f){
				foreach (Renderer r in renderers) {
					if (r.enabled == true){
						r.enabled = false;
					}	
					else if (r.enabled == false){
						r.enabled = true;
					}
				}
			}
			else if (damageTimeoutTimer > damage.enemyDamage.secondsToStayInvincibleAfterBeingHurt - 0.2f){
				foreach (Renderer r in renderers) {
					r.enabled = true;
				}
			}
		}
		blinkTimer += 0.02f;
		damageTimeoutTimer += 0.02f;
		
		//setting respawn location and rotation
		if (!respawn.respawnAtStartLocationAndRotation){
			respawnLocation = respawn.respawnLocation;
			respawnRotation = Quaternion.Euler(respawn.respawnRotation.x, respawn.respawnRotation.y, respawn.respawnRotation.z);
		}
		
	}
	
	void FixedUpdate() {
		
		//reloading health if player is killed
		if (currentHealth <= 0){
			//reloading player's health
			Invoke("Reload", 0.25f);
			//disabling player's collider so that our sudden change in position (when we respawn) does not throw an enemy, object, etc.
			if (GetComponent<Collider>() && GetComponent<Collider>().enabled){
				disableCollider = true;
			}
			else {
				disableCollider = false;
			}
			if (disableCollider){
				GetComponent<Collider>().enabled = false;
			}
			//respawn
			transform.position = respawnLocation;
			transform.rotation = respawnRotation;
			
			//setting player's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
			if (GetComponent<PlayerController>()){
				float yRotationValue;
				if (respawn.respawnRotation.y > 180){
					yRotationValue = transform.eulerAngles.y - 360;
				}
				else {
					yRotationValue = transform.eulerAngles.y;
				}
				//getting rotation on z-axis (x-axis is locked)
				if (GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnXAxis && !GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnZAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 90){
						GetComponent<PlayerController>().horizontalValue = -1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = 180.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 179.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						GetComponent<PlayerController>().horizontalValue = 1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = -0.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 0.001f;
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnZAxis && !GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnXAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 0){
						GetComponent<PlayerController>().horizontalValue = 1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = 90.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 89.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						GetComponent<PlayerController>().horizontalValue = -1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = -90.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = -89.999f;
						}
					}
				}
			}
			
			
			
			//re-enable collider
			if (disableCollider){
				GetComponent<Collider>().enabled = true;
			}
		}
		
		//getting number of hearts
		if (health.numberOfHearts >= 0 && health.maxHeartsPerRow > 0){
			newNumberOfHearts = health.numberOfHearts;
		}
		else {
			newNumberOfHearts = 0;
		}
		
		//positioning hearts
		for (int i = 0; i < hearts.Count; i++){
			
			int posY;
			if (health.maxHeartsPerRow > 0){
				posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
			}
			else {
				posY = (int)(Mathf.FloorToInt(0));
			}
			int posX = (int)(i - posY * health.maxHeartsPerRow);
			
			//spacing hearts
			if (hearts[i].GetComponent<Image>()){
				hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2((posX * spacingX) * health.healthUI.uISpacing.x, (posY * spacingY) * health.healthUI.uISpacing.y);
				hearts[i].GetComponent<RectTransform>().sizeDelta = new Vector2(health.healthUI.uIScale.x, health.healthUI.uIScale.y);
				hearts[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
				if (health.healthUI.overlayHearts){
					hearts[i].transform.SetSiblingIndex(i);
				}
				else {
					hearts[i].transform.SetSiblingIndex(0);
				}
			}
			
			if (i + 1 > newNumberOfHearts){
				Destroy(hearts[i]);
				hearts.RemoveAt(i);
			}
			
		}
		
		if (currentHealth > newNumberOfHearts*40){
			currentHealth = newNumberOfHearts*40;
		}
		
		if (newNumberOfHearts > lastNumberOfHearts){
			AddHearts(newNumberOfHearts - lastNumberOfHearts);
		}
		lastNumberOfHearts = newNumberOfHearts;
		
		//getting the canvas to hold the UI
		if (canvas == null){
			canvas = GameObject.Find("Canvas");
		}
		//creating parent for health UI
		if (healthHolder == null){
			healthHolder = new GameObject();
			healthHolder.AddComponent<RectTransform>();
			healthHolder.gameObject.name = "PlayerHealth";
			healthHolder.gameObject.layer = 5;
		}
		healthHolder.transform.SetParent(canvas.transform);
		healthHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		
		//anchoring the UI to a set location on the screen
		AutoAnchor();
		healthHolder.transform.SetSiblingIndex((int)health.healthUI.uIPosition.z);
		
		//regaining health (if the enemy is allowed to)
		if (health.regainHealthOverTime){
			//increasing regainHealthTimer
			if (currentHealth > 0 && currentHealth < newNumberOfHearts*40){
				regainHealthTimer += 0.02f;
			}
			else {
				regainHealthTimer = 0.0f;
			}
			
			//regaining health
			if (regainHealthTimer > health.timeNeededToRegainHealth){
				modifyHealth(health.quartersOfHealthToRegain);
				regainHealthTimer = 0.0f;
			}
		}
		else {
			regainHealthTimer = 0.0f;
		}
		
		//getting values for fall damage
		if (damage.fallDamage.receiveFallDamage){
			FallingDamage();
		}
		
	}
	
	void LateUpdate () {
		
		//getting the canvas to hold the UI
		if (canvas == null){
			canvas = GameObject.Find("Canvas");
		}
		//creating parent for health UI
		if (healthHolder == null){
			healthHolder = new GameObject();
			healthHolder.AddComponent<RectTransform>();
			healthHolder.gameObject.name = "PlayerHealth";
			healthHolder.gameObject.layer = 5;
		}
		healthHolder.transform.SetParent(canvas.transform);
		healthHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		
		//anchoring the UI to a set location on the screen
		AutoAnchor();
		healthHolder.transform.SetSiblingIndex((int)health.healthUI.uIPosition.z);
		
		//positioning hearts
		for (int i = 0; i < hearts.Count; i++){
			
			int posY;
			if (health.maxHeartsPerRow > 0){
				posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
			}
			else {
				posY = (int)(Mathf.FloorToInt(0));
			}
			int posX = (int)(i - posY * health.maxHeartsPerRow);
			
			//spacing hearts
			if (hearts[i].GetComponent<Image>()){
				hearts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2((posX * spacingX) * health.healthUI.uISpacing.x, (posY * spacingY) * health.healthUI.uISpacing.y);
				hearts[i].GetComponent<RectTransform>().sizeDelta = new Vector2(health.healthUI.uIScale.x, health.healthUI.uIScale.y);
				hearts[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
				if (health.healthUI.overlayHearts){
					hearts[i].transform.SetSiblingIndex(i);
				}
				else {
					hearts[i].transform.SetSiblingIndex(0);
				}
			}
			
			if (i + 1 > newNumberOfHearts){
				Destroy(hearts[i]);
				hearts.RemoveAt(i);
			}
			
		}
		
	}
	
	void FallingDamage () {
		
		//determining whether player is grounded or not
		Vector3 pos = transform.position;
        pos.y = GetComponent<Collider>().bounds.min.y + 0.1f;
		if (Physics.Raycast(pos, Vector3.down, damage.fallDamage.maxGroundedDistance, damage.fallDamage.collisionLayers)
		|| Physics.Raycast(pos - transform.forward/10, Vector3.down, damage.fallDamage.maxGroundedDistance, damage.fallDamage.collisionLayers) || Physics.Raycast(pos + transform.forward/10, Vector3.down, damage.fallDamage.maxGroundedDistance, damage.fallDamage.collisionLayers)
		|| Physics.Raycast(pos - transform.right/10, Vector3.down, damage.fallDamage.maxGroundedDistance, damage.fallDamage.collisionLayers) || Physics.Raycast(pos + transform.right/10, Vector3.down, damage.fallDamage.maxGroundedDistance, damage.fallDamage.collisionLayers)){
			//receiving fall damage
			if (!grounded && fallDamageToReceive > 0){
				modifyHealth(-fallDamageToReceive);
				BlinkOut();
			}
			grounded = true;
		}
		else {
			grounded = false;
		}
		
		//getting the amount of fall damage to receive
		if (lastYPos - transform.position.y >= damage.fallDamage.minimumFallSpeedToReceiveDamage/10){
			fallDamageToReceive = damage.fallDamage.minimumReceivableDamage + (int)(((lastYPos - transform.position.y) - (damage.fallDamage.minimumFallSpeedToReceiveDamage/10)) * damage.fallDamage.speedDamageMultiple);
		}
		else {
			fallDamageToReceive = 0;
		}
		
		lastYPos = transform.position.y;
		
	}
	
	void Reload() {
		if (!enabled) return;
		currentHealth = newNumberOfHearts*40;
		modifyHealth(1);
	}
	
	public void AddHearts(int n) {
		if (!enabled) return;
		
		if (healthHolder == null){
			healthHolder = new GameObject();
			healthHolder.AddComponent<RectTransform>();
			healthHolder.gameObject.name = "PlayerHealth";
			healthHolder.gameObject.layer = 5;
		}
		
		if (newNumberOfHearts >= 0 && health.maxHeartsPerRow > 0){
			for (int i = 0; i < n; i ++) {
				//creating heart UI Sprite so that we can create the heart images
				GameObject heartUI = new GameObject();
				heartUI.transform.localScale = new Vector3(0, 0, 0);
				heartUI.AddComponent<Image>();
				//creating hearts
				Transform newHeart = ((GameObject)Instantiate(heartUI, healthHolder.transform.position, Quaternion.identity)).transform; // Creates a new heart
				newHeart.gameObject.name = "Heart" + (i + 1);
				newHeart.gameObject.layer = 5;
				newHeart.transform.SetParent(healthHolder.transform);
				//destroying heart UI Sprite
				Destroy(heartUI);
				
				int posY;
				if (health.maxHeartsPerRow > 0){
					posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
				}
				else {
					posY = (int)(Mathf.FloorToInt(0));
				}
				int posX = (int)(hearts.Count - posY * health.maxHeartsPerRow);
				
				newHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2((posX * spacingX) * health.healthUI.uISpacing.x, (posY * spacingY) * health.healthUI.uISpacing.y);
				newHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(health.healthUI.uIScale.x, health.healthUI.uIScale.y);
				newHeart.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
				newHeart.GetComponent<Image>().sprite = health.healthUI.heartSprites[0];
				hearts.Add(newHeart.gameObject);
				
			}
		}
		currentHealth = newNumberOfHearts*40;
		UpdateHearts();
	}

	
	public void modifyHealth(int amount) {
		if (!enabled) return;
		currentHealth += (10)*amount;
		regainHealthTimer = 0.0f;
		UpdateHearts();
	}

	void UpdateHearts() {
		if (!enabled) return;
		bool restAreEmpty = false;
		int i = 0;
		
		foreach (GameObject heart in hearts) {
			
			if (restAreEmpty) {
				heart.GetComponent<Image>().sprite = health.healthUI.heartSprites[0]; // heart is empty
			}
			else {
				i += 1; // current iteration
				if (currentHealth >= i * 40) {
					heart.GetComponent<Image>().sprite = health.healthUI.heartSprites[health.healthUI.heartSprites.Length-1]; // health of current heart is full
				}
				else {
					int currentHeartHealth = (int)(40 - (40 * i - currentHealth));
					int healthPerImage = 40 / health.healthUI.heartSprites.Length; // how much health is there per image
					int imageIndex;
					if ((int)(currentHeartHealth / healthPerImage) >= 0) {
						imageIndex = currentHeartHealth / healthPerImage;
					}
					else {
						imageIndex = 0;
					}
					
					
					if (imageIndex == 0 && currentHeartHealth > 0) {
						imageIndex = 1;
					}

					heart.GetComponent<Image>().sprite = health.healthUI.heartSprites[imageIndex];
					restAreEmpty = true;
				}
			}
			
		}
	}
	
	void OnGUI() {
		
		if (health.debugHealth){
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4, 100, 25), "Regain Health")) {
				modifyHealth(1);
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10, 100, 25), "Take Damage")) {
				modifyHealth(-1);
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10 * 2, 100, 25), "Add Heart")) {
				health.numberOfHearts += 1;
				currentHealth = health.numberOfHearts;
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10 * 3, 100, 25), "Remove Heart") && newNumberOfHearts != 0) {
				health.numberOfHearts -= 1;
			}
		}
		
	}
	
	
	public void RegainHealth () {
		if (!enabled) return;
		modifyHealth(quartersOfHealthToRegain);
	}
	
	public void LoseHealth () {
		if (!enabled) return;
		modifyHealth(-quartersOfHealthToLose);
	}
	
	public void ApplyDamage () {
		if (!enabled) return;
		modifyHealth(-enemyAttackPower);
		canDamage = false;
		damageTimer = 0.0f;
	}

	public void BlinkOut() {
		if (!enabled) return;
		blinkTimer = 0;
		damageTimeoutTimer = 0;
	}
	
	private Vector2 anchorMin;
	public void AutoAnchor () {
		
		RectTransform r = healthHolder.GetComponent<RectTransform>();
		RectTransform p = canvas.GetComponent<RectTransform>();
		
		Vector2 offsetMin = r.offsetMin;
		Vector2 offsetMax = r.offsetMax;
		
		if (anchorMin == Vector2.zero){
			anchorMin = r.anchorMin;
		}
		Vector2 _anchorMin = anchorMin;
		Vector2 _anchorMax = r.anchorMax;
		
		float parent_width = p.rect.width;      
		float parent_height = p.rect.height;  
		
		anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width), _anchorMin.y + (offsetMin.y / parent_height));
		Vector2 anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width), _anchorMax.y + (offsetMax.y / parent_height));
		
		r.anchorMin = anchorMin + new Vector2(health.healthUI.uIPosition.x/100f, health.healthUI.uIPosition.y/100f);
		r.anchorMax = anchorMax;
		
		r.offsetMin = new Vector2(0, 0);
		r.offsetMax = new Vector2(0, 0);
		r.pivot = new Vector2(0.5f, 0.5f);
		
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!enabled) return;
		
		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		if (!enabled) return;

		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		if (!enabled) return;
		
		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnEnable () {
		
		if (healthHolder != null){
			healthHolder.SetActive(true);
		}
		
	}
	
	void OnDisable () {
		
		if (healthHolder != null){
			healthHolder.SetActive(false);
		}
		
	}
	
}