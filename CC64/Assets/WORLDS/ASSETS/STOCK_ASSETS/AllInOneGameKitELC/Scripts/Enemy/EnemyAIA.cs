/// All in One Game Kit - Easy Ledge Climb Character System
/// EnemyAI.cs
///
/// As long as the enemy has a CharacterController or Rigidbody component, this script allows the enemy to:
/// 1. Follow the player at a set speed and rotation.
/// 2. Attack the player.
/// 3. Receive damage.
/// 4. Regain health.
/// 5. Use a health bar.
/// 6. Respawn.
///
/// NOTE: *You should always set a layer for your enemy so that you can disable collisions with that layer (by unchecking it in the script's Collision Layers).
///	If you do not, the raycasts and linecasts will collide with the enemy itself and keep the script from working properly!*
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class EnemyAIAdam : MonoBehaviour {
	
	public Transform player;
    public GameObject playerhello;
    public bool squishable = true;
    public float Xsquish = 1;
    public float Ysquish = 1;
    public float Zsquish = 1;
    public float Timesquish = 100;
    public float squished = 0;
    public Vector3 squishtemp;
    public float squishYlimit = 1;
    public Animator myAnimationController;

    //movement
    [System.Serializable]
	public class Movement {
		public float movementSpeed = 2.5f; //the enemy's movement speed
		public float rotationSpeed = 6; //the enemy's rotation speed
		public float distanceToFacePlayerFrom = 10; //the maximum distance the player can be for the enemy to face/look at the player
		public float distanceToChasePlayerFrom = 5; //the maximum distance the player can be for the enemy to chase the player
		public float viewpointPlayerMustBeInToFollow = 70; //the size of the enemy's viewpoint; if the player is inside the viewpoint, the enemy fill follow him
		public bool stopAtEdges = true; //stops the enemy before he walks off the edge
		public bool showEdgeDetectors = true; //detectors that determine where the edge is located
		public float edgeDetectorHeight = 0.0f; //the height of the edge detectors
		public float edgeDetectorForward = 0.0f; //the forward distance of the edge detectors
		public float minimumEdgeHeight = 0.0f; //the height of the edge detector that determines if there is surface below the player
		//side-scrolling
		[System.Serializable]
		public class SideScrolling {
			public bool lockMovementOnZAxis = false; //locks the movement of the enemy on the z-axis
			public float zValue = 0; //the permanent z-value of the enemy if his movement on the z-axis is locked
			public bool lockMovementOnXAxis = false; //locks the movement of the enemy on the x-axis
			public float xValue = 0; //the permanent x-value of the enemy if his movement on the x-axis is locked
			public bool rotateInwards = true; //when the enemy rotates from side to side, he rotates inward (so that you see his front side while he is rotating)
		}
		public SideScrolling sideScrolling = new SideScrolling(); //variables that determine whether or not the enemy uses 2.5D side-scrolling
	}
	
	//attack
	[System.Serializable]
	public class Attack {
		public int attackPower = 2; //the amount of damage you inflict to the player on collision (measured in quarter hearts)
	}
	
	//health
	[System.Serializable]
	public class EnemyHealth {
		public float maximumHealth = 3; //the health of the enemy
		public bool regainHealthOverTime = true; //allows the enemy to regain his health over a certain amount of time
		public float healthToRegain = 3; //the health that the enemy regains after a certain amount of time
		public float timeNeededToRegainHealth = 7; //the amount of time before the enemy regains health
		public float minimumDistanceFromPlayerToRegainHealth = 0; //the distance from the player that the enemy must be to regain health
		
		[System.Serializable]
		public class HealthBar {
			public Transform playerCamera; //the camera set to follow the player (automatically assigns to Camera.main if not set)
			public bool showHealthBar = true; //determines whether or not to show a health bar above the enemy
			public float healthBarUpAmount = 0; //the up distance of the health bar above the enemy
			public float healthBarOverallHeight = 1; //the overall height of the health bar above the enemy
			public float healthBarOverallWidth = 1; //the overall width of the health bar above the enemy
			public float outlineHeight = 1; //the height of the health bar's outline
			public float outlineWidth = 1; //the width of the health bar's outline
			public float healthHeight = 1; //the height of the health bar's health
			public float healthWidth = 1; //the width of the health bar's health
			[System.Serializable]
			public class HealthBarMaterials {
				public Material outlineMaterial; //the material of the health bar's outline
				public Material healthMaterial; //the material of the health bar's health percentage
				public Material noHealthMaterial; //the material of the health bar's no health percentage
			}
			public HealthBarMaterials healthBarMaterials = new HealthBarMaterials(); //the materials of the health bar
			public Color outlineColor = Color.black; //the color of the health bar's outline (affects the outlineMaterial)
			public Color healthColor = Color.green; //the color of the health bar's health percentage (affects the healthMaterial)
			public Color noHealthColor = Color.red; //the color of the health bar's no health percentage (affects the noHealthMaterial)
			public float secondsToShowHealthBarAfterEnemyDeath = 0.5f; //the amount of time that the health bar stays visible after the enemy has been killed
		}
		public HealthBar healthBar = new HealthBar(); //variables that determine whether the enemy has a health bar or not
		
	}
	
	//damage
	[System.Serializable]
	public class Damage {
		public bool acquirePlayerAttackButtonFromPlayerIfPossible = true; //if the player has the "PlayerController.cs" script attatched to him, aquire the player attack button from that script
		public string playerAttackButton = "Fire1"; //the button (found in "Edit > Project Settings > Input") that the player uses to attack the enemy
		public float damageRadius = 1.25f; //the radius the player must be in to hurt the enemy
		public float knockBackFactor = 1; //the distance that the enemy gets knocked back after getting hit
		public float playerViewpointEnemyMustBeInToGetHit = 80; //the size of the player's viewpoint; if the enemy is inside of the player's viewpoint (and the player is inside of the enemy's hit area) and the player attacks, the enemy will receive damage
		public GameObject hurtEffect; //optional effect to appear when the enemy gets hurt
		public GameObject deathEffect; //optional effect to appear when the enemy gets killed
	}
	
	//respawn
	[System.Serializable]
	public class Respawn {
		public bool allowRespawn = true; //determines whether or not the enemy can respawn after being killed
		public float respawnWaitTime = 3; //the amount of time to wait, after the enemy has been killed, to respawn
		public Vector3 respawnLocation; //the location at which the enemy respawns
		public Vector3 respawnRotation; //the rotation at which the enemy respawns
		public bool respawnAtStartLocationAndRotation = true; //enables the enemy to respawn at the same location and rotation he started with
		public float minimumDistanceFromPlayerToRespawn = 3; //the minimum distance the player must be from the respawn location in order to respawn
		public GameObject respawnEffect; //optional effect to appear when the enemy respawns
	}


    public Movement movement = new Movement(); //variables that determine the enemy's movement
	public Attack attack = new Attack(); //variables that determine the enemy's attack
	public EnemyHealth health = new EnemyHealth(); //variables that determine the enemy's health
	public Damage damage = new Damage(); //variables that determine the enemy's damage
	public Respawn respawn = new Respawn(); //variables that control the enemy's respawn

    //private variables
    //movement
    private Vector3 edgeDetectorHeight2;
	private Vector3 edgeDetectorForward2;
	private Vector3 minimumEdgeHeight2;
	//attack
	[HideInInspector]
	public int attackPower;
	[HideInInspector]
	public string playerAttackButton;
	//health
	[HideInInspector]
	public float maximumHealth;
	[HideInInspector]
	public bool regainHealthOverTime = true;
	[HideInInspector]
	public float healthToRegain;
	[HideInInspector]
	public float timeNeededToRegainHealth;
	[HideInInspector]
	public float minimumDistanceFromPlayerToRegainHealth;
	private float regainHealthTimer;
	//respawn
	[HideInInspector]
	public bool allowRespawn;
	[HideInInspector]
	public float respawnWaitTime;
	[HideInInspector]
	public Vector3 respawnLocation;
	[HideInInspector]
	public Quaternion respawnRotation;
	[HideInInspector]
	public float minimumDistanceFromPlayerToRespawn;
	private bool dead = false;
	private float deathTimer;
	private bool colliderOnOff;
	private bool rendererOnOff;
	private bool ccOnOff;
	private bool rbOnOff;
	private Renderer[] childRenderers;
	//movement
	private float movementSpeed2;
	private Quaternion rotationDifference;
	//distance from player
	private float dist;
	private float distY;
	//angle from player
	private float playerAngle;
	private float enemyAngle;
	private bool playerInView;
	//attacked by player
	private float hitCount;
	private float lastHitCount;
	private float attackPressedTimer;
	private bool attackPressed;
	private bool attackButtonPressed;
	private bool playerCanAttack;
	private bool knockedBack;
	private Vector3 knockedBackDirection;
	private Vector3 dir;
	private Vector3 lastPosBeforePlayerDeath;
	
	//health bar hierarchy
	private GameObject enemyHealth;
		private GameObject outline;
			private GameObject black;
		private GameObject healthBarObject;
			private GameObject red;
			private GameObject healthObject;
				private GameObject green;
	//private health bar variables
	private Material outlineMaterial;
	private Material healthMaterial;
	private Material noHealthMaterial;
	private float feetPosition;
	private float headPosition;
	private float enemyHeight;
	private float showHealthBarAfterDeathTimer;
	private Vector3 healthBarBeforeDeathPos;
	//no class name health bar variables
	private Transform playerCamera;
	private float healthBarUpAmount;
	private float healthBarOverallHeight;
	private float healthBarOverallWidth;
	private float outlineHeight;
	private float outlineWidth;
	private float healthHeight;
	private float healthWidth;
	private float secondsToShowHealthBarAfterEnemyDeath;
	
	public LayerMask collisionLayers = ~(1 << 2); //the layers that the detectors (raycasts/linecasts) will collide with

    Collider m_Collider;

    void Start ()
	{
        m_Collider = GetComponent<Collider>();

        //setting respawn location and rotation
        lastPosBeforePlayerDeath = transform.position;
		if (respawn.respawnAtStartLocationAndRotation){
			respawnLocation = transform.position;
			respawnRotation = transform.rotation;
		}
		else {
			respawnLocation = respawn.respawnLocation;
			respawnRotation = Quaternion.Euler(respawn.respawnRotation.x, respawn.respawnRotation.y, respawn.respawnRotation.z);
		}
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Squisher"))
        {
        squished = 1;
        movement.movementSpeed = 0;
        movementSpeed2 = 0;
        movement.rotationSpeed = 0;
        movement.distanceToFacePlayerFrom = 0;
        movement.distanceToChasePlayerFrom = 0; 
        myAnimationController.SetBool("Squished", true);
        dead = true;
        this.GetComponent<CapsuleCollider>().enabled = false;
           this.GetComponent<BoxCollider>().enabled = false;
            this.GetComponent<Rigidbody>().isKinematic = true;
            playerhello.GetComponent<PlayerController>().gravity = -100;






        }

    }

    void FixedUpdate ()
	{
		
		SettingVariables();

        Squish();

        GetAngleBetweenPlayer();
		
		GetDistanceFromPlayer();
		
		RotateTowardsPlayer();
		
		RotateTowardsPlayerSideScrolling();
		
		SetHealthBarDeathPosition();
		
		LockingAxisForSideScrolling();
		
		MovingEnemy();
		
		AttackedByPlayer();
		
		RegainHealth();
		
		EnemyDeath();
		
		KnockBack();
		
		CreatingEnemyHealthBar();
		
		StabilizeEnemy();
		
	}

    void Squish()
    {
    

        if (myAnimationController.GetCurrentAnimatorStateInfo(0).IsName("BotDead"))
        {
            gameObject.SetActive(false);
        }

    }

    void SettingVariables () {
		
		//setting variables
		//attack
		attackPower = attack.attackPower;
		if (damage.acquirePlayerAttackButtonFromPlayerIfPossible && player.GetComponent<PlayerController>()){
			playerAttackButton = player.GetComponent<PlayerController>().attacking.attackInputButton;
		}
		else {
			playerAttackButton = damage.playerAttackButton;
		}
		//health
		maximumHealth = health.maximumHealth;
		regainHealthOverTime = health.regainHealthOverTime;
		healthToRegain = health.healthToRegain;
		timeNeededToRegainHealth = health.timeNeededToRegainHealth;
		minimumDistanceFromPlayerToRegainHealth = health.minimumDistanceFromPlayerToRegainHealth;
		//respawn
		allowRespawn = respawn.allowRespawn;
		respawnWaitTime = respawn.respawnWaitTime;
		if (!respawn.respawnAtStartLocationAndRotation){
			respawnLocation = respawn.respawnLocation;
			respawnRotation = Quaternion.Euler(respawn.respawnRotation.x, respawn.respawnRotation.y, respawn.respawnRotation.z);
		}
		minimumDistanceFromPlayerToRespawn = respawn.minimumDistanceFromPlayerToRespawn;
		//camera
		if (health.healthBar.playerCamera == null){
			if (Camera.main.transform != null){
				playerCamera = Camera.main.transform;
			}
		}
		else {
			playerCamera = health.healthBar.playerCamera;
		}
		
	}
	
	void GetAngleBetweenPlayer () {
		
		//getting angle from player
		playerAngle = Vector3.Angle(transform.position - player.position, player.forward);
		enemyAngle = Vector3.Angle(player.position - transform.position, transform.forward);
		
		if (enemyAngle <= movement.viewpointPlayerMustBeInToFollow){
			playerInView = true;
		}
		else if (dist > movement.distanceToChasePlayerFrom || distY > 3){
			playerInView = false;
		}
		
	}
	
	void GetDistanceFromPlayer () {
		
		//move towards player
		dir = new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position;
		dir.Normalize();
		dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(player.position.x - transform.position.x),2) + Mathf.Pow(Mathf.Abs(player.position.z - transform.position.z),2));
		distY = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(player.position.y - transform.position.y),2));
		
	}
	
	void RotateTowardsPlayer () {
	
		//rotate towards player
		if (dist <= movement.distanceToFacePlayerFrom && distY <= 3 && (playerInView || (movement.sideScrolling.lockMovementOnZAxis || movement.sideScrolling.lockMovementOnXAxis))){
			
			//getting rotation on z-axis (x-axis is locked)
			if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
				//if our rotation is closer to the left, set the bodyRotation to the left
				if (player.transform.position.z < transform.position.z){
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 180.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 179.999f, Quaternion.LookRotation(dir).z);
					}
					
				}
				//if our rotation is closer to the right, set the bodyRotation to the right
				else {
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -0.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 0.001f, Quaternion.LookRotation(dir).z);
					}
				}
			}
			//getting rotation on x-axis (z-axis is locked)
			else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (player.transform.position.x >= transform.position.x){
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 90.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 89.999f, Quaternion.LookRotation(dir).z);
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -90.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -89.999f, Quaternion.LookRotation(dir).z);
					}
				}
			}
			//neither (or both) axis are locked
			else {
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), movement.rotationSpeed * Time.deltaTime);
			}
		}
		
	}
	
	void RotateTowardsPlayerSideScrolling () {
		
		if (dist > movement.distanceToFacePlayerFrom){
			//setting enemy's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
			float yRotationValue;
			if (transform.eulerAngles.y > 180){
				yRotationValue = transform.eulerAngles.y - 360;
			}
			else {
				yRotationValue = transform.eulerAngles.y;
			}
			//getting rotation on z-axis (x-axis is locked)
			if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 90){
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 180.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 179.999f, Quaternion.LookRotation(dir).z);
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -0.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 0.001f, Quaternion.LookRotation(dir).z);
					}
				}
			}
			//getting rotation on x-axis (z-axis is locked)
			else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 0){
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 90.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, 89.999f, Quaternion.LookRotation(dir).z);
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					if (movement.sideScrolling.rotateInwards){
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -90.001f, Quaternion.LookRotation(dir).z);
					}
					else {
						rotationDifference = Quaternion.Euler(Quaternion.LookRotation(dir).x, -89.999f, Quaternion.LookRotation(dir).z);
					}
				}
			}
		}
		//setting rotation if side-scrolling
		if (movement.sideScrolling.lockMovementOnXAxis || movement.sideScrolling.lockMovementOnZAxis){
			transform.rotation = Quaternion.Slerp(transform.rotation, rotationDifference, movement.rotationSpeed * Time.deltaTime);
		}
		
	}
	
	void SetHealthBarDeathPosition () {
		
		//setting enemy health bar position when enemy is dead
		if (dead && enemyHealth != null){
			enemyHealth.transform.position = healthBarBeforeDeathPos;
		}
	}
		
	void LockingAxisForSideScrolling () {
	
		//locking axis for side-scrolling
		if (movement.sideScrolling.lockMovementOnXAxis){
			transform.position = new Vector3(movement.sideScrolling.xValue, transform.position.y, transform.position.z);
		}
		if (movement.sideScrolling.lockMovementOnZAxis){
			transform.position = new Vector3(transform.position.x, transform.position.y, movement.sideScrolling.zValue);
		}
		
	}
	
	void MovingEnemy () {
		
		//finding the edge in front of the enemy
		edgeDetectorHeight2 = movement.edgeDetectorHeight*transform.up;
		edgeDetectorForward2 = movement.edgeDetectorForward*transform.forward;
		minimumEdgeHeight2 = movement.minimumEdgeHeight*transform.up;
		if (movement.showEdgeDetectors){
			Debug.DrawLine(transform.position + (transform.up*0.2f) + edgeDetectorHeight2, transform.position + (transform.up*0.2f) + edgeDetectorHeight2 + transform.forward + edgeDetectorForward2, Color.blue);
			Debug.DrawLine(transform.position + (transform.up*0.2f) + edgeDetectorHeight2 + transform.forward + edgeDetectorForward2, transform.position - (transform.up*0.8f) - minimumEdgeHeight2 + transform.forward + edgeDetectorForward2, Color.blue);
		}
		RaycastHit hit = new RaycastHit();
		//movement
		if (!movement.stopAtEdges || (Physics.Linecast(transform.position + (transform.up*0.2f) + edgeDetectorHeight2, transform.position + (transform.up*0.2f) + edgeDetectorHeight2 + transform.forward + edgeDetectorForward2, out hit, collisionLayers) || Physics.Linecast(transform.position + (transform.up*0.2f) + edgeDetectorHeight2 + transform.forward + edgeDetectorForward2, transform.position - (transform.up*0.8f) + minimumEdgeHeight2 + transform.forward + edgeDetectorForward2, out hit, collisionLayers))){
			
			//getting direction to move towards
			Vector3 movementVector;
			if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
				movementVector = new Vector3(movement.sideScrolling.xValue, transform.forward.y, transform.forward.z);
			}
			if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
				movementVector = new Vector3(transform.forward.x, transform.forward.y, movement.sideScrolling.zValue);
			}
			else {
				movementVector = transform.forward;
			}
			
			//moving enemy
			if (dist <= movement.distanceToChasePlayerFrom && distY <= 3 && playerInView){
				
				//animating movement
				if (GetComponent<Animator>()){
					GetComponent<Animator>().SetBool("Walking", true);
				}
				
				movementSpeed2 = Mathf.Lerp(movementSpeed2, movement.movementSpeed, 8 * Time.deltaTime);
				if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * (movementSpeed2*Time.deltaTime));
				}
				else if (GetComponent<CharacterController>()){
					GetComponent<CharacterController>().Move(movementVector * (movementSpeed2*Time.deltaTime));
				}
				
			}
			else {
				if (GetComponent<Animator>()){
					GetComponent<Animator>().SetBool("Walking", false);
				}
				movementSpeed2 = 0;
			}
			
		}
		
	}
	
	void AttackedByPlayer () {
		
		//if the "Attack" button was pressed, attackPressed equals true
		if ((Input.GetButton(playerAttackButton) && !attackButtonPressed || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().attackFinishedLastUpdate)
		&& (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.Joystick1Button8) && !player.GetComponent<PlayerController>().crouchCancelsAttack && !player.GetComponent<PlayerController>().currentlyOnWall && !player.GetComponent<PlayerController>().currentlyClimbingWall)){
			attackPressedTimer = 0.0f;
			attackPressed = true;
			playerCanAttack = true;
			attackButtonPressed = true;
		}
		else {
			attackPressedTimer += 0.02f;
			if (!Input.GetButton(playerAttackButton)){
				attackButtonPressed = false;
			}
		}
		
		//wait 0.2 seconds for attackPressed to become false
		//this allows the player to press the "Attack" button slightly early and still attack even if they were not close enough to the enemy
		if (attackPressedTimer > 0.2f){
			attackPressed = false;
		}
		
	}
	
	void RegainHealth () {
		
		//regaining health (if the enemy is allowed to)
		if (regainHealthOverTime){
			//increasing regainHealthTimer
			if (!dead){
				if (dist >= minimumDistanceFromPlayerToRegainHealth){
					regainHealthTimer += 0.02f;
				}
			}
			else {
				regainHealthTimer = 0.0f;
			}
			
			//regaining health
			if (regainHealthTimer > timeNeededToRegainHealth){
				if (healthToRegain < maximumHealth){
					hitCount -= healthToRegain;
				}
				else {
					hitCount = 0;
				}
			}
		}
		else {
			regainHealthTimer = 0.0f;
		}
		
	}
	
	void EnemyDeath () {
		
		childRenderers = GetComponentsInChildren<Renderer>();
		
		//if enemy is killed
		if (hitCount >= maximumHealth){
			if (!dead){
				if (damage.hurtEffect != null){
					Instantiate(damage.hurtEffect, transform.position, transform.rotation);
				}
				if (damage.deathEffect != null){
					Instantiate(damage.deathEffect, transform.position, damage.deathEffect.transform.rotation);
				}
			}
			
			if (!allowRespawn && (secondsToShowHealthBarAfterEnemyDeath <= 0 || !health.healthBar.showHealthBar)){
				Destroy(gameObject);
			}
			else {
				
				if (!dead && enemyHealth != null){
					healthBarBeforeDeathPos = enemyHealth.transform.position;
				}
				dead = true;
				showHealthBarAfterDeathTimer += Time.deltaTime;
				if (Vector3.Distance(player.transform.position, respawnLocation) >= minimumDistanceFromPlayerToRespawn){
					deathTimer += Time.deltaTime;
				}
				if (GetComponent<Collider>() && GetComponent<Collider>().enabled){
					colliderOnOff = true;
					GetComponent<Collider>().enabled = false;
				}
				foreach (Renderer renderer in childRenderers){
					if (renderer && renderer.enabled){
						rendererOnOff = true;
						renderer.enabled = false;
					}
				}
				if (dead && showHealthBarAfterDeathTimer >= secondsToShowHealthBarAfterEnemyDeath){
					//disabling health bar renderers
					if (allowRespawn){
						if (health.healthBar.showHealthBar){
							if (black != null && black.GetComponent<Renderer>()){
								black.GetComponent<Renderer>().enabled = false;
							}
							if (red != null && red.GetComponent<Renderer>()){
								red.GetComponent<Renderer>().enabled = false;
							}
							if (green != null && green.GetComponent<Renderer>()){
								green.GetComponent<Renderer>().enabled = false;
							}
						}
					}
					//destroy enemy
					else {
						Destroy(gameObject);
					}
				}
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					ccOnOff = true;
					GetComponent<CharacterController>().enabled = false;
				}
				if (GetComponent<Rigidbody>() && GetComponent<Rigidbody>().useGravity){
					rbOnOff = true;
					GetComponent<Rigidbody>().useGravity = false;
				}
				transform.position = respawnLocation;
				transform.rotation = respawnRotation;
				if (enemyHealth != null){
					enemyHealth.transform.position = healthBarBeforeDeathPos;
				}
				knockedBackDirection = Vector3.zero;
				
				//turning everything back on, and respawning
				if (deathTimer > respawnWaitTime){
					
					if (colliderOnOff){
						GetComponent<Collider>().enabled = true;
						colliderOnOff = false;
					}
					if (rendererOnOff){
						foreach (Renderer renderer in childRenderers){
							renderer.enabled = true;
						}
						//enabling health bar renderers
						if (health.healthBar.showHealthBar){
							if (black != null && black.GetComponent<Renderer>()){
								black.GetComponent<Renderer>().enabled = true;
							}
							if (red != null && red.GetComponent<Renderer>()){
								red.GetComponent<Renderer>().enabled = true;
							}
							if (green != null && green.GetComponent<Renderer>()){
								green.GetComponent<Renderer>().enabled = true;
							}
						}
						rendererOnOff = false;
					}
					if (ccOnOff){
						GetComponent<CharacterController>().enabled = true;
						ccOnOff = false;
					}
					if (rbOnOff){
						GetComponent<Rigidbody>().useGravity = true;
						rbOnOff = false;
					}
					if (respawn.respawnEffect != null){
						Instantiate(respawn.respawnEffect, transform.position, transform.rotation);
					}
					dead = false;
					hitCount = 0;
					deathTimer = 0;
					showHealthBarAfterDeathTimer = 0;
					
				}
				
			}
		}
		
	}
	
	void KnockBack () {
		
		//getting knocked back after being hit
		if (knockedBack && damage.knockBackFactor > 0){
			
			movementSpeed2 = 0;
			if (Vector3.Distance(knockedBackDirection, Vector3.zero) > 0.1f){
				knockedBackDirection = Vector3.Slerp(knockedBackDirection, Vector3.zero, 8 * Time.deltaTime);
				if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + knockedBackDirection * Time.deltaTime);
				}
				else if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(knockedBackDirection * Time.deltaTime);
				}
			}
			else {
				knockedBack = false;
			}
		}
		
	}
	
	void CreatingEnemyHealthBar () {
		
		//health bar
		if (health.healthBar.showHealthBar){
			
			//setting health bar variables
			SetHealthBarVariables();
			
			//creating health bar materials from colors
			CreateHealthBarMaterials();
			
			//creating health bar
			CreateHealthBar();
			
			//updating health bar materials
			UpdateHealthBarMaterials();
			
			//updating health bar sizes
			UpdateHealthBarSizes();
			
			//getting height of enemy
			GetEnemyHeight();
			
			//positioning and rotating health bar
			PositionHealthBar();
			
		}
		//removing health bar
		else {
			if (enemyHealth != null){
				Destroy(enemyHealth);
			}
		}
		
	}
	
	void SetHealthBarVariables () {
		
		//setting health bar variables
		healthBarUpAmount = health.healthBar.healthBarUpAmount - 0.38f;
		healthBarOverallHeight = health.healthBar.healthBarOverallHeight;
		healthBarOverallWidth = health.healthBar.healthBarOverallWidth;
		outlineHeight = health.healthBar.outlineHeight - 0.37f;
		outlineWidth = health.healthBar.outlineWidth - 0.35f;
		healthHeight = health.healthBar.healthHeight - 0.5f;
		healthWidth = health.healthBar.healthWidth - 0.37f;
		secondsToShowHealthBarAfterEnemyDeath = health.healthBar.secondsToShowHealthBarAfterEnemyDeath;
	
	}
	
	void CreateHealthBarMaterials () {
		
		//creating health bar materials from colors
		//outline
		if (health.healthBar.healthBarMaterials.outlineMaterial != null){
			outlineMaterial = health.healthBar.healthBarMaterials.outlineMaterial;
			outlineMaterial.color = health.healthBar.outlineColor;
		}
		//health
		if (health.healthBar.healthBarMaterials.healthMaterial != null){
			healthMaterial = health.healthBar.healthBarMaterials.healthMaterial;
			healthMaterial.color = health.healthBar.healthColor;
		}
		//no health
		if (health.healthBar.healthBarMaterials.noHealthMaterial != null){
			noHealthMaterial = health.healthBar.healthBarMaterials.noHealthMaterial;
			noHealthMaterial.color = health.healthBar.noHealthColor;
		}
	
	}
	
	void CreateHealthBar () {
		
		//creating health bar
		if (enemyHealth == null){
			enemyHealth = new GameObject();
			enemyHealth.name = "EnemyHealth";
		}
			if (outline == null){
				outline = new GameObject();
				outline.name = "Outline";
				outline.transform.parent = enemyHealth.transform;
			}
				if (black == null){
					black = GameObject.CreatePrimitive(PrimitiveType.Quad);
					black.name = "Black";
					black.transform.parent = outline.transform;
					black.transform.localScale = new Vector3(1, 0.1829655f, 1);
					if (outlineMaterial != null){
						black.GetComponent<Renderer>().material = outlineMaterial;
					}
					Destroy(black.GetComponent<Collider>());
				}
			if (healthBarObject == null){
				healthBarObject = new GameObject();
				healthBarObject.name = "HealthBar";
				healthBarObject.transform.parent = enemyHealth.transform;
			}
				if (red == null){
					red = GameObject.CreatePrimitive(PrimitiveType.Quad);
					red.name = "Red";
					red.transform.parent = healthBarObject.transform;
					red.transform.localScale = new Vector3(0.9552082f, 0.1394147f, 1);
					red.transform.localPosition = new Vector3(0, 0, -0.001f);
					if (noHealthMaterial != null){
						red.GetComponent<Renderer>().material = noHealthMaterial;
					}
					Destroy(red.GetComponent<Collider>());
				}
				if (healthObject == null){
					healthObject = new GameObject();
					healthObject.name = "HealthObject";
					healthObject.transform.parent = healthBarObject.transform;
					healthObject.transform.localPosition = new Vector3(-0.4774f, 0, -0.002f);
				}
					if (green == null){
						green = GameObject.CreatePrimitive(PrimitiveType.Quad);
						green.name = "Green";
						green.transform.parent = healthObject.transform;
						green.transform.localScale = new Vector3(0.9552084f, 0.1394147f, 1);
						green.transform.localPosition = new Vector3(0.4774f, 0, -0.002f);
						if (healthMaterial != null){
							green.GetComponent<Renderer>().material = healthMaterial;
						}
						Destroy(green.GetComponent<Collider>());
					}
		//end of hierarchy
	
	}
	
	void UpdateHealthBarMaterials () {
		
		//updating health bar materials
		if (black != null && outlineMaterial != null){
			black.GetComponent<Renderer>().material = outlineMaterial;
		}
		if (red != null && noHealthMaterial != null){
			red.GetComponent<Renderer>().material = noHealthMaterial;
		}
		if (green != null && healthMaterial != null){
			green.GetComponent<Renderer>().material = healthMaterial;
		}
	
	}
	
	void UpdateHealthBarSizes () {
		
		//updating health bar sizes
		if (enemyHealth != null){
			enemyHealth.transform.localScale = new Vector3(healthBarOverallWidth / transform.localScale.x, healthBarOverallHeight / transform.localScale.y, 1  / transform.localScale.z);
		}
		if (outline != null){
			outline.transform.localScale = new Vector3(outlineWidth, outlineHeight, 1);
		}
		if (healthBarObject != null){
			healthBarObject.transform.localScale = new Vector3(healthWidth, healthHeight, 1);
		}
		if (healthObject != null){
			if (maximumHealth > 0 && hitCount <= maximumHealth){
				healthObject.transform.localScale = new Vector3((maximumHealth - hitCount)/maximumHealth, 1, 1);
			}
			else {
				healthObject.transform.localScale = new Vector3(0, 1, 1);
			}
		}
	
	}
	
	void GetEnemyHeight () {
		
		//getting height of enemy
		if (transform.GetComponent<CapsuleCollider>()){
			feetPosition = GetComponent<CapsuleCollider>().bounds.min.y;
			headPosition = GetComponent<CapsuleCollider>().bounds.max.y;
			if (transform.localScale.y >= 1){
				enemyHeight = (headPosition - feetPosition) * 1.4f;
			}
			else {
				enemyHeight = (transform.localScale.y * GetComponent<CapsuleCollider>().height) * 1.4f;
			}
		}
		else if (transform.GetComponent<SphereCollider>()){
			feetPosition = GetComponent<SphereCollider>().bounds.min.y;
			headPosition = GetComponent<SphereCollider>().bounds.max.y;
			if (transform.localScale.y >= 1){
				enemyHeight = (headPosition - feetPosition) * 1.4f;
			}
			else {
				enemyHeight = ((transform.localScale.y/1.83f * GetComponent<SphereCollider>().radius) * 4.2f) + 0.7f;
			}
		}
		else if (transform.GetComponent<BoxCollider>()){
			feetPosition = GetComponent<BoxCollider>().bounds.min.y;
			headPosition = GetComponent<BoxCollider>().bounds.max.y;
			if (transform.localScale.y >= 1){
				enemyHeight = (headPosition - feetPosition) * 1.4f;
			}
			else {
				enemyHeight = transform.localScale.y * GetComponent<BoxCollider>().size.y;
			}
		}
		else if (GetComponent<CharacterController>()){
			feetPosition = GetComponent<CharacterController>().bounds.min.y;
			headPosition = GetComponent<CharacterController>().bounds.max.y;
			if (transform.localScale.y >= 1){
				enemyHeight = (headPosition - feetPosition) * 1.4f;
			}
			else {
				enemyHeight = (transform.localScale.y * GetComponent<CharacterController>().height) * 1.4f;
			}
		}
		else {
			enemyHeight = transform.localScale.y;
		}
	
	}
	
	void PositionHealthBar () {
		
		//positioning and rotating health bar
		if (enemyHealth != null){
			//as long as enemy is not dead, position the health bar over the enemy
			if (!dead){
				enemyHealth.transform.position = transform.position + new Vector3 (0, enemyHeight + healthBarUpAmount, 0);
			}
			else {
				enemyHealth.transform.position = healthBarBeforeDeathPos;
			}
			//set the enemy as the parent to the health bar (as long as none of the health bar objects have a collider)
			if (!enemyHealth.GetComponent<Collider>() && !outline.GetComponent<Collider>() && !black.GetComponent<Collider>()
			&& !healthBarObject.GetComponent<Collider>() && !red.GetComponent<Collider>() && !healthObject.GetComponent<Collider>() && !green.GetComponent<Collider>()){
				enemyHealth.transform.parent = transform;
			}
			else {
				enemyHealth.transform.parent = null;
			}
			//rotating health bar
			float localX = ((playerCamera.transform.position.y - enemyHealth.transform.position.y) - 0.5230548f)*5;
			enemyHealth.transform.rotation = playerCamera.transform.rotation;
			enemyHealth.transform.localRotation *= Quaternion.Euler(localX, 0, 0);
		}
		
	}
	
	void StabilizeEnemy () {
		
		//keeping enemy from going with the player when the player respawns
		if (player.GetComponent<Health>() && player.GetComponent<Health>().enabled
		&& player.GetComponent<Health>().currentHealth <= 0){
			transform.position = lastPosBeforePlayerDeath;
		}
		else {
			lastPosBeforePlayerDeath = transform.position;
		}
		
		//setting enemy health bar position when enemy is dead
		if (dead && enemyHealth != null){
			enemyHealth.transform.position = healthBarBeforeDeathPos;
		}
		
	}
	
	void LateUpdate () {
		
		if (attackPressed){
			if (playerCanAttack){
				
				if (dist <= damage.damageRadius && distY <= 7 && hitCount < maximumHealth && lastHitCount == hitCount){
					if (playerAngle < damage.playerViewpointEnemyMustBeInToGetHit){
						
						if (damage.hurtEffect != null){
							Instantiate(damage.hurtEffect, transform.position, transform.rotation);
						}
						regainHealthTimer = 0.0f;
						
						playerInView = true;
						
						if (playerAngle < 60){
							knockedBackDirection = -transform.forward*damage.knockBackFactor;
						}
						else {
							knockedBackDirection = player.transform.forward*damage.knockBackFactor;
						}
						knockedBack = true;
						
						//animating getting knocked back
						if (GetComponent<Animator>()){
							GetComponent<Animator>().CrossFade("BotKnockBack", 0f, -1, 0f);
						}
						
						if (player.GetComponent<PlayerController>() != null && player.GetComponent<PlayerController>().enabled){
							//getting attack values from the player (when not crouching)
							if (!player.GetComponent<PlayerController>().crouching){
								if (player.GetComponent<PlayerController>().currentAttackNumber - 1 <= player.GetComponent<PlayerController>().totalAttackNumber && player.GetComponent<PlayerController>().currentAttackNumber - 1 > 0
								&&  player.GetComponent<PlayerController>().attacksToPerform[player.GetComponent<PlayerController>().currentAttackNumber - 1] >= 0){
									hitCount += player.GetComponent<PlayerController>().attacksToPerform[player.GetComponent<PlayerController>().currentAttackNumber - 1];
								}
								else if (player.GetComponent<PlayerController>().currentAttackNumber > 0){
									hitCount += player.GetComponent<PlayerController>().attacksToPerform[0];
								}
							}
							//getting attack values from the player (when crouching)
							else {
								hitCount += player.GetComponent<PlayerController>().attacking.crouch.crouchAttackStrength;
							}
						}
						else {
							hitCount ++;
						}
						
						playerCanAttack = false;
					}
				}
				
			}
			
		}
		else {
			playerCanAttack = true;
		}
		lastHitCount = hitCount;
		
		//setting enemy health bar position when enemy is dead
		if (dead && enemyHealth != null){
			enemyHealth.transform.position = healthBarBeforeDeathPos;
		}
		
	}
	
	void OnDisable () {
		
		if (enemyHealth != null){
			Destroy(enemyHealth);
		}
		
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		if (!enabled) return;
		if (player.GetComponent<Health>() && player.GetComponent<Health>().enabled
		&& player.GetComponent<Health>().canDamage && player.GetComponent<Health>().canDamage2){
			
			//damage player
			if (hit.gameObject.name == player.gameObject.name){
				player.GetComponent<Health>().enemyAttackPower = attackPower;
				player.GetComponent<Health>().applyDamage = true;
				player.GetComponent<Health>().blink = true;
			}
			
		}
	}
	
	void OnCollisionStay (Collision hit) {
		if (!enabled) return;
		if (player.GetComponent<Health>() && player.GetComponent<Health>().enabled
		&& player.GetComponent<Health>().canDamage && player.GetComponent<Health>().canDamage2){
			
			//damage player
			if (hit.gameObject.name == player.gameObject.name){
				player.GetComponent<Health>().enemyAttackPower = attackPower;
				player.GetComponent<Health>().applyDamage = true;
				player.GetComponent<Health>().blink = true;
			}
			
		}
	}
	
}