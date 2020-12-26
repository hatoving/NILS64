/// All in One Game Kit - Easy Ledge Climb Character System
/// PlayerController.cs
///
/// As long as the player has a CharacterController or Rigidbody component, this script allows the player to:
/// 1. Move and rotate (Movement).
/// 2. Slide down slopes (Movement).
/// 3. Perform any combo of jumps with different heights and animations (Jumping).
/// 4. Perform a double jump (Jumping).
/// 5. Wall jump (Wall Jumping).
/// 6. Perform any combo of attacks with different strengths and animations (Attacking).
/// 7. Climb ladders and walls (Climbing).
/// 8. Swim (Swimming).
/// 9. Ride moving and rotating platforms (Moving Platforms).
///
/// NOTE: *You should always set a layer for your player so that you can disable collisions with that layer (by unchecking it in the script's Collision Layers).
///	If you do not, the raycasts and linecasts will collide with the player himself and keep the script from working properly!*
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public Transform playerCamera; //the camera set to follow the player
	public float gravity = 20.00f; //the amount of downward force, or "gravity," that is constantly being applied to the player
	public float slopeLimit = 25.00f; //the maximum angle of a slope you can stand on without sliding down
	
	//Grounded
	[System.Serializable]
	public class Grounded {
		public bool showGroundDetectionRays; //shows the rays that detect whether the player is grounded or not
		public float maxGroundedHeight = 0.2f; //the maximum height of the ground the ground detectors can hit to be considered grounded
		public float maxGroundedRadius = 0.2f; //the maximum radius of the area ground detectors can hit to be considered grounded
		public float maxGroundedDistance = 0.2f; //the maximum distance you can be from the ground to be considered grounded
		public bool currentlyGrounded; //determines if player is currently grounded/on the ground
	}
	
	//Movement
	[System.Serializable]
	public class Movement {
		public float forwardSpeed = 6.0f; //player's speed when running forward
		public float sideSpeed = 4.0f; //player's speed when running sideways
		public float backSpeed = 5.0f; //player's speed when running backwards
		[System.Serializable]
		public class Running {
			public bool useRunningButton = false; //allows the player to multiply his movement speed when the run button is pressed
			public string runInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to make the player run
			public float runSpeedMultiple = 1.3f; //player's movement speed while the player is running/the run button is held down (multiplied by move speed)
		}
		public Running running = new Running(); //variables that determine whether or not the player uses a running button to run
		[System.Serializable]
		public class Crouching {
			public bool allowCrouching = true; //determines whether or not the player is allowed to crouch
			public float crouchMovementSpeedMultiple = 0.4f; //player's movement speed while crouching (multiplied by move speed)
			public float crouchColliderHeightMultiple = 0.7f; //what to multiply the player's collider height while crouching by
		}
		public Crouching crouching = new Crouching(); //variables that determine whether or not the player can crouch
		public float midAirMovementSpeedMultiple = 1.1f; //player's movement speed in mid-air (multiplied by move speed)
		public float acceleration = 50; //how fast the player will reach their maximum speed
		public float movementFriction = 0; //the amount of friction applied to the player's movement
		public float rotationSpeed = 8; //player's rotation speed
		public float midAirRotationSpeedMultiple = 1; //player's rotation speed in mid-air (multiplied by rotationSpeed)
		public float slopeSlideSpeed = 1; //how quickly you slide down slopes
		public float slideFriction = 4; //the amount of friction applied to the player from sliding down a slope
		public bool hardStickToGround = false; //by using a raycast, this option sets the position of the player to the position of the ground under him
		
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
		public SideScrolling sideScrolling = new SideScrolling(); //variables that determine whether or not the player uses 2.5D side-scrolling
		
		[System.Serializable]
		public class FirstPerson {
			public bool useCameraControllerSettingsIfPossible = true; //if the player camera has the script: "CameraController.cs" attached to it, the player will use the same first person settings as the camera
			public bool alwaysUseFirstPerson = false; //allows the player to always stay in first person mode
			public bool switchToFirstPersonIfInputButtonPressed = false; //switches to first person mode and back when the "firstPersonInputButton" is pressed
			public string firstPersonInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to enter first person mode
			public bool startOffInFirstPersonModeForSwitching = false; //if the player is allowed to switch to first person mode, start off in first person mode instead of having to switch to it first
			public bool walkBackwardsWhenDownKeyIsPressed = true; //allows the player to walk backwards (instead of turn around) when the down key is pressed
			public bool onlyRotateWithCamera = true; //does not allow the arrow keys to change the player's direction; only allows the player to rotate to the direction that the camera is facing
		}
		public FirstPerson firstPerson = new FirstPerson(); //variables that determine whether or not the player uses first person mode
		
	}
	
	//Jumping
	[System.Serializable]
	public class Jumping {
		public float [] numberAndHeightOfJumps = {6, 8, 12}; //the number of jumps the player can perform and the height of the jumps (the elements)
		public float timeLimitBetweenJumps = 1; //the amount of time you have between each jump to continue the jump combo
		public bool allowJumpWhenSlidingFacingUphill = false; //determines whether or not you are allowed to jump when you are facing uphill and sliding down a slope
		public bool allowJumpWhenSlidingFacingDownhill = true; //determines whether or not you are allowed to jump when you are facing downhill and sliding down a slope
		public bool doNotIncreaseJumpNumberWhenSliding = true; //only allows the player to perform their first jump when sliding down a slope
		public GameObject jumpLandingEffect; //optional dust effect to appear after landing jump
		public bool allowDoubleJump = true; //determines whether or not you are allowed to double jump
        public bool allowEndlessDoubleJump = true; //determines whether or not you are allowed to double jump
        public bool doubleJumpPerformableOutOfWallJump = true; //(if allowDoubleJump is true) determines whether or not the player can perform their double jump if they are in mid-air as a result of wall jumping
		public bool doubleJumpPerformableIfInMidAirInGeneral = true; //(if allowDoubleJump is true) determines whether or not the player can perform their double jump simply because they are in mid-air (instead of having to be in mid-air as a result of jumping)
		public float doubleJumpHeight = 7; //height of double jump
		public GameObject doubleJumpEffect; //optional effect to appear when performing a double jump
		public float maxFallingSpeed = 90; //the maximum speed you can fall
	}
	
	//Wall Jumping
	[System.Serializable]
	public class WallJumping {
		public bool allowWallJumping = true; //determines whether or not you are allowed to wall jump
		public float minimumWallAngle = 80; //the minimum angle a wall can be to wall jump off of it
		public float wallJumpDistance = 6; //distance of wall jump
		public float wallJumpHeight = 10; //height of wall jump
		public float wallJumpDecelerationRate = 2; //how quickly the momentum from the wall jump stops
		public float overallMovementSpeed = 2; //player's movement speed in mid-air
		public float forwardMovementSpeedMultiple = 0.85f; //player's speed when moving forward in mid air (multiplied by overallMovementSpeed)
		public float sideMovementSpeedMultiple = 0.85f; //player's speed when moving sideways in mid air (multiplied by overallMovementSpeed)
		public float backMovementSpeedMultiple = 0.75f; //player's speed when moving backwards in mid air (multiplied by overallMovementSpeed)
		public float rotationSpeedMultiple = 0; //player's rotation speed in mid-air (multiplied by rotationSpeed)
		public float distanceToKeepFromWallWhenOnWall = 1; //the distance the player keeps from the wall he is currently stuck to
		public bool useWallJumpTimeLimit = true; //allows the use of a time limit to wall jump when on walls
		public float wallJumpTimeLimit = 2; //the amount of time you can stay on a wall before falling
		public bool slideDownWalls = true; //allows player to slide down if on a wall
		public float slideDownSpeed = 8; //the speed at which the player slides down walls
		public float rotationToWallSpeed = 6; //how quickly the player rotates onto a wall for a wall jump
		public float inputPercentageNeededToWallJump = 50; //the amount of input needed to be applied to the joystick or key in order to stick to a wall for a wall jump
		public bool showWallJumpDetectors = false; //determines whether to show or hide the detectors that allow wall jumping
		public float spaceOnWallNeededToWallJumpUpAmount = 0.0f; //moves the rays that detect the amount of open space on a wall up and down
		public float spaceOnWallNeededToWallJumpHeight = 0.0f; //changes the height of the rays that detect the amount of open space on a wall
		public float spaceOnWallNeededToWallJumpLength = 0.0f; //changes the length of the rays that detect the amount of open space on a wall
		public float spaceOnWallNeededToWallJumpWidth = 0.0f; //changes the width of the rays that detect the amount of open space on a wall
		public float spaceBelowNeededToWallJump = 0.0f; //changes the minimum distance from the ground you must be in order to wall jump
	}
	
	//Attacking
	[System.Serializable]
	public class Attacking {
		
		//ground attacks
		[System.Serializable]
		public class Ground {
			public float [] numberAndStrengthOfAttacks = {1, 1, 2}; //the number of attacks the player can perform and the strength of each one (the elements)
			public float comboTimeLimit = 0.5f; //the amount of time you have to wait between each attack to continue the ground attack combo
			public bool rememberAttackButtonPresses = false; //allows the player to press the attack button multiple times, then continue the combo based on the number of times the button was pressed (before the combo resets)
			[System.Serializable]
			public class WaitingBeforeAttackingAgain {
				public float waitingTime = 0.2f; //the amount of time you have to wait (after each attack) before you can continue the attack combo
				public bool waitForAnimationToFinish = false; //waits for the current attack animation to finish before continuing the combo
			}
			public WaitingBeforeAttackingAgain waitingBeforeAttackingAgain = new WaitingBeforeAttackingAgain(); //variables that determine how long the player must wait before attacking again
		}
		public Ground ground = new Ground(); //variables for the player's ground attacks
		
		//crouch attacks
		[System.Serializable]
		public class Crouch {
			public bool allowCrouchAttack = true; //allows the player to attack while crouching
			public float crouchAttackStrength = 1; //the strength of the player's crouch attack
			public float timeLimitBetweenCrouchAttacks = 0.5f; //the amount of time you have to wait between each attack to crouch attack again
		}
		public Crouch crouch = new Crouch(); //variables for the player's crouch attacks
		
		//air attacks
		[System.Serializable]
		public class Air {
			public float [] numberAndStrengthOfMidAirAttacks = {1}; //the number of maximum attacks the player can perform and the strength of each one (the elements)
			public bool onlyAllowAttackOnceInMidAir = true; //only allows the player to use his mid-air attack once while in the air
			public float comboTimeLimit = 0.5f; //the amount of time you have to wait between each attack to continue the mid air attack combo
			public bool rememberAttackButtonPresses = false; //allows the player to press the attack button multiple times, then continue the combo based on the number of times the button was pressed (before the combo resets)
			[System.Serializable]
			public class WaitingBeforeAttackingAgain {
				public float waitingTime = 0.2f; //the amount of time you have to wait (after each attack) before you can continue the attack combo
				public bool waitForAnimationToFinish = false; //waits for the current attack animation to finish before continuing the combo
			}
			public WaitingBeforeAttackingAgain waitingBeforeAttackingAgain = new WaitingBeforeAttackingAgain(); //variables that determine how long the player must wait before attacking again
		}
		public Air air = new Air(); //variables for the player's mid air attacks
		
		public string attackInputButton = "Fire1"; //the button (found in "Edit > Project Settings > Input") that is used to attack
		
	}
	
	//Climbing
	[System.Serializable]
	public class Climbing {
		
		public string climbableTag = "Ladder"; //the tag of a climbable object
		public bool climbVertically = true; //determines whether or not the player is allowed to climb vertically
		public bool climbHorizontally = false; //determines whether or not the player is allowed to climb horizontally
		public float climbMovementSpeed = 4; //how quickly the player climbs on walls
		public float climbRotationSpeed = 10; //how quickly the player rotates on walls
		public bool snapToCenterOfObject = true; //snaps the player to the middle (along the x and z-axis) of the climbable object (most useful for ladders)
		public bool moveInBursts = true; //move in bursts (while on a climbable object)
		public float burstLength = 1; //the amount of time a movement burst lasts
		public bool stayUpright = false; //determines whether or not the player can rotate up and down
		public float distanceToPushOffAfterLettingGo = 0.5f; //the distance the player pushes off of a ladder/wall after letting go
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
		public string[] scriptsToDisableOnGrab = {"LedgeClimbController"}; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
		
		public bool pushAgainstWallIfPlayerIsStuck = true; //if the script considers the player stuck, the player pushes himself away from the wall until he is free
		
	}
	
	//Swimming
	[System.Serializable]
	public class Swimming {
		
		public bool allowSwimming = true; //determines whether or not the player is allowed to swim
		public float swimMovementSpeed = 6; //the player's movement speed while swimming
		public float accelerationRate = 6; //how fast the player will reach their maximum swimming speed
		public float decelerationRate = 4; //how fast the player will reach their minimum swimming speed (0 speed)
		public float swimRotationSpeed = 6; //the player's rotation speed while swimming
		public int ySwimmingRotationMaxLimit = 70; //the maximum y value the player can rotate to while swimming vertically
		public int ySwimmingRotationMinLimit = -80; //the minimum y value the player can rotate to while swimming vertically
		public int yIdleRotationMaxLimit = 40; //the maximum y value the player can rotate to while not swimming vertically
		public int yIdleRotationMinLimit = -50; //the minimum y value the player can rotate to while not swimming vertically
		public Vector3 idleFloatingDirection = new Vector3(0, 0.2f, 0); //the direction the player floats in while he is not swimming
		public bool invertXRotation = false; //inverts the player's left and right swimming rotation
		public bool invertYRotation = false; //inverts the player's up and down swimming rotation
		public float angleNeededToJumpOut = 39; //the minimum angle the player must be rotated to jump out of the water
		public float jumpInHeight = 0; //the height the player must sink before entering the water
		public float jumpOutHeight = 0; //the height the player must swim before jumping out of the water (the player cannot swim any higher once he reaches this point)
		public float walkOutHeight = 0; //the height the player must swim before walking out of the water, and on to the surface (note: the player cannot swim any higher than the jump out height)
		
		public bool allowColliderRotationByUsingRigidbody = true; //allows the collider of the player to rotate while swimming (by adding a Rigidbody+CapsuleCollider, and disabling the CharacterController temporarily, since CharacterController colliders can only stay upright)
		
		[System.Serializable]
		public class FirstPerson {
			public bool allowMovement = true; //allows the player to move while in first person mode
			public bool allowShiftingWithJoystick = true; //allows the player to rotate with the camera, and move left, right, up, or down with the joystick, while mouse orbiting in first person mode
			public float joystickMovementSpeed = 4; //the speed the player moves left, right, up, or down (with the joystick) while mouse orbiting in first person mode
			public float joystickDecelerationSpeed = 3; //the speed the player stops moving left, right, up, or down (with the joystick) while mouse orbiting in first person mode
			public float cameraAngleOffset = 0; //the angle of the player's head upon entering first person mode
			
			[System.Serializable]
			public class RotationLimitsIfMovementIsNotAllowed {
				public int yMinLimit = -68; //the minimum y value the camera can orbit to
				public int yMaxLimit = 68; //the maximum y value the camera can orbit to
				public int xMinLimit = -68; //the minimum x value the camera can orbit to
				public int xMaxLimit = 68; //the maximum x value the camera can orbit to
			}
			public RotationLimitsIfMovementIsNotAllowed rotationLimitsIfMovementIsNotAllowed = new RotationLimitsIfMovementIsNotAllowed(); //variables that limit the camera's (and in turn, the player's) first person rotation while swimming
		}
		public FirstPerson firstPerson = new FirstPerson(); //variables that determine how the player swims in first person mode
		
		public string[] scriptsToEnableOnEnter; //scripts to enable when the player enters the water (scripts disable when the player exits the water)
		public string[] scriptsToDisableOnEnter = {"LedgeClimbController"}; //scripts to disable when the player enters the water (scripts enable when the player exits the water)
		
		public GameObject splashEffect; //optional splash effect to appear after landing in water
		
	}
	
	[System.Serializable]
	public class MovingPlatforms {
		public bool allowMovingPlatformSupport = true; //determines whether or not the player can move with moving platforms
		public string movingPlatformTag = "Platform"; //the tag of the moving platform objects
	}
	
	public Grounded grounded = new Grounded(); //variables that detect whether the player is grounded or not
	public Movement movement = new Movement(); //variables that control the player's movement
	public Jumping jumping = new Jumping(); //variables that control the player's jumping
	public WallJumping wallJumping = new WallJumping(); //variables that control the player's wall jumping
	public Attacking attacking = new Attacking(); //variables that control the player's attacks
	public Climbing[] climbing = new Climbing[1]; //variables that control the player's ladder and wall climbing
	public Swimming swimming = new Swimming(); //variables that control the player's swimming
	public MovingPlatforms movingPlatforms = new MovingPlatforms(); //variables that determine whether the player moves with moving platforms or not
	
	//Grounded variables without class name
	private Vector3 maxGroundedHeight2;
	private float maxGroundedRadius2;
	private float maxGroundedDistance2;
	private Vector3 maxGroundedDistanceDown;
	
	//Movement variables without class name
	private float forwardSpeed2;
	private float sideSpeed2;
	private float backSpeed2;
	private float midAirMovementSpeedMultiple2;
	private float acceleration2;
	private float rotationSpeed2;
	private float slopeSlideSpeed2;
	
	//Jumping variables without class name
	private float [] jumpsToPerform;
	private bool allowDoubleJump2;
	private bool doubleJumpPerformableIfInMidAirInGeneral2;
	private float doubleJumpHeight2;
	private float timeLimitBetweenJumps2;
	private float maxFallingSpeed2;
	private GameObject jumpLandingEffect2;
	private GameObject doubleJumpEffect2;
	
	//WallJumping variables without class name
	private Vector3 spaceOnWallNeededToWallJumpUpAmount2;
	private float spaceOnWallNeededToWallJumpHeight2;
	private float spaceOnWallNeededToWallJumpLength2;
	private float spaceOnWallNeededToWallJumpWidth2;
	private Vector3 spaceBelowNeededToWallJump2;
	
	//Attacking variables without class name
	[System.NonSerialized]
	public float [] attacksToPerform;
	private float comboTimeLimitGround;
	private float comboTimeLimitAir;
	private bool rememberAttackButtonPressesGround;
	private bool rememberAttackButtonPressesAir;
	
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
	
	//Moving platform variables without class name
	private bool allowMovingPlatformSupport; //determines whether or not the player can move with moving platforms
	private string movingPlatformTag; //the tag of the moving platform objects
	
	//Swimming variables without class name
	private float jumpInHeight2;
	private float jumpOutHeight2;
	
	//private movement variables
	private Vector3 moveDirection; //the direction that the player moves in
	private float moveSpeed; //the current speed of the player
	private float moveSpeedAndFriction; //the current speed of the player with friction applied
	private float runSpeedMultiplier; //what to multiply the player's move speed by if we are running/the run button is held down
	private float accelerationRate; //how fast the player is accelerating
	private float deceleration = 1; //how fast the player will reach the speed of 0
	private float decelerationRate; //how fast the player is decelerating
	private float h; //the absolute value of the "Horizontal" axis minus the absolute value of the "Vertical" axis
	private float v; //the absolute value of the "Vertical" axis minus the absolute value of the "Horizontal" axis
	private Vector3 directionVector; //the direction that the joystick is being pushed in
	private bool inBetweenSlidableSurfaces; //determines whether you are in between two slidable surfaces or not
	private bool uphill; //determines whether you are going uphill on a slope or not
	private bool angHit; //determines whether or not a raycast going straight down (with a distance of 1) is hitting
	private float collisionSlopeAngle; //the angle of the surface you are currently standing on
	private float raycastSlopeAngle; //the angle of the surface being raycasted on
	private float slidingAngle; //the angle of the last slidable surface you collided with or are currently colliding with
	private bool slidePossible; //determines whether you can slide down a slope or not
	private bool sliding; //determines whether you are sliding down a slope or not
	private float slideSpeed = 6; //player's downward speed on slopes
	private Vector3 slidingVector; //the normal of the object you are colliding with
	private Vector3 slideMovement; //Vector3 that slerps to the normal of the object you are colliding with (slidingVector)
	[HideInInspector]
	public float bodyRotation; //the rotation that the player lerps to
	//wall detecting
	private bool touchingWall;
	private bool middleTouchingWall;
	private bool leftTouchingWall;
	private bool rightTouchingWall;
	private bool pushingThroughWall;
	private bool collidingWithWall;
	private bool inCorner;
	private float updateFrame = 2;
	//crouching
	private bool canCrouch; //determines if the player can crouch (or if he will be able to crouch after landing on the ground)
	[HideInInspector]
	public bool crouching; //determines whether the player is currently crouching or not
	[HideInInspector]
	public bool crouchCancelsAttack; //if the player's attack key is the same as his crouch key, crouch instead of attacking
	private bool finishedCrouching; //determines whether the player has finished crouching/uncrouching
	private float originalColliderY;
	private float originalColliderHeight;
	private bool colliderAdjusted;
	private float oldYPos;
	private Vector3 feetPosition;
	private Vector3 headPosition;
	private float feetPositionNew;
	private float headPositionNew;
	[HideInInspector]
	public Vector3 colliderBottom; //actual bottom of collider
	[HideInInspector]
	public Vector3 colliderTop; //actual top of collider
	[HideInInspector]
	public Vector3 colliderBottom2;
	[HideInInspector]
	public Vector3 colliderTop2;
	private Vector3 colliderLeft; //actual left side of collider
	private Vector3 colliderRight; //actual right side of collider
	private Vector3 colliderLeft2;
	private Vector3 colliderRight2;
	private Vector3 colliderLeftLonger;
	private Vector3 colliderRightLonger;
	private Vector3 colliderLeftLonger2;
	private Vector3 colliderRightLonger2;
	private Vector3 colliderBack; //actual back of collider
	private Vector3 colliderFront; //actual front of collider
	private Vector3 colliderBackLonger;
	private Vector3 colliderFrontLonger;
	private Vector3 colliderFrontLonger2;
	private bool colliderInWall;
	[HideInInspector]
	public bool canCrouchToAction;
	//first person mode
	[HideInInspector]
	public bool firstPersonButtonPressed;
	[HideInInspector]
	public bool firstPersonStart;
	[HideInInspector]
	public bool inFirstPersonMode;
	private bool enabledLastUpdate;
	
	//private jumping variables
	private int currentJumpNumber; //the number of the most current jump performed
	private int totalJumpNumber; //the total amount of jumps set
	private float airSpeed; //player's movement speed in mid-air
	private float jumpTimer; //time since last jump was performed
	private float jumpPerformedTime; //time since last jump was first performed
	private bool inMidAirFromJump; //player is in mid-air as a result of jumping
	private bool jumpEnabled; //enables jumping while the script is enabled and disables jumping when the script is disabled
	private bool jumpPossible; //determines whether a jump is possible or not
	private bool doubleJumpPossible = true; //determines whether a double jump is possible or not
	private bool jumpPressed; //"Jump" button was pressed
	private float jumpPressedTimer; //time since "Jump" button was last pressed
	private bool jumpPerformed; //determines whether a jump was just performed
	private bool headHit; //determines if player's head hit the ceiling
	private float yPos; //player's position on the y-axis
	private float yVel; //player's y velocity
	private Vector3 pos; //position and collider bounds of the player
	private Vector3 contactPoint; //the specific point where the player and another object are colliding
	private float noCollisionTimer; //time since last collision
	
	//private attacking variables
	[System.NonSerialized]
	public int attackState; //the current attacking state of the player (on the ground or in the air)
	[System.NonSerialized]
	public int currentAttackNumber; //the number of the most current jump performed
	[System.NonSerialized]
	public int totalAttackNumber; //the total amount of jumps set
	private int attackPressesRemembered; //the number of times the attack button has been pressed before the combo resets
	private float attackPressedTimer; //time since attack button was last pressed
	private bool attackPressed;
	private bool attackButtonPressed;
	[System.NonSerialized]
	public float attackTimer; //time since last attack was performed
	private bool attackFinished; //the attack has finished
	[HideInInspector]
	public bool attackFinishedLastUpdate; //the attack finished in the last update
	private float waitingBetweenAttacksTimerGround; //time since last ground attack was performed or the attack state was switched
	private float waitingBetweenAttacksTimerAir; //time since last air attack was performed or the attack state was switched
	private float attackPerformedTime; //time since last jump was first performed
	private bool canAttack; //determines whether or not the player can currently attack
	private bool attackedInMidAir; //determines if the player already attacked while in mid-air
	
	//private wall jumping variables
	[HideInInspector]
	public bool currentlyOnWall;
	private bool onWallLastUpdate;
	[HideInInspector]
	public bool turningToWall;
	private bool middleWallJumpable;
	private bool leftWallJumpable;
	private bool rightWallJumpable;
	private Vector3 wallNormal;
	private Vector3 wallHitPoint;
	private float forwardDir;
	private float rightDir;
	private float originalForwardDir;
	private float originalRightDir;
	private bool jumpedOffWallForWallJump;
	private Vector3 originalWallJumpDirection;
	private Vector3 wallJumpDirection;
	private float angleBetweenPlayerAndWall;
	private bool wallBackHit;
	private float distFromWall;
	private float firstDistFromWall;
	private bool inMidAirFromWallJump;
	private float wallJumpTimer;
	private float slideDownSpeed2;
	private bool onWallAnimation;
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
	private string[] wallScriptsToEnableOnGrab; //scripts to enable when the player grabs on to a wall (scripts disable when the player lets go of a wall)
	private MonoBehaviour wallScriptToEnable; //the current script being enabled (or disabled when the player lets go of a wall) in the wallScriptsToEnableOnGrab array
	private string[] wallScriptsToDisableOnGrab; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
	private MonoBehaviour wallScriptToDisable; //the current script being disabled (or enabled when the player lets go of a wall) in the wallScriptsToDisableOnGrab array
	private bool currentlyEnablingAndDisablingWallScripts = false; //determines whether the scripts in wallScriptsToEnableOnGrab or wallScriptsToDisableOnGrab are currently being enabled/disabled or not
	private bool wallScriptWarning = false; //determines whether or not the user entered any script names that do not exist on the player
	private bool onWallScriptsFinished = false; //determines whether the scripts in wallScriptsToEnableOnGrab or wallScriptsToDisableOnGrab have finished being enabled/disabled or not
	
	//private swimming variables
	[HideInInspector]
	public bool inWater;
	private float inWaterTimer = 5;
	[HideInInspector]
	public float outOfWaterTimer = 5;
	[HideInInspector]
	public float currentHeadPosition;
	[HideInInspector]
	public float currentFeetPosition;
	private float rbHeadPositionOriginal;
	private float rbFeetPositionOriginal;
	private float rbHeadPositionInWater;
	private float rbFeetPositionInWater;
	[HideInInspector]
	public float rbHeadPositionInWater2;
	[HideInInspector]
	public float rbFeetPositionInWater2;
	private Vector3 boundMin1;
	private Vector3 boundMax1;
	private Vector3 boundMin2;
	private Vector3 boundMax2;
	private bool submergedEnough;
	private bool submergedOnceAlready;
	private bool canPressSwimButton;
	private bool swimButtonPressed;
	private bool reachedTopOfWater;
	private bool atJumpHeight;
	private bool stillAtTop;
	private bool groundedAfterSwimming;
	private bool notInWaterCollider;
	private float inWaterPosY;
	private Vector3 lastPosBeforeCollision;
	private Vector3 lastRotBeforeCollision;
	//swimming rotation
	[HideInInspector]
	public float xDeg = 0.0f;
	[HideInInspector]
    public float yDeg = 0.0f;
	private float xDeg2 = 0.0f;
	private bool yDegIsLessThan92;
	private float swimHorizontalValue = 1;
	private bool axisNotPushedLeftOrRight;
	private bool hPositive = true;
	private bool vPositive = true;
	private float hAxis;
	private float vAxis;
	private float hAxis2;
	private float vAxis2;
	private float horizontalRotValue = 0.0f;
	private float verticalRotValue = 0.0f;
	private float cameraYDeg;
	private float cameraYDeg2;
	private float cameraYDeg3;
	private float rotDiff;
	private bool rotDiffSet;
	private float yRotMinLimit;
	private float yRotMaxLimit;
	private Quaternion swimRotation;
	private bool canJumpOut;
	[HideInInspector]
	public float noCollisionWithWaterTimer = 5; //time since last collision with water tag
	private float noCollisionWithWaterSplashTimer = 5; //time since last collision with water tag when a splash was able to be made
	private bool collidingWithWater;
	private float ranThroughUpdateCount = 0;
	//swimming movement
	private Vector3 swimDirection;
	private float swimmingMovementSpeed;
	private bool swimmingMovementTransferred = true;
	//swimming animation
	private float swimState = 0;
	private bool donePulling;
	private bool justSwitchedToSwimming;
	private bool heldSwimButtonOnEnter;
	//enabling and disabling script variables
	private string[] swimScriptsToEnableOnEnter; //scripts to enable when the player enters the water (scripts disable when the player exits the water)
	private MonoBehaviour swimScriptToEnable; //the current script being enabled (or disabled when the player exits the water) in the swimScriptsToEnableOnGrab array
	private string[] swimScriptsToDisableOnEnter; //scripts to disable when the player enters the water (scripts enable when the player exits the water)
	private MonoBehaviour swimScriptToDisable; //the current script being disabled (or enabled when the player exits the water) in the swimScriptsToDisableOnGrab array
	private bool currentlyEnablingAndDisablingSwimScripts = false; //determines whether the scripts in swimScriptsToEnableOnGrab or swimScriptsToDisableOnGrab are currently being enabled/disabled or not
	private bool swimScriptWarning = false; //determines whether or not the user entered any script names that do not exist on the player
	private bool swimScriptsFinished = false; //determines whether the scripts in swimScriptsToEnableOnGrab or swimScriptsToDisableOnGrab have finished being enabled/disabled or not
	//enabling and disabling Rigidbody/CharacterController variables
	private bool playerUsesCC;
	private bool canChangeCC;
	
	//private moving platform variables
	private float noCollisionWithPlatformTimer; //time since last collision with platform tag
	[HideInInspector]
	public GameObject oldParent; //the player's parent before coming in contact with a platform
	[HideInInspector]
	public GameObject emptyObject; //empty object that undoes the platform's properties that affect the player's scale
	private GameObject emptyObjectParent; //parent of the empty object (the platform itself)
	
	private RaycastHit hit = new RaycastHit(); //information on the hit point of a raycast
	private RaycastHit frontHit = new RaycastHit(); //information on the hit point of a raycast in front of a player
	private RaycastHit backHit = new RaycastHit(); //information on the hit point of a raycast behind the player
	private Animator animator; //the "Animator" component of the script holder
	private CharacterController characterController; //the "CharacterController" component of the script holder (if he has one)
	private Rigidbody rigidBody; //the "Rigidbody" component of the script holder (if he has one)
	public LayerMask collisionLayers = ~(1 << 2); //the layers that the detectors (raycasts/linecasts) will collide with
	private LayerMask noWaterCollisionLayers = ~((1 << 2) | (1 << 4)); //the layers that the detectors (raycasts/linecasts) will collide with (minus the water layer)
	
	// Use this for initialization
	void Start () {
  

        //getting the player's CharacterController component (if he has one)
        if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			characterController = GetComponent<CharacterController>();
			rigidBody = null;
		}
		//getting the player's Rigidbody component (if he has one)
		else if (GetComponent<Rigidbody>()){
			rigidBody = GetComponent<Rigidbody>();
			characterController = null;
		}
		
		//initializing script
		StartUp();
		
		//resetting attack timer
		waitingBetweenAttacksTimerGround = attacking.ground.waitingBeforeAttackingAgain.waitingTime;
		waitingBetweenAttacksTimerAir = attacking.air.waitingBeforeAttackingAgain.waitingTime;
		
		//starting off in first person mode
		if (movement.firstPerson.switchToFirstPersonIfInputButtonPressed && movement.firstPerson.startOffInFirstPersonModeForSwitching
		&& (!playerCamera.GetComponent<CameraController>() || playerCamera.GetComponent<CameraController>() && !playerCamera.GetComponent<CameraController>().mouseOrbit.startOffMouseOrbitingForSwitching)){
			firstPersonStart = true;
			firstPersonButtonPressed = true;
		}
		//setting rotation limits for swimming
		yRotMinLimit = swimming.yIdleRotationMinLimit;
		yRotMaxLimit = swimming.yIdleRotationMaxLimit;
		
		//checking to see if the player (if using a Rigidbody) is using the "Use Gravity" option
		if (rigidBody && rigidBody.useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2 && !inWater){
			rbUsesGravity = true;
		}
		//checking to see if the player is currently using a CharacterController
		if (characterController && characterController.enabled){
			playerUsesCC = true;
		}
		
	}
	
	void StartUp () {
		//resetting script to make sure that everything initializes
		enabled = false;
		enabled = true;
	}
	
	// Update is called once per frame
	void Update () {

        //if (gravity < 20) 
        //{
         //   gravity += 12;
       // }

        //if (gravity > 20)
        //{
         //   gravity = 20;
        //}

        //getting the player's CharacterController component (if he has one)
        if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			characterController = GetComponent<CharacterController>();
			rigidBody = null;
		}
		//getting the player's Rigidbody component (if he has one)
		else if (GetComponent<Rigidbody>()){
			rigidBody = GetComponent<Rigidbody>();
			characterController = null;
		}
		
		//if the player is currently on a wall, set jumpedOffWallForWallJump to false
		if (currentlyOnWall){
			jumpedOffWallForWallJump = false;
		}
		//if the player has jumped off of a wall, set jumpedOffWallForWallJump to true
		else if (inMidAirFromWallJump && noCollisionTimer >= 5){
			jumpedOffWallForWallJump = true;
		}
		if (jumpedOffWallForWallJump && noCollisionTimer < 5 && inMidAirFromWallJump){
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
			inMidAirFromWallJump = false;
		}
		
		//storing values to variables
		//Grounded variables
		maxGroundedHeight2 = transform.up * grounded.maxGroundedHeight;
		maxGroundedRadius2 = grounded.maxGroundedRadius - 0.0075f;
		maxGroundedDistance2 = grounded.maxGroundedDistance;
		maxGroundedDistanceDown = Vector3.down*grounded.maxGroundedDistance;
		//Movement variables
		forwardSpeed2 = movement.forwardSpeed;
		sideSpeed2 = movement.sideSpeed;
		backSpeed2 = movement.backSpeed;
		if (!inMidAirFromWallJump){
			midAirMovementSpeedMultiple2 = movement.midAirMovementSpeedMultiple;
		}
		//wall jumps have their own mid-air speed and dampening, so during a wall jump, we set midAirMovementSpeedMultiple2 to 0 to avoid affecting it
		else {
			midAirMovementSpeedMultiple2 = 0;
		}
		acceleration2 = movement.acceleration;
		slopeSlideSpeed2 = movement.slopeSlideSpeed;
		//Jumping variables
		jumpsToPerform = jumping.numberAndHeightOfJumps;
		timeLimitBetweenJumps2 = jumping.timeLimitBetweenJumps;
		jumpLandingEffect2 = jumping.jumpLandingEffect;
		allowDoubleJump2 = jumping.allowDoubleJump;
		doubleJumpPerformableIfInMidAirInGeneral2 = jumping.doubleJumpPerformableIfInMidAirInGeneral;
		doubleJumpHeight2 = jumping.doubleJumpHeight;
		doubleJumpEffect2 = jumping.doubleJumpEffect;
		maxFallingSpeed2 = jumping.maxFallingSpeed;
		//Wall Jumping variables
		spaceOnWallNeededToWallJumpUpAmount2 = transform.up * wallJumping.spaceOnWallNeededToWallJumpUpAmount;
		spaceOnWallNeededToWallJumpHeight2 = wallJumping.spaceOnWallNeededToWallJumpHeight;
		spaceOnWallNeededToWallJumpLength2 = wallJumping.spaceOnWallNeededToWallJumpLength - 0.5f;
		spaceOnWallNeededToWallJumpWidth2 = wallJumping.spaceOnWallNeededToWallJumpWidth;
		spaceBelowNeededToWallJump2 = transform.up * wallJumping.spaceBelowNeededToWallJump;
		//Attacking variables
		if (attackState == 0){
			attacksToPerform = attacking.ground.numberAndStrengthOfAttacks;
		}
		else {
			attacksToPerform = attacking.air.numberAndStrengthOfMidAirAttacks;
		}
		comboTimeLimitGround = attacking.ground.comboTimeLimit;
		comboTimeLimitAir = attacking.air.comboTimeLimit;
		rememberAttackButtonPressesGround = attacking.ground.rememberAttackButtonPresses;
		rememberAttackButtonPressesAir = attacking.air.rememberAttackButtonPresses;
		//MovingPlatform variables
		allowMovingPlatformSupport = movingPlatforms.allowMovingPlatformSupport;
		movingPlatformTag = movingPlatforms.movingPlatformTag;
		//Swimming variables
		jumpInHeight2 = swimming.jumpInHeight/2;
		jumpOutHeight2 = (swimming.jumpOutHeight - 0.8f)/2;
		//enabling and disabling swim script variables
		swimScriptsToEnableOnEnter = swimming.scriptsToEnableOnEnter;
		swimScriptsToDisableOnEnter = swimming.scriptsToDisableOnEnter;

		//getting position and collider information for raycasts
		pos = transform.position;
        pos.y = GetComponent<Collider>().bounds.min.y + 0.1f;
		
		
		//showing wall jump detectors
		if (wallJumping.showWallJumpDetectors){
			//middle
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, Color.yellow);
			//left
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			//right
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			//below
			Debug.DrawLine(transform.position, transform.position - transform.up*0.3f - spaceBelowNeededToWallJump2, Color.cyan);
		}
		
		//showing climbing detectors
		DrawClimbingDetectors();
		
		//setting the player's "Animator" component (if player has one) to animator
		if (GetComponent<Animator>()){
			animator = GetComponent<Animator>();
		}
		
		//only allow regular movement if the player is not swimming
		if (!inWater){
			
			if (characterController && characterController.enabled){
				//checking to see if player's head hit the ceiling
				if (characterController.velocity.y < 0.5 && moveDirection.y > 0.5 && !grounded.currentlyGrounded){
					headHit = true;
				}
				else {
					headHit = false;
				}
				yVel = characterController.velocity.y;
			}
			else if (rigidBody){
				//checking to see if player's head hit the ceiling
				if (yPos == transform.position.y && rigidBody.velocity.y + 0.1f > yVel && !jumpPerformed && noCollisionTimer == 0 && !sliding && moveDirection.y > 0 && !grounded.currentlyGrounded){
					if (collisionSlopeAngle < slopeLimit || collisionSlopeAngle > 91){
						headHit = true;
					}
					else {
						headHit = false;
					}
				}
				else {
					headHit = false;
				}
				yPos = transform.position.y;
				yVel = rigidBody.velocity.y;
			}
			
			//if user set jumps, totalJumpNumber equals the number set
			if (jumpsToPerform.Length > 0){
				totalJumpNumber = jumpsToPerform.Length;
			}
			//if user did not set jumps, totalJumpNumber equals 0
			else {
				totalJumpNumber = 0;
			}
			
			//if the "Jump" button was pressed, jumpPressed equals true
			if (Input.GetButtonDown("Jump") && (!GetComponent<LedgeClimbController>() || GetComponent<LedgeClimbController>() && GetComponent<LedgeClimbController>().canGrabBackOn)){
				jumpPressedTimer = 0.0f;
				jumpPressed = true;
			}
			else{
				jumpPressedTimer += 0.02f;
			}
			
			//wait 0.2 seconds for jumpPressed to become false
			//this allows the player to press the "Jump" button slightly early and still jump once they have landed
			if (jumpPressedTimer > 0.2f){
				jumpPressed = false;
			}

			//jump
			if (grounded.currentlyGrounded){
				if (jumpPressed && jumpPossible && !jumpPerformed && totalJumpNumber > 0 && !onWallLastUpdate && jumpEnabled && (raycastSlopeAngle > slopeLimit && (uphill && jumping.allowJumpWhenSlidingFacingUphill || !uphill && jumping.allowJumpWhenSlidingFacingDownhill || inBetweenSlidableSurfaces) || raycastSlopeAngle <= slopeLimit) && canCrouchToAction){
					Jump();
				}
				doubleJumpPossible = true;
			}
			
			//double jump
			if (Input.GetButtonDown("Jump") && doubleJumpPossible && !grounded.currentlyGrounded && allowDoubleJump2 && jumpEnabled && (doubleJumpPerformableIfInMidAirInGeneral2 || !doubleJumpPerformableIfInMidAirInGeneral2 && inMidAirFromJump) && (jumping.doubleJumpPerformableOutOfWallJump || !inMidAirFromWallJump) && !currentlyOnWall && !currentlyClimbingWall && !onWallLastUpdate && canCrouchToAction){
                if ((!Physics.Raycast(pos, Vector3.down, out hit, 0.5f, noWaterCollisionLayers) || Physics.Raycast(pos, Vector3.down, out hit, 0.5f, noWaterCollisionLayers) && hit.transform.GetComponent<Collider>().isTrigger) && moveDirection.y < 0 || !grounded.currentlyGrounded && moveDirection.y >= 0)
                {
                    DoubleJump();
                    jumpPressed = false;

                    if (jumping.allowEndlessDoubleJump == false) 
                    {
                    doubleJumpPossible = false;
                }
				}
			}
			
			//enabling jumping while the script is enabled
			jumpEnabled = true;
			
			//checking to see if player was on the wall in the last update
			if (currentlyOnWall || currentlyClimbingWall || turnBack || back2){
				onWallLastUpdate = true;
			}
			else {
				onWallLastUpdate = false;
			}
			
			//counting how long it has been since last jump was first performed
			if (jumpPerformed){
				jumpPerformedTime += 0.02f;
			}
			else {
				jumpPerformedTime = 0;
			}
			
			//if in mid air as a result of jumping
			if (inMidAirFromJump){
				
				if (grounded.currentlyGrounded){
					
					if (!jumpPerformed){
						//creating the optional dust effect after landing a jump
						if (jumpLandingEffect2 != null){
							Instantiate(jumpLandingEffect2, transform.position + new Vector3(0, 0.05f, 0), jumpLandingEffect2.transform.rotation);
						}
					
						//once player has landed jump, stop jumping animation and return to movement
						if (animator != null && animator.runtimeAnimatorController != null){
							animator.CrossFade("Movement", 0.2f);
						}
					
						//once player has landed jump, set inMidAirFromJump to false
						inMidAirFromJump = false;
					}
					
					if (jumpTimer == 0 && jumpPerformedTime > 0.1f){
						//creating the optional dust effect after landing a jump
						if (jumpLandingEffect2 != null){
							Instantiate(jumpLandingEffect2, transform.position + new Vector3(0, 0.05f, 0), jumpLandingEffect2.transform.rotation);
						}
						jumpPerformed = false;
						inMidAirFromJump = false;
					}
					
				}
				
			}
			
			
			//if player is not in mid-air as a result of jumping, increase the jump timer
			if (!inMidAirFromJump) {
				jumpTimer += 0.02f;
			}
			
			//if the jump timer is greater than the jump time limit, reset current jump number
			if (jumpTimer > timeLimitBetweenJumps2 && timeLimitBetweenJumps2 > 0) {
				currentJumpNumber = totalJumpNumber;
			}
			
			//set animator's float parameter, "jumpNumber," to currentJumpNumber
			if (animator != null && animator.runtimeAnimatorController != null){
				animator.SetFloat("jumpNumber", currentJumpNumber);
			}
			
			//after jump is performed and jumpPerformed is true, set jumpPerformed to false
			if (jumpPerformed){
				if (!grounded.currentlyGrounded || headHit){
					jumpPerformed = false;
				}
			}
			
			//crouching
			if (movement.crouching.allowCrouching){
				//when the crouching button is pressed, determine if the player can crouch (or if he will be able to crouch after landing on the ground)
				if (!canCrouch && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && finishedCrouching){
					finishedCrouching = false;
					crouchCancelsAttack = true;
					canCrouch = true;
				}
				//when the crouching button is pressed and the player is already crouched: uncrouch (if there is enough space above the player's head)
				else if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, noWaterCollisionLayers) && finishedCrouching){
					finishedCrouching = false;
					crouchCancelsAttack = true;
					crouching = false;
					canCrouch = false;
				}
				//if the player is grounded and canCrouch is true, crouch
				if (grounded.currentlyGrounded && canCrouch && !crouching){
					crouchCancelsAttack = true;
					crouching = true;
				}
				//if the player performs a jump, wall climb, etc.: uncrouch
				if (crouching && (currentlyClimbingWall || currentlyOnWall) && !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, noWaterCollisionLayers)){
					finishedCrouching = false;
					canCrouch = false;
					crouching = false;
				}
				
			}
			else {
				canCrouch = false;
				crouching = false;
			}
			
			//telling animator to crouch when the player is crouching
			if (animator != null){
				animator.SetBool("crouch", crouching);
			}
			finishedCrouching = true;
			
			//determining if player can uncrouch then jump, attack, etc.
			if (!crouching || !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, noWaterCollisionLayers)){
				canCrouchToAction = true;
			}
			else {
				canCrouchToAction = false;
			}
			
			//if the player is not colliding with a platform (and he is not on the ledge of the platform), set the player's parent to what it was before
			if (allowMovingPlatformSupport && noCollisionWithPlatformTimer >= 5 && emptyObject != null){
				
				//unparenting player from platform
				if (transform.parent == emptyObject.transform){
					if (oldParent != null){
						transform.parent = oldParent.transform;
					}
					else {
						transform.parent = null;
					}
				}
				
				//deleting empty object once the player is no longer a child of it
				if (transform.parent != emptyObject.transform && emptyObject.transform.childCount == 0 && (transform.parent == oldParent || transform.parent == null)){
					//making sure we are no longer attached to the empty object (so that we don't delete ourself)
					transform.parent = null;
					//deleting the emptyObject
					if (transform.parent != emptyObject.transform){
						transform.parent = null;
						Destroy(emptyObject);
					}
					//setting parent back to normal
					if (oldParent != null){
						transform.parent = oldParent.transform;
					}
					else {
						transform.parent = null;
					}
				}
				
			}
			
		}
		
	}
	
	void FixedUpdate () {
		
		//only allow regular movement if the player is not swimming
		if (!inWater){
			ChangeColliderHeightForCrouch();
		}
		noWaterCollisionLayers = collisionLayers & ~(1 << 4);
		
		if (climbing.Length > 0){
			GettingClimbingValues();
		}
		
		//increase the noCollisionTimer (if there is a collision, the noCollisionTimer is later set to 0)
		noCollisionTimer++;
		
		//only allow regular movement if the player is not swimming
		if (!inWater){
			AvoidSlidingWhileClimbing();
			
			MovingPlatformParenting();
			
			Attacks();
			
			GettingRotationDirection();
		}
		
		CheckRBForUseGravity();
		
		//generating splash effect when entering the water
		if (!collidingWithWater && noCollisionWithWaterSplashTimer < 5 && !grounded.currentlyGrounded && (swimming.splashEffect != null && Physics.Linecast(new Vector3(transform.position.x, currentHeadPosition - jumpInHeight2, transform.position.z), transform.position, out hit, collisionLayers))){
			if (ranThroughUpdateCount > 1){
				Instantiate(swimming.splashEffect, new Vector3(transform.localPosition.x, hit.point.y, transform.localPosition.z) + new Vector3(0, 0.05f, 0), swimming.splashEffect.transform.rotation);
			}	
			collidingWithWater = true;
		}
		if (noCollisionWithWaterSplashTimer >= 5){
			collidingWithWater = false;
		}
		else {
			collidingWithWater = true;
		}
		
		//checking if the player has already ran through one update, so he does not splash if he starts out in the water
		ranThroughUpdateCount++;
		
		if (!inWater){
			CheckIfWallJumpIsPossible();
			
			DirectionOfWallJump();
			
			PerformWallJump();
			
			CheckForWallRigidbody();
			
			KeepRigidbodyFromStickingToWall();
			
			//wall and ladder climbing
			if (climbing.Length > 0){
				CheckIfPlayerCanClimb();
				
				ClimbingWall();
				
				JumpOffOfClimb();
				
				ClimbableObjectEdgeDetection();
				
				PullingUpClimbableObject();
				
				AnimatingClimbing();
			}
			
			RotatePlayer();
			
			SettingPlayerSpeed();
			
			DetermineGroundedState();
			
			GettingGroundAndSlopeAngles();
			
			GettingMovementDirection();
		}
		
		LockAxisForSideScrolling();
		
		//only allow regular movement if the player is not swimming
		if (!inWater){
			SlopeSliding();
			
			PreventBouncing();
			
			ApplyGravity();
			
			MovePlayer();
			
			AvoidFallingWhileClimbing();
			
			CrouchAttack();
		}
		
		SwitchingToFirstPersonMode();
		
		Swim();
	
	}
	
	void ChangeColliderHeightForCrouch () {
		
		//changing height of collider while crouching
		//determining if player can uncrouch then jump, attack, etc.
		if (!crouching || !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, noWaterCollisionLayers)){
			canCrouchToAction = true;
		}
		else {
			canCrouchToAction = false;
		}
		//if player has a character controller collider
		if (characterController && characterController.enabled){
			if (crouching){
				if (!colliderAdjusted){
					Vector3 colliderCenter = characterController.center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					characterController.center = colliderCenter;
					characterController.height = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
			}
			else {
				if (colliderAdjusted){
					Vector3 colliderCenter2 = characterController.center;
					colliderCenter2.y = originalColliderY;
					characterController.center = colliderCenter2;
					characterController.height = originalColliderHeight;
				}
				colliderAdjusted = false;
				originalColliderY = characterController.center.y;
				originalColliderHeight = characterController.height;
				feetPosition = characterController.bounds.min;
				headPosition = characterController.bounds.max;
				oldYPos = transform.position.y;
			}
		}
		//if player has a rigidbody
		else if (rigidBody){
			//if player has a capsule collider
			if (GetComponent<CapsuleCollider>()){
				
				if (crouching){
					Vector3 colliderCenter = GetComponent<CapsuleCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<CapsuleCollider>().center = colliderCenter;
					GetComponent<CapsuleCollider>().height = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						Vector3 colliderCenter2 = GetComponent<CapsuleCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<CapsuleCollider>().center = colliderCenter2;
						GetComponent<CapsuleCollider>().height = originalColliderHeight;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<CapsuleCollider>().center.y;
					originalColliderHeight = GetComponent<CapsuleCollider>().height;
					feetPosition = GetComponent<CapsuleCollider>().bounds.min;
					headPosition = GetComponent<CapsuleCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
			//if player has a sphere collider
			else if (GetComponent<SphereCollider>()){
				
				if (crouching){
					Vector3 colliderCenter = GetComponent<SphereCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<SphereCollider>().center = colliderCenter;
					GetComponent<SphereCollider>().radius = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						Vector3 colliderCenter2 = GetComponent<SphereCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<SphereCollider>().center = colliderCenter2;
						GetComponent<SphereCollider>().radius = originalColliderHeight;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<SphereCollider>().center.y;
					originalColliderHeight = GetComponent<SphereCollider>().radius;
					feetPosition = GetComponent<SphereCollider>().bounds.min;
					headPosition = GetComponent<SphereCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
			//if player has a box collider
			else if (GetComponent<BoxCollider>()){
				
				if (crouching){
					//position
					Vector3 colliderCenter = GetComponent<BoxCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<BoxCollider>().center = colliderCenter;
					//height
					Vector3 colliderSize = GetComponent<BoxCollider>().size;
					colliderSize.y = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<BoxCollider>().size = colliderSize;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						//position
						Vector3 colliderCenter2 = GetComponent<BoxCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<BoxCollider>().center = colliderCenter2;
						//height
						Vector3 colliderSize2 = GetComponent<BoxCollider>().size;
						colliderSize2.y = originalColliderHeight;
						GetComponent<BoxCollider>().size = colliderSize2;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<BoxCollider>().center.y;
					originalColliderHeight = GetComponent<BoxCollider>().size.y;
					feetPosition = GetComponent<BoxCollider>().bounds.min;
					headPosition = GetComponent<BoxCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
		}
		//getting the new positions of the top and bottom of the player's collider (his collider from when he is not crouching)
		feetPositionNew = feetPosition.y + (transform.position.y - oldYPos);
		headPositionNew = headPosition.y + (transform.position.y - oldYPos);
		
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
		wallScriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		wallScriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
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
			if (rigidBody && !startedClimbing){
				rigidBody.velocity = Vector3.zero;
			}
			startedClimbing = true;
		}
		else {
			startedClimbing = false;
		}
		
	}
	
	void MovingPlatformParenting () {
		
		//moving with moving platforms
		//increase the noCollisionWithPlatformTimer (if there is a collision with a platform, the noCollisionWithPlatformTimer is later set to 0)
		noCollisionWithPlatformTimer++;
		
		//undoing parent's properties that affect the player's scale 
		if (emptyObject != null){
			emptyObjectParent = emptyObject.transform.parent.gameObject;
			emptyObject.transform.localScale = new Vector3(1/emptyObjectParent.transform.localScale.x, 1/emptyObjectParent.transform.localScale.y, 1/emptyObjectParent.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-emptyObjectParent.transform.localRotation.x, -emptyObjectParent.transform.localRotation.y, -emptyObjectParent.transform.localRotation.z);
		}
		else {
			emptyObjectParent = null;
		}
		
		//getting what the player's parent was before coming in contact with a platform
		if (transform.parent == null){
			oldParent = null;
		}
		else if (emptyObject == null || transform.parent != emptyObject.transform){
			oldParent = transform.parent.gameObject;
		}
		else {
			oldParent = null;
		}
		
	}
	
	void Attacks () {
		
		//if user set attacks, totalAttackNumber equals the number set
		if (attacking.ground.numberAndStrengthOfAttacks.Length > 0){
			if (attackState == 0){
				totalAttackNumber = attacking.ground.numberAndStrengthOfAttacks.Length;
				if (attackedInMidAir){
					currentAttackNumber = totalAttackNumber;
					attackPressesRemembered = totalAttackNumber;
				}
				attackedInMidAir = false;
			}
			else {
				totalAttackNumber = attacking.air.numberAndStrengthOfMidAirAttacks.Length;
			}
		}
		//if user did not set attacks, totalAttackNumber equals 0
		else {
			totalAttackNumber = 0;
		}
		
		//if player is on ground or wall
		if (!crouching){
			if (grounded.currentlyGrounded || currentlyOnWall || currentlyClimbingWall || turnBack || back2){
				if (attackState == 1){
					waitingBetweenAttacksTimerGround = attacking.ground.waitingBeforeAttackingAgain.waitingTime;
				}
				attackState = 0;
			}
			//if player is in the air
			else {
				if (attackState == 0){
					waitingBetweenAttacksTimerGround = attacking.air.waitingBeforeAttackingAgain.waitingTime;
				}
				attackState = 1;
			}
		}
		else {
			attackState = 2;
		}
		
		//attack
		//if the "Attack" button was pressed (and player is not going in to a crouch), attackPressed equals true
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.Joystick1Button8) || !movement.crouching.allowCrouching){
			if (Input.GetButton(attacking.attackInputButton) && !attackButtonPressed){
				attackPressedTimer = 0.0f;
				attackPressed = true;
				canAttack = true;
				attackButtonPressed = true;
			}
			else {
				attackPressedTimer += 0.02f;
				if (!Input.GetButton(attacking.attackInputButton)){
					attackButtonPressed = false;
				}
			}
			
			//wait 0.2 seconds for attackPressed to become false
			//this allows the player to press the "Attack" button slightly early and still attack even if waitingBetweenAttacksTimerGround was not high enough yet
			if (attackPressedTimer > 0.2f){
				attackPressed = false;
			}
			
			if (attackPressed && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2 && !crouchCancelsAttack && canCrouchToAction && !crouching){
				//ground attacks
				if (canAttack && (attackState == 0 && (attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime || !attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime || rememberAttackButtonPressesGround)
				//air attacks
				|| attackState == 1 && (attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime || !attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime || rememberAttackButtonPressesAir) && (!attackedInMidAir || !attacking.air.onlyAllowAttackOnceInMidAir))){
					Attack();
					if (attackState == 1){
						attackedInMidAir = true;
					}
					canAttack = false;
				}
			}
			else if (attackState == 0){
				canAttack = true;
			}
			//increasing the current attack number if the current attack is finished playing, and the attack button was pressed again
			if ((attackState == 0 && rememberAttackButtonPressesGround && (attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime || !attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime)
			|| attackState == 1 && rememberAttackButtonPressesAir && (attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime || !attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime))
			&& currentAttackNumber != attackPressesRemembered){
				currentAttackNumber++;
				if (animator != null && animator.runtimeAnimatorController != null){
					animator.CrossFade("Attack", 0f, -1, 0f);
				}
				waitingBetweenAttacksTimerGround = 0;
				waitingBetweenAttacksTimerAir = 0;
				attackTimer = 0.0f;
			}
			
			//increase the attack timers
			attackTimer += 0.02f;
			waitingBetweenAttacksTimerGround += 0.02f;
			waitingBetweenAttacksTimerAir += 0.02f;
			
			//if the attack timer is greater than the attack time limit, reset current attack number
			if (attackState == 0 && attackTimer > comboTimeLimitGround && comboTimeLimitGround > 0 || attackState == 1 && attackTimer > comboTimeLimitAir && comboTimeLimitAir > 0) {
				currentAttackNumber = totalAttackNumber;
				attackPressesRemembered = totalAttackNumber;
			}
			
			//set animator's float parameter, "attackNumber," to currentAttackNumber
			if (animator != null && animator.runtimeAnimatorController != null){
				animator.SetFloat("attackState", attackState);
				animator.SetFloat("attackNumber", currentAttackNumber);
			}
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
		
		//checking to see if the player (if using a Rigidbody) is using the "Use Gravity" option
		if (rigidBody){
			if (rigidBody.useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2 && !inWater){
				rbUsesGravity = true;
			}
			
			//disabling the Rigidbody's gravity while climbing or swimming
			if (currentlyOnWall || currentlyClimbingWall || turnBack || back2 || inWater){
				canChangeRbGravity = true;
				rigidBody.useGravity = false;
			}
			else if (rbUsesGravity && canChangeRbGravity){
				rigidBody.useGravity = true;
				canChangeRbGravity = false;
			}
			
			if (!rigidBody.useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2 && !inWater){
				rbUsesGravity = false;
			}
		}
		
	}
	
	void CheckForCharacterController () {
		
		//adding a Rigidbody to the player (while swimming) to allow collider rotation
		//checking to see if the player is using a CharacterController
		if (swimming.allowColliderRotationByUsingRigidbody){
			if (characterController && characterController.enabled){
				playerUsesCC = true;
			}
			
			//disabling the player's CharacterController, and adding a Rigidbody+CapsuleCollider while swimming
			if (inWater){
				canChangeCC = true;
				if (characterController && characterController.enabled){
					//adding the Rigidbody
					Rigidbody tempRB = gameObject.AddComponent<Rigidbody>() as Rigidbody;
					tempRB.freezeRotation = true;
					tempRB.useGravity = false;
					//adding a CapsuleCollider
					CapsuleCollider capsCollider = gameObject.AddComponent<CapsuleCollider>() as CapsuleCollider;
					capsCollider.center = characterController.center;
					capsCollider.radius = characterController.radius;
					capsCollider.height = characterController.height;
				}
				//disabling CharacterController
				if (characterController != null){
					characterController.enabled = false;
				}
			}
			else if (playerUsesCC && canChangeCC && rigidBody){
				if (!characterController || !characterController.enabled){
					//removing the Rigidbody
					Destroy(rigidBody);
					//removing the CapsuleCollider
					Destroy(GetComponent<CapsuleCollider>());
				}
				//re-enabling the CharacterController
				GetComponent<CharacterController>().enabled = true;
				//CharacterController has been re-enabled, so we set canChangeCC to false
				canChangeCC = false;
			}
			
			//if player is not in the water, and not using a CharacterController (meaning that he is using a Rigidbody instead), set playerUsesCC to false
			if (!characterController && !inWater){
				playerUsesCC = false;
			}
		}
		
	}
	
	void CheckIfWallJumpIsPossible () {
		
		//checking to see if wall jumping is possible
		//middle
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			middleWallJumpable = true;
		}
		else {
			middleWallJumpable = false;
		}
		//left
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			leftWallJumpable = true;
		}
		else {
			leftWallJumpable = false;
		}
		//right
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			rightWallJumpable = true;
		}
		else {
			rightWallJumpable = false;
		}
		
		if (!headHit && wallJumping.allowWallJumping && !grounded.currentlyGrounded && !wallIsClimbable && !turnBack && !back2 && (!GetComponent<LedgeClimbController>() || GetComponent<LedgeClimbController>() && GetComponent<LedgeClimbController>().canGrabBackOn)){
			if ((middleWallJumpable && leftWallJumpable || middleWallJumpable && rightWallJumpable) && !Physics.Linecast(transform.position, transform.position - transform.up*0.3f - spaceBelowNeededToWallJump2, out hit, noWaterCollisionLayers)){
				if (inMidAirFromWallJump){
					transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
					inMidAirFromWallJump = false;
				}
				wallJumpDirection = Vector3.zero;
				if (!currentlyOnWall){
					wallJumpTimer = 0.0f;
					slideDownSpeed2 = 0;
					if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					
					angleBetweenPlayerAndWall = Vector3.Angle(transform.right, hit.normal);
				}
				if (!sliding){
					turningToWall = true;
					currentlyOnWall = true;
				}
			}
		}
		
	}
	
	void DirectionOfWallJump () {
		
		//getting directions of wall jump
		//new directions (using wallJumpDirection, which is modified by the joystick)
		//forward
		if (Vector3.Dot(transform.forward, wallJumpDirection) + 1 > 0){
			forwardDir = Vector3.Dot(transform.forward, wallJumpDirection) + 1;
		}
		//back
		else if (Mathf.Abs(Vector3.Dot(transform.forward, wallJumpDirection)) != 0){
			forwardDir = 1 / Mathf.Abs(Vector3.Dot(transform.forward, wallJumpDirection));
		}
		//right
		if (Vector3.Dot(transform.right, wallJumpDirection) + 1 > 0){
			rightDir = Vector3.Dot(transform.right, wallJumpDirection) + 1;
		}
		//left
		else if (Mathf.Abs(Vector3.Dot(transform.right, wallJumpDirection)) != 0){
			rightDir = 1 / Mathf.Abs(Vector3.Dot(transform.right, wallJumpDirection));
		}
		
		//original directions (using originalWallJumpDirection, which is not modified by the joystick)
		//original forward
		if (Vector3.Dot(transform.forward, originalWallJumpDirection) + 1 >= 0){
			originalForwardDir = Vector3.Dot(transform.forward, originalWallJumpDirection) + 1;
		}
		//original back
		else if (Mathf.Abs(Vector3.Dot(transform.forward, originalWallJumpDirection)) != 0){
			originalForwardDir = 1 / Mathf.Abs(Vector3.Dot(transform.forward, originalWallJumpDirection));
		}
		//original right
		if (Vector3.Dot(transform.right, originalWallJumpDirection) + 1 >= 0){
			originalRightDir = Vector3.Dot(transform.right, originalWallJumpDirection) + 1;
		}
		//original left
		else if (Mathf.Abs(Vector3.Dot(transform.right, originalWallJumpDirection)) != 0){
			originalRightDir = 1 / Mathf.Abs(Vector3.Dot(transform.right, originalWallJumpDirection));
		}
		
		//checking to make sure the player did not slide off of the wall
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers)){
			wallBackHit = true;
			distFromWall = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
		}
		else {
			wallBackHit = false;
		}
		//if player did slide off wall, set currentlyOnWall to false
		if (wallNormal != Vector3.zero
		&&  Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && !wallBackHit){
			if (animator != null && currentlyOnWall){
				onWallAnimation = false;
			}
			currentlyOnWall = false;
		}
		
		if (wallJumping.allowWallJumping && currentlyOnWall){
			if (animator != null && !onWallAnimation && !sliding){
				
				//if the angle between the player and wall is greater than or equal to 90, turn to the left
				if (angleBetweenPlayerAndWall >= 90){
					animator.SetFloat("wallState", 4);
				}
				//if the angle between the player and wall is less than 90, turn to the right
				else {
					animator.SetFloat("wallState", 3);
				}
				animator.CrossFade("WallJump", 0f, -1, 0f);
				onWallAnimation = true;

			}
			
			wallJumpTimer += 0.02f;
			swimDirection = Vector3.zero;
			swimmingMovementSpeed = 0;
			swimmingMovementTransferred = true;
			moveDirection = Vector3.zero;
			if (rigidBody){
				rigidBody.velocity = Vector3.zero;
			}
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && wallJumping.slideDownWalls && noCollisionTimer < 5){
				wallHitPoint += transform.forward/125;
			}
			transform.position = Vector3.Lerp(transform.position, new Vector3(wallHitPoint.x, transform.position.y, wallHitPoint.z), 10 * Time.deltaTime);
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) >= 0.01f){
				firstDistFromWall = distFromWall;
			}
			//if the wall becomes further away from the player (most likely because it is sloped), push the player backwards towards the wall
			else if (distFromWall - firstDistFromWall > 0.01f) {
				transform.position -= transform.forward/125;
			}
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 1){
				turningToWall = false;
			}
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(wallNormal), (wallJumping.rotationToWallSpeed*2) * Time.deltaTime);
			originalWallJumpDirection = (transform.forward * wallJumping.wallJumpDistance) + (transform.up * wallJumping.wallJumpHeight);
			wallJumpDirection = (transform.forward * wallJumping.wallJumpDistance) + (transform.up * wallJumping.wallJumpHeight);
			
			//sliding down walls
			if (wallJumping.slideDownWalls && wallJumping.slideDownSpeed != 0){
				
				if (slideDownSpeed2 < gravity){
					slideDownSpeed2 += (wallJumping.slideDownSpeed/100) * wallJumpTimer;
				}
				else {
					slideDownSpeed2 = gravity;
				}
				
				if (characterController && characterController.enabled){
					characterController.Move(new Vector3(0, -slideDownSpeed2, 0) * Time.deltaTime);
				}
				else if (rigidBody){
					rigidBody.MovePosition(transform.position + new Vector3(0, -slideDownSpeed2, 0) * Time.deltaTime);
				}
				
				//if the player has finished rotating and is colliding with the wall, push the player slightly forward to avoid becoming stuck
				if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && noCollisionTimer < 5){
					wallHitPoint = transform.position + transform.forward/125;
				}
				
			}
			
			if (jumpPressed && canCrouchToAction){
				inMidAirFromWallJump = true;
				doubleJumpPossible = true;
				WallJump();
				currentlyOnWall = false;
			}
			if (wallJumping.useWallJumpTimeLimit && wallJumpTimer > wallJumping.wallJumpTimeLimit){
				currentlyOnWall = false;
			}
			
		}
		else if (animator != null){
			onWallAnimation = false;
		}
		if (!currentlyOnWall){
			turningToWall = false;
		}
		
		if (animator != null){
			if (grounded.currentlyGrounded && outOfWaterTimer > 0.2f){
				animator.SetBool("grounded", true);
			}
			else {
				animator.SetBool("grounded", false);
			}
		}
		
		if (grounded.currentlyGrounded){
			currentlyOnWall = false;
			if (inMidAirFromWallJump){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				inMidAirFromWallJump = false;
			}
			if (animator != null){
				//return to regular movement
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("WallJump") && animator.GetFloat("wallState") == 0
				|| animator.GetAnimatorTransitionInfo(0).IsName("Movement -> Falling") || animator.GetAnimatorTransitionInfo(0).IsName("Falling -> Movement") || animator.GetAnimatorTransitionInfo(0).IsName("Jump -> Falling") || animator.GetAnimatorTransitionInfo(0).IsName("DoubleJump -> Falling")){
					animator.CrossFade("Movement", 0f, -1, 0f);
				}
				//go to crouching
				if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching") && !animator.GetAnimatorTransitionInfo(0).IsName("Movement -> Crouching") && attackState != 2 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && crouching){
					animator.CrossFade("Crouching", 0f, -1, 0f);
				}
			}
		}
		
	}
	
	void PerformWallJump () {
		
		if (inMidAirFromWallJump){
			//animating jumping on to wall
			if (animator != null && onWallAnimation){
				
				//if the angle between the player and wall is greater than or equal to 90, turn to the left
				if (angleBetweenPlayerAndWall >= 90){
					animator.SetFloat("wallState", 2);
				}
				//if the angle between the player and wall is less than 90, turn to the right
				else {
					animator.SetFloat("wallState", 1);
				}
				animator.CrossFade("WallJump", 0f, -1, 0f);
				onWallAnimation = false;
			}
			
			if (jumpedOffWallForWallJump){
				originalWallJumpDirection = Vector3.Lerp(originalWallJumpDirection, new Vector3(0, originalWallJumpDirection.y, 0), wallJumping.wallJumpDecelerationRate * Time.deltaTime);
				wallJumpDirection = Vector3.Lerp(wallJumpDirection, new Vector3(0, wallJumpDirection.y, 0), wallJumping.wallJumpDecelerationRate * Time.deltaTime);
				if (noCollisionTimer >= 5 && !currentlyOnWall){
					//if neither axis is locked
					if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
						wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * playerCamera.transform.right);
						wallJumpDirection += (Input.GetAxis("Vertical")*(wallJumping.overallMovementSpeed/8) * playerCamera.transform.forward);
					}
					else {
						if (movement.sideScrolling.lockMovementOnXAxis){
							//if the rotation on the axis is not flipped
							if (!movement.sideScrolling.flipAxisRotation){
								if (transform.eulerAngles.y >= 270 && transform.eulerAngles.y <= 360 || transform.eulerAngles.y <= 90 && transform.eulerAngles.y >= -90){
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
							//if the rotation on the axis is flipped
							else {
								if (transform.eulerAngles.y >= 270 || transform.eulerAngles.y <= 90 && transform.eulerAngles.y >= -90){
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
						}
						if (movement.sideScrolling.lockMovementOnZAxis){
							//if the rotation on the axis is not flipped
							if (!movement.sideScrolling.flipAxisRotation){
								if (transform.eulerAngles.y >= 0 && transform.eulerAngles.y <= 180){
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
							//if the rotation on the axis is flipped
							else {
								if (transform.eulerAngles.y >= 0 && transform.eulerAngles.y <= 180){
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
						}
					}
					
					//moving forward
					if (forwardDir > originalForwardDir){
						
						wallJumpDirection -= (transform.forward*(forwardDir/originalForwardDir))*(-(wallJumping.forwardMovementSpeedMultiple*0.1f + 0.9f) + 1);
						
					}
					//moving backwards
					else if (forwardDir < originalForwardDir){
						
						wallJumpDirection += (transform.forward/(forwardDir/originalForwardDir))*(-(wallJumping.backMovementSpeedMultiple*0.1f + 0.9f) + 1);
						
					}
					
					//moving right
					if (rightDir > originalRightDir){
						
						wallJumpDirection -= (transform.right*((rightDir - originalRightDir)*originalRightDir))*(-(wallJumping.sideMovementSpeedMultiple/2 + 0.5f) + 1);
						
					}
					//moving left
					else if (rightDir < originalRightDir){
						
						wallJumpDirection += (transform.right/((rightDir/originalRightDir)*2))*(-(wallJumping.sideMovementSpeedMultiple*0.075f + 0.925f) + 1);
						
					}
					
				}
			}
			
			if (characterController && characterController.enabled){
				characterController.Move(new Vector3(wallJumpDirection.x, 0, wallJumpDirection.z) * Time.deltaTime);
			}
			else if (rigidBody){
				rigidBody.MovePosition(transform.position + new Vector3(wallJumpDirection.x, 0, wallJumpDirection.z) * Time.deltaTime);
			}
			
		}
		
		//if player is no longer on wall or is on the ground, set wallState to 0
		if (animator != null){
			if (!currentlyOnWall && (animator.GetFloat("wallState") == 3 || animator.GetFloat("wallState") == 4) || grounded.currentlyGrounded){
				animator.SetFloat("wallState", 0);
			}
		}
		
	}
	
	void CheckForWallRigidbody () {
		
		Vector3 scaleForward = transform.forward*transform.localScale.z;
		Vector3 scaleRight = transform.right*transform.localScale.x;
		Vector3 scaleUp = transform.up*transform.localScale.y;
		
		//getting collider's actual width and length
		if (characterController && characterController.enabled){
			//width
			colliderLeft = characterController.bounds.center - (characterController.radius*scaleRight);
			colliderRight = characterController.bounds.center + (characterController.radius*scaleRight);
			//length
			colliderBack = characterController.bounds.center - (characterController.radius*scaleForward);
			colliderFront = characterController.bounds.center + (characterController.radius*scaleForward);
		}
		else if (rigidBody){
			if (GetComponent<CapsuleCollider>()){
				//width
				colliderLeft = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*scaleRight);
				colliderRight = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*scaleRight);
				colliderLeftLonger = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*1.5f*scaleRight);
				colliderRightLonger = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*1.5f*scaleRight);
				colliderLeftLonger2 = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*2*scaleRight);
				colliderRightLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*2*scaleRight);
				//length
				colliderBack = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*scaleForward);
				colliderFront = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*scaleForward);
				colliderBackLonger = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*1.5f*scaleForward);
				colliderFrontLonger = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*1.5f*scaleForward);
				colliderFrontLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*2*scaleForward);
				//height
				colliderBottom = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().height*0.5f*scaleUp);
				colliderTop = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().height*0.5f*scaleUp);
			}
			else if (GetComponent<SphereCollider>()){
				//width
				colliderLeft = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*scaleRight);
				colliderRight = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*scaleRight);
				colliderLeftLonger = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*1.5f*scaleRight);
				colliderRightLonger = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*1.5f*scaleRight);
				colliderLeftLonger2 = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*2*scaleRight);
				colliderRightLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*2*scaleRight);
				//length
				colliderBack = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*scaleForward);
				colliderFront = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*scaleForward);
				colliderBackLonger = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*1.5f*scaleForward);
				colliderFrontLonger = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*1.5f*scaleForward);
				colliderFrontLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*2*scaleForward);
				//height
				colliderBottom = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*scaleUp);
				colliderTop = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*scaleUp);
			}
			else if (GetComponent<BoxCollider>()){
				//width
				colliderLeft = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.x*0.5f*scaleRight);
				colliderRight = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.x*0.5f*scaleRight);
				colliderLeftLonger = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.x*0.5f*1.5f*scaleRight);
				colliderRightLonger = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.x*0.5f*1.5f*scaleRight);
				colliderLeftLonger2 = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.x*0.5f*2*scaleRight);
				colliderRightLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.x*0.5f*2*scaleRight);
				//length
				colliderBack = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.z*0.5f*scaleForward);
				colliderFront = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.z*0.5f*scaleForward);
				colliderBackLonger = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.z*0.5f*1.5f*scaleForward);
				colliderFrontLonger = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.z*0.5f*1.5f*scaleForward);
				colliderFrontLonger2 = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.z*0.5f*2*scaleForward);
				//height
				colliderBottom = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.y*0.5f*scaleUp);
				colliderTop = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.y*0.5f*scaleUp);
			}
		}
		
		
		if (GetComponent<Rigidbody>()){
			
			//preventing player from going through walls
			if (Physics.Linecast((colliderBack*8.5f + colliderFront*1.5f)/10, (colliderFront*8.5f + colliderBack*1.5f)/10, out hit, noWaterCollisionLayers)
			|| Physics.Linecast((colliderLeft*8.5f + colliderRight*1.5f)/10, (colliderRight*8.5f + colliderLeft*1.5f)/10, out hit, noWaterCollisionLayers)
			//
			//back right/left to front right/left
			|| (Physics.Linecast((colliderBackLonger + colliderRightLonger)/2, (colliderFrontLonger + colliderLeftLonger)/2, out hit, noWaterCollisionLayers) && Physics.Linecast((colliderBackLonger + colliderLeftLonger)/2, (colliderFrontLonger + colliderRightLonger)/2, out hit, noWaterCollisionLayers)
			&& !Physics.Linecast(colliderBack, colliderFront, out hit, noWaterCollisionLayers)
			//not left/right to right/left
			&& !Physics.Linecast(GetComponent<Collider>().bounds.center, colliderRightLonger, out hit, noWaterCollisionLayers) && !Physics.Linecast(GetComponent<Collider>().bounds.center, colliderLeftLonger, out hit, noWaterCollisionLayers)) ){
				
				pushingThroughWall = true;
				
			}
			else {
				pushingThroughWall = false;
			}
			
			//preventing player from pushing through corner
			//right and left
			if (Physics.Linecast(GetComponent<Collider>().bounds.center, colliderRightLonger, out hit, noWaterCollisionLayers) && Physics.Linecast(GetComponent<Collider>().bounds.center, colliderLeftLonger, out hit, noWaterCollisionLayers)
			//front left and front right, but not front center
			|| Physics.Linecast((colliderBackLonger + colliderRightLonger)/2, (colliderFrontLonger + colliderLeftLonger)/2, out hit, noWaterCollisionLayers) && Physics.Linecast((colliderBackLonger + colliderLeftLonger)/2, (colliderFrontLonger + colliderRightLonger)/2, out hit, noWaterCollisionLayers)
			//right and front left
			|| Physics.Linecast(GetComponent<Collider>().bounds.center, colliderRightLonger, out hit, noWaterCollisionLayers) && Physics.Linecast(GetComponent<Collider>().bounds.center, (colliderFrontLonger2 + colliderLeftLonger2)/2, out hit, noWaterCollisionLayers)
			//left and front right
			|| Physics.Linecast(GetComponent<Collider>().bounds.center, colliderLeftLonger, out hit, noWaterCollisionLayers) && Physics.Linecast(GetComponent<Collider>().bounds.center, (colliderFrontLonger2 + colliderRightLonger2)/2, out hit, noWaterCollisionLayers)){
				
				inCorner = true;
				
			}
			else {
				inCorner = false;
			}
			
		}
		else {
			pushingThroughWall = false;
			inCorner = false;
		}
		
		
		//checking to see if the player's collider is going through the wall
		//front to back
		if (!Physics.Linecast((colliderBack*9.5f + colliderFront*0.5f)/10, (colliderFront*9.5f + colliderBack*0.5f)/10, out hit, noWaterCollisionLayers)
		//back right to front left
		&& !Physics.Linecast((colliderBackLonger + colliderRightLonger)/2, (colliderFrontLonger + colliderLeftLonger)/2, out hit, noWaterCollisionLayers)
		//back left to front right
		&& !Physics.Linecast((colliderBackLonger + colliderLeftLonger)/2, (colliderFrontLonger + colliderRightLonger)/2, out hit, noWaterCollisionLayers)
		//left to right
		&& !Physics.Linecast((colliderLeft*9.5f + colliderRight*0.5f)/10, (colliderRight*9.5f + colliderLeft*0.5f)/10, out hit, noWaterCollisionLayers)
		//right to left
		&& !Physics.Linecast((colliderRight*9.5f + colliderLeft*0.5f)/10, (colliderLeft*9.5f + colliderRight*0.5f)/10, out hit, noWaterCollisionLayers)
		//if ground is directly underneath player
		|| Physics.Linecast(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		|| !GetComponent<Rigidbody>()){
			colliderInWall = false;
		}
		else {
			colliderInWall = true;
		}
		
		
		//checking to see if player is touching the wall
		//middle
		if ((Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle)
		&&  noCollisionTimer < 5 && hit.transform.tag != climbableTag2){
			middleTouchingWall = true;
		}
		else {
			middleTouchingWall = false;
		}
		//left
		if ((Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle)
		&&  noCollisionTimer < 5 && hit.transform.tag != climbableTag2){
			leftTouchingWall = true;
		}
		else {
			leftTouchingWall = false;
		}
		//right
		if ((Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle)
		&&  noCollisionTimer < 5 && hit.transform.tag != climbableTag2){
			rightTouchingWall = true;
		}
		else {
			rightTouchingWall = false;
		}
		
	}
	
	void KeepRigidbodyFromStickingToWall () {
		
		//if player is touching the wall, get the wall's normal
		if (GetComponent<Rigidbody>() && !wallJumping.allowWallJumping && !headHit && !wallIsClimbable && !turnBack && !back2
		&& (leftTouchingWall || middleTouchingWall || rightTouchingWall)){
			
			if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
				wallNormal = hit.normal;
			}
			touchingWall = true;
			
		}
		else {
			touchingWall = false;
		}
		
	}
	
	void CheckIfPlayerCanClimb () {
		
		//enabling and disabling scripts (and warning the user if any script names they entered do not exist on the player) when climbing on wall
		WallScriptEnablingDisabling();
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//detecting whether the player can climb or not
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
			
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
					animator.CrossFade("Movement", 0f, -1, 0f);
				}
				currentlyClimbingWall = false;
			}
			
			//if we are not climbing the wall yet
			if (!currentlyClimbingWall){
				finishedRotatingToWall = false;
			}
		}
		
		//checking if there is a wall in front of the player
		if ((Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, noWaterCollisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, out hit, noWaterCollisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, out hit, noWaterCollisionLayers))){
			
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
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
			
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
					swimDirection = Vector3.zero;
					swimmingMovementSpeed = 0;
					swimmingMovementTransferred = true;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					inMidAirFromJump = false;
					inMidAirFromWallJump = false;
					currentJumpNumber = totalJumpNumber;
					moveSpeed = 0;
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
				swimDirection = Vector3.zero;
				swimmingMovementSpeed = 0;
				swimmingMovementTransferred = true;
				moveDirection = Vector3.zero;
				moveSpeed = 0;
			}
			
			//rotating player to face the wall
			WallClimbingRotation();
			
			//checking to see if the player is stuck on a wall
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
				if (climbHorizontally2 && climbVertically2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
					
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
				else if (climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
					
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
			if (characterController && characterController.enabled){
				characterController.Move(climbDirection * Time.deltaTime);
			}
			else if (rigidBody){
				rigidBody.MovePosition(transform.position + climbDirection * Time.deltaTime);
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
		
		if (directionVector.magnitude != 0 || currentlyOnWall || inMidAirFromWallJump){
		//setting player's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
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
					horizontalValue = -1;
					swimHorizontalValue = -1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = 180.001f;
					}
					else {
						bodyRotation = 179.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = 1;
					swimHorizontalValue = 1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = -0.001f;
					}
					else {
						bodyRotation = 0.001f;
					}
				}
			}
			//getting rotation on x-axis (z-axis is locked)
			else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 0){
					horizontalValue = 1;
					swimHorizontalValue = 1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = 90.001f;
					}
					else {
						bodyRotation = 89.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = -1;
					swimHorizontalValue = -1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = -90.001f;
					}
					else {
						bodyRotation = -89.999f;
					}
				}
			}
		}
		lastAxis = Input.GetAxisRaw("Horizontal");
		
	}
	
	void JumpOffOfClimb () {
		
		//jumping off of ladder/wall
		if ((currentlyClimbingWall || turnBack || back2) && Input.GetButtonDown("Jump")){
			moveDirection = Vector3.zero;
			swimDirection = Vector3.zero;
			swimmingMovementSpeed = 0;
			swimmingMovementTransferred = true;
			jumpedOffClimbableObjectTimer = 0;
			if (turnBack || back2){
				transform.rotation = backRotation;
			}
			jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
			currentJumpNumber = totalJumpNumber;
			moveSpeed = 0;
			turnBack = false;
			back2 = false;
			grounded.currentlyGrounded = true;
			jumpPressed = false;
			jumpPossible = true;
			doubleJumpPossible = true;
			Jump();
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
				if (Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, noWaterCollisionLayers)) {
					hitPoint = hit.point;
				}
				pullUpLocationAcquired = false;
			}
			
			if (movingToPullUpLocation){
				movementVector = (new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z) - transform.position).normalized * pullUpSpeed;
				if (characterController && characterController.enabled){
					characterController.Move(movementVector * Time.deltaTime);
				}
				else if (rigidBody){
					rigidBody.MovePosition(transform.position + movementVector * Time.deltaTime);
				}
			}
			if (Vector3.Distance(transform.position, new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z)) <= 0.2f || pullingUpTimer > Mathf.Abs((pullUpSpeed/4)) && onWallLastUpdate){
				pullUpLocationAcquired = false;
				movingToPullUpLocation = false;
			}
			
			if (!movingToPullUpLocation){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				movementVector = ((hitPoint + transform.forward/10) - transform.position).normalized * pullUpSpeed;
				if (characterController && characterController.enabled){
					characterController.Move(movementVector * Time.deltaTime);
				}
				else if (rigidBody){
					rigidBody.MovePosition(transform.position + movementVector * Time.deltaTime);
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
					if ((grounded.currentlyGrounded && animator.GetFloat("climbState") != 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")) && !inMidAirFromJump){
						animator.CrossFade("Movement", 0f, -1, 0f);
					}
					animator.SetFloat("climbState", 0);
					
				}
				
				//animating the player's climbing motions while he is on a wall
				if (currentlyClimbingWall){
					if (climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
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
					if (Input.GetAxis("Horizontal") != 0 && climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
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
	
	void RotatePlayer () {
		
		//if joystick/arrow keys are being pushed
		if ((directionVector.magnitude > 0 || (movement.sideScrolling.lockMovementOnXAxis || movement.sideScrolling.lockMovementOnZAxis)) && !currentlyClimbingWall && !turnBack && !back2){
			
			float myAngle = Mathf.Atan2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) * Mathf.Rad2Deg;
			
			//normal rotation from side to side (detected by which way joystick was pushed last)
			if (!inMidAirFromWallJump && !currentlyOnWall){
				//getting which direction we are turning to (only used if axis is locked)
				if (Input.GetAxis("Horizontal") > 0){
					horizontalValue = 1;
					swimHorizontalValue = 1;
				}
				else if (Input.GetAxis("Horizontal") < 0){
					horizontalValue = -1;
					swimHorizontalValue = -1;
				}
			}
			//normal rotation from side to side (detected by which way the player is closest to facing)
			else {
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
						horizontalValue = -1;
						swimHorizontalValue = -1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = 180.001f;
						}
						else {
							bodyRotation = 179.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						horizontalValue = 1;
						swimHorizontalValue = 1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = -0.001f;
						}
						else {
							bodyRotation = 0.001f;
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 0){
						horizontalValue = 1;
						swimHorizontalValue = 1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = 90.001f;
						}
						else {
							bodyRotation = 89.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						horizontalValue = -1;
						swimHorizontalValue = -1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = -90.001f;
						}
						else {
							bodyRotation = -89.999f;
						}
					}
				}
			}
			
			if (!currentlyOnWall){
				//getting rotation on z-axis (x-axis is locked)
				if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					//moving to right
					if (horizontalValue == 1){
						//if the rotation on the axis is not flipped
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 180.001f;
							}
							else {
								bodyRotation = 179.999f;
							}
						}
						//if the rotation on the axis is flipped
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 180.001f + 180;
							}
							else {
								bodyRotation = 179.999f + 180;
							}
						}
					}
					//moving to left
					else {
						//if the rotation on the axis is not flipped
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -0.001f;
							}
							else {
								bodyRotation = 0.001f;
							}
						}
						//if the rotation on the axis is flipped
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -0.001f + 180;
							}
							else {
								bodyRotation = 0.001f + 180;
							}
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
					//moving to right
					if (horizontalValue == 1){
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 90.001f;
							}
							else {
								bodyRotation = 89.999f;
							}
						}
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 90.001f + 180;
							}
							else {
								bodyRotation = 89.999f + 180;
							}
						}
					}
					//moving to left
					else {
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -90.001f;
							}
							else {
								bodyRotation = -89.999f;
							}
						}
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -90.001f + 180;
							}
							else {
								bodyRotation = -89.999f + 180;
							}
						}
					}
				}
				//getting rotation if neither or both axis are locked)
				else {
					bodyRotation = myAngle + playerCamera.eulerAngles.y;
				}
				
				//setting player's rotation
				//if in mid-air from wall jumping, rotate using the rotationSpeedMultiple
				if (inMidAirFromWallJump){
					//rotating in air when not side-scrolling
					if (!movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), (rotationSpeed2*wallJumping.rotationSpeedMultiple) * Time.deltaTime);
					}
					//rotating (from side to side) in air when side-scrolling
					else {
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), rotationSpeed2 * Time.deltaTime);
					}
				}
				//if we are not allowed to move straight backwards or we are simply not going that direction, continue to rotate
				else if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), rotationSpeed2 * Time.deltaTime);
					}
				}
				
				//increase the player's acceleration until accelerationRate has reached 1
				if (accelerationRate < 1){
					accelerationRate += 0.01f*acceleration2;
				}
				else {
					accelerationRate = 1;
				}
				
			}
			
		}
		else {
			accelerationRate = 0;
		}
		//if the player only rotates with the camera
		if (enabledLastUpdate){
			if (movement.firstPerson.onlyRotateWithCamera && (!currentlyClimbingWall && !turnBack && !back2 && !currentlyOnWall) && inFirstPersonMode){
				transform.eulerAngles = new Vector3(0, playerCamera.transform.eulerAngles.y, 0);
			}
		}
		enabledLastUpdate = true;
		
	}
	
	void SettingPlayerSpeed () {
		
		//getting the running speed of the player (if we are using a run button)
		if (movement.running.useRunningButton){
			if (Input.GetButton(movement.running.runInputButton) || Input.GetAxis(movement.running.runInputButton) != 0){
				runSpeedMultiplier = movement.running.runSpeedMultiple;
			}
			else {
				runSpeedMultiplier = 1;
			}
		}
		else {
			runSpeedMultiplier = 1;
		}
		
		//setting the speed of the player
		h = Mathf.Lerp(h, (Mathf.Abs(Input.GetAxisRaw ("Horizontal")) - Mathf.Abs(Input.GetAxisRaw ("Vertical")) + 1)/2, 8 * Time.deltaTime);
		v = Mathf.Lerp(v, (Mathf.Abs(Input.GetAxisRaw ("Vertical")) - Mathf.Abs(Input.GetAxisRaw ("Horizontal")) + 1)/2, 8 * Time.deltaTime);
		if (directionVector.magnitude != 0){
			if (Input.GetAxis("Vertical") >= 0){
				
				//if not side-scrolling (neither axis is locked)
				if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (h*sideSpeed2 + v*forwardSpeed2)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((h*sideSpeed2 + v*forwardSpeed2)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				//if side-scrolling (either axis is locked)
				else {
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
			
			}
			else {
				
				//if not side-scrolling (neither axis is locked)
				if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (h*sideSpeed2 + v*backSpeed2)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((h*sideSpeed2 + v*backSpeed2)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				//if side-scrolling (either axis is locked)
				else {
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				
			}
			
		}
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.SetFloat ("speed", moveSpeed);
		}
		
		decelerationRate += deceleration/10;
		airSpeed = moveSpeed * midAirMovementSpeedMultiple2;
		
		//applying friction to the player's movement
		if (movement.movementFriction > 0){
			moveSpeedAndFriction = Mathf.Lerp(moveSpeedAndFriction, moveSpeed, (24/movement.movementFriction) * Time.deltaTime);
		}
		else {
			moveSpeedAndFriction = moveSpeed;
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
		if (Physics.Linecast(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)){
			
			//if player is not on water
			if (!colliderInWall && (Physics.Linecast(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, out hit, noWaterCollisionLayers) || sliding || !rigidBody)){
				if (!angHit){
					raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
				}
				grounded.currentlyGrounded = true;
			}
			else {
				grounded.currentlyGrounded = false;
			}
			
		}
		else {
			grounded.currentlyGrounded = false;
		}
		
	}
	
	void GettingGroundAndSlopeAngles () {

		//determining the slope of the surface you are currently standing on
		float myAng2 = 0.0f;
		if (Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers)){
			angHit = true;
			myAng2 = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
		}
		else {
			angHit = false;
		}
		
		//raycasting to determine whether sliding is possible or not
		RaycastHit altHit = new RaycastHit();
		if (Physics.Raycast(pos, Vector3.down, out hit, maxGroundedDistance2, noWaterCollisionLayers)){
			slidePossible = true;
			if (Physics.Raycast(pos + transform.forward/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.forward/10 + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.forward/10 - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10 + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10 - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, noWaterCollisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)){
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f;
			}
			else {
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
			}
		}
		else if (Physics.Raycast(contactPoint + Vector3.up, Vector3.down, out hit, 5f, noWaterCollisionLayers) && collisionSlopeAngle < 90 && collisionSlopeAngle > slopeLimit){
			if (angHit && myAng2 > slopeLimit || !angHit){
				slidePossible = true;
				if (angHit){
					raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
				}
			}

		}
		else if (Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers)){
			slidePossible = true;
			if (angHit){
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
			}
		}
		else {
			slidePossible = false;
		}
		
		
		//checking to see if player is stuck between two slopes
		CheckIfInBetweenSlopes();
		
		
		//checking to see if player is facing uphill or downhill on a slope
		if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, noWaterCollisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, noWaterCollisionLayers)){
			if (frontHit.point.y >= backHit.point.y){
				uphill = true;
			}
			else{
				uphill = false;
			}
		}
		else if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, noWaterCollisionLayers)){
			uphill = true;
		}
		else {
			uphill = false;
		}
		
	}
	
	void GettingMovementDirection () {
		
		if (grounded.currentlyGrounded && (noCollisionTimer < 5 || Physics.Raycast(pos, Vector3.down, maxGroundedDistance2, noWaterCollisionLayers))) {
			//since we are grounded, recalculate move direction directly from axes
			if (!jumpPerformed){
				moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			}
			else {
				moveDirection = new Vector3(Input.GetAxis("Horizontal"), moveDirection.y, Input.GetAxis("Vertical"));
			}
			
			Vector3 desiredPosition = (Quaternion.Euler(transform.eulerAngles.x, bodyRotation, transform.eulerAngles.z) * Vector3.forward);
			if (directionVector.magnitude != 0){
				//if we are not allowed to move straight backwards or we are simply not going that direction, do not move straight backwards
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(desiredPosition.x, moveDirection.y, desiredPosition.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-desiredPosition.x, moveDirection.y, -desiredPosition.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				decelerationRate = 0;
			}
			
			
			if (directionVector.magnitude == 0 ){
				
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(desiredPosition.x, moveDirection.y, desiredPosition.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-desiredPosition.x, moveDirection.y, -desiredPosition.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				
				if(moveSpeed > 0){
					moveSpeed -= decelerationRate * moveSpeed;
				}
				if (moveSpeed <= 0){
					moveSpeed = 0;
				}
			}
			moveDirection.x *= moveSpeedAndFriction;
			moveDirection.z *= moveSpeedAndFriction;
			
			rotationSpeed2 = movement.rotationSpeed;
		}
		else {
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), moveDirection.y, Input.GetAxis("Vertical"));
			if (directionVector.magnitude != 0) {
				//if we are not allowed to move straight backwards or we are simply not going that direction, do not move straight backwards
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(transform.forward.x, moveDirection.y, transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-transform.forward.x, moveDirection.y, -transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
			}
			else {
				moveSpeed = 0;
			}
			moveDirection.x *= airSpeed;
			moveDirection.z *= airSpeed;
			
			rotationSpeed2 = movement.rotationSpeed * movement.midAirRotationSpeedMultiple;
		}
		
	}
	
	void LockAxisForSideScrolling () {
		
		//locking axis for side-scrolling
		if (movement.sideScrolling.lockMovementOnXAxis){
			moveDirection.x = 0;
			if (characterController && characterController.enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp && !inMidAirFromWallJump){
				transform.position = new Vector3(movement.sideScrolling.xValue, transform.position.y, transform.position.z);
			}
		}
		if (movement.sideScrolling.lockMovementOnZAxis){
			moveDirection.z = 0;
			if (characterController && characterController.enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp && !inMidAirFromWallJump){
				transform.position = new Vector3(transform.position.x, transform.position.y, movement.sideScrolling.zValue);
			}
		}
		
		if (rigidBody && currentlyClimbingWall){
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			slidingVector = Vector3.zero;
			slideMovement = Vector3.zero;
		}
		
		if (noCollisionTimer >= 5 && !grounded.currentlyGrounded || inMidAirFromJump || jumpPerformed || !sliding){
			slidingVector = Vector3.zero;
		}
		if (!angHit && noCollisionTimer < 5 && slidingVector != Vector3.zero && moveDirection.y <= -gravity){
			moveDirection.y = -gravity;
		}
		
	}
	
	void SlopeSliding () {
		
		//sliding
		if (raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && !inMidAirFromJump && slidePossible && !inBetweenSlidableSurfaces){
			if (!inBetweenSlidableSurfaces && (uphill && !jumping.allowJumpWhenSlidingFacingUphill || !uphill && !jumping.allowJumpWhenSlidingFacingDownhill)){
				jumpPossible = false;
			}
			else {
				jumpPossible = true;
			}

			if (noCollisionTimer < 5 || grounded.currentlyGrounded){
				if (!sliding){
					slideSpeed = 1.0f;
				}
				sliding = true;
				if (jumping.doNotIncreaseJumpNumberWhenSliding){
					currentJumpNumber = 0;
				}
				slideMovement = Vector3.Slerp(slideMovement, new Vector3(slidingVector.x, -slidingVector.y, slidingVector.z), 6 * Time.deltaTime);
				moveDirection.x += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).x;
				moveDirection.z += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).z;
				if (noCollisionTimer < 2 || !jumpPossible){
					
					if (characterController && characterController.enabled){
						moveDirection.y += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).y;
					}
					else if (rigidBody){
						
						if (yVel < -0.01f * gravity && Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit){
							if (transform.position.y - hit.point.y < 0.2f){
								transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
							}
							else {
								moveDirection.y += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).y;
							}
						}
						
					}
					
				}
				slideSpeed += -slideMovement.y * Time.deltaTime * gravity;
				if (noCollisionTimer > 2 && !grounded.currentlyGrounded || moveDirection.y <= -gravity){
					if (!inMidAirFromJump && characterController && characterController.enabled){
						moveDirection.y = -gravity;
					}
				}
			}
			else if (!inMidAirFromJump && characterController && characterController.enabled){
				if (sliding){
					moveDirection.y = -gravity;
				}
				sliding = false;
			}
			else {
				sliding = false;
			}
			
		}
		else {
			jumpPossible = true;
			sliding = false;
		}
		
		//applying friction after sliding
		if (!sliding){
			if (movement.slideFriction > 0){
				if (!inMidAirFromJump){
					slideMovement = Vector3.Slerp(slideMovement, Vector3.zero, (24/movement.slideFriction) * Time.deltaTime);
				}
				else {
					slideMovement = Vector3.Slerp(slideMovement, Vector3.zero, (24/(movement.slideFriction*1.5f)) * Time.deltaTime);
				}
				if (slideMovement != Vector3.zero){
					if (rigidBody && !jumpPerformed && !inMidAirFromJump && grounded.currentlyGrounded && noCollisionTimer < 5 && Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers)){
						transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
					}
					moveDirection.x += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).x;
					moveDirection.z += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).z;
					slideSpeed += -slideMovement.y * Time.deltaTime * gravity;
				}
			}
			else {
				slideSpeed = 1.0f;
			}
		}
		
	}
	
	void PreventBouncing () {
		
		//if we are grounded, and are no longer jumping, set jumpPerformed to false
		if (grounded.currentlyGrounded || angHit){
			if (jumpPerformed && noCollisionTimer < 5 && !inMidAirFromJump){
				jumpPerformed = false;
			}
		}
		
		//keeping player from bouncing down slopes
		if (Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers) && !rigidBody || Physics.Raycast(pos + transform.forward/10, Vector3.down, out hit, 1f, noWaterCollisionLayers) && rigidBody){
			if (grounded.currentlyGrounded && !jumpPerformed){
				
				if (characterController && characterController.enabled && (raycastSlopeAngle > 1 || raycastSlopeAngle < -1)){
					//applying a downward force to keep the player from bouncing down slopes
					moveDirection.y -= hit.point.y;
					if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, noWaterCollisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, noWaterCollisionLayers)){
						
						if (frontHit.point.y < backHit.point.y){
							moveDirection.y -= hit.normal.y;
						}
						
					}
					
				}
				else if (rigidBody && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
					//applying a downward force to keep the player from bouncing down slopes
					if (grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - rigidBody.velocity.y) < transform.position.y && moveDirection.y <= rigidBody.velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
						moveDirection.y -= hit.point.y;
						if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, noWaterCollisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, noWaterCollisionLayers)){
							
							if (frontHit.point.y < backHit.point.y){
								moveDirection.y -= hit.normal.y;
							}
							
						}
					}
					
				}

			}
		}
		
	}
	
	void ApplyGravity () {
		
		//apply gravity
		if (!jumpPressed || !grounded.currentlyGrounded){
			if (!currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				moveDirection.y -= gravity * Time.deltaTime;
			}
		}
		
		//telling the player to not fall faster than the maximum falling speed
		if (characterController && characterController.enabled){
			if (moveDirection.y <= -maxFallingSpeed2){
				moveDirection.y = -maxFallingSpeed2;
			}
		}
		else if (rigidBody) {
			if (rigidBody.velocity.y <= -maxFallingSpeed2){
				moveDirection.y = -maxFallingSpeed2;
			}
		}

		//if head is blocked/hits the ceiling, stop going up
		if (headHit){
			moveDirection.y = 0;
		}
		
	}
	
	void MovePlayer () {
		
		if (!currentlyOnWall && !currentlyClimbingWall){
			//if player is using a CharacterController
			if (characterController && characterController.enabled){
				
				//applying a downward force to keep the player falling instead of slowly floating to the ground
				if (grounded.currentlyGrounded && moveDirection.y >= 0 && (noCollisionTimer > 5 || uphill) && !sliding && !jumpPerformed){
					if (Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
						moveDirection.y -= -transform.position.y + hit.normal.y;
					}
				}

				// move the player if grounded
				//checking for grounded (using the position variable pos)
				if (noCollisionTimer < 5 && grounded.currentlyGrounded
				&&  Physics.Linecast(pos + maxGroundedHeight2, pos + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				//checking for grounded in a different way (using transform.position, instead of the position variable pos), and checking for sliding
				||  (raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && slidePossible || inBetweenSlidableSurfaces)
				&&  (Physics.Linecast(transform.position + maxGroundedHeight2, transform.position + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
				||	Physics.Linecast(transform.position - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
				||	Physics.Linecast(transform.position + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
				||	Physics.Linecast(transform.position - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
				||	Physics.Linecast(transform.position + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, noWaterCollisionLayers)
				||  noCollisionTimer < 5)){
					
					//moving player, and avoiding bouncing if on a sloped surface
					if ((grounded.currentlyGrounded && !inMidAirFromJump && !inMidAirFromWallJump || raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && slidePossible || inBetweenSlidableSurfaces)
					&& (moveDirection.y > transform.position.y)){
						characterController.Move((moveDirection + new Vector3(0, transform.position.y, 0)) * Time.deltaTime);
					}
					//moving player
					else {
						characterController.Move(moveDirection * Time.deltaTime);
					}
					
				}
				// move the player if not grounded
				else {
					//moving player
					characterController.Move(moveDirection * Time.deltaTime);
				}
			
			}
			//if player is using a Rigidbody
			else if (rigidBody){
			
				//applying a downward force to keep the player falling instead of slowly floating to the ground
				if (grounded.currentlyGrounded && Mathf.Abs(rigidBody.velocity.y) > 1f && moveDirection.y >= 0 && (noCollisionTimer > 5 || uphill) && !sliding && !jumpPerformed){
					if (Physics.Raycast(pos, Vector3.down, out hit, 1f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
						if (grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - rigidBody.velocity.y) < transform.position.y && moveDirection.y <= rigidBody.velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
							moveDirection.y -= -transform.position.y + hit.normal.y;
						}
					}
				}
			
				// move the player if grounded
				//checking for grounded (using the position variable pos)
				if ((grounded.currentlyGrounded && !jumpPressed && !jumpPerformed) && !sliding && noCollisionTimer < 5
				&&  Physics.Linecast(pos + maxGroundedHeight2, pos + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)
				&&	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, noWaterCollisionLayers)){
					
					//moving player, and avoiding bouncing if on a sloped surface
					if (raycastSlopeAngle <= slopeLimit && grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - rigidBody.velocity.y) < transform.position.y && moveDirection.y <= rigidBody.velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
						if ((!touchingWall && !collidingWithWall) || (pushingThroughWall || inCorner || colliderInWall)){
							rigidBody.velocity = moveDirection + new Vector3(0, transform.position.y, 0);
						}
						else {
							//keeping player from moving in unwanted directions
							rigidBody.velocity = new Vector3(0, moveDirection.y + transform.position.y, 0);
							//moving player
							rigidBody.MovePosition(transform.position + new Vector3(moveDirection.x, 0, moveDirection.z) * Time.deltaTime);
						}
					}
					//moving player
					else {
						rigidBody.velocity = moveDirection;
					}
					
				}
				// move the player if not grounded
				else {
					
					//moving player
					if (touchingWall && pushingThroughWall || raycastSlopeAngle > slopeLimit || sliding){
						rigidBody.velocity = moveDirection;
					}
					else {
						if (!inCorner){
							//keeping player from moving in unwanted directions
							rigidBody.velocity = Vector3.zero;
							//moving player
							rigidBody.MovePosition(transform.position + moveDirection * Time.deltaTime);
						}
						else {
							//keeping player from moving in unwanted directions
							rigidBody.velocity = new Vector3(moveDirection.x, 0, moveDirection.z);
							//moving player
							rigidBody.MovePosition(transform.position + new Vector3(0, moveDirection.y, 0) * Time.deltaTime);
						}
					}
					
					//switching value back and forth when in corner
					if (inCorner){
						if (updateFrame == 1){
							updateFrame = 2;
						}
						else if (updateFrame == 2){
							updateFrame = 1;
							rigidBody.velocity = moveDirection;
						}
					}
					else {
						updateFrame = 2;
					}
					
				}
				
			}
			
			//hard sticking to the ground
			if (movement.hardStickToGround){
				if (Physics.Linecast(transform.position, transform.position - transform.up/10, out hit, noWaterCollisionLayers) && !jumpPressed && !jumpPerformed && !inMidAirFromJump && !inMidAirFromWallJump){
					transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
				}
			}
		}
		
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
	
	void SwitchingToFirstPersonMode () {
		
		//getting first person settings from camera (if possible)
		if (movement.firstPerson.useCameraControllerSettingsIfPossible && playerCamera.GetComponent<CameraController>()){
			movement.firstPerson.alwaysUseFirstPerson = playerCamera.GetComponent<CameraController>().firstPerson.alwaysUseFirstPerson;
			movement.firstPerson.switchToFirstPersonIfInputButtonPressed = playerCamera.GetComponent<CameraController>().firstPerson.switchToFirstPersonIfInputButtonPressed;
			movement.firstPerson.firstPersonInputButton = playerCamera.GetComponent<CameraController>().firstPerson.firstPersonInputButton;
			movement.firstPerson.startOffInFirstPersonModeForSwitching = playerCamera.GetComponent<CameraController>().firstPerson.startOffInFirstPersonModeForSwitching;
			movement.firstPerson.onlyRotateWithCamera = playerCamera.GetComponent<CameraController>().inFirstPersonMode;
			movement.firstPerson.onlyRotateWithCamera = playerCamera.GetComponent<CameraController>().firstPerson.mouseOrbiting.mouseOrbitInFirstPersonMode;
		}
		//if the firstPersonInputButton has been pressed and we are entering first person mode, set firstPersonButtonPressed to true
		if (movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			if (firstPersonStart){
				firstPersonButtonPressed = true;
			}
			else {
				firstPersonButtonPressed = false;
			}
		}
		else {
			firstPersonButtonPressed = false;
		}
		//determining if we are using first person mode or not
		if (movement.firstPerson.alwaysUseFirstPerson || movement.firstPerson.switchToFirstPersonIfInputButtonPressed && firstPersonButtonPressed){
			inFirstPersonMode = true;
		}
		else {
			inFirstPersonMode = false;
		}
		
	}
	
	void CrouchAttack () {
		
		//crouch attacking
		if (movement.crouching.allowCrouching){
			//if the player attacks while crouching: perform attack
			if (crouching && attackPressed && finishedCrouching && attacking.crouch.allowCrouchAttack && attackTimer > attacking.crouch.timeLimitBetweenCrouchAttacks){
				crouchCancelsAttack = false;
				attackFinished = false;
				attackFinishedLastUpdate = false;
				if (animator != null){
					animator.SetFloat("attackState", 2);
				}
				Attack();
			}
		}
		//if the crouch attack has finished, set crouchCancelsAttack to true
		if ((crouching || canCrouch) && !Input.GetButtonDown(attacking.attackInputButton) && attackFinishedLastUpdate){
			crouchCancelsAttack = true;
		}
		if (!crouching && !canCrouch || !movement.crouching.allowCrouching){
			crouchCancelsAttack = false;
		}
		//if the crouch attack finished in the last update
		if (attackFinished){
			attackFinishedLastUpdate = true;
		}
		else {
			attackFinishedLastUpdate = false;
		}
		attackFinished = true;
		
	}
	
	void Swim () {
		
		//getting swimming values
		Swim1();
		Swim2();
		
		//swimming
		if (inWater){
			Swim3();
			Swim4();
			Swim5();
			Swim6();
			Swim7();
		}
		//resetting swimming values
		else {
			Swim8();
			Swim9();
		}
		
	}
	
	void Swim1 () {
		
		//enabling and disabling scripts (and warning the user if any script names they entered do not exist on the player) when swimming
		SwimScriptEnablingDisabling();
		
		//checking to see if the player has a CharacterController, and if we can add a Rigidbody to the player (while swimming) to allow collider rotation
		CheckForCharacterController();
		
		//increase the noCollisionWithWaterTimer (if there is a collision with water, the noCollisionWithWaterTimer is set to 0)
		noCollisionWithWaterTimer++;
		noCollisionWithWaterSplashTimer++;
		
		//if the player's feet is colliding the water, set noCollisionWithWaterSplashTimer to 0 so he can make a splash
		if (Physics.Linecast(transform.position + transform.up/2, transform.position, out hit, collisionLayers) && hit.transform.gameObject.layer == 4){
			noCollisionWithWaterSplashTimer = 0;
		}
		
		//getting the player's current head and feet positions
		if (characterController && characterController.enabled){
			//y-axis only bounds
			currentHeadPosition = characterController.bounds.max.y;
			currentFeetPosition = characterController.bounds.min.y;
			//Vector3 bounds
			boundMin1 = characterController.bounds.min;
			boundMax1 = characterController.bounds.max;
		}
		else if (rigidBody && GetComponent<Collider>()){
			//y-axis only bounds
			currentHeadPosition = GetComponent<Collider>().bounds.max.y;
			currentFeetPosition = GetComponent<Collider>().bounds.min.y;
			//Vector3 bounds
			boundMin1 = GetComponent<Collider>().bounds.min;
			boundMax1 = GetComponent<Collider>().bounds.max;
			if (!inWater){
				//getting rigidbody collider bounds while not in the water (player is unrotated)
				rbHeadPositionOriginal = currentHeadPosition - transform.position.y;
				rbFeetPositionOriginal = currentFeetPosition - transform.position.y;
			}
			else {
				//getting rigidbody collider bounds while in the water (player is rotated)
				//unlike character controller colliders, rigidbody colliders rotate, so we must make modified versions of their collider bounds that do not rotate
				
				//getting collider height (head and feet bounds) relative to the player's position
				rbHeadPositionInWater = rbHeadPositionOriginal + transform.position.y;
				rbFeetPositionInWater = rbFeetPositionOriginal + transform.position.y;
				//getting collider height (head and feet bounds) relative to the player's center (like the character controller bounds)
				rbHeadPositionInWater2 = rbHeadPositionInWater - ((rbHeadPositionInWater + rbFeetPositionInWater)/2 - (currentHeadPosition + currentFeetPosition)/2);
				rbFeetPositionInWater2 = rbFeetPositionInWater - ((rbHeadPositionInWater + rbFeetPositionInWater)/2 - (currentHeadPosition + currentFeetPosition)/2);
			}
		}
		//Vector3 bounds with switched y-coordinates
		boundMin2 = new Vector3(boundMin1.x, boundMax1.y, boundMin1.x);
		boundMax2 = new Vector3(boundMax1.x, boundMin1.y, boundMax1.x);
		
		//checking if the player is submerged enough in water to swim
		//if the player is half way in the water, set submergedEnough equal to true
		if (Physics.Linecast(new Vector3(transform.position.x, currentHeadPosition - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 - jumpInHeight2, transform.position.z), out hit, collisionLayers)
		&& hit.transform.gameObject.layer == 4){
			
			submergedEnough = true;
			
		}
		//if player is colliding with water, but the linecast is not colliding with the water tag, or not colliding at all, the player is probably inside of the water completely
		else if ((!Physics.Linecast(new Vector3(transform.position.x, currentHeadPosition - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition - jumpInHeight2, transform.position.z), out hit, collisionLayers)
		&& !Physics.Linecast(boundMax1, boundMin1, out hit, collisionLayers) && !Physics.Linecast(boundMin1, boundMax1, out hit, collisionLayers) && !Physics.Linecast(boundMax2, boundMin2, out hit, collisionLayers) && !Physics.Linecast(boundMin2, boundMax2, out hit, collisionLayers)
		//
		|| Physics.Linecast(new Vector3(transform.position.x, currentHeadPosition - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition - jumpInHeight2, transform.position.z), out hit, collisionLayers) && hit.transform.gameObject.layer != 4)
		&& noCollisionWithWaterTimer < 5 && (groundedAfterSwimming || notInWaterCollider)){
			
			submergedEnough = true;
			
		}
		//if the player is not in the water or is half way out of the water, set submergedEnough equal to false
		else if (!inWater || canJumpOut && Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 - 0.2f - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition - jumpInHeight2, transform.position.z), out hit, collisionLayers)
		&& hit.transform.gameObject.layer == 4 && inWater){
			
			submergedEnough = false;
			
		}
		
		//determining if the player is under the top of the water
		if (submergedEnough && (!Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + 0.3f - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition - jumpInHeight2, transform.position.z), out hit, collisionLayers)
		|| Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + 0.3f - jumpInHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition - jumpInHeight2, transform.position.z), out hit, collisionLayers) && hit.transform.gameObject.layer != 4)){
			submergedOnceAlready = true;
		}
		else if (canJumpOut || !inWater){
			submergedOnceAlready = false;
		}
		
		//determining if we are in the water, and are able to swim
		if (noCollisionWithWaterTimer < 5 && (!inMidAirFromJump || currentlyOnWall || moveDirection.y < 0 && !jumpPerformed) && submergedEnough && submergedOnceAlready && swimming.allowSwimming
		&& (!canJumpOut || !atJumpHeight) && (moveDirection.y <= 0 || groundedAfterSwimming || notInWaterCollider || inWater)){
			if (!inWater){
				inWater = true;
				inWaterTimer = 0;
				swimmingMovementTransferred = false;
				//checking to see if the player has a CharacterController, and if we can add a Rigidbody to the player (while swimming) to allow collider rotation
				CheckForCharacterController();
				//resetting movement variables when player enters the water
				climbDirection = Vector3.zero;
				swimDirection = Vector3.zero;
				currentlyOnWall = false;
				moveSpeed = 0;
				if (rigidBody){
					rigidBody.velocity = Vector3.zero;
				}
				slidingVector = Vector3.zero;
				slideMovement = Vector3.zero;
				groundedAfterSwimming = false;
				inMidAirFromJump = false;
				inMidAirFromWallJump = false;
				if (Input.GetButton("Jump")){
					heldSwimButtonOnEnter = true;
				}
				else {
					heldSwimButtonOnEnter = false;
				}
				
				//getting rotation direction on z-axis (if x-axis is locked)
				if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					//if the joystick is pushed to the right
					if (Input.GetAxisRaw("Horizontal") > 0){
						swimHorizontalValue = -1;
						horizontalValue = -1;
					}
					//if the joystick is pushed to the left
					else if (Input.GetAxisRaw("Horizontal") < 0){
						swimHorizontalValue = 1;
						horizontalValue = 1;
					}
				}
				//getting rotation direction on x-axis (if z-axis is locked)
				else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
					//if the joystick is pushed to the right
					if (Input.GetAxisRaw("Horizontal") > 0){
						swimHorizontalValue = 1;
						horizontalValue = 1;
					}
					//if the joystick is pushed to the left
					else if (Input.GetAxisRaw("Horizontal") < 0){
						swimHorizontalValue = -1;
						horizontalValue = -1;
					}
				}
				
			}
		}
		else {
			//jumping out of water
			if (inWater){
				inWater = false;
				outOfWaterTimer = 0;
				//checking to see if the player has a CharacterController, and if we can add a Rigidbody to the player (while swimming) to allow collider rotation
				CheckForCharacterController();
				//resetting swimming variables
				swimmingMovementSpeed = 0;
				swimmingMovementTransferred = true;
				//telling the script that the player is grounded so he is allowed to jump
				grounded.currentlyGrounded = true;
				//resetting movement and jumping variables
				inMidAirFromJump = false;
				inMidAirFromWallJump = false;
				currentJumpNumber = totalJumpNumber;
				turnBack = false;
				back2 = false;
				currentlyClimbingWall = false;
				jumpPressed = false;
				jumpPossible = true;
				doubleJumpPossible = true;
				//jumping out of the water (if player is at jump height, and not swimming out the bottom)
				if (atJumpHeight) {
					Jump();
				}
				swimDirection = new Vector3(swimDirection.x, 0, swimDirection.z);
				//the player is not really grounded here, so we set groundedAfterSwimming to false
				groundedAfterSwimming = false;
			}
		}
		inWaterTimer += 0.2f;
		outOfWaterTimer += 0.2f;
		
	}
	
	void Swim2 () {
		
		//getting movement direction
		if (atJumpHeight && yDeg < swimming.angleNeededToJumpOut){
			stillAtTop = true;
		}
		if (yDeg >= swimming.angleNeededToJumpOut || !inWater || !Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + 0.5f, transform.position.z), new Vector3(transform.position.x, currentFeetPosition, transform.position.z), out hit, collisionLayers)
		|| Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + 0.5f, transform.position.z), new Vector3(transform.position.x, currentFeetPosition, transform.position.z), out hit, collisionLayers) && hit.transform.gameObject.layer != 4){
			stillAtTop = false;
		}
		
		
		//getting the actual joystick input values
		//getting actual horizontal input
		if (Input.GetAxis("Horizontal") > 0){
			hPositive = true;
		}
		else if (Input.GetAxis("Horizontal") < 0){
			hPositive = false;
		}
		if (hPositive){
			hAxis = (Mathf.Abs(Input.GetAxis("Horizontal")) - Mathf.Abs(Input.GetAxis("Vertical")) + 1)/2;
			if (hAxis != 0){
				hAxis2 = hAxis*2 - 1;
			}
			else {
				hAxis2 = hAxis;
			}
		}
		else {
			hAxis = -(Mathf.Abs(Input.GetAxis("Horizontal")) - Mathf.Abs(Input.GetAxis("Vertical")) + 1)/2;
			if (hAxis != 0){
				hAxis2 = hAxis*2 + 1;
			}
			else {
				hAxis2 = hAxis;
			}
		}
		//if joystick is being pressed diagonally
		if (vPositive && hPositive && (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") > 0)
		|| !vPositive && !hPositive && (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") < 0 || Input.GetAxis("Vertical") < 0 && Input.GetAxis("Horizontal") < 0)
		|| vPositive && !hPositive && (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") < 0 || Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") < 0)
		|| !vPositive && hPositive && (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0 && Input.GetAxis("Horizontal") > 0)){
			hAxis2 = hAxis;
		}
		//if joystick is being pressed left and right
		else {
			if (hPositive && hAxis2 < 0){
				hAxis2 = 0;
			}
			if (!hPositive && hAxis2 > 0){
				hAxis2 = 0;
			}
		}
		
		//getting actual vertical input
		if (Input.GetAxis("Vertical") > 0){
			vPositive = true;
		}
		else if (Input.GetAxis("Vertical") < 0){
			vPositive = false;
		}
		if (vPositive){
			vAxis = (Mathf.Abs(Input.GetAxis("Vertical")) - Mathf.Abs(Input.GetAxis("Horizontal")) + 1)/2;
			if (vAxis != 0){
				vAxis2 = vAxis*2 - 1;
			}
			else {
				vAxis2 = vAxis;
			}
		}
		else {
			vAxis = -(Mathf.Abs(Input.GetAxis("Vertical")) - Mathf.Abs(Input.GetAxis("Horizontal")) + 1)/2;
			if (vAxis != 0){
				vAxis2 = vAxis*2 + 1;
			}
			else {
				vAxis2 = vAxis;
			}
		}
		//if joystick is being pressed diagonally
		if (vPositive && hPositive && (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") > 0)
		|| !vPositive && !hPositive && (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") < 0 || Input.GetAxis("Vertical") < 0 && Input.GetAxis("Horizontal") < 0)
		|| vPositive && !hPositive && (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") < 0 || Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") < 0)
		|| !vPositive && hPositive && (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0 && Input.GetAxis("Horizontal") > 0)){
			vAxis2 = vAxis;
		}
		//if joystick is being pressed up and down
		else {
			if (vPositive && vAxis2 < 0){
				vAxis2 = 0;
			}
			if (!vPositive && vAxis2 > 0){
				vAxis2 = 0;
			}
		}
		
	}
	
	void Swim3 () {
		
		//if we are no longer holding the swim button
		if (!Input.GetButton("Jump")){
			heldSwimButtonOnEnter = false;
		}
		
		//determining if the player has reached the top of the water before jumping
		if (atJumpHeight && yDeg < swimming.angleNeededToJumpOut){
			if (!Input.GetButton("Jump")){
				canPressSwimButton = true;
			}
		}
		else {
			canPressSwimButton = false;
		}
		//determining if we are allowed to jump out of the water yet
		if (canPressSwimButton && Input.GetButton("Jump")){
			swimButtonPressed = true;
			outOfWaterTimer = 0;
		}
		else {
			swimButtonPressed = false;
		}
		
		
		//rotation
		//getting degrees of rotation for player (if not in first person mode)
		if ((!inFirstPersonMode || !movement.firstPerson.onlyRotateWithCamera)
		|| (playerCamera.GetComponent<CameraController>() && !playerCamera.GetComponent<CameraController>().goneThroughFirstPersonUpdate)) {
			//getting left and right rotation
			if (!swimming.invertXRotation){
				xDeg += Input.GetAxis("Horizontal") * swimming.swimRotationSpeed * 0.3f;
			}
			//getting left and right rotation inverted
			else {
				xDeg -= Input.GetAxis("Horizontal") * swimming.swimRotationSpeed * 0.3f;
			}
			//getting up and down rotation
			if (!swimming.invertYRotation){
				yDeg += Input.GetAxis("Vertical") * swimming.swimRotationSpeed * 0.2f;
			}
			//getting up and down rotation inverted
			else {
				yDeg -= Input.GetAxis("Vertical") * swimming.swimRotationSpeed * 0.2f;
			}
			
			//setting values for when we enter first person mode
			cameraYDeg = 0;
			cameraYDeg2 = 0;
			horizontalRotValue = 0;
			verticalRotValue = 0;
			rotDiff = 0;
		}
		//getting degrees of rotation for player (if in first person mode)
		else {
			
			//allowing the player to move with the joystick and camera (first person mouse orbiting)
			if ((hAxis2 != 0 || vAxis2 != 0) && swimming.firstPerson.allowShiftingWithJoystick){
				//getting left and right rotation
				//keeping the player from spinning in circles to the left and right
				if (Mathf.Abs(horizontalRotValue) < 90){
					if (!swimming.invertXRotation){
						horizontalRotValue += hAxis2 * swimming.firstPerson.joystickMovementSpeed * 0.6f;
					}
					//getting left and right rotation inverted
					else {
						horizontalRotValue -= hAxis2 * swimming.firstPerson.joystickMovementSpeed * 0.6f;
					}
				}
				if (horizontalRotValue >= 89.99f){
					horizontalRotValue = 89.99f;
				}
				else if (horizontalRotValue <= -89.99f){
					horizontalRotValue = -89.99f;
				}
				
				//getting up and down rotation
				if (Mathf.Abs(verticalRotValue) < 180){
					if (!swimming.invertYRotation){
						verticalRotValue += vAxis2 * swimming.firstPerson.joystickMovementSpeed * 0.4f;
					}
					//getting up and down rotation inverted
					else {
						verticalRotValue -= vAxis2 * swimming.firstPerson.joystickMovementSpeed * 0.4f;
					}
				}
				if (verticalRotValue >= 89.99f){
					verticalRotValue = 89.99f;
				}
				else if (verticalRotValue <= -89.99f){
					verticalRotValue = -89.99f;
				}
			}
			else {
				horizontalRotValue = Mathf.Lerp(horizontalRotValue, 0, swimming.firstPerson.joystickDecelerationSpeed * Time.deltaTime);
				verticalRotValue = Mathf.Lerp(verticalRotValue, 0, swimming.firstPerson.joystickDecelerationSpeed * Time.deltaTime);
			}
			if (verticalRotValue >= 180){
				verticalRotValue = 360 - verticalRotValue;
			}
			//getting rotation of camera
			if (!playerCamera.GetComponent<CameraController>()){
				if (playerCamera.transform.eulerAngles.x < 180){
					cameraYDeg = playerCamera.transform.eulerAngles.x;
				}
				else {
					cameraYDeg = playerCamera.transform.eulerAngles.x - 360;
				}
			}
			else {
				//getting up and down rotation of camera
				cameraYDeg2 = playerCamera.GetComponent<CameraController>().yFpDeg2;
				
				//getting what our current y degrees should be
				rotDiff = 90 + swimming.firstPerson.cameraAngleOffset;
				cameraYDeg = cameraYDeg2 + rotDiff;
				cameraYDeg = ClampAngle (cameraYDeg, yRotMaxLimit, yRotMinLimit);
			}
			
			if (swimming.firstPerson.allowMovement || !inFirstPersonMode){
				xDeg = playerCamera.transform.eulerAngles.y + horizontalRotValue;
				yDeg = cameraYDeg + verticalRotValue;
			}
		}
		
	}
	
	void Swim4 () {
		
		//setting the player's rotation limits
		//if player is not swimming vertically, use the yIdleRotation limits
		if (Input.GetAxis("Vertical") == 0){
			//adjusting min limit
			if (yDeg > -swimming.yIdleRotationMinLimit + 80){
				yRotMinLimit = Mathf.Lerp(yRotMinLimit, -swimming.yIdleRotationMinLimit + 80, 3 * Time.deltaTime);
			}
			else {
				yRotMinLimit = -swimming.yIdleRotationMinLimit + 80;
			}
			//adjusting max limit
			if (yDeg < -swimming.yIdleRotationMaxLimit + 80){
				yRotMaxLimit = Mathf.Lerp(yRotMaxLimit, -swimming.yIdleRotationMaxLimit + 80, 3 * Time.deltaTime);
			}
			else {
				yRotMaxLimit = -swimming.yIdleRotationMaxLimit + 80;
			}
		}
		//if player is swimming vertically, use the ySwimmingRotation limits
		else {
			//adjusting min limit
			yRotMinLimit = -swimming.ySwimmingRotationMinLimit + 80;
			//adjusting max limit
			yRotMaxLimit = -swimming.ySwimmingRotationMaxLimit + 80;
		}
		//clamping yDeg between the minimum and maximum y limit
		if (yDeg >= 180){
			yDeg = 360 - yDeg;
		}
		yDeg = ClampAngle (yDeg, yRotMaxLimit, yRotMinLimit);
		
		
		//determining if player is angled up enough to jump out of the water
		if (yDeg < swimming.angleNeededToJumpOut && swimButtonPressed){
			canJumpOut = true;
			outOfWaterTimer = 0;
		}
		else {
			canJumpOut = false;
		}
		
		
		//getting rotation on z-axis (x-axis is locked)
		if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
			//if the joystick is pushed to the right
			if (hAxis2 > 0.25f){
				if (axisNotPushedLeftOrRight){
					swimHorizontalValue = -1;
					horizontalValue = -1;
				}
				axisNotPushedLeftOrRight = false;
			}
			//if the joystick is pushed to the left
			else if (hAxis2 < -0.25f){
				if (axisNotPushedLeftOrRight){
					swimHorizontalValue = 1;
					horizontalValue = 1;
				}
				axisNotPushedLeftOrRight = false;
			}
			if (Mathf.Abs(hAxis2) <= 0.25f && Mathf.Abs(vAxis2) <= 0.5f){
				axisNotPushedLeftOrRight = true;
			}
			
			//getting rotation to the right
			if (swimHorizontalValue == 1){
				if (!movement.sideScrolling.flipAxisRotation){
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 359.999f, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 0.001f, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
				else {
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 359.999f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 0.001f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
			}
			//getting rotation to the left
			else if (swimHorizontalValue == -1){
				if (!movement.sideScrolling.flipAxisRotation){
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 180.001f, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 179.999f, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
				else {
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 180.001f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 179.999f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
			}
			
			//getting rotation
			swimRotation = Quaternion.Euler (yDeg, xDeg2, 0);
		}
		//getting rotation on x-axis (z-axis is locked)
		else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
			//if the joystick is pushed to the right
			if (hAxis2 > 0.25f){
				if (axisNotPushedLeftOrRight){
					swimHorizontalValue = 1;
					horizontalValue = 1;
				}
				axisNotPushedLeftOrRight = false;
			}
			//if the joystick is pushed to the left
			else if (hAxis2 < -0.25f){
				if (axisNotPushedLeftOrRight){
					swimHorizontalValue = -1;
					horizontalValue = -1;
				}
				axisNotPushedLeftOrRight = false;
			}
			if (Mathf.Abs(hAxis2) <= 0.25f && Mathf.Abs(vAxis2) <= 0.5f){
				axisNotPushedLeftOrRight = true;
			}
			
			//getting rotation to the right
			if (swimHorizontalValue == 1){
				if (!movement.sideScrolling.flipAxisRotation){
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 90.001f, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 89.999f, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
				else {
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 90.001f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, 89.999f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
			}
			//getting rotation to the left
			else if (swimHorizontalValue == -1){
				if (!movement.sideScrolling.flipAxisRotation){
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 269.001f, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, -89.999f, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
				else {
					if (movement.sideScrolling.rotateInwards){
						xDeg2 = Mathf.Lerp(xDeg2, 269.001f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
					else {
						xDeg2 = Mathf.Lerp(xDeg2, -89.999f + 180, swimming.swimRotationSpeed * Time.deltaTime);
					}
				}
			}
			
			//getting rotation
			swimRotation = Quaternion.Euler (yDeg, xDeg2, 0);
		}
		//if neither axis is locked
		else {
			
			//getting swimming rotation
			swimRotation = Quaternion.Euler (yDeg, xDeg, 0);
			
		}
		
		//setting rotation after jumping in water
		if (inWaterTimer < 5){
			transform.localRotation = Quaternion.Slerp(transform.localRotation, swimRotation, 15 * Time.deltaTime);
		}
		//setting swimming rotation
		else {
			transform.localRotation = swimRotation;
		}
		
	}
	
	void Swim5 () {
		
		//movement
		if (Input.GetButton("Jump")){
			//accelerating
			swimmingMovementSpeed = Mathf.Lerp(swimmingMovementSpeed, swimming.swimMovementSpeed, swimming.accelerationRate * Time.deltaTime);
		}
		else {
			//decelerating
			swimmingMovementSpeed = Mathf.Lerp(swimmingMovementSpeed, 0, swimming.decelerationRate * Time.deltaTime);
		}
		
		//getting movement direction
		if (swimming.firstPerson.allowMovement || !inFirstPersonMode){
			if ((!atJumpHeight || yDeg >= swimming.angleNeededToJumpOut) && !stillAtTop){
				swimDirection = transform.up * swimmingMovementSpeed;
			}
			else {
				swimDirection = new Vector3((transform.up * swimmingMovementSpeed).x, 0, (transform.up * swimmingMovementSpeed).z);
			}
		}
		else {
			swimDirection = Vector3.Lerp(swimDirection, Vector3.zero, 3 * Time.deltaTime);
		}
		
		//getting the character controller top of water (character controller collider stays vertical, so we can get the top directly)
		if (characterController && characterController.enabled){
			//getting top water position
			if (Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + (currentHeadPosition - currentFeetPosition) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition + jumpOutHeight2, transform.position.z), out hit, collisionLayers)
			&& hit.transform.gameObject.layer == 4){
				inWaterPosY = hit.point.y - (currentHeadPosition - currentFeetPosition) + (transform.position.y - currentFeetPosition)/1.75f + 0.5f + jumpOutHeight2;
				reachedTopOfWater = true;
			}
			else {
				atJumpHeight = false;
			}
		}
		//getting the rigidbody top of water (rigidbody colliders rotate, so we must use a vertically modified version of their collider bounds to get the top)
		else if (rigidBody){
			//getting top water position
			if (Physics.Linecast(new Vector3(transform.position.x, (rbHeadPositionInWater2 + rbFeetPositionInWater2)/2 + (rbHeadPositionInWater2 - rbFeetPositionInWater2) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, rbFeetPositionInWater2 + jumpOutHeight2, transform.position.z), out hit, collisionLayers)
			&& hit.transform.gameObject.layer == 4){
				inWaterPosY = hit.point.y - (rbHeadPositionInWater2 - rbFeetPositionInWater2) + (transform.position.y - rbFeetPositionInWater2)/1.75f + 0.5f + jumpOutHeight2;
				reachedTopOfWater = true;
			}
			else {
				atJumpHeight = false;
			}
		}
		
		//if player is above water level and colliding with land, stop swimming and transition back to movement
		if (reachedTopOfWater && transform.position.y >= inWaterPosY + (swimming.walkOutHeight/10 + 0.17f) && noCollisionTimer < 5){
			//walking out of water when touching land
			if (inWater){
				inWater = false;
				outOfWaterTimer = 0;
				//checking to see if the player has a CharacterController, and if we can add a Rigidbody to the player (while swimming) to allow collider rotation
				CheckForCharacterController();
				//setting rotation back to normal after jumping out of water
				if (yDeg >= 92.55f){
					transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z);
				}
				//resetting swimming variables
				swimmingMovementSpeed = 0;
				swimmingMovementTransferred = true;
				grounded.currentlyGrounded = true;
				//resetting movement and jumping variables
				inMidAirFromJump = false;
				inMidAirFromWallJump = false;
				currentJumpNumber = totalJumpNumber;
				turnBack = false;
				back2 = false;
				currentlyClimbingWall = false;
				jumpPressed = false;
				jumpPossible = false;
				jumpEnabled = false;
				doubleJumpPossible = true;
				submergedEnough = false;
				submergedOnceAlready = false;
				swimDirection = new Vector3(swimDirection.x, 0, swimDirection.z);
				groundedAfterSwimming = true;
			}
		}
		
		//determining if player has reached the top of the water, and is able to jump
		if (reachedTopOfWater && transform.position.y >= inWaterPosY && yDeg < 92.55f){
			atJumpHeight = true;
		}
		else if (yDeg >= 92.55f){
			atJumpHeight = false;
		}
		//if player is at the top of the water (jump height), keep him from going any higher
		if ((atJumpHeight || reachedTopOfWater && transform.position.y + 0.01f >= inWaterPosY && !Input.GetButton("Jump")) && Mathf.Abs(inWaterPosY - transform.position.y) >= 0.1f && noCollisionTimer >= 5){
			transform.position = new Vector3 (transform.position.x, inWaterPosY, transform.position.z);
		}
		if (atJumpHeight || reachedTopOfWater && transform.position.y + 0.01f >= inWaterPosY && !Input.GetButton("Jump") || stillAtTop){
			swimDirection = new Vector3(swimDirection.x, 0, swimDirection.z);
		}
		
		//continuing and decreasing movement from moveDirection when we jumped into the water
		moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, 6 * Time.deltaTime);
		//if player has already reached the top of the water, set the y-axis to 0, and only continue movement on the x and z-axis
		if (atJumpHeight){
			moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
		}
		
	}
	
	void Swim6 () {
		
		//moving
		if (characterController && characterController.enabled){
			
			//swimming
			characterController.Move(swimDirection * Time.deltaTime);
			//floating when player is not actually swimming
			if (!swimButtonPressed && submergedEnough){
				//float in any direction if we have not reached top
				if (!atJumpHeight
				&& (transform.position.y + 0.01f < inWaterPosY || !Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + (currentHeadPosition - currentFeetPosition) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition + jumpOutHeight2, transform.position.z), out hit, collisionLayers)
				|| Physics.Linecast(new Vector3(transform.position.x, (currentHeadPosition + currentFeetPosition)/2 + (currentHeadPosition - currentFeetPosition) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, currentFeetPosition + jumpOutHeight2, transform.position.z), out hit, collisionLayers) && hit.transform.gameObject.layer != 4)
				|| swimming.idleFloatingDirection.y <= 0){
					characterController.Move(swimming.idleFloatingDirection * Time.deltaTime);
				}
				//do not float up if we have reached the top
				else {
					characterController.Move(new Vector3(swimming.idleFloatingDirection.x, 0, swimming.idleFloatingDirection.z) * Time.deltaTime);
				}
			}
			//continuing moveDirection
			characterController.Move(moveDirection * 0.8f * Time.deltaTime);
		
		}
		else if (rigidBody){
			
			//keeping player from moving in unwanted directions
			rigidBody.velocity = Vector3.zero;
			//floating when player is not actually swimming
			if (!swimButtonPressed && submergedEnough){
				//float in any direction if we have not reached top
				if (!atJumpHeight
				&& (transform.position.y + 0.01f < inWaterPosY || !Physics.Linecast(new Vector3(transform.position.x, (rbHeadPositionInWater2 + rbFeetPositionInWater2)/2 + (rbHeadPositionInWater2 - rbFeetPositionInWater2) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, rbFeetPositionInWater2 + jumpOutHeight2, transform.position.z), out hit, collisionLayers)
				|| Physics.Linecast(new Vector3(transform.position.x, (rbHeadPositionInWater2 + rbFeetPositionInWater2)/2 + (rbHeadPositionInWater2 - rbFeetPositionInWater2) + jumpOutHeight2, transform.position.z), new Vector3(transform.position.x, rbFeetPositionInWater2 + jumpOutHeight2, transform.position.z), out hit, collisionLayers) && hit.transform.gameObject.layer != 4)
				|| swimming.idleFloatingDirection.y <= 0){
					rigidBody.MovePosition(transform.position + (swimDirection + swimming.idleFloatingDirection + (moveDirection * 0.8f)) * Time.deltaTime);
				}
				//do not float up if we have reached the top
				else {
					rigidBody.MovePosition(transform.position + (swimDirection + new Vector3(swimming.idleFloatingDirection.x, 0, swimming.idleFloatingDirection.z) + (moveDirection * 0.8f)) * Time.deltaTime);
				}
			}
			//swimming and continuing movementDirection
			else {
				rigidBody.MovePosition(transform.position + (swimDirection + (moveDirection * 0.8f)) * Time.deltaTime);
			}
			
			//getting collider's actual height
			if (characterController && characterController.enabled){
				colliderBottom = characterController.bounds.center - (characterController.height*0.5f*transform.up);
				colliderTop = characterController.bounds.center + (characterController.height*0.5f*transform.up);
			}
			else if (rigidBody){
				if (GetComponent<CapsuleCollider>()){
					colliderBottom = GetComponent<CapsuleCollider>().bounds.center - (GetComponent<CapsuleCollider>().height*0.5f*transform.up);
					colliderTop = GetComponent<CapsuleCollider>().bounds.center + (GetComponent<CapsuleCollider>().height*0.5f*transform.up);
				}
				else if (GetComponent<SphereCollider>()){
					colliderBottom = GetComponent<SphereCollider>().bounds.center - (GetComponent<SphereCollider>().radius*transform.up);
					colliderTop = GetComponent<SphereCollider>().bounds.center + (GetComponent<SphereCollider>().radius*transform.up);
				}
				else if (GetComponent<BoxCollider>()){
					colliderBottom = GetComponent<BoxCollider>().bounds.center - (GetComponent<BoxCollider>().size.y*0.5f*transform.up);
					colliderTop = GetComponent<BoxCollider>().bounds.center + (GetComponent<BoxCollider>().size.y*0.5f*transform.up);
				}
			}
			//preventing player from swimming through walls
			if (!Physics.Linecast((colliderBottom*9 + colliderTop)/10, (colliderTop*9 + colliderBottom)/10, out hit, collisionLayers)){
				lastPosBeforeCollision = transform.position;
				lastRotBeforeCollision = transform.eulerAngles;
			}
			else if (Physics.Linecast((colliderBottom*8.5f + colliderTop*1.5f)/10, (colliderTop*8.5f + colliderBottom*1.5f)/10, out hit, collisionLayers)){
				transform.position = lastPosBeforeCollision;
				transform.eulerAngles = new Vector3(lastRotBeforeCollision.x, transform.eulerAngles.y, lastRotBeforeCollision.z);
				swimRotation = transform.rotation;
			}
			
		}
		
	}
	
	void Swim7 () {
		
		//animating the player
		if (animator != null){
			
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Swimming")){
				animator.CrossFade("Swimming", 0f, -1, 0f);
			}
			
			//swimming idle animation
			if (!Input.GetButton("Jump")){
				animator.speed = 1;
				donePulling = false;
				justSwitchedToSwimming = false;
				swimState = Mathf.Lerp(swimState, 0, 6 * Time.deltaTime);
			}
			//swimming movement animations
			else if (outOfWaterTimer > 0){
				
				//if we are already holding the swim button as we enter the water, go straight to kicking (swim state 2)
				if (heldSwimButtonOnEnter){
					donePulling = true;
					justSwitchedToSwimming = true;
					swimState = 2;
					animator.CrossFade("Swimming", 0f, -1, 1f);
					animator.speed = 1;
					heldSwimButtonOnEnter = false;
				}
				
				//pull to idle transition (swim state 1 to 0)
				if (swimState < 1){
					animator.speed = 1;
					if (!donePulling){
						animator.CrossFade("Swimming", 0f, -1, 0f);
						donePulling = false;
						swimState = 1;
					}
					donePulling = true;
				}
				//kick to pull transition (swim state 2 to 1)
				else if (swimState >= 1){
					//resetting animation at beginning of pull
					if (!justSwitchedToSwimming){
						animator.CrossFade("Swimming", 0f, -1, 0f);
						animator.speed = 0.1f;
						justSwitchedToSwimming = true;
					}
					//pulling (swim state 1)
					if (!donePulling){
						if (swimState < 2){
							swimState = Mathf.Lerp(swimState, 1, 8 * Time.deltaTime);
						}
						if (Mathf.Abs(swimState - 1) < 0.1f){
							animator.CrossFade("Swimming", 0f, -1, 0f);
							animator.speed = 1;
							donePulling = true;
						}
					}
					else {
						animator.speed = 1;
					}
					//kicking (swim state 2)
					if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && donePulling){
						swimState = Mathf.Lerp(swimState, 2, 6 * Time.deltaTime);
						animator.speed = 1;
					}
				}
			}
			
			animator.SetFloat("swimState", swimState);
		}
		
	}
	
	void Swim8 () {
		
		//setting swimming rotation to what the player's current rotation of the water is
		xDeg = transform.eulerAngles.y;
		yDeg = transform.eulerAngles.x;
		xDeg2 = transform.eulerAngles.y;
		horizontalRotValue = 0;
		verticalRotValue = 0;
		//setting rotation limits for swimming
		yRotMinLimit = swimming.yIdleRotationMinLimit;
		yRotMaxLimit = swimming.yIdleRotationMaxLimit;
		//determining whether the player is moving left or right for side-scrolling
		//if the joystick is pushed to the right
		if (hAxis2 > 0){
			swimHorizontalValue = 1;
			axisNotPushedLeftOrRight = false;
		}
		//if the joystick is pushed to the left
		else if (hAxis2 < 0){
			swimHorizontalValue = -1;
			axisNotPushedLeftOrRight = false;
		}
		else {
			axisNotPushedLeftOrRight = true;
		}
		//player cannot press the swim button if he is out of the water
		canPressSwimButton = false;
		swimButtonPressed = false;
		reachedTopOfWater = false;
		atJumpHeight = false;
		canJumpOut = false;
		//continuing swimming movement if player just leaped out of the water and has not been grounded yet
		if (!grounded.currentlyGrounded && Vector3.Distance(swimDirection, Vector3.zero) >= 0.01f
		&& !swimmingMovementTransferred){
			swimDirection = Vector3.Lerp(swimDirection, Vector3.zero, 3 * Time.deltaTime);
			if (characterController && characterController.enabled){
				characterController.Move(swimDirection * Time.deltaTime);
			}
			else if (rigidBody){
				rigidBody.MovePosition(transform.position + swimDirection * Time.deltaTime);
			}
		}
		//discontinue swimming movement
		else {
			swimDirection = Vector3.zero;
			swimmingMovementSpeed = 0;
			swimmingMovementTransferred = true;
		}
		
		//getting rigidbody collider bounds while not in the water
		rbHeadPositionOriginal = currentHeadPosition - transform.position.y;
		rbFeetPositionOriginal = currentFeetPosition - transform.position.y;
		
		//if player is no longer in the air (most likely from jumping) after swimming, set groundedAfterSwimming to true
		if (!inMidAirFromJump){
			groundedAfterSwimming = true;
		}
		
		//setting rotation back to normal after jumping out of water
		if (outOfWaterTimer < 5){
			Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15 * Time.deltaTime);
		}
		
	}
	
	void Swim9 () {
		
		//setting the player's animations back to normal if he is no longer in the water
		if (animator != null && !inWater && (animator.GetCurrentAnimatorStateInfo(0).IsName("Swimming") || animator.GetCurrentAnimatorStateInfo(0).IsName("Falling")) && outOfWaterTimer < 1){
			if (grounded.currentlyGrounded){
				animator.CrossFade("Movement", 0.0075f, -1, 0f);
			}
			else if (!currentlyOnWall && !currentlyClimbingWall){
				animator.CrossFade("Falling", 0.0075f, -1, 0f);
			}
			animator.speed = 1;
		}
		
		//getting collider's bounds for raycasting
		//getting collider's actual height
		if (characterController && characterController.enabled){
			colliderBottom2 = characterController.bounds.center - (characterController.height*0.5f*Vector3.up);
			colliderTop2 = characterController.bounds.center + (characterController.height*0.5f*Vector3.up);
		}
		else if (rigidBody){
			if (GetComponent<CapsuleCollider>()){
				colliderBottom2 = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().height*0.5f*Vector3.up);
				colliderTop2 = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().height*0.5f*Vector3.up);
			}
			else if (GetComponent<SphereCollider>()){
				colliderBottom2 = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*Vector3.up);
				colliderTop2 = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*Vector3.up);
			}
			else if (GetComponent<BoxCollider>()){
				colliderBottom2 = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.y*0.5f*Vector3.up);
				colliderTop2 = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.y*0.5f*Vector3.up);
			}
		}
		//getting collider's actual width
		if (characterController && characterController.enabled){
			colliderLeft2 = characterController.bounds.center - (characterController.radius*0.5f*Vector3.right);
			colliderRight2 = characterController.bounds.center + (characterController.radius*0.5f*Vector3.right);
		}
		else if (rigidBody){
			if (GetComponent<CapsuleCollider>()){
				colliderLeft2 = GetComponent<Collider>().bounds.center - (GetComponent<CapsuleCollider>().radius*Vector3.right);
				colliderRight2 = GetComponent<Collider>().bounds.center + (GetComponent<CapsuleCollider>().radius*Vector3.right);
			}
			else if (GetComponent<SphereCollider>()){
				colliderLeft2 = GetComponent<Collider>().bounds.center - (GetComponent<SphereCollider>().radius*Vector3.right);
				colliderRight2 = GetComponent<Collider>().bounds.center + (GetComponent<SphereCollider>().radius*Vector3.right);
			}
			else if (GetComponent<BoxCollider>()){
				colliderLeft2 = GetComponent<Collider>().bounds.center - (GetComponent<BoxCollider>().size.x*0.5f*Vector3.right);
				colliderRight2 = GetComponent<Collider>().bounds.center + (GetComponent<BoxCollider>().size.x*0.5f*Vector3.right);
			}
		}
		//checking to see if player is colliding with water (relative to the user-defined Jump In Height)
		if (Physics.Raycast(colliderBottom2 - new Vector3(0, jumpInHeight2, 0), colliderTop2 - colliderBottom2, out hit, collisionLayers) || Physics.Raycast(colliderTop2 - new Vector3(0, jumpInHeight2, 0), colliderBottom2 - colliderTop2, out hit, collisionLayers)
		|| Physics.Raycast(colliderLeft2 - new Vector3(0, jumpInHeight2, 0), colliderRight2 - colliderLeft2, out hit, collisionLayers) || Physics.Raycast(colliderRight2 - new Vector3(0, jumpInHeight2, 0), colliderLeft2 - colliderRight2, out hit, collisionLayers)){
			if (hit.transform.gameObject.layer == 4 && swimming.allowSwimming){
				noCollisionWithWaterTimer = 0;
				noCollisionWithWaterSplashTimer = 0;
			}
		}
		
		//checking to see if player is fully out of the water yet
		if (inWater){
			notInWaterCollider = false;
		}
		else if (noCollisionWithWaterTimer >= 5){
			notInWaterCollider = true;
		}
		
	}
	
	public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f){
            angle += 360f;
		}
        if (angle > 360f){
            angle -= 360f;
		}
        return Mathf.Clamp(angle, min, max);
    }
	
	void LateUpdate () {
		
		//checking to see if the first person button has been pressed
		if (Input.GetButtonDown(movement.firstPerson.firstPersonInputButton) && movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			if (firstPersonStart){
				firstPersonStart = false;
			}
			else {
				firstPersonStart = true;
			}
		}
		else if (!movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			firstPersonStart = false;
		}
		
		if (enabledLastUpdate){
			//if the player only rotates with the camera
			if (movement.firstPerson.onlyRotateWithCamera && (!currentlyClimbingWall && !turnBack && !back2 && !currentlyOnWall) && inFirstPersonMode){
				if (!inWater){
					transform.eulerAngles = new Vector3(0, playerCamera.transform.eulerAngles.y, 0);
				}
				else {
					transform.localRotation = Quaternion.Euler(yDeg, xDeg, 0);
				}
			}
		}
		enabledLastUpdate = true;
		
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
		wallScriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		wallScriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
	}
	
	void CheckIfInBetweenSlopes () {
		
		//checking to see if player is stuck between two slopes
		
		RaycastHit hit2 = new RaycastHit();
		
		//checking left and right
		if (Physics.Raycast(pos - transform.right/5, Vector3.down, out hit, maxGroundedDistance2, noWaterCollisionLayers) && Physics.Raycast(pos + transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position - transform.right/5 + transform.up/4, transform.position - transform.right/5 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position + transform.right/5 + transform.up/4, transform.position + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)	
		|| Physics.Linecast(transform.position - transform.right/25 + transform.up/4, transform.position - transform.right/25 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position + transform.right/25 + transform.up/4, transform.position + transform.right/25 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward left and back right
		|| Physics.Raycast(pos + transform.forward/25 - transform.right/5, Vector3.down, out hit, maxGroundedDistance2, noWaterCollisionLayers) && Physics.Raycast(pos - transform.forward/25 + transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 - transform.right/5 + transform.up/4, transform.position + transform.forward/50 - transform.right/5 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/50 - transform.right/5 + transform.up/4, transform.position - transform.forward/50 - transform.right/5 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/5 + transform.up/4, transform.position + transform.forward/25 - transform.right/5 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/5 + transform.up/4, transform.position - transform.forward/25 + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/25 + transform.up/4, transform.position + transform.forward/25 - transform.right/25 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/25 + transform.up/4, transform.position - transform.forward/25 + transform.right/25 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward right and back left
		|| Physics.Raycast(pos + transform.forward/25 + transform.right/5, Vector3.down, out hit, maxGroundedDistance2, noWaterCollisionLayers) && Physics.Raycast(pos - transform.forward/25 - transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 + transform.right/5 + transform.up/4, transform.position + transform.forward/50 + transform.right/5 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/50 + transform.right/5 + transform.up/4, transform.position - transform.forward/50 + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/5 + transform.up/4, transform.position + transform.forward/25 + transform.right/5 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/5 + transform.up/4, transform.position - transform.forward/25 - transform.right/5 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/25 + transform.up/4, transform.position + transform.forward/25 + transform.right/25 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/25 + transform.up/4, transform.position - transform.forward/25 - transform.right/25 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward and back
		|| Physics.Linecast(transform.position + transform.forward/10 + transform.up/4, transform.position + transform.forward/10 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/10 + transform.up/4, transform.position - transform.forward/10 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.up/4, transform.position + transform.forward/25 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.up/4, transform.position - transform.forward/25 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward left and back left
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/3 + transform.up/4, transform.position + transform.forward/25 - transform.right/3 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/3 + transform.up/4, transform.position - transform.forward/25 - transform.right/3 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 - transform.right/3 + transform.up/4, transform.position + transform.forward/50 - transform.right/3 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/50 - transform.right/3 + transform.up/4, transform.position - transform.forward/50 - transform.right/3 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward right and back right
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/3 + transform.up/4, transform.position + transform.forward/25 + transform.right/3 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/3 + transform.up/4, transform.position - transform.forward/25 + transform.right/3 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 + transform.right/3 + transform.up/4, transform.position + transform.forward/50 + transform.right/3 - transform.up*maxGroundedDistance2, out hit, noWaterCollisionLayers) && Physics.Linecast(transform.position - transform.forward/50 + transform.right/3 + transform.up/4, transform.position - transform.forward/50 + transform.right/3 - transform.up*maxGroundedDistance2, out hit2, noWaterCollisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)){
			
			if (((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit && ((Mathf.Acos(Mathf.Clamp(hit2.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit){
			
				inBetweenSlidableSurfaces = true;
				uphill = false;
				
			}
			else {
				inBetweenSlidableSurfaces = false;
			}
			
		}
		else if (!inMidAirFromJump){
			inBetweenSlidableSurfaces = false;
		}
		
	}

	void Jump () {
		canCrouch = false;
		crouching = false;
		jumpPerformed = true;
		if (currentJumpNumber == totalJumpNumber || timeLimitBetweenJumps2 <= 0 || jumping.doNotIncreaseJumpNumberWhenSliding && sliding){
			currentJumpNumber = 0;
		}
		currentJumpNumber++;
		if (animator != null && animator.runtimeAnimatorController != null && outOfWaterTimer > 0){
			animator.CrossFade("Jump", 0f, -1, 0f);
		}
		jumpTimer = 0.0f;
		moveDirection.y = jumpsToPerform[currentJumpNumber - 1];
		inMidAirFromJump = true;
		jumpPressed = false;
		return;
		
	}
	
	void DoubleJump () {
        inMidAirFromJump = true;
        if (inMidAirFromWallJump){
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            inMidAirFromWallJump = false;
        }
		
        moveDirection.y = doubleJumpHeight2;
        if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
            GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
        }
        if (GetComponent<Rigidbody>()){
            GetComponent<Rigidbody>().velocity = moveDirection;
        }
   
        if (animator != null && animator.runtimeAnimatorController != null){
            animator.CrossFade("DoubleJump", 0f, -1, 0f);
        }
        if (doubleJumpEffect2 != null){
            Instantiate(doubleJumpEffect2, transform.position + new Vector3(0, 0.2f, 0), doubleJumpEffect2.transform.rotation);
        }
        return;
    }
	
	void WallJump () {
		canCrouch = false;
		crouching = false;
		inMidAirFromJump = true;
		moveDirection = wallJumpDirection;
		return;
	}
	
	void Attack () {
		if (!crouching){
			if ((currentAttackNumber == totalAttackNumber || attackState == 0 && comboTimeLimitGround <= 0 || attackState == 1 && comboTimeLimitAir <= 0)
			//ground attacks
			&& (attackState == 0 && (!rememberAttackButtonPressesGround || rememberAttackButtonPressesGround && (attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime || !attacking.ground.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerGround > attacking.ground.waitingBeforeAttackingAgain.waitingTime))
			//air attacks
			|| attackState == 1 && (!rememberAttackButtonPressesAir || rememberAttackButtonPressesAir && (attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime || !attacking.air.waitingBeforeAttackingAgain.waitForAnimationToFinish && waitingBetweenAttacksTimerAir > attacking.air.waitingBeforeAttackingAgain.waitingTime)))){
				attackPressesRemembered = 0;
				currentAttackNumber = 0;
			}
			if (attackPressesRemembered < totalAttackNumber){
				attackPressesRemembered++;
			}
		}
		if (attackState == 0 && !rememberAttackButtonPressesGround
		|| attackState == 1 && !rememberAttackButtonPressesAir
		|| attackState == 2){
			currentAttackNumber++;
			if (animator != null && animator.runtimeAnimatorController != null){
				animator.CrossFade("Attack", 0f, -1, 0f);
			}
			waitingBetweenAttacksTimerGround = 0;
			waitingBetweenAttacksTimerAir = 0;
			attackTimer = 0.0f;
		}
		return;
	}
	
	void PushOffWall () {
		
		//pushing off of wall after letting go
		jumpedOffClimbableObjectTimer += 0.02f;
		if (jumpedOffClimbableObjectTimer < 0.3f){
			currentlyClimbingWall = false;
			jumpedOffClimbableObjectDirection = Vector3.Slerp(jumpedOffClimbableObjectDirection, Vector3.zero, 8 * Time.deltaTime);
			if (characterController && characterController.enabled){
				characterController.Move(jumpedOffClimbableObjectDirection * Time.deltaTime);
			}
			else if (rigidBody){
				rigidBody.MovePosition(transform.position + jumpedOffClimbableObjectDirection * Time.deltaTime);
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
				if (characterController && characterController.enabled){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						characterController.Move(centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						characterController.Move(centerPoint * 13 * Time.deltaTime);
					}
				}
				else if (rigidBody){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						rigidBody.MovePosition(transform.position + centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						rigidBody.MovePosition(transform.position + centerPoint * 13 * Time.deltaTime);
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
		if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&	Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f + transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up + transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f + transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if right is blocked, go to back left
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
	
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f - transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up - transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f - transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if neither side is blocked, go directly back
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
		&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, noWaterCollisionLayers)){
			
			fifthTest = true;
			
		}
		else {
			fifthTest = false;
		}
		
		
		if (!turnBack){
			//checking to see if either side of a ledge is blocked
			if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
				if (!wallIsClimbable && !Physics.Linecast(transform.position + transform.up*0.5f, transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, out hit, noWaterCollisionLayers) && !back2 && !currentlyClimbingWall){
					if (!Physics.Linecast(transform.position + transform.forward/4 + transform.up*0.5f + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*1.5f - spaceBelowNeededToGrabBackOnHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && !Physics.Linecast(transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, transform.position + transform.forward/1.5f - transform.up*1.5f + spaceInFrontNeededToGrabBackOn2 - spaceBelowNeededToGrabBackOnHeight2, out hit, noWaterCollisionLayers)){
						
						//if left is blocked, go to back right
						if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&	Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f + transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up + transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f + transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
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
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
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
						else if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
					
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f - transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up - transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f - transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
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
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
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
						else if ((Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
						&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, noWaterCollisionLayers)){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, noWaterCollisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
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
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2){
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
							if (characterController && characterController.enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (rigidBody){
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
							if (characterController && characterController.enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (rigidBody){
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
				swimDirection = Vector3.zero;
				swimmingMovementSpeed = 0;
				swimmingMovementTransferred = true;
				moveDirection = Vector3.zero;
				moveSpeed = 0;
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
		if (characterController && characterController.enabled){
			
			//center
			colCenter = characterController.bounds.center;
			//top
			colTop = characterController.bounds.center + (characterController.height*0.5f*transform.up);
			
		}
		else if (rigidBody){
			
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
			if (!Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers)
			||
			Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2){
				
				//bottom of climbable object has been reached
				reachedBottomPoint = true;
				
				//dropping off at bottom
				if (currentlyClimbingWall && dropOffAtBottom2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") < 0){
					moveDirection = Vector3.zero;
					swimDirection = Vector3.zero;
					swimmingMovementSpeed = 0;
					swimmingMovementTransferred = true;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					inMidAirFromJump = false;
					inMidAirFromWallJump = false;
					currentJumpNumber = totalJumpNumber;
					moveSpeed = 0;
					if (animator != null){
						animator.CrossFade("Movement", 0f, -1, 0f);
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
			if (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers)
			||
			Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2){
				aboveTop = true;
			}
			else {
				aboveTop = false;
			}
			
			//checking to see if player has reached the top of the ladder
			if ((aboveTop
			||
			//if top and left is blocked
			((!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2)
			||
			//if top and right is blocked
			(!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2)))
			||
			//if top is blocked
			Physics.Linecast(colCenter, colTop + transform.up/6, out hit, noWaterCollisionLayers)){
				
				//top of climbable object has been reached
				reachedTopPoint = true;
				
				//checking to see if surface can be pulled up to
				if (aboveTop){
					if (currentlyClimbingWall && pullUpAtTop2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") > 0
					&& Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, noWaterCollisionLayers)){
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
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top right to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the right side
			|| aboveTop){
				//if we have room to move to the right, we have not reached the farthest right point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to right more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the right to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers)&& hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)){
					
					//right edge of climbable object has not been reached
					thirdTest = true;
					reachedRightPoint = false;
					
				}
				//checking to see if player has reached the right edge (first: if not touching anything to right; next: if touching something to right, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2){
					
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
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top left to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the left side
			|| aboveTop){
				//if we have room to move to the left, we have not reached the farthest left point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to left more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the left to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, noWaterCollisionLayers) && hit.transform.tag == climbableTag2)){
					
					//left edge of climbable object has not been reached
					fourthTest = true;
					reachedLeftPoint = false;
					
				}
				//checking to see if player has reached the left edge (first: if not touching anything to left; next: if touching something to left, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, noWaterCollisionLayers) && hit.transform.tag != climbableTag2){
					
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
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				//to the left of player
				else if ((Input.GetAxis("Horizontal") < 0 || transform.rotation != lastRot2) && (wallIsClimbable)){
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				
				//in front of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the right
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the left
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				
				//inward turn, front and to the right of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				
				//inward turn, front and to the left of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, out hit, noWaterCollisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
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
			if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward + transform.up, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward + transform.up*1.1f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward + transform.up*1.2f, out hit, noWaterCollisionLayers)){
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
				if (currentlyClimbingWall && climbingMovement > 0 && (Input.GetAxisRaw("Horizontal") > 0 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < 0 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Vertical") < 0) || pullingUp){
					
					//getting distance from the wall we are colliding with
					if (transform.position == lastPos){
						
						if (noCollisionTimer < 5 ){
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, noWaterCollisionLayers)){
								distFromWallWhenStuck = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
							}
							if (!hasNotMovedOnWallYet && (Input.GetAxisRaw("Horizontal") > 0.1f && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < -0.1f && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)
							|| Input.GetAxisRaw("Vertical") > 0.1f || Input.GetAxisRaw("Vertical") < -0.1f)){
								stuckInSamePos = true;
							}
						}
					}
					if (transform.rotation != lastRot2 || Mathf.Abs(transform.position.y - lastPos.y) > 0.001f || stuckInSamePosNoCol && noCollisionTimer < 2){
						stuckInSamePos = false;
						stuckInSamePosNoCol = false;
					}
					
					if (characterController && characterController.enabled){
						
						//if player is stuck
						if (!pullingUp && stuckInSamePos){
							
							//move the player slightly back to avoid collision
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, noWaterCollisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, noWaterCollisionLayers)){
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
	
	void WallScriptEnablingDisabling () {
		
		//enabling and disabling scripts while player is on wall
		if (currentlyClimbingWall || turnBack || back2 || pullingUp){
			//if scripts have not been disabled/enabled yet
			if (!onWallScriptsFinished){
				if (wallScriptsToDisableOnGrab != null){
					foreach (string script in wallScriptsToDisableOnGrab)
					{
						wallScriptToDisable = GetComponent(script) as MonoBehaviour;
						if (wallScriptToDisable != null){
							wallScriptToDisable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingWallScripts){
							wallScriptWarning = true;
						}
					}
				}
				if (wallScriptsToEnableOnGrab != null){
					foreach (string script in wallScriptsToEnableOnGrab)
					{
						wallScriptToEnable = GetComponent(script) as MonoBehaviour;
						if (wallScriptToEnable != null){
							wallScriptToEnable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingWallScripts){
							wallScriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingWallScripts = true;
			}
			onWallScriptsFinished = true;
			
		}
		//undoing enabling and disabling scripts when player lets go of wall
		else {
			//if scripts have not been un-disabled/enabled yet
			if (onWallScriptsFinished){
				if (wallScriptsToDisableOnGrab != null){
					foreach (string script in wallScriptsToDisableOnGrab)
					{
						wallScriptToDisable = GetComponent(script) as MonoBehaviour;
						if (wallScriptToDisable != null){
							wallScriptToDisable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingWallScripts || currentlyClimbingWall || turnBack || back2){
							wallScriptWarning = true;
						}
					}
				}
				if (wallScriptsToEnableOnGrab != null){
					foreach (string script in wallScriptsToEnableOnGrab)
					{
						wallScriptToEnable = GetComponent(script) as MonoBehaviour;
						if (wallScriptToEnable != null){
							wallScriptToEnable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingWallScripts || currentlyClimbingWall || turnBack || back2){
							wallScriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingWallScripts = true;
			}
			onWallScriptsFinished = false;
			
		}
		
		//all loops that enable or disable scripts have finished, so we set currentlyEnablingAndDisablingWallScripts to false
		if (!currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			currentlyEnablingAndDisablingWallScripts = false;
		}
		//warns the user if any script names they entered do not exist on the player
		if (wallScriptWarning){
			if (wallScriptsToDisableOnGrab != null){
				foreach (string script in wallScriptsToDisableOnGrab)
				{
					wallScriptToDisable = GetComponent(script) as MonoBehaviour;
					if (wallScriptToDisable == null){
						Debug.Log("<color=red>The script to disable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			if (wallScriptsToEnableOnGrab != null){
				foreach (string script in wallScriptsToEnableOnGrab)
				{
					wallScriptToEnable = GetComponent(script) as MonoBehaviour;
					if (wallScriptToEnable == null){
						Debug.Log("<color=red>The script to enable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			wallScriptWarning = false;
		}
		
	}
	
	void SwimScriptEnablingDisabling () {
		
		//enabling and disabling scripts while player is on wall
		if (inWater){
			//if scripts have not been disabled/enabled yet
			if (!swimScriptsFinished){
				if (swimScriptsToDisableOnEnter != null){
					foreach (string script in swimScriptsToDisableOnEnter)
					{
						swimScriptToDisable = GetComponent(script) as MonoBehaviour;
						if (swimScriptToDisable != null){
							swimScriptToDisable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingSwimScripts){
							swimScriptWarning = true;
						}
					}
				}
				if (swimScriptsToEnableOnEnter != null){
					foreach (string script in swimScriptsToEnableOnEnter)
					{
						swimScriptToEnable = GetComponent(script) as MonoBehaviour;
						if (swimScriptToEnable != null){
							swimScriptToEnable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingSwimScripts){
							swimScriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingSwimScripts = true;
			}
			swimScriptsFinished = true;
			
		}
		//undoing enabling and disabling scripts when player lets go of wall
		else {
			//if scripts have not been un-disabled/enabled yet
			if (swimScriptsFinished){
				if (swimScriptsToDisableOnEnter != null){
					foreach (string script in swimScriptsToDisableOnEnter)
					{
						swimScriptToDisable = GetComponent(script) as MonoBehaviour;
						if (swimScriptToDisable != null){
							swimScriptToDisable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingSwimScripts || inWater){
							swimScriptWarning = true;
						}
					}
				}
				if (swimScriptsToEnableOnEnter != null){
					foreach (string script in swimScriptsToEnableOnEnter)
					{
						swimScriptToEnable = GetComponent(script) as MonoBehaviour;
						if (swimScriptToEnable != null){
							swimScriptToEnable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingSwimScripts || inWater){
							swimScriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingSwimScripts = true;
			}
			swimScriptsFinished = false;
			
		}
		
		//all loops that enable or disable scripts have finished, so we set currentlyEnablingAndDisablingSwimScripts to false
		if (!inWater){
			currentlyEnablingAndDisablingSwimScripts = false;
		}
		//warns the user if any script names they entered do not exist on the player
		if (swimScriptWarning){
			if (swimScriptsToDisableOnEnter != null){
				foreach (string script in swimScriptsToDisableOnEnter)
				{
					swimScriptToDisable = GetComponent(script) as MonoBehaviour;
					if (swimScriptToDisable == null){
						Debug.Log("<color=red>The script to disable on enter named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			if (swimScriptsToEnableOnEnter != null){
				foreach (string script in swimScriptsToEnableOnEnter)
				{
					swimScriptToEnable = GetComponent(script) as MonoBehaviour;
					if (swimScriptToEnable == null){
						Debug.Log("<color=red>The script to enable on enter named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			swimScriptWarning = false;
		}
		
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		
		contactPoint = hit.point;
		collisionSlopeAngle = Vector3.Angle(Vector3.up, hit.normal);
		noCollisionTimer = 0;
		
		//checking to see if the player is colliding with a wall (and not just the floor)
		if (hit.point.y > colliderBottom.y + 0.5f){
			collidingWithWall = true;
		}
		else {
			collidingWithWall = false;
		}
		
		//determining slope angles
		slidingAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slidingAngle >= slopeLimit) {
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
            if (slidingAngle > slopeLimit){
				slidingAngle = slopeLimit;
			}
            if (slidingAngle < slopeLimit){
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
			if (!currentlyClimbingWall && rigidBody){
				rigidBody.velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			climbDirection = Vector3.zero;
			swimDirection = Vector3.zero;
			swimmingMovementSpeed = 0;
			swimmingMovementTransferred = true;
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
		}
		
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		
		foreach (ContactPoint contact in hit.contacts) {
			contactPoint = contact.point;
			collisionSlopeAngle = Vector3.Angle(Vector3.up, contact.normal);
			noCollisionTimer = 0;
			
			//checking to see if the player is colliding with a wall (and not just the floor)
			if (contactPoint.y > colliderBottom.y + 0.5f){
				collidingWithWall = true;
			}
			else {
				collidingWithWall = false;
			}
			
			//determining slope angles
			slidingAngle = Vector3.Angle(contact.normal, Vector3.up);
			if (slidingAngle >= slopeLimit) {
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
				if (slidingAngle > slopeLimit){
					slidingAngle = slopeLimit;
				}
				if (slidingAngle < slopeLimit){
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
			if (!currentlyClimbingWall && rigidBody){
				rigidBody.velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			climbDirection = Vector3.zero;
			swimDirection = Vector3.zero;
			swimmingMovementSpeed = 0;
			swimmingMovementTransferred = true;
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
		}
		
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
		//checking if player is colliding with water
		if (hit.transform.gameObject.layer == 4 && swimming.allowSwimming){
			noCollisionWithWaterTimer = 0;
			noCollisionWithWaterSplashTimer = 0;
		}
		
	}
	
	void OnDisable() {
		
		//resetting values
		
		if (rigidBody){
			if (!characterController || characterController && !characterController.enabled){
				rigidBody.velocity = Vector3.zero;
			}
		}
		inMidAirFromJump = false;
		inMidAirFromWallJump = false;
        currentJumpNumber = totalJumpNumber;
		moveDirection.y = 0;
		moveSpeed = 0;
		jumpPressed = false;
		jumpPossible = false;
		jumpEnabled = false;
		doubleJumpPossible = true;
		middleWallJumpable = false;
		leftWallJumpable = false;
		rightWallJumpable = false;
		currentlyOnWall = false;
		currentlyClimbingWall = false;
		turnBack = false;
		back2 = false;
		canCrouch = false;
		crouching = false;
		swimDirection = Vector3.zero;
		swimmingMovementSpeed = 0;
		swimmingMovementTransferred = true;
		submergedEnough = false;
		inWater = false;
		enabledLastUpdate = false;
		slidingVector = Vector3.zero;
		slideMovement = Vector3.zero;
		
    }
	
	void OnEnable () {
		
		//resetting values
		swimDirection = Vector3.zero;
		swimmingMovementSpeed = 0;
		swimmingMovementTransferred = true;
		noCollisionWithWaterTimer = 5;
		noCollisionWithWaterSplashTimer = 5;
		submergedEnough = false;
		inWater = false;
		jumpPressed = false;
		
	}
	
}