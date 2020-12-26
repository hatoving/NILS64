/// All in One Game Kit - Easy Ledge Climb Character System
/// WallClimbController_Standalone.cs
///
/// As long as the player has a CharacterController or Rigidbody component, this script allows the player to:
/// 1. Climb ladders and walls (Climbing).
/// 2. Enable or disable scripts when grabbing on to a ladder or wall, as well as when letting go of one (Scripts To Enable On Grab/Scripts To Disable On Grab).
///		-You should always disable any other scripts that affect the movement or rotation of the player. If you do not, this script will not work properly.
///
/// NOTE: *You should always set a layer for your player so that you can disable collisions with that layer (by unchecking it in the script's Collision Layers).
///	If you do not, the raycasts and linecasts will collide with the player himself and keep the script from working properly!*
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class WallClimbController_Standalone : MonoBehaviour {
	
	//Grounded
	[System.Serializable]
	public class Grounded {
		public float slopeLimit = 25.00f; //the maximum angle of a slope you can stand on without sliding down
		public bool showGroundDetectionRays; //shows the rays that detect whether the player is grounded or not
		public float maxGroundedHeight = 0.2f; //the maximum height of the ground the ground detectors can hit to be considered grounded
		public float maxGroundedRadius = 0.2f; //the maximum radius of the area ground detectors can hit to be considered grounded
		public float maxGroundedDistance = 0.2f; //the maximum distance you can be from the ground to be considered grounded
		public bool currentlyGrounded; //determines if player is currently grounded/on the ground
	}
	
	//Climbing
	[System.Serializable]
	public class Climbing {
		
		public string climbableTag = "Wall"; //the tag of a climbable object
		public bool climbVertically = true; //determines whether or not the player is allowed to climb vertically
		public bool climbHorizontally = true; //determines whether or not the player is allowed to climb horizontally
		public float climbMovementSpeed = 4; //how quickly the player climbs on walls
		public float climbRotationSpeed = 10; //how quickly the player rotates on walls
		public bool snapToCenterOfObject = false; //snaps the player to the middle (along the x and z-axis) of the climbable object (most useful for ladders)
		public bool moveInBursts = true; //move in bursts (while on a climbable object)
		public float burstLength = 1; //the amount of time a movement burst lasts
		public bool stayUpright = false; //determines whether or not the player can rotate up and down
		public float distanceToPushOffAfterLettingGo = 1.75f; //the distance the player pushes off of a ladder/wall after letting go
		public float rotationToClimbableObjectSpeed = 6; //how quickly the player rotates onto a wall to climb
		public bool showClimbingDetectors = false; //determines whether to show or hide the detectors that allow climbing
		public float climbingSurfaceDetectorsUpAmount = 0.0f; //moves the rays that detect the surface of a wall up and down
		public float climbingSurfaceDetectorsHeight = 0.0f; //changes the height of the rays that detect the surface of a wall
		public float climbingSurfaceDetectorsLength = 0.0f; //changes the length of the rays that detect the surface of a wall
		public bool showEdgeOfObjectDetctors = false; //determines whether or not to show the detectors that determine where the top and bottom of a climbable object is
		public float topNoSurfaceDetectorHeight = 0.0f; //the height of the detector that determines if there is no surface detected at the top of the climbable object, so that the player can pull up or stop before climbing any higher
		public float bottomNoSurfaceDetectorHeight = 0.0f; //the height of the detector that determines if there is no surface detected at the bottom of the climbable object, so that the player can drop off or stop before climbing any lower
		public float topAndBottomNoSurfaceDetectorsWidth = 0.0f; //the width of the detectors that determines if there is no surface detected at the top and bottom of the climbable object
		public float sideNoSurfaceDetectorsHeight = 0.0f; //the height of the detectors that determines if there is no surface detected at the sides of the climbable object
		public float sideNoSurfaceDetectorsWidth = 0.0f; //the width of the detectors that determines if there is no surface detected at the sides of the climbable object
		public bool stopAtSides = true; //keeps player from climbing any further sideways once he has reached the side
		public bool dropOffAtBottom = false; //allows player to drop off of a climbable object once he has reached the bottom
		public bool dropOffAtFloor = true; //allows player to drop off of a climbable object once he has reached the floor
		public bool pullUpAtTop = true; //allows player to pull up and over a climbable object once he has reached the top
		public float pullUpSpeed = 4; //the speed the player pulls up and over ledges once he has reached the top of a climbable object
		public bool showPullUpDetector = false; //determines whether or not to show the detector that determines where the player pulls up to
		public float pullUpLocationForward = 0.0f; //the forward distance of the detector that determines where the player pulls up to
		
		[System.Serializable]
		public class WalkingOffOfClimbableSurface {
			public bool allowGrabbingOnAfterWalkingOffLedge = true; //allows the player to grab on to a climbable surface under the ledge that he is walking off of
			public bool showWalkingOffLedgeRays = false; //shows the rays that detect if the player is walking off of a ledge
			public float spaceInFrontNeededToGrabBackOn = 0.0f; //the amount of space in front of the player needed to grab on to a climbable object under a ledge
			public float spaceBelowNeededToGrabBackOnHeight = 0.0f; //the height of the detectors that determine the amount of space below the player needed to grab on to a climbable object under a ledge
			public float spaceBelowNeededToGrabBackOnForward = 0.0f; //the forward distance of the detectors that determine the amount of space below the player needed to grab on to a climbable object under a ledge
			public float firstSideLedgeDetectorsHeight = 0.0f; //the height of the first set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float secondSideLedgeDetectorsHeight = 0.0f; //the height of the second set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float thirdSideLedgeDetectorsHeight = 0.0f; //the height of the third set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float sideLedgeDetectorsWidth = 0.0f; //the width of the detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float sideLedgeDetectorsLength = 0.0f; //the length of the detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float grabBackOnLocationHeight = 0.0f; //the height of the detectors that determine where the player will grab on to
			public float grabBackOnLocationWidth = 0.0f; //the height of the detectors that determine where the player will grab on to
			public float grabBackOnLocationForward = 0.0f; //the forward distance of the detectors that determine where the player will grab on to
		}
		public WalkingOffOfClimbableSurface walkingOffOfClimbableSurface = new WalkingOffOfClimbableSurface(); //variables that detect whether the player has walked off a ledge and can grab on to a ladder
		
		public string[] scriptsToEnableOnGrab; //scripts to enable when the player grabs on to a wall (scripts disable when the player lets go of a wall)
		public string[] scriptsToDisableOnGrab = {"PlayerController", "LedgeClimbController"}; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
		
		public bool pushAgainstWallIfPlayerIsStuck = true; //if the script considers the player stuck, the player pushes himself away from the wall until he is free
		
	}
	
	[System.Serializable]
	public class SideScrolling {
		public float movementSpeedIfAxisLocked = 6.0f; //the move speed of the player if one of the axis are locked
		public bool lockMovementOnZAxis = false; //locks the movement of the player on the z-axis
		public float zValue = 0; //the permanent z-value of the player if his movement on the z-axis is locked
		public bool lockMovementOnXAxis = false; //locks the movement of the player on the x-axis
		public float xValue = 0; //the permanent x-value of the player if his movement on the x-axis is locked
		public bool flipAxisRotation = false; //flips the player's rotation on the non-locked axis (it adds 180 degrees to the player's rotation)
		public bool rotateInwards = true; //when the player rotates from side to side, he rotates inward (so that you see his front side while he is rotating)
	}
	
	public Grounded grounded = new Grounded(); //variables that detect whether the player is grounded or not
	public Climbing[] climbing = new Climbing[1]; //variables that control the player's ladder and wall climbing
	public SideScrolling sideScrolling = new SideScrolling(); //variables that determine whether or not the player uses 2.5D side-scrolling
	
	//Grounded variables without class name
	private Vector3 maxGroundedHeight2;
	private float maxGroundedRadius2;
	private Vector3 maxGroundedDistanceDown;
	
	//Climbing variables without class name
	private Vector3 climbingSurfaceDetectorsUpAmount2;
	private float climbingSurfaceDetectorsHeight2;
	private float climbingSurfaceDetectorsLength2;
	private float distanceToPushOffAfterLettingGo2;
	private float rotationToClimbableObjectSpeed2;
	private bool climbHorizontally2;
	private bool climbVertically2;
	private float climbMovementSpeed2;
	private float climbRotationSpeed2;
	private bool moveInBursts;
	private float burstLength;
	[HideInInspector]
	public string climbableTag2;
	private bool stayUpright2;
	private bool snapToCenterOfObject2;
	private Vector3 bottomNoSurfaceDetectorHeight2;
	private Vector3 topNoSurfaceDetectorHeight2;
	private Vector3 topAndBottomNoSurfaceDetectorsWidth2;
	private float sideNoSurfaceDetectorsHeight2;
	private Vector3 sideNoSurfaceDetectorsWidth2;
	private float sideNoSurfaceDetectorsWidthTurnBack2;
	private bool stopAtSides2;
	private bool dropOffAtBottom2;
	private bool dropOffAtFloor2;
	private bool pullUpAtTop2;
	private float pullUpSpeed;
	private Vector3 pullUpLocationForward2;
	private bool pushAgainstWallIfPlayerIsStuck2;
	//walk off ledge detectors
	private bool allowGrabbingOnAfterWalkingOffLedge2;
	private Vector3 spaceInFrontNeededToGrabBackOn2;
	private Vector3 spaceBelowNeededToGrabBackOnHeight2;
	private Vector3 spaceBelowNeededToGrabBackOnForward2;
	private Vector3 firstSideLedgeDetectorsHeight2;
	private Vector3 secondSideLedgeDetectorsHeight2;
	private Vector3 thirdSideLedgeDetectorsHeight2;
	private Vector3 sideLedgeDetectorsWidth2;
	private Vector3 sideLedgeDetectorsLength2;
	//climbing variables used for drawing
	private bool showClimbingDetectors3;
	private Vector3 climbingSurfaceDetectorsUpAmount3;
	private float climbingSurfaceDetectorsHeight3;
	private float climbingSurfaceDetectorsLength3;
	private bool showEdgeOfObjectDetctors3;
	private Vector3 bottomNoSurfaceDetectorHeight3;
	private Vector3 topNoSurfaceDetectorHeight3;
	private Vector3 topAndBottomNoSurfaceDetectorsWidth3;
	private float sideNoSurfaceDetectorsHeight3;
	private Vector3 sideNoSurfaceDetectorsWidth3;
	private bool showPullUpDetector3;
	private Vector3 pullUpLocationForward3;
	//walk off ledge then transition to climb variables
	private bool showWalkingOffLedgeRays3;
	private Vector3 spaceInFrontNeededToGrabBackOn3;
	private Vector3 firstSideLedgeDetectorsHeight3;
	private Vector3 secondSideLedgeDetectorsHeight3;
	private Vector3 thirdSideLedgeDetectorsHeight3;
	private Vector3 sideLedgeDetectorsWidth3;
	private Vector3 sideLedgeDetectorsLength3;
	private Vector3 spaceBelowNeededToGrabBackOnHeight3;
	private Vector3 spaceBelowNeededToGrabBackOnForward3;
	private Vector3 grabBackOnLocationHeight3;
	private Vector3 grabBackOnLocationWidth3;
	private Vector3 grabBackOnLocationForward3;
	
	//private movement variables
	private Vector3 pos; //position and collider bounds of the player
	private Vector3 moveDirection; //the direction that the player moves in
	private Vector3 directionVector; //the direction that the joystick is being pushed in
	private float slidingAngle; //the angle of the last slidable surface you collided with or are currently colliding with
	private Vector3 slidingVector; //the normal of the object you are colliding with
	[HideInInspector]
	public float bodyRotation; //the rotation that the player lerps to
	private float noCollisionTimer; //time since last collision
	
	//private on wall variables
	[HideInInspector]
	public bool currentlyOnWall;
	private bool onWallLastUpdate;
	private bool rbUsesGravity;
	private bool canChangeRbGravity;
	
	//private ladder and wall climbing variables
	[HideInInspector]
	public bool currentlyClimbingWall = false;
	[HideInInspector]
	public bool wallIsClimbable;
	[HideInInspector]
	public bool climbableWallInFront;
	private bool wallInFront;
	[HideInInspector]
	public bool finishedRotatingToWall;
	private float jumpedOffClimbableObjectTimer = 1.0f;
	private Vector3 jumpedOffClimbableObjectDirection;
	private Vector3 climbDirection;
	private bool downCanBePressed;
	private bool climbedUpAlready;
	private Vector3 colCenter;
	private Vector3 colTop;
	private bool aboveTop;
	private bool reachedTopPoint = false;
	private bool reachedBottomPoint = false;
	private bool reachedRightPoint = false;
	private bool reachedLeftPoint = false;
	private float climbingMovement;
	private bool beginClimbBurst;
	private bool switchedDirection;
	private float lastAxis;
	private float horizontalClimbSpeed;
	private float verticalClimbSpeed;
	private float arm;
	private Vector3 centerPoint;
	private float snapTimer = 1;
	private bool snappingToCenter;
	private bool startedClimbing;
	private bool firstTest;
	private bool secondTest;
	private bool thirdTest;
	private bool fourthTest;
	[HideInInspector]
	public bool fifthTest;
	private int i = 0;
	private int tagNum;
	//walking off ledge variables
	[HideInInspector]
	public bool turnBack = false;
	private float turnBackTimer = 0.0f;
	private bool turnBackMiddle = true;
	private bool turnBackLeft;
	private bool turnBackRight;
	[HideInInspector]
	public bool back2 = false;
	private Vector3 backPoint;
	private Vector3 turnBackPoint;
	private Quaternion backRotation;
	private Quaternion normalRotation;
	private Vector3 playerPosXZ;
	private float playerPosY;
	//pulling up variables
	private bool pullingUp;
	private float pullingUpTimer;
	private bool pullUpLocationAcquired;
	private bool movingToPullUpLocation;
	private Vector3 movementVector;
	private Vector3 hitPoint;
	private bool animatePullingUp;
	//climbing rotation variables
	[HideInInspector]
	public Vector3 rotationHit;
	private Quaternion rotationNormal;
	private float rotationState;
	private bool hasNotMovedOnWallYet;
	private Quaternion lastRot3;
	private bool axisChanged;
	private float horizontalAxis;
	private float climbingHeight;
	private float lastYPosOnWall;
	[HideInInspector]
	public float horizontalValue = 1;
	//avoiding getting stuck to wall variables
	private Vector3 lastPos;
	private Quaternion lastRot2;
	private float distFromWallWhenStuck;
	private float firstDistFromWallWhenStuck;
	private bool stuckInSamePos;
	private bool stuckInSamePosNoCol;
	//enabling and disabling script variables
	private string[] scriptsToEnableOnGrab; //scripts to enable when the player grabs on to a wall (scripts disable when the player lets go of a wall)
	private MonoBehaviour scriptToEnable; //the current script being enabled (or disabled when the player lets go of a wall) in the scriptsToEnableOnGrab array
	private string[] scriptsToDisableOnGrab; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
	private MonoBehaviour scriptToDisable; //the current script being disabled (or enabled when the player lets go of a wall) in the scriptsToDisableOnGrab array
	private bool currentlyEnablingAndDisablingScripts = false; //determines whether the scripts in scriptsToEnableOnGrab or scriptsToDisableOnGrab are currently being enabled/disabled or not
	private bool scriptWarning = false; //determines whether or not the user entered any script names that do not exist on the player
	private bool onWallScriptsFinished = false; //determines whether the scripts in scriptsToEnableOnGrab or scriptsToDisableOnGrab have finished being enabled/disabled or not
	
	private RaycastHit hit = new RaycastHit(); //information on the hit point of a raycast
	private Animator animator; //the "Animator" component of the script holder
	private int entryAnimation; //the stateNameHash of the default/entry animation in the player's Animator component
	public LayerMask collisionLayers = -1; //the layers that the detectors (raycasts/linecasts) will collide with
	
	// Use this for initialization
	void Start () {
		StartUp();
		//getting the player's default animation, so that we know what animation to switch to when letting go of a ladder or wall
		if (GetComponent<Animator>()){
			entryAnimation = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash;
		}
	}
	
	void StartUp () {
		//resetting script to make sure that everything initializes
		enabled = false;
		enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		//storing values to variables
		//Grounded variables
		maxGroundedHeight2 = transform.up * grounded.maxGroundedHeight;
		maxGroundedRadius2 = grounded.maxGroundedRadius - 0.0075f;
		maxGroundedDistanceDown = Vector3.down*grounded.maxGroundedDistance;
		
		//getting position and collider information for raycasts
		pos = transform.position;
        pos.y = GetComponent<Collider>().bounds.min.y + 0.1f;
		
		//showing climbing detectors
		DrawClimbingDetectors();
		
		//setting the player's "Animator" component (if player has one) to animator
		if (GetComponent<Animator>()){
			animator = GetComponent<Animator>();
		}
		
		//checking to see if player was on the wall in the last update
		if (currentlyOnWall || currentlyClimbingWall || turnBack || back2){
			onWallLastUpdate = true;
		}
		else {
			onWallLastUpdate = false;
		}
		
	}
	
	void FixedUpdate () {
		
		if (climbing.Length > 0){
			GettingClimbingValues();
		}
		
		//increase the noCollisionTimer (if there is a collision, the noCollisionTimer is later set to 0)
		noCollisionTimer++;
		
		AvoidSlidingWhileClimbing();
		
		GettingRotationDirection();
		
		CheckRBForUseGravity();
		
		//wall and ladder climbing
		if (climbing.Length > 0){
			CheckIfPlayerCanClimb();
			
			ClimbingWall();
			
			JumpOffOfClimb();
			
			ClimbableObjectEdgeDetection();
			
			PullingUpClimbableObject();
			
			AnimatingClimbing();
		}
		
		DetermineGroundedState();
		
		LockAxisForSideScrolling();
		
		AvoidFallingWhileClimbing();
	
	}
	
	void GettingClimbingValues () {
		
		if (!wallIsClimbable && !currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				tagNum = i;
			}
		}
		//Climbing variables
		climbingSurfaceDetectorsUpAmount2 = transform.up * (climbing[tagNum].climbingSurfaceDetectorsUpAmount + 0.2f);
		climbingSurfaceDetectorsHeight2 = climbing[tagNum].climbingSurfaceDetectorsHeight - 0.18f;
		climbingSurfaceDetectorsLength2 = climbing[tagNum].climbingSurfaceDetectorsLength - 0.5f;
		distanceToPushOffAfterLettingGo2 = climbing[tagNum].distanceToPushOffAfterLettingGo;
		rotationToClimbableObjectSpeed2 = climbing[tagNum].rotationToClimbableObjectSpeed;
		climbHorizontally2 = climbing[tagNum].climbHorizontally;
		climbVertically2 = climbing[tagNum].climbVertically;
		climbMovementSpeed2 = climbing[tagNum].climbMovementSpeed;
		climbRotationSpeed2 = climbing[tagNum].climbRotationSpeed;
		moveInBursts = climbing[tagNum].moveInBursts;
		burstLength = climbing[tagNum].burstLength;
		climbableTag2 = climbing[tagNum].climbableTag;
		stayUpright2 = climbing[tagNum].stayUpright;
		snapToCenterOfObject2 = climbing[tagNum].snapToCenterOfObject;
		bottomNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].bottomNoSurfaceDetectorHeight + 0.15f);
		topNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].topNoSurfaceDetectorHeight + 0.15f);
		topAndBottomNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].topAndBottomNoSurfaceDetectorsWidth);
		sideNoSurfaceDetectorsHeight2 = climbing[tagNum].sideNoSurfaceDetectorsHeight - 0.15f;
		sideNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f);
		sideNoSurfaceDetectorsWidthTurnBack2 = climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f;
		stopAtSides2 = climbing[tagNum].stopAtSides;
		dropOffAtBottom2 = climbing[tagNum].dropOffAtBottom;
		dropOffAtFloor2 = climbing[tagNum].dropOffAtFloor;
		pullUpAtTop2 = climbing[tagNum].pullUpAtTop;
		pullUpSpeed = climbing[tagNum].pullUpSpeed;
		pullUpLocationForward2 = transform.forward*(climbing[tagNum].pullUpLocationForward);
		pushAgainstWallIfPlayerIsStuck2 = climbing[tagNum].pushAgainstWallIfPlayerIsStuck;
		//walk off ledge then transition to climb variables
		allowGrabbingOnAfterWalkingOffLedge2 = climbing[tagNum].walkingOffOfClimbableSurface.allowGrabbingOnAfterWalkingOffLedge;
		spaceInFrontNeededToGrabBackOn2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
		firstSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
		secondSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
		thirdSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
		sideLedgeDetectorsWidth2 = transform.right*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
		sideLedgeDetectorsLength2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
		spaceBelowNeededToGrabBackOnHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
		spaceBelowNeededToGrabBackOnForward2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
		//enabling and disabling script variables
		scriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		scriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
	}
	
	void AvoidSlidingWhileClimbing () {
		
		//keeping player from randomly dropping while on wall (at the beginning of the function)
		if (currentlyClimbingWall && !pullingUp){
			if (Mathf.Abs(transform.position.y - lastYPosOnWall) >= 0.2f * (climbMovementSpeed2/4) && lastYPosOnWall != 0){
				transform.position = new Vector3(transform.position.x, climbingHeight, transform.position.z);
			}
			else {
				climbingHeight = transform.position.y;
			}
			lastYPosOnWall = transform.position.y;
		}
		else {
			lastYPosOnWall = 0;
		}
		
		//keeping player from randomly sliding while climbing wall
		if (currentlyClimbingWall || pullingUp || turnBack || back2){
			if (GetComponent<Rigidbody>() && !startedClimbing){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			startedClimbing = true;
		}
		else {
			startedClimbing = false;
		}
		
	}
	
	void GettingRotationDirection () {
		
		//getting the direction to rotate towards
		directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (directionVector != Vector3.zero) {
			
            //getting the length of the direction vector and normalizing it
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            //setting the maximum direction length to 1
            directionLength = Mathf.Min(1, directionLength);

            directionLength *= directionLength;

            //multiply the normalized direction vector by the modified direction length
            directionVector *= directionLength;
			
        }
		
	}
	
	void CheckRBForUseGravity () {
		
		//checking to see if player (if using a Rigidbody) is using "Use Gravity"
		if (GetComponent<Rigidbody>()){
			if (GetComponent<Rigidbody>().useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				rbUsesGravity = true;
			}
			
			if (currentlyOnWall || currentlyClimbingWall || turnBack || back2){
				canChangeRbGravity = true;
				GetComponent<Rigidbody>().useGravity = false;
			}
			else if (rbUsesGravity && canChangeRbGravity){
				GetComponent<Rigidbody>().useGravity = true;
				canChangeRbGravity = false;
			}
			
			if (!GetComponent<Rigidbody>().useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				rbUsesGravity = false;
			}
		}
		
	}
	
	void CheckIfPlayerCanClimb () {
		
		//enabling and disabling scripts (and warning the user if any script names they entered do not exist on the player) when climbing on wall
		ScriptEnablingDisabling();
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//detecting whether the player can climb or not
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			//setting initial rotation
			if (!currentlyClimbingWall){
				rotationNormal = Quaternion.LookRotation(-hit.normal);
				rotationState = 1;
			}
			
			firstTest = true;
			//if we are not over an edge of a climbable object, the object can be climbed
			if (currentlyClimbingWall || !currentlyClimbingWall && !reachedTopPoint && !reachedBottomPoint && (!reachedLeftPoint && !reachedRightPoint || !stopAtSides2)){
				wallIsClimbable = true;
			}
			//if we are over an edge of a climbable object, the object cannot be climbed
			else if (!currentlyClimbingWall){
				wallIsClimbable = false;
			}
			climbableWallInFront = true;
			
			//if we are currently climbing the wall and are no longer rotating, set finishedRotatingToWall to true
			if (currentlyClimbingWall && transform.rotation == lastRot2){
				finishedRotatingToWall = true;
			}
			
		}
		else if (!pullingUp){
			firstTest = false;
			wallIsClimbable = false;
			climbableWallInFront = false;
			
			//if there is a wall in front of the player, but we are not actually on the wall, set currentlyClimbingWall to false
			if (transform.rotation == lastRot2 && wallInFront){
				if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing") && !currentlyClimbingWall){
					animator.CrossFade(entryAnimation, 0f, -1, 0f);
				}
				currentlyClimbingWall = false;
			}
			
			//if we are not climbing the wall yet
			if (!currentlyClimbingWall){
				finishedRotatingToWall = false;
			}
		}
		
		//checking if there is a wall in front of the player
		if ((Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, collisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, out hit, collisionLayers))){
			
			wallInFront = true;
			
		}
		else {
			wallInFront = false;
		}
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//getting center of climbable object
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			secondTest = true;
			centerPoint = new Vector3(hit.transform.position.x - transform.position.x, 0, hit.transform.position.z - transform.position.z);
			
		}
		else {
			secondTest = false;
		}
		
	}
	
	void ClimbingWall () {
		
		if (currentlyClimbingWall && !pullingUp){
			
			//if the horizontal axis is switched, set switchedDirection to true
			if (Input.GetAxisRaw("Horizontal") > 0 && lastAxis <= 0 || Input.GetAxisRaw("Horizontal") < 0 && lastAxis >= 0){
				switchedDirection = true;
			}
			lastAxis = Input.GetAxisRaw("Horizontal");
			
			//climbing down/off of ladder or wall
			if (Input.GetAxisRaw("Vertical") >= 0 || switchedDirection && !downCanBePressed || !grounded.currentlyGrounded){
				downCanBePressed = true;
				if (Input.GetAxisRaw("Vertical") > 0 || !grounded.currentlyGrounded){
					climbedUpAlready = true;
				}
			}
			else if (downCanBePressed && grounded.currentlyGrounded){
				if (dropOffAtFloor2){
					moveDirection = Vector3.zero;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					currentlyClimbingWall = false;
				}
				else {
					downCanBePressed = false;
					climbedUpAlready = false;
					reachedBottomPoint = true;
				}
			}
			switchedDirection = false;
			
			//if player's movement is stopped at the bottom, set the speed and direction of the player to 0
			if (Input.GetAxisRaw("Vertical") < 0 && Input.GetAxisRaw("Horizontal") == 0 && (!downCanBePressed || reachedBottomPoint && (!dropOffAtBottom2 || grounded.currentlyGrounded && !dropOffAtFloor2))){
				climbDirection = Vector3.zero;
				moveDirection = Vector3.zero;
			}
			
			//rotating player to face the wall
			WallClimbingRotation();
			
			//checking to see if player is stuck on wall
			CheckIfStuck();
			
			//moving in bursts
			if (moveInBursts){
				
				if (beginClimbBurst){
					climbingMovement = Mathf.Lerp(climbingMovement, climbMovementSpeed2, ((2 + climbMovementSpeed2)*(climbingMovement/2 + 1))/burstLength * Time.deltaTime);
				}
				else {
					climbingMovement = Mathf.Lerp(climbingMovement, 0, ((2 + climbMovementSpeed2)*(climbingMovement/2 + 1))/burstLength * Time.deltaTime);
				}
				if (climbMovementSpeed2 - climbingMovement < 0.1f){
					beginClimbBurst = false;
				}
				else if (climbingMovement < 0.1f){
					beginClimbBurst = true;
				}
				
			}
			else {
				climbingMovement = climbMovementSpeed2;
			}
			
			//getting direction to climb towards
			if (directionVector.magnitude != 0){
				
				//climbing horizontally and vertically
				if (climbHorizontally2 && climbVertically2 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis)){
					
					//if we have reached the top, bottom, right, or left point, do not allow movement in any direction
					if (stopAtSides2 && (reachedRightPoint && Input.GetAxis("Horizontal") > 0 || reachedLeftPoint && Input.GetAxis("Horizontal") < 0)
					&& (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0)){
						climbDirection = Vector3.zero;
					}
					//if we have reached the left or right point, do not allow horizontal movement in that direction
					else if (downCanBePressed && stopAtSides2 && (reachedRightPoint && Input.GetAxis("Horizontal") > 0 || reachedLeftPoint && Input.GetAxis("Horizontal") < 0)
					&& ((!reachedTopPoint || Input.GetAxis("Vertical") <= 0) && (!reachedBottomPoint || Input.GetAxis("Vertical") >= 0))){
						climbDirection = (Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
					//if down cannot be pressed, or we have reached the top or bottom point, do not allow vertical movement in that direction
					else if (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right) * climbingMovement;
					}
					//if down can be pressed, and we have not reached the top, bottom, right, or left point, allow movement in every direction
					else if (downCanBePressed){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right + Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
					
				}
				//climbing horizontally
				else if (climbHorizontally2 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis)){
					
					//if we have not reached the sides of the climbable object
					if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right) * climbingMovement;
					}
					else {
						climbDirection = Vector3.zero;
					}
					
				}
				//climbing vertically
				else if (climbVertically2){
					
					//if down cannot be pressed or we have reached the top point, do not allow vertical movement in that direction
					if (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0){
						climbDirection = Vector3.zero;
					}
					//if not, allow vertical movement
					else if (downCanBePressed){
						climbDirection = (Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
				}
				else {
					climbDirection = Vector3.zero;
				}
				
			}
			else {
				climbDirection = Vector3.Slerp(climbDirection, Vector3.zero, 15 * Time.deltaTime);
			}
			
			//moving player on wall
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(climbDirection * Time.deltaTime);
			}
			else if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().MovePosition(transform.position + climbDirection * Time.deltaTime);
			}
			
			
			//setting player's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
			float yRotationValue;
			if (transform.eulerAngles.y > 180){
				yRotationValue = transform.eulerAngles.y - 360;
			}
			else {
				yRotationValue = transform.eulerAngles.y;
			}
			//getting rotation on z-axis (x-axis is locked)
			if (sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 90){
					horizontalValue = -1;
					if (sideScrolling.rotateInwards){
						bodyRotation = 180.001f;
					}
					else {
						bodyRotation = 179.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = 1;
					if (sideScrolling.rotateInwards){
						bodyRotation = -0.001f;
					}
					else {
						bodyRotation = 0.001f;
					}
				}
			}
			//getting rotation on x-axis (z-axis is locked)
			else if (sideScrolling.lockMovementOnZAxis && !sideScrolling.lockMovementOnXAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 0){
					horizontalValue = 1;
					if (sideScrolling.rotateInwards){
						bodyRotation = 90.001f;
					}
					else {
						bodyRotation = 89.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = -1;
					if (sideScrolling.rotateInwards){
						bodyRotation = -90.001f;
					}
					else {
						bodyRotation = -89.999f;
					}
				}
			}
			
		}
		else {
			climbDirection = Vector3.zero;
			downCanBePressed = false;
			climbedUpAlready = false;
			climbingMovement = 0;
			beginClimbBurst = true;
			switchedDirection = false;
		}
		lastAxis = Input.GetAxisRaw("Horizontal");
		
	}
	
	void JumpOffOfClimb () {
		
		//jumping off of ladder/wall
		if ((currentlyClimbingWall || turnBack || back2) && Input.GetButtonDown("Jump")){
			moveDirection = Vector3.zero;
			jumpedOffClimbableObjectTimer = 0;
			if (turnBack || back2){
				transform.rotation = backRotation;
			}
			jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
			turnBack = false;
			back2 = false;
			grounded.currentlyGrounded = true;
			currentlyClimbingWall = false;
		}
		PushOffWall();
		
	}
	
	void ClimbableObjectEdgeDetection () {
		
		//snapping to the center of the ladder
		SnapToCenter();
		
		//grabbing on to a ladder after walking off of a ledge
		TurnToGrabLadder();
		
		//determining if player has reached any of the edges of the climbable object
		CheckClimbableEdges();
		
		//set values to 0 and false if not climbing wall
		if (!currentlyClimbingWall){
			climbDirection = Vector3.zero;
			pullUpLocationAcquired = false;
			movingToPullUpLocation = false;
			pullingUp = false;
		}
		
	}
	
	void PullingUpClimbableObject () {
		
		//pulling up once player has reached the top of ladder
		if (pullingUp){
			pullingUpTimer += 0.02f;
			
			if (pullUpLocationAcquired){
				if (Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, collisionLayers)) {
					hitPoint = hit.point;
				}
				pullUpLocationAcquired = false;
			}
			
			if (movingToPullUpLocation){
				movementVector = (new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z) - transform.position).normalized * pullUpSpeed;
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(movementVector * Time.deltaTime);
				}
				else if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * Time.deltaTime);
				}
			}
			if (Vector3.Distance(transform.position, new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z)) <= 0.2f || pullingUpTimer > Mathf.Abs((pullUpSpeed/4)) && onWallLastUpdate){
				pullUpLocationAcquired = false;
				movingToPullUpLocation = false;
			}
			
			if (!movingToPullUpLocation){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				movementVector = ((hitPoint + transform.forward/10) - transform.position).normalized * pullUpSpeed;
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(movementVector * Time.deltaTime);
				}
				else if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * Time.deltaTime);
				}
			}
			if (Vector3.Distance(transform.position, (hitPoint + transform.forward/10)) <= 0.2f || pullingUpTimer > Mathf.Abs((pullUpSpeed/4)) && onWallLastUpdate){
				movingToPullUpLocation = false;
				wallIsClimbable = false;
				currentlyClimbingWall = false;
				pullingUp = false;
			}
		}
		else {
			pullingUpTimer = 0;
		}
		
	}
	
	void AnimatingClimbing () {
		
		//animating player climbing wall
		if (animator != null){
			
			if (!pullingUp){
				
				//animating player when he turns on to wall
				if (currentlyClimbingWall || turnBack || back2){
					if (animator.GetFloat("climbState") < 1){
						animator.CrossFade("Climbing", 0f, -1, 0f);
						animator.SetFloat("climbState", 1);
					}
				}
				else {
					if ((grounded.currentlyGrounded && animator.GetFloat("climbState") != 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing"))){
						animator.CrossFade(entryAnimation, 0f, -1, 0f);
					}
					animator.SetFloat("climbState", 0);
					
				}
				
				//animating the player's climbing motions while he is on a wall
				if (currentlyClimbingWall){
					if (climbHorizontally2 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis)){
						//if we have not reached the sides of the climbable object
						if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
							animator.SetFloat("climbState", Mathf.Abs(Input.GetAxis("Horizontal")) + 1);
						}
						else {
							animator.SetFloat("climbState", Mathf.Lerp(animator.GetFloat("climbState"), 1, 8 * Time.deltaTime));
						}
					}
					if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
						animator.CrossFade("Climbing", 0f, -1, 0f);
					}
				}
				
				//horizontal movement
				if (Input.GetAxis("Horizontal") > 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && (!reachedRightPoint || !stopAtSides2)){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the right while grabbed on to a ledge
					if (horizontalClimbSpeed <= 0 || climbingMovement < 0.1f){
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
					}
					horizontalClimbSpeed = 1;
					
				}
				else if (Input.GetAxis("Horizontal") < 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && (!reachedLeftPoint || !stopAtSides2)){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the left while grabbed on to a ledge
					if (horizontalClimbSpeed >= 0 || climbingMovement < 0.1f){
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
					}
					horizontalClimbSpeed = -1;
					
				}
				else {
					animator.SetFloat ("climbSpeedHorizontal", Mathf.Lerp(animator.GetFloat("climbSpeedHorizontal"), 0, 15 * Time.deltaTime) );
				}
				//vertical movement
				if (Input.GetAxis("Vertical") > 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && !reachedTopPoint){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the right while grabbed on to a ledge
					if (verticalClimbSpeed <= 0 || climbingMovement < 0.1f){
						//immediately transitioning to climbing animation
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
						
						//switching climbing arms
						if (arm == 1){
							arm = 2;
						}
						else {
							arm = 1;
						}
					}
					verticalClimbSpeed = 1;
					
				}
				else if (Input.GetAxis("Vertical") < 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && downCanBePressed && !reachedBottomPoint){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the left while grabbed on to a ledge
					if (verticalClimbSpeed >= 0 || climbingMovement < 0.1f){
						//immediately transitioning to climbing animation
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
						
						//switching climbing arms
						if (arm == 1){
							arm = 2;
						}
						else {
							arm = 1;
						}
					}
					verticalClimbSpeed = -1;
					
				}
				else {
					animator.SetFloat ("climbSpeedVertical", Mathf.Lerp(animator.GetFloat("climbSpeedVertical"), 0, 15 * Time.deltaTime) );
				}
				//switching arm to climb with in animator
				animator.SetFloat ("armToClimbWith", Mathf.Lerp(animator.GetFloat("armToClimbWith"), arm, 15 * Time.deltaTime) );
				
				//setting climbing speeds in animator
				if (currentlyClimbingWall && climbMovementSpeed2 > 0){
					
					//setting vertical climb speed in animator
					if (Input.GetAxis("Vertical") != 0 && climbVertically2 && climbedUpAlready){
						if ((!reachedTopPoint || Input.GetAxis("Vertical") < 0) && (!reachedBottomPoint || Input.GetAxis("Vertical") > 0)){
							animator.SetFloat ("climbSpeedVertical", Mathf.Lerp(animator.GetFloat("climbSpeedVertical"), verticalClimbSpeed, 15 * Time.deltaTime) );
						}
					}
					//setting horizontal climb speed in animator
					if (Input.GetAxis("Horizontal") != 0 && climbHorizontally2 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis)){
						//if we have not reached sides or are not moving toward them
						if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
							animator.SetFloat ("climbSpeedHorizontal", Mathf.Lerp(animator.GetFloat("climbSpeedHorizontal"), horizontalClimbSpeed, 15 * Time.deltaTime) );
						}
					}
				}
				else {
					animator.SetFloat ("climbSpeedVertical", 0);
					animator.SetFloat ("climbSpeedHorizontal", 0);
				}
				
			}
			
			//animating climbing over a ledge
			if (pullingUp){
				if (animator.GetFloat("climbState") != 0 || !animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
					if (!animatePullingUp){
						if (onWallLastUpdate){
							animator.speed = Mathf.Abs(pullUpSpeed/4);
						}
						else {
							animator.speed = pullUpSpeed/4;
						}
						animator.SetFloat ("climbState", 0);
						animator.CrossFade("Climbing", 0f, -1, 0f);
						animatePullingUp = true;
					}
				}
			}
			else if (animatePullingUp){
				animator.speed = 1;
				animatePullingUp = false;
			}
		}
		
	}
	
	void DetermineGroundedState () {
		
		//determining whether the player is grounded or not
		//drawing ground detection rays
		if (grounded.showGroundDetectionRays){
			Debug.DrawLine(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
		}
		//determining if grounded
		if (Physics.Linecast(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)){
			grounded.currentlyGrounded = true;
		}
		else {
			grounded.currentlyGrounded = false;
		}
		
	}
	
	void LockAxisForSideScrolling () {
		
		//locking axis for side-scrolling
		if (sideScrolling.lockMovementOnXAxis){
			moveDirection.x = 0;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp){
				transform.position = new Vector3(sideScrolling.xValue, transform.position.y, transform.position.z);
			}
		}
		if (sideScrolling.lockMovementOnZAxis){
			moveDirection.z = 0;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp){
				transform.position = new Vector3(transform.position.x, transform.position.y, sideScrolling.zValue);
			}
		}
		
		if (GetComponent<Rigidbody>() && currentlyClimbingWall){
			moveDirection = Vector3.zero;
		}
		
		slidingVector = Vector3.zero;
		
	}
	
	void AvoidFallingWhileClimbing () {
		
		//keeping player from randomly dropping while on wall (at the end of the function)
		if (currentlyClimbingWall && !pullingUp){
			if (Mathf.Abs(transform.position.y - lastYPosOnWall) >= 0.2f * (climbMovementSpeed2/4) && lastYPosOnWall != 0){
				transform.position = new Vector3(transform.position.x, climbingHeight, transform.position.z);
			}
			else {
				climbingHeight = transform.position.y;
			}
			lastYPosOnWall = transform.position.y;
		}
		else {
			lastYPosOnWall = 0;
		}
		
	}
	
	void DrawClimbingDetectors () {
		
		//showing climbing detectors
		for (int i = 0; i < climbing.Length; i++) {
			
			//setting the climbing variables that we will be using to draw
			showClimbingDetectors3 = climbing[i].showClimbingDetectors;
			climbingSurfaceDetectorsUpAmount3 = transform.up * (climbing[i].climbingSurfaceDetectorsUpAmount + 0.2f);
			climbingSurfaceDetectorsHeight3 = climbing[i].climbingSurfaceDetectorsHeight - 0.18f;
			climbingSurfaceDetectorsLength3 = climbing[i].climbingSurfaceDetectorsLength - 0.5f;
			showEdgeOfObjectDetctors3 = climbing[i].showEdgeOfObjectDetctors;
			bottomNoSurfaceDetectorHeight3 = transform.up*(climbing[i].bottomNoSurfaceDetectorHeight + 0.15f);
			topNoSurfaceDetectorHeight3 = transform.up*(climbing[i].topNoSurfaceDetectorHeight + 0.15f);
			topAndBottomNoSurfaceDetectorsWidth3 = transform.right*(climbing[i].topAndBottomNoSurfaceDetectorsWidth);
			sideNoSurfaceDetectorsHeight3 = climbing[i].sideNoSurfaceDetectorsHeight - 0.15f;
			sideNoSurfaceDetectorsWidth3 = transform.right*(climbing[i].sideNoSurfaceDetectorsWidth - 0.25f);
			showPullUpDetector3 = climbing[i].showPullUpDetector;
			pullUpLocationForward3 = transform.forward*(climbing[i].pullUpLocationForward);
			//walk off ledge then transition to climb variables
			showWalkingOffLedgeRays3 = climbing[i].walkingOffOfClimbableSurface.showWalkingOffLedgeRays;
			spaceInFrontNeededToGrabBackOn3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
			firstSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
			secondSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
			thirdSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
			sideLedgeDetectorsWidth3 = transform.right*(climbing[i].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
			sideLedgeDetectorsLength3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
			spaceBelowNeededToGrabBackOnHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
			spaceBelowNeededToGrabBackOnForward3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
			grabBackOnLocationHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationHeight);
			grabBackOnLocationWidth3 = transform.right*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationWidth);
			grabBackOnLocationForward3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationForward);
			
			if (showClimbingDetectors3){
				//middle
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.1875f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.375f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.5625f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.75f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.9375f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*1.125f, Color.red);
			}
			//drawing the rays that detect the top and bottom of a climbable object
			if (showEdgeOfObjectDetctors3){
				//bottom
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				//top
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				//right
				Debug.DrawLine(transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				//left
				Debug.DrawLine(transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
			}
			//show grabbing on to climbable object under ledge detectors
			if (showWalkingOffLedgeRays3){
				//in front
				Debug.DrawLine(transform.position + transform.up*0.5f, transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn3, Color.red);
				//below
				Debug.DrawLine(transform.position + transform.forward/4 + transform.up*0.5f + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/4 - transform.up*1.5f - spaceBelowNeededToGrabBackOnHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn3, transform.position + transform.forward/1.5f - transform.up*1.5f + spaceInFrontNeededToGrabBackOn3 - spaceBelowNeededToGrabBackOnHeight3, Color.red);
				
				//grab back on location
				//middle
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 + grabBackOnLocationForward3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 + grabBackOnLocationForward3, Color.yellow);
				//left
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 - transform.right*0.4f + grabBackOnLocationForward3 - grabBackOnLocationWidth3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 - transform.right*0.4f + grabBackOnLocationForward3 - grabBackOnLocationWidth3, Color.yellow);
				//right
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 + transform.right*0.4f + grabBackOnLocationForward3 + grabBackOnLocationWidth3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 + transform.right*0.4f + grabBackOnLocationForward3 + grabBackOnLocationWidth3, Color.yellow);
				
				//side blocking ledge detectors
				//left
				//first
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.1f + transform.right/4 - sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//second
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.5f + transform.right/4 - sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//third
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.right/4 + sideLedgeDetectorsLength3 - transform.up + thirdSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up + transform.right/4 - sideLedgeDetectorsLength3 + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				
				//right
				//first
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.1f - transform.right/4 - sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//second
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.5f - transform.right/4 - sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//third
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 + transform.right/4 + sideLedgeDetectorsLength3 - transform.up + thirdSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up - transform.right/4 - sideLedgeDetectorsLength3 + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
			}
			//showing the linecast that detects where the player pulls up to
			if (showPullUpDetector3){
				Debug.DrawLine(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward3), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward3), Color.red);
			}
		}
		
	}
	
	void SetClimbingVariables () {
		
		if (!wallIsClimbable && !currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				tagNum = i;
			}
		}
		//Climbing variables
		climbingSurfaceDetectorsUpAmount2 = transform.up * (climbing[tagNum].climbingSurfaceDetectorsUpAmount + 0.2f);
		climbingSurfaceDetectorsHeight2 = climbing[tagNum].climbingSurfaceDetectorsHeight - 0.18f;
		climbingSurfaceDetectorsLength2 = climbing[tagNum].climbingSurfaceDetectorsLength - 0.5f;
		distanceToPushOffAfterLettingGo2 = climbing[tagNum].distanceToPushOffAfterLettingGo;
		rotationToClimbableObjectSpeed2 = climbing[tagNum].rotationToClimbableObjectSpeed;
		climbHorizontally2 = climbing[tagNum].climbHorizontally;
		climbVertically2 = climbing[tagNum].climbVertically;
		climbMovementSpeed2 = climbing[tagNum].climbMovementSpeed;
		climbRotationSpeed2 = climbing[tagNum].climbRotationSpeed;
		moveInBursts = climbing[tagNum].moveInBursts;
		burstLength = climbing[tagNum].burstLength;
		climbableTag2 = climbing[tagNum].climbableTag;
		stayUpright2 = climbing[tagNum].stayUpright;
		snapToCenterOfObject2 = climbing[tagNum].snapToCenterOfObject;
		bottomNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].bottomNoSurfaceDetectorHeight + 0.15f);
		topNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].topNoSurfaceDetectorHeight + 0.15f);
		topAndBottomNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].topAndBottomNoSurfaceDetectorsWidth);
		sideNoSurfaceDetectorsHeight2 = climbing[tagNum].sideNoSurfaceDetectorsHeight - 0.15f;
		sideNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f);
		sideNoSurfaceDetectorsWidthTurnBack2 = climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f;
		stopAtSides2 = climbing[tagNum].stopAtSides;
		dropOffAtBottom2 = climbing[tagNum].dropOffAtBottom;
		dropOffAtFloor2 = climbing[tagNum].dropOffAtFloor;
		pullUpAtTop2 = climbing[tagNum].pullUpAtTop;
		pullUpSpeed = climbing[tagNum].pullUpSpeed;
		pullUpLocationForward2 = transform.forward*(climbing[tagNum].pullUpLocationForward);
		pushAgainstWallIfPlayerIsStuck2 = climbing[tagNum].pushAgainstWallIfPlayerIsStuck;
		//walk off ledge then transition to climb variables
		allowGrabbingOnAfterWalkingOffLedge2 = climbing[tagNum].walkingOffOfClimbableSurface.allowGrabbingOnAfterWalkingOffLedge;
		spaceInFrontNeededToGrabBackOn2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
		firstSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
		secondSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
		thirdSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
		sideLedgeDetectorsWidth2 = transform.right*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
		sideLedgeDetectorsLength2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
		spaceBelowNeededToGrabBackOnHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
		spaceBelowNeededToGrabBackOnForward2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
		//enabling and disabling script variables
		scriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		scriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
	}
	
	void PushOffWall () {
		
		//pushing off of wall after letting go
		jumpedOffClimbableObjectTimer += 0.02f;
		if (jumpedOffClimbableObjectTimer < 0.3f){
			currentlyClimbingWall = false;
			jumpedOffClimbableObjectDirection = Vector3.Slerp(jumpedOffClimbableObjectDirection, Vector3.zero, 8 * Time.deltaTime);
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(jumpedOffClimbableObjectDirection * Time.deltaTime);
			}
			else if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().MovePosition(transform.position + jumpedOffClimbableObjectDirection * Time.deltaTime);
			}
		}
		
	}
	
	void SnapToCenter () {
		
		if (snapToCenterOfObject2 && (turnBack || back2)){
			snappingToCenter = true;
		}
		if (currentlyClimbingWall && !pullingUp || turnBack || back2){
			
			//increasing snapTimer so the player knows when to let go
			if (!turnBack || wallIsClimbable || currentlyClimbingWall){
				snapTimer += 0.02f;
			}
			
			//snapping player to center of climbable object
			if (snappingToCenter && snapTimer < 0.6f && (!turnBack || turnBackTimer >= 0.1f)){
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						GetComponent<CharacterController>().Move(centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						GetComponent<CharacterController>().Move(centerPoint * 13 * Time.deltaTime);
					}
				}
				else if (GetComponent<Rigidbody>()){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						GetComponent<Rigidbody>().MovePosition(transform.position + centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						GetComponent<Rigidbody>().MovePosition(transform.position + centerPoint * 13 * Time.deltaTime);
					}
				}
			}
			
		}
		else {
			snapTimer = 0;
			snappingToCenter = false;
		}
		
	}
	
	void TurnToGrabLadder () {
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//checking to see if we are about to turn on to a climbable wall
		//if left is blocked, go to back right
		if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&	Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f + transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up + transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f + transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if right is blocked, go to back left
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
	
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f - transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up - transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f - transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if neither side is blocked, go directly back
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
		&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, collisionLayers)){
			
			fifthTest = true;
			
		}
		else {
			fifthTest = false;
		}
		
		
		if (!turnBack){
			//checking to see if either side of a ledge is blocked
			if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
				if (!wallIsClimbable && !Physics.Linecast(transform.position + transform.up*0.5f, transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, out hit, collisionLayers) && !back2 && !currentlyClimbingWall){
					if (!Physics.Linecast(transform.position + transform.forward/4 + transform.up*0.5f + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*1.5f - spaceBelowNeededToGrabBackOnHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && !Physics.Linecast(transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, transform.position + transform.forward/1.5f - transform.up*1.5f + spaceInFrontNeededToGrabBackOn2 - spaceBelowNeededToGrabBackOnHeight2, out hit, collisionLayers)){
						
						//if left is blocked, go to back right
						if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&	Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f + transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up + transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f + transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackRight = true;
									turnBackLeft = false;
									turnBackMiddle = false;
									turnBack = true;
								}
							}
						}
						//if right is blocked, go to back left
						else if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
					
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f - transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up - transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f - transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackLeft = true;
									turnBackRight = false;
									turnBackMiddle = false;
									turnBack = true;
								}
							}
						}
						//if neither side is blocked, go directly back
						else if ((Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
						&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, collisionLayers)){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackMiddle = true;
									turnBackLeft = false;
									turnBackRight = false;
									turnBack = true;
								}
							}
						}
						else {
							turnBackMiddle = false;
							turnBackLeft = false;
							turnBackRight = false;
							turnBack = false;
						}
						
					}
				}
			}
			
		}
		
		//turning around when you walk off a ledge
		if (turnBack){
			
			if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
				animator.CrossFade("Climbing", 0f, -1, 0f);
			}
			
			if (allowGrabbingOnAfterWalkingOffLedge2 && (turnBackMiddle || turnBackLeft || turnBackRight)){
				if (!stayUpright2){
					if ((Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z)) > 0.3f && !snapToCenterOfObject2 || Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z)) > 0.3f && snapToCenterOfObject2
					|| Quaternion.Angle(transform.rotation, backRotation) > 0.1f) && (!snapToCenterOfObject2 && turnBackTimer < 0.5f || turnBackTimer < 0.2f)){
						turnBackTimer += 0.02f;
						back2 = false;
						currentlyClimbingWall = false;
						//movement
						if (!snapToCenterOfObject2){
							if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (GetComponent<Rigidbody>()){
								transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
						}
						else {
							transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z), 10 * Time.deltaTime);
							
						}
						//moving over if player is past side
						if (!snapToCenterOfObject2 && Quaternion.Angle(transform.rotation, backRotation) < 45){
							if (reachedLeftPoint){
								turnBackPoint += transform.right/30;
							}
							if (reachedRightPoint){
								turnBackPoint -= transform.right/30;
							}
						}
						//rotation
						rotationNormal = backRotation;
						transform.rotation = Quaternion.Slerp(transform.rotation, backRotation, 12 * Time.deltaTime);
						playerPosXZ = transform.position;
					}
					else {
						turnBackTimer = 0.0f;
						back2 = true;
						turnBack = false;
					}
				}
				else {
					if ((Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z)) > 0.3f && !snapToCenterOfObject2 || Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z)) > 0.3f && snapToCenterOfObject2
					|| Quaternion.Angle(transform.rotation, backRotation) > 0.1f) && (!snapToCenterOfObject2 && turnBackTimer < 0.5f || turnBackTimer < 0.2f)){
						turnBackTimer += 0.02f;
						back2 = false;
						currentlyClimbingWall = false;
						//movement
						if (!snapToCenterOfObject2){
							if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (GetComponent<Rigidbody>()){
								transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
						}
						else {
							transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z), 10 * Time.deltaTime);
							
						}
						//moving over if player is past side
						if (!snapToCenterOfObject2 && Quaternion.Angle(transform.rotation, backRotation) < 45){
							if (reachedLeftPoint){
								turnBackPoint += transform.right/30;
							}
							if (reachedRightPoint){
								turnBackPoint -= transform.right/30;
							}
						}
						//rotation
						rotationNormal = backRotation;
						transform.rotation = Quaternion.Slerp(transform.rotation, backRotation, 12 * Time.deltaTime);
						playerPosXZ = transform.position;
					}
					else {
						turnBackTimer = 0.0f;
						back2 = true;
						turnBack = false;
					}
				}
			}
			else {
				back2 = false;
				turnBack = false;
			}
			
		}
		
		
		if (allowGrabbingOnAfterWalkingOffLedge2){
			if (turnBack || back2){
				climbDirection = Vector3.zero;
				moveDirection = Vector3.zero;
			}
			if (back2){
				turnBack = false;
				if (!currentlyClimbingWall){
					if (!snapToCenterOfObject2){
						transform.position = new Vector3(playerPosXZ.x, transform.position.y, playerPosXZ.z);
					}
					rotationNormal = backRotation;
					transform.rotation = backRotation;
					currentlyClimbingWall = true;
				}
				back2 = false;
			}
		}
		
	}
	
	void CheckClimbableEdges () {
		
		//getting the positions for the inner and outer bounds of the player's collider
		if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			
			//center
			colCenter = GetComponent<CharacterController>().bounds.center;
			//top
			colTop = GetComponent<CharacterController>().bounds.center + (GetComponent<CharacterController>().height*0.5f*transform.up);
			
		}
		else if (GetComponent<Rigidbody>()){
			
			//center
			colCenter = GetComponent<Collider>().bounds.center;
			
			if (GetComponent<CapsuleCollider>()){
				//top
				colTop = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().height*0.5f*transform.up);
			}
			else if (GetComponent<SphereCollider>()){
				//top
				colTop = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*transform.up);
			}
			else if (GetComponent<BoxCollider>()){
				//top
				colTop = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.y*0.5f*transform.up);
			}
			
		}
		
		//determining if player has reached any of the edges of the climbable object
		if (!pullingUp){
			
			//BOTTOM POINT
			//checking to see if player has reached the bottom of the ladder (first: if not touching anything at bottom; next: if touching something at bottom, but does not have climbable tag)
			if (!Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			||
			Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2){
				
				//bottom of climbable object has been reached
				reachedBottomPoint = true;
				
				//dropping off at bottom
				if (currentlyClimbingWall && dropOffAtBottom2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") < 0){
					moveDirection = Vector3.zero;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					if (animator != null){
						animator.CrossFade(entryAnimation, 0f, -1, 0f);
					}
					animator.SetFloat("climbState", 0);
					currentlyClimbingWall = false;
				}
				
			}
			else if (!currentlyClimbingWall || currentlyClimbingWall && (!grounded.currentlyGrounded || dropOffAtBottom2)){
				//bottom of climbable object has not been reached
				reachedBottomPoint = false;
			}
			
			
			//TOP POINT
			//if our front, right and left side are above the top
			if (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			||
			Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2){
				aboveTop = true;
			}
			else {
				aboveTop = false;
			}
			
			//checking to see if player has reached the top of the ladder
			if ((aboveTop
			||
			//if top and left is blocked
			((!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			||
			//if top and right is blocked
			(!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)))
			||
			//if top is blocked
			Physics.Linecast(colCenter, colTop + transform.up/6, out hit, collisionLayers)){
				
				//top of climbable object has been reached
				reachedTopPoint = true;
				
				//checking to see if surface can be pulled up to
				if (aboveTop){
					if (currentlyClimbingWall && pullUpAtTop2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") > 0
					&& Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, collisionLayers)){
						pullUpLocationAcquired = true;
						movingToPullUpLocation = true;
						pullingUp = true;
					}
				}
				
			}
			else {
				//top of climbable object has not been reached
				reachedTopPoint = false;
			}
			
			
			//RIGHT POINT
			//Climbing variables
			i++;
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				SetClimbingVariables();
			}
			
			//making sure we are not above the top point, and not moving to the right
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top right to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the right side
			|| aboveTop){
				//if we have room to move to the right, we have not reached the farthest right point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to right more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the right to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, out hit, collisionLayers)&& hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)){
					
					//right edge of climbable object has not been reached
					thirdTest = true;
					reachedRightPoint = false;
					
				}
				//checking to see if player has reached the right edge (first: if not touching anything to right; next: if touching something to right, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2){
					
					//right edge of climbable object has been reached
					thirdTest = false;
					reachedRightPoint = true;
					
				}
				else {
					//right edge of climbable object has not been reached
					reachedRightPoint = false;
				}
			}
			else {
				//right edge of climbable object has not been reached
				reachedRightPoint = true;
			}
			
			
			//LEFT POINT
			//Climbing variables
			i++;
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				SetClimbingVariables();
			}
			
			//making sure that we are not above the top point and moving to the left
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top left to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the left side
			|| aboveTop){
				//if we have room to move to the left, we have not reached the farthest left point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to left more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the left to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)){
					
					//left edge of climbable object has not been reached
					fourthTest = true;
					reachedLeftPoint = false;
					
				}
				//checking to see if player has reached the left edge (first: if not touching anything to left; next: if touching something to left, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2){
					
					//left edge of climbable object has been reached
					fourthTest = false;
					reachedLeftPoint = true;
					
				}
				else {
					//left edge of climbable object has not been reached
					reachedLeftPoint = false;
				}
			}
			else {
				//left edge of climbable object has not been reached
				reachedLeftPoint = true;
			}
			
		}
		
	}
	
	void WallClimbingRotation () {
		
		//rotation
		if (currentlyClimbingWall && !pullingUp){
			
			//only change the rotation normal if player is moving
			if ((transform.rotation == lastRot3 || axisChanged) && (climbingMovement > 0 || Input.GetAxis("Horizontal") != 0) || hasNotMovedOnWallYet){
				
				//to the right of player
				if ((Input.GetAxis("Horizontal") > 0 || transform.rotation != lastRot2) && (wallIsClimbable)){
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				//to the left of player
				else if ((Input.GetAxis("Horizontal") < 0 || transform.rotation != lastRot2) && (wallIsClimbable)){
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				
				//in front of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the right
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the left
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				
				//inward turn, front and to the right of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				
				//inward turn, front and to the left of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (noCollisionTimer >= 5 && !wallIsClimbable){
					currentlyClimbingWall = false;
				}
				
				
				if (climbingMovement > 0 || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0){
					hasNotMovedOnWallYet = false;
				}
			}
			
			//checking to see if player changed their direction from left to right/right to left
			if ((Input.GetAxis("Horizontal") > 0 && horizontalAxis <= 0 || Input.GetAxis("Horizontal") < 0 && horizontalAxis >= 0)){
				axisChanged = true;
			}
			else {
				axisChanged = false;
			}
			horizontalAxis = Input.GetAxis("Horizontal");
			lastRot3 = transform.rotation;
			
			//rotating the player
			if (rotationState != 0){
				//if we are just getting on the wall
				if (!finishedRotatingToWall){
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationNormal, (rotationToClimbableObjectSpeed2*2) * Time.deltaTime);
				}
				//if we are already on the wall, and have finished rotating to it
				else {
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationNormal, climbRotationSpeed2 * Time.deltaTime);
				}
			}
			if (stayUpright2){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
			}
			
		}
		else {
			lastYPosOnWall = 0;
			climbingHeight = transform.position.y;
			hasNotMovedOnWallYet = true;
		}
		
	}
	
	void CheckIfStuck () {
		
		if (pushAgainstWallIfPlayerIsStuck2){
			//if player is off of the surface of the wall
			if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward + transform.up*1.2f, out hit, collisionLayers)){
				distFromWallWhenStuck = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
				//push player forward to wall
				if (currentlyClimbingWall && !pullingUp
				&& (distFromWallWhenStuck >= 0.35f || firstDistFromWallWhenStuck != 0 && distFromWallWhenStuck >= firstDistFromWallWhenStuck + 0.05f)
				&& noCollisionTimer >= 5){
					transform.position += transform.forward/30;
				}
				
				//getting the player's first distance from the wall
				if (currentlyClimbingWall){
					if (firstDistFromWallWhenStuck == 0){
						firstDistFromWallWhenStuck = distFromWallWhenStuck;
					}
				}
				else {
					firstDistFromWallWhenStuck = 0;
				}
			}
			
			if (climbedUpAlready){
				//checking to see if the player is stuckInSamePos and not colliding
				if (!stuckInSamePos || pullingUp){
					stuckInSamePosNoCol = false;
				}
				else if (noCollisionTimer > 25){
					stuckInSamePosNoCol = true;
				}
				//checking to see if player is stuck on a collider
				if (currentlyClimbingWall && climbingMovement > 0 && (Input.GetAxisRaw("Horizontal") > 0 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < 0 && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Vertical") < 0) || pullingUp){
					
					//getting distance from the wall we are colliding with
					if (transform.position == lastPos){
						
						if (noCollisionTimer < 5 ){
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, collisionLayers)){
								distFromWallWhenStuck = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
							}
							if (!hasNotMovedOnWallYet && (Input.GetAxisRaw("Horizontal") > 0.1f && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < -0.1f && (!sideScrolling.lockMovementOnXAxis && !sideScrolling.lockMovementOnZAxis)
							|| Input.GetAxisRaw("Vertical") > 0.1f || Input.GetAxisRaw("Vertical") < -0.1f)){
								stuckInSamePos = true;
							}
						}
					}
					if (transform.rotation != lastRot2 || Mathf.Abs(transform.position.y - lastPos.y) > 0.001f || stuckInSamePosNoCol && noCollisionTimer < 2){
						stuckInSamePos = false;
						stuckInSamePosNoCol = false;
					}
					
					if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
						
						//if player is stuck
						if (!pullingUp && stuckInSamePos){
							
							//move the player slightly back to avoid collision
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, collisionLayers)){
								if (distFromWallWhenStuck != 0){
									transform.position = new Vector3((hit.point + (hit.normal / (distFromWallWhenStuck/(0.07f * (distFromWallWhenStuck/0.2601f))))*(distFromWallWhenStuck/0.2601f)).x, transform.position.y, (hit.point + (hit.normal / (distFromWallWhenStuck/(0.07f * (distFromWallWhenStuck/0.2601f))))*(distFromWallWhenStuck/0.2601f)).z);
								}
								else {
									transform.position = new Vector3((hit.point + (hit.normal/3.5f)).x, transform.position.y, (hit.point + (hit.normal/3.5f)).z);
								}
							}
							else if (transform.position == lastPos && transform.rotation == lastRot2 || noCollisionTimer < 2){
								transform.position -= transform.forward/100;
							}
							
						}
						
						//if player is stuck while climbing over a ledge, move the player slightly back and up to avoid collision
						if (pullingUp && noCollisionTimer < 5 && transform.position == lastPos){
							transform.position -= transform.forward/25;
							transform.position += transform.up/15;
						}
						
					}
					
				}
			}
			lastPos = transform.position;
		}
		lastRot2 = transform.rotation;
		
	}
	
	void ScriptEnablingDisabling () {
		
		//enabling and disabling scripts while player is on wall
		if (currentlyClimbingWall || turnBack || back2 || pullingUp){
			//if scripts have not been disabled/enabled yet
			if (!onWallScriptsFinished){
				if (scriptsToDisableOnGrab != null){
					foreach (string script in scriptsToDisableOnGrab)
					{
						scriptToDisable = GetComponent(script) as MonoBehaviour;
						if (scriptToDisable != null){
							scriptToDisable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingScripts){
							scriptWarning = true;
						}
					}
				}
				if (scriptsToEnableOnGrab != null){
					foreach (string script in scriptsToEnableOnGrab)
					{
						scriptToEnable = GetComponent(script) as MonoBehaviour;
						if (scriptToEnable != null){
							scriptToEnable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingScripts){
							scriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingScripts = true;
			}
			onWallScriptsFinished = true;
			
		}
		//undoing enabling and disabling scripts when player lets go of wall
		else {
			//if scripts have not been un-disabled/enabled yet
			if (onWallScriptsFinished){
				if (scriptsToDisableOnGrab != null){
					foreach (string script in scriptsToDisableOnGrab)
					{
						scriptToDisable = GetComponent(script) as MonoBehaviour;
						if (scriptToDisable != null){
							scriptToDisable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingScripts || currentlyClimbingWall || turnBack || back2){
							scriptWarning = true;
						}
					}
				}
				if (scriptsToEnableOnGrab != null){
					foreach (string script in scriptsToEnableOnGrab)
					{
						scriptToEnable = GetComponent(script) as MonoBehaviour;
						if (scriptToEnable != null){
							scriptToEnable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingScripts || currentlyClimbingWall || turnBack || back2){
							scriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingScripts = true;
			}
			onWallScriptsFinished = false;
			
		}
		
		//all loops that enable or disable scripts have finished, so we set currentlyEnablingAndDisablingScripts to false
		if (!currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			currentlyEnablingAndDisablingScripts = false;
		}
		//warns the user if any script names they entered do not exist on the player
		if (scriptWarning){
			if (scriptsToDisableOnGrab != null){
				foreach (string script in scriptsToDisableOnGrab)
				{
					scriptToDisable = GetComponent(script) as MonoBehaviour;
					if (scriptToDisable == null){
						Debug.Log("<color=red>The script to disable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			if (scriptsToEnableOnGrab != null){
				foreach (string script in scriptsToEnableOnGrab)
				{
					scriptToEnable = GetComponent(script) as MonoBehaviour;
					if (scriptToEnable == null){
						Debug.Log("<color=red>The script to enable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			scriptWarning = false;
		}
		
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		
		noCollisionTimer = 0;
		
		//determining slope angles
		slidingAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slidingAngle >= grounded.slopeLimit) {
            slidingVector = hit.normal;
            if (slidingVector.y == 0){
				slidingVector = Vector3.zero;
			}
        }
		else {
            slidingVector = Vector3.zero;
        }
 
        slidingAngle = Vector3.Angle(hit.normal, moveDirection - Vector3.up * moveDirection.y);
        if (slidingAngle > 90) {
            slidingAngle -= 90.0f;
            if (slidingAngle > grounded.slopeLimit){
				slidingAngle = grounded.slopeLimit;
			}
            if (slidingAngle < grounded.slopeLimit){
				slidingAngle = 0;
			}
        }
		
		//climbing walls/ladders
		if (hit.gameObject.tag == climbableTag2 && wallIsClimbable && jumpedOffClimbableObjectTimer >= 0.3f
		&& (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)){
			if (snapToCenterOfObject2 && !snappingToCenter && !currentlyClimbingWall){
				snapTimer = 0;
				snappingToCenter = true;
			}
			if (!currentlyClimbingWall && GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			climbDirection = Vector3.zero;
			moveDirection = Vector3.zero;
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		
		foreach (ContactPoint contact in hit.contacts) {
			noCollisionTimer = 0;
			
			//determining slope angles
			slidingAngle = Vector3.Angle(contact.normal, Vector3.up);
			if (slidingAngle >= grounded.slopeLimit) {
				slidingVector = contact.normal;
				if (slidingVector.y == 0){
					slidingVector = Vector3.zero;
				}
			}
			else {
				slidingVector = Vector3.zero;
			}
 
			slidingAngle = Vector3.Angle(contact.normal, moveDirection - Vector3.up * moveDirection.y);
			if (slidingAngle > 90) {
				slidingAngle -= 90.0f;
				if (slidingAngle > grounded.slopeLimit){
					slidingAngle = grounded.slopeLimit;
				}
				if (slidingAngle < grounded.slopeLimit){
					slidingAngle = 0;
				}
			}
        }
		
		//climbing walls/ladders
		if (hit.gameObject.tag == climbableTag2 && wallIsClimbable && jumpedOffClimbableObjectTimer >= 0.3f
		&& (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)){
			if (snapToCenterOfObject2 && !snappingToCenter && !currentlyClimbingWall){
				snapTimer = 0;
				snappingToCenter = true;
			}
			if (!currentlyClimbingWall && GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			moveDirection = Vector3.zero;
		}
		
	}
	
	void OnDisable() {
		
		//resetting values
		if (GetComponent<Rigidbody>()){
			if (!GetComponent<CharacterController>() || GetComponent<CharacterController>() && !GetComponent<CharacterController>().enabled){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}
		moveDirection.y = 0;
		currentlyOnWall = false;
		currentlyClimbingWall = false;
		turnBack = false;
		back2 = false;
		slidingVector = Vector3.zero;
		
    }
	
}