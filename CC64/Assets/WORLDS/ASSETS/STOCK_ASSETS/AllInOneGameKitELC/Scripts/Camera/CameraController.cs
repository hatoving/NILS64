/// All in One Game Kit - Easy Ledge Climb Character System
/// CameraController.cs
///
/// This script allows the camera to:
/// 1. Follow the player at a set speed, height, and distance.
/// 2. Follow the player as a side-scroller.
/// 3. Lock on behind the player.
/// 4. Lock on to objects.
/// 5. Mouse Orbit the player.
/// 6. Enter First Person mode.
/// 7. Mouse Orbit in First Person mode.
///
/// NOTE: *You should always set a layer for your player so that you can disable collisions with that layer (by unchecking it in the script's Collision Layers).
///	If you do not, the camera will collide with the player himself!*
///
/// (C) 2015-2018 Grant Marrs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    public bool CursorInvisible = true; 
    public Transform player; //the player set for the camera to follow
    public float cameraDistance = 2.95f; //the distance between the camera and the player
    public float offsetFromWall = 0.1f; //the distance between the camera and the wall if they are colliding

    public bool NewTriggered;
    public Transform NewCameraTarget;
    public float NewcameraDistance = 2.95f;
    public float NewcameraHeight = 1f;
    public float NewplayerHeight;
    public float NewSpeed = 0.1f;

    //following
    [System.Serializable]
    public class Follow
    {

        public bool alwaysFollow = true; //allows the camera to always follow the player
        public float playerHeight; //the height of the player
        public float cameraHeight = 1f; //the height of the camera
        public float movementDampening = 0.1f; //the amount of dampening applied to the movement of the camera
        public float rotationDampening = 0.5f; //the amount of dampening applied to the rotation of the camera
        public bool stayBehindPlayerWhileClimbingLedgeOrWall = true; //makes the camera stay behind the player if he is climbing a ledge or wall
        [System.Serializable]
        public class SideScrolling
        {
            public bool lockZAxis = false; //does not allow the camera to follow the player's z-axis movement
            public bool lockXAxis = false; //does not allow the camera to follow the player's x-axis movement
            public bool flipAxis = false; //flips the rotation of the non-locked axis (adds 180 degrees to the rotation)
        }
        public SideScrolling sideScrolling = new SideScrolling(); //variables that allow the camera to follow the player as a 2.5D side-scroller

    }
    public Follow follow = new Follow(); //variables that determine the way the camera locks on

    //mouse orbiting
    [System.Serializable]
    public class MouseOrbit
    {

        public bool alwaysMouseOrbit = false; //allows the camera to always orbit the player (without the use of a button), as long as the camera is not following the player
        public bool switchToMouseOrbitIfInputButtonPressed = true; //switches to mouse orbit mode and back when the "mouseOrbitInputButton" is pressed
        public string mouseOrbitInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to mouse orbit
        public bool startOffMouseOrbitingForSwitching = false; //if the camera is allowed to switch to mouse orbiting, start off mouse orbiting instead of having to switch to it first
        public float cameraHeight = 1f; //the height of the camera
        public float nearClippingPlane = 0.1f; //the nearest possible distance between the camera and the player (when the camera is pushed against a wall)
        public bool allowZoomingWithScrollWheel = false; //allows the user to zoom in between the "distance" and "minZoomInDistance" values (by using the mouse's scroll wheel)
        public int zoomSpeed = 40; //the speed the camera zooms in and out when using the scroll wheel
        public float minZoomInDistance = 0.5f; //the nearest possible distance to the player that the camera can zoom in to
        public float xSpeed = 200; //the camera's orbit speed (look sensitivity) on the x-axis
        public float ySpeed = 120; //the camera's orbit speed (look sensitivity) on the y-axis
        public int yMinLimit = -68; //the minimum y value the camera can orbit to
        public int yMaxLimit = 68; //the maximum y value the camera can orbit to
        public float rotationDampening = 3.0f; //the amount of dampening applied to the rotation of the camera
        public float zoomDampening = 5.0f; //the amount of dampening applied to the zooming of the camera (this zoom is applied when the camera collides with a wall, and zooms in or out to avoid it)

    }
    public MouseOrbit mouseOrbit = new MouseOrbit(); //variables that determine the way the camera orbits the player

    //first person
    [System.Serializable]
    public class FirstPerson
    {

        public bool alwaysUseFirstPerson = false; //allows the camera to always stay in first person mode (as long as the camera is not following or mouse orbiting)
        public bool switchToFirstPersonIfInputButtonPressed = false; //switches to first person mode and back when the "firstPersonInputButton" is pressed
        public string firstPersonInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to enter first person mode
        public bool startOffInFirstPersonModeForSwitching = false; //if the camera is allowed to switch to first person mode, start off in first person mode instead of having to switch to it first
        public float cameraDistance = 0.0f; //the distance of the camera from the player
        public float cameraHeight = 1.15f; //the height of the camera
        [System.Serializable]
        public class Crouching
        {
            public bool allowCrouching = true; //determines if the camera can crouch (lower down)
            public bool crouchWithPlayerIfPossible = true; //allows the camera to crouch (lower down) if the player has the script, "PlayerController.cs," and is crouching
            public float crouchCameraHeightMultiple = 0.7f; //what to multiply the height of the camera by while crouching
            public float crouchSpeed = 8; //the speed at which the camera crouches down and uncrouches
        }
        public Crouching crouching = new Crouching(); //variables that determine if the camera can crouch (lower down) with the player while in first person mode
        [System.Serializable]
        public class MouseOrbiting
        {
            public bool mouseOrbitInFirstPersonMode = true; //allows the camera to mouse orbit while in first person mode
            public float xSpeed = 200; //the camera's orbit speed (look sensitivity) on the x-axis
            public float ySpeed = 120; //the camera's orbit speed (look sensitivity) on the y-axis
            public int yMinLimit = -68; //the minimum y value the camera can orbit to
            public int yMaxLimit = 68; //the maximum y value the camera can orbit to
        }
        public MouseOrbiting mouseOrbiting = new MouseOrbiting(); //variables that determine if the camera can mouse orbit while in first person mode
        public GameObject[] objectsToEnableInFirstPerson; //gameObjects to enable when the camera is in first person mode (gameObjects disable when the camera exits first person mode)
        public GameObject[] objectsToDisableInFirstPerson; //gameObjects to disable when the camera is in first person mode (gameObjects enable when the camera exits first person mode)
    }
    public FirstPerson firstPerson = new FirstPerson(); //variables that determine if the player can enter first person mode

    //locking on
    [System.Serializable]
    public class LockingOn
    {
        public bool allowLockingOn = true; //determines whether or not to allow locking on
        public string lockOnInputButton = "Fire2"; //the button or axis (found in "Edit > Project Settings > Input") pressed to lock on
        public Sprite barSprite; //the sprite that appears on the top and bottom portion of the screen while locking on
        public float barCoverage = 28f; //the percentage of the screen that the bar sprite covers
        public float lockOnSpeed = 2f; //the speed at which the camera locks on behind the player
        [System.Serializable]
        public class LockingOnToTags
        {
            public List<string> tagsToLockOnTo = new List<string>() { "Enemy" }; //the tags of the objects that can be locked on to
            public float lockInDistance = 6.0f; //how close you have to be to the tagged object to lock on
            public float lockOutDistance = 10.0f; //how far you have to be from the tagged object (if you are already locked on to it) to lock out
            public GameObject arrowPointer; //the object (arrow) that appears over the tagged objects head when you are able to lock on to him
            public float arrowPointerHeight = 0.0f; //how high the arrow pointer is above the tagged objects head
            public float arrowPointerBounceHeight = 0.3f; //how high the arrow pointer bounces
            public float arrowPointerBounceSpeed = 4; //the speed at which the arrow pointer bounces
            public bool targetSameObjectWhileLockedOn = false; //stays locked on to the same target, even if another target moves closer
        }
        public LockingOnToTags lockingOnToTags = new LockingOnToTags(); //variables that determine which objects to lock on to
    }
    public LockingOn lockingOn = new LockingOn(); //variables that determine how the camera locks on

    //mouse orbit variables
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;
    private Vector3 position;
    private Quaternion rotation;
    private bool camTooRotated;
    private float playerColliderHeight;
    private float mouseOrbitMaxDistance;
    private float mouseOrbitMinDistance;
    private bool mouseOrbitButtonPressed;
    private bool mouseOrbitStart;

    //first person mode variables
    private float firstPersonMaxDistance;
    private float firstPersonMinDistance;
    private Vector3 fpPosition;
    private Vector3 fpRotation;
    private Quaternion fpOrbitRotation;
    private float xFpDeg;
    private float yFpDeg;
    [HideInInspector]
    public float yFpDeg2;
    private float oldXFpDeg;
    private Vector3 oldEuler;
    [HideInInspector]
    public bool goneThroughFirstPersonUpdate;
    private bool firstPersonButtonPressed;
    private bool firstPersonStart;
    [HideInInspector]
    public bool inFirstPersonMode;
    private bool turningToClimb;
    private float firstPersonCameraHeight;
    private bool atFPPoint;
    private Vector3 swimmingQuat;
    private Vector3 finalSwimQuat;
    private Quaternion lookAtPos;
    private float outOfWaterSlerpValue = 20.0f;
    private float outOfFPInWaterTimer = 5.0f;
    private bool firstPersonModeWasTrueLastUpdate;
    //crouching
    private bool canCrouch;
    private bool crouching;
    private bool finishedCrouching;
    private float oldYPos;
    private float newYPos;
    //enabling and disabling script variables
    private bool currentlyEnablingAndDisablingObjects = false;
    private bool objectWarning = false;
    private bool objectsFinished = false;

    //camera following variables
    private bool following;
    private Vector3 playerOffset;
    private Vector3 playerPos;
    private float playerYDeg;
    private float playerYDeg2;
    private float playerYRot;
    private float cameraYRot;
    private Vector3 velocityCamSmooth;
    private bool playerClimbing;
    private Vector3 playerForward;
    private Quaternion lastPlayerRot;
    private Quaternion rotationNormal;

    //camera locking on variables
    private bool lockedOn;
    private float barHeight;
    private GameObject barHolder;
    private GameObject topBar;
    private GameObject bottomBar;
    private float halfScreenHeight;
    private List<GameObject> targetableObjects = new List<GameObject>();
    private GameObject closestTarget;
    private float dist;
    private float curDistance;
    private float distanceFromClosestTarget;
    private bool lockedOnToTarget;
    private bool canLockOn;
    private bool notBlocked;
    private float feetPosition;
    private float headPosition;
    private float height;
    private GameObject pointer;
    private float arrowBounciness;
    private float arrowPointerBounceHeightHalf;
    private float arrowPointerBounceHeight2;

    //camera rotation variables
    private Vector3 lookAt;
    private Vector3 lookDir;
    private Vector3 velocityLookDir;
    private Vector3 vector2;
    private Vector3 landLookAt;
    private bool playerEnteredWaterOnceAlready = false;

    //camera frustrum variables
    private Vector3[] viewFrustum;
    private Vector3 nearClipDimensions;
    private const int frustrumBottomLeftPoint = 0;
    private const int frustrumTopLeftPoint = 1;
    private const int frustrumTopRightPoint = 2;
    private const int frustrumBottomRightPoint = 3;
    private const int frustrumBottomLeftVec = 4;
    private const int frustrumTopLeftVec = 5;
    private const int frustrumTopRightVec = 6;
    private const int frustrumBottomRightVec = 7;
    private const int frustrumSize = 8;

    private GameObject canvas; //the parent of the UI

    public LayerMask collisionLayers = ~((1 << 2) | (1 << 4)); //the layers that the detectors (raycasts/linecasts) will collide with

    public Transform Newplayertarget { get; set; }

    void Start()
    {
if (CursorInvisible == true) { 
        Cursor.visible = false;
        }

        //following player
        following = true;
        lockedOn = false;
        lookDir = player.transform.forward;
        playerOffset = player.transform.position + follow.cameraHeight * player.transform.up;

        //mouse orbit
        mouseOrbitMaxDistance = cameraDistance + (0.73f * (1 + (1.5f / cameraDistance)) / 1.8577f) / Vector3.Distance(transform.position, player.transform.position);
        mouseOrbitMinDistance = mouseOrbit.minZoomInDistance;
        desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
        correctedDistance = desiredDistance;
        currentDistance = desiredDistance;
        xDeg = transform.eulerAngles.y;
        yDeg = transform.eulerAngles.x;
        rotation = transform.rotation;
        position = transform.position;
        //starting off mouse orbiting
        if (mouseOrbit.switchToMouseOrbitIfInputButtonPressed && mouseOrbit.startOffMouseOrbitingForSwitching)
        {
            mouseOrbitStart = true;
            mouseOrbitButtonPressed = true;
        }

        //first person mode
        xFpDeg = transform.eulerAngles.y;
        if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
        {
            yFpDeg = transform.eulerAngles.x;
        }
        else
        {
            yFpDeg = transform.eulerAngles.x - player.GetComponent<PlayerController>().swimming.firstPerson.cameraAngleOffset - 90;
        }
        yFpDeg2 = yFpDeg;
        //starting off in first person mode
        if (firstPerson.switchToFirstPersonIfInputButtonPressed && firstPerson.startOffInFirstPersonModeForSwitching && !mouseOrbit.startOffMouseOrbitingForSwitching)
        {
            firstPersonStart = true;
            firstPersonButtonPressed = true;
        }
        oldXFpDeg = transform.eulerAngles.y;

        //getting first person camera height
        firstPersonCameraHeight = firstPerson.cameraHeight;

        //getting the canvas to hold the UI
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas");
        }

    }




    void Update()
    {

        if (Input.GetKeyDown("z"))
        {
            NewTriggered = true;
        }

        if (NewTriggered == true)
        {
        if (cameraDistance < NewcameraDistance)
               {
                cameraDistance += NewSpeed;
                }
            if (cameraDistance > NewcameraDistance)
            {
                cameraDistance -= NewSpeed;
            }

        }

        if (NewTriggered == true)
        {
            if (follow.cameraHeight < NewcameraHeight)
            {
                follow.cameraHeight += NewSpeed;
            }
            if (follow.cameraHeight > NewcameraHeight)
            {
                follow.cameraHeight -= NewSpeed;
            }

        }

        if (NewTriggered == true)
        {
            if (follow.playerHeight < NewplayerHeight)
            {
                follow.playerHeight += NewSpeed;
            }
            if (follow.playerHeight > NewplayerHeight)
            {
                follow.playerHeight -= NewSpeed;
            }

            if (NewTriggered == true)
            {
                player = NewCameraTarget;
            }

        }
        {

  }
           

        //getting the canvas to hold the UI
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas");
        }

        //creating black bar holder and black bars for locking on
        if (barHolder == null)
        {
            barHolder = new GameObject();
            barHolder.AddComponent<RectTransform>();
            barHolder.gameObject.name = "BarHolder";
            barHolder.gameObject.layer = 5;
        }
        else
        {

            barHolder.transform.SetParent(canvas.transform);
            barHolder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            barHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            barHolder.transform.SetAsFirstSibling();

        }

        //crouching
        RaycastHit hit = new RaycastHit();
        if (firstPerson.crouching.allowCrouching && (!player.GetComponent<PlayerController>() || !firstPerson.crouching.crouchWithPlayerIfPossible))
        {
            //when the crouching button is pressed, determine if the player can crouch (or if he will be able to crouch after landing on the ground)
            if (!canCrouch && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && finishedCrouching)
            {
                finishedCrouching = false;
                canCrouch = true;
            }
            //when the crouching button is pressed and the player is already crouched: uncrouch (if there is enough space above the player's head)
            else if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && !Physics.Linecast(transform.position, new Vector3(transform.position.x, newYPos, transform.position.z), out hit, collisionLayers) && finishedCrouching)
            {
                finishedCrouching = false;
                crouching = false;
                canCrouch = false;
            }
            //if the player is grounded and canCrouch is true, crouch
            if (canCrouch && !crouching)
            {
                crouching = true;
            }
        }
        else
        {
            canCrouch = false;
            crouching = false;
        }
        finishedCrouching = true;

    }


        void FixedUpdate()
    {

        //getting the canvas to hold the UI
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas");
        }

        //getting camera height and distance from player
        CamHeightDist();

        //getting locking on values
        LockOnValues();

        //following
        FollowingState();

        //creating locking on arrow pointer and bars
        LockOnUI();

        //if the mouseOrbitInputButton has been pressed and we are starting to mouse orbit, set mouseOrbitButtonPressed to true
        if (mouseOrbit.switchToMouseOrbitIfInputButtonPressed)
        {
            if (mouseOrbitStart)
            {
                mouseOrbitButtonPressed = true;
            }
            else
            {
                mouseOrbitButtonPressed = false;
            }
        }
        else
        {
            mouseOrbitButtonPressed = false;
        }

        //if the firstPersonInputButton has been pressed and we are entering first person mode, set firstPersonButtonPressed to true
        if (firstPerson.switchToFirstPersonIfInputButtonPressed)
        {
            if (firstPersonStart)
            {
                firstPersonButtonPressed = true;
            }
            else
            {
                firstPersonButtonPressed = false;
            }
        }
        else
        {
            firstPersonButtonPressed = false;
        }

        //first person mode
        FirstPersonState();

        //following and locking on
        FollowingPlayerAndLockingOn();

        //mouse orbiting
        MouseOrbitMode();

        //camera in water fp mode
        if (inFirstPersonMode)
        {
            swimmingQuat = transform.localEulerAngles;
        }
        //getting position to look at
        if (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
        {
            Quaternion lookAtPos = Quaternion.LookRotation((lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f)) - playerPos);
            finalSwimQuat = lookAtPos.eulerAngles;
        }
        else
        {
            Quaternion lookAtPos = Quaternion.LookRotation((player.transform.position + new Vector3(0f, follow.playerHeight + 0.5f, 0f)) - playerPos);
            finalSwimQuat = lookAtPos.eulerAngles;
        }
        //keeping values below 180
        if (swimmingQuat.x >= 180)
        {
            swimmingQuat = new Vector3(360 - swimmingQuat.x, swimmingQuat.y, swimmingQuat.z);
        }
        //
        if (finalSwimQuat.x >= 180)
        {
            finalSwimQuat = new Vector3(360 - finalSwimQuat.x, finalSwimQuat.y, finalSwimQuat.z);
        }

        //resetting camera position if player respawns
        if (player.GetComponent<Health>().currentHealth <= 0)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
            lookDir = player.transform.forward;
            //if we are not in first person mode, lock on to target and follow player accordingly
            if (!inFirstPersonMode)
            {
                playerPos = playerOffset + player.transform.up * follow.cameraHeight - lookDir * cameraDistance;
                lookAt = player.transform.position;
            }
            //if we are in first person mode, only lock on to target
            else
            {
                //calculating camera position
                Vector3 vTargetOffset;
                vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                playerPos = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
                transform.position = playerPos;
                transform.eulerAngles = player.transform.eulerAngles;
            }
            transform.position = playerPos;
            transform.LookAt(lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));

            //setting mouse orbit values
            desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
            correctedDistance = desiredDistance;
            currentDistance = desiredDistance;
            xDeg = transform.eulerAngles.y;
            yDeg = transform.eulerAngles.x;
            rotation = transform.rotation;
            position = transform.position;
        }

    }

    void CamHeightDist()
    {

        //getting first person camera height
        //if crouching is allowed
        if (firstPerson.crouching.allowCrouching)
        {
            //crouching based on what the player controller is doing
            if (player.GetComponent<PlayerController>() && firstPerson.crouching.crouchWithPlayerIfPossible)
            {
                //camera height while not crouching
                if (!player.GetComponent<PlayerController>().crouching)
                {
                    firstPersonCameraHeight = Mathf.Lerp(firstPersonCameraHeight, firstPerson.cameraHeight, firstPerson.crouching.crouchSpeed * Time.deltaTime);
                }
                //camera height while crouching
                else
                {
                    firstPersonCameraHeight = Mathf.Lerp(firstPersonCameraHeight, firstPerson.cameraHeight * firstPerson.crouching.crouchCameraHeightMultiple, firstPerson.crouching.crouchSpeed * Time.deltaTime);
                }
            }
            //crouching based on the camera alone
            else
            {
                //camera height while not crouching
                if (!crouching)
                {
                    firstPersonCameraHeight = Mathf.Lerp(firstPersonCameraHeight, firstPerson.cameraHeight, firstPerson.crouching.crouchSpeed * Time.deltaTime);
                }
                //camera height while crouching
                else
                {
                    firstPersonCameraHeight = Mathf.Lerp(firstPersonCameraHeight, firstPerson.cameraHeight * firstPerson.crouching.crouchCameraHeightMultiple, firstPerson.crouching.crouchSpeed * Time.deltaTime);
                }
            }
        }
        //if crouching is not allowed
        else
        {
            firstPersonCameraHeight = firstPerson.cameraHeight;
        }

        if (crouching)
        {
            newYPos = oldYPos + (transform.position.y - oldYPos);
        }
        else
        {
            oldYPos = transform.position.y;
        }

        //adjusting distance from player relative to height
        float valueOne;
        float valueTwo;
        if (follow.cameraHeight <= 1)
        {
            valueOne = 0.4f;
            valueTwo = 0.6f;
        }
        else
        {
            valueOne = 0.65f;
            valueTwo = 0.35f;
        }
        //mouse orbit distances
        if (cameraDistance >= 0.6f)
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (1 + (1.5f / cameraDistance)) / 1.8577f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        else if (cameraDistance >= 0.4f)
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (1 + (cameraDistance)) / 0.84f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        else
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (20 + (cameraDistance)) / 10.9338f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        mouseOrbitMinDistance = mouseOrbit.minZoomInDistance;

        //calculating view frustum
        viewFrustum = CameraController.CalculateViewFrustum(GetComponent<Camera>(), ref nearClipDimensions);

        //determining if the camera is too rotated to follow the player
        if (transform.eulerAngles.x >= 90 && transform.eulerAngles.x < 180)
        {
            camTooRotated = true;
        }
        else if (transform.eulerAngles.x <= 60 || transform.eulerAngles.x <= 75 && (!player.GetComponent<PlayerController>().swimming.invertYRotation && Input.GetAxis("Vertical") <= 0 || player.GetComponent<PlayerController>().swimming.invertYRotation && Input.GetAxis("Vertical") >= 0))
        {
            camTooRotated = false;
        }

        //getting playerOffset if player is not swimming
        if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
        {
            playerOffset = player.transform.position + follow.cameraHeight * player.transform.up;
        }
        //getting playerOffset if player is swimming
        else
        {
            //if the camera is not too rotated, it can follow the player regularly
            if (!camTooRotated && !follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
            {

                //if neither axis is locked
                playerOffset = player.transform.position + follow.cameraHeight * -player.transform.forward;

            }
            //if the camera is too rotated, it must follow the player using his up direction and collider height (instead of his forward direction)
            else
            {

                //getting the y-position of the player's collider
                if (player.GetComponent<CharacterController>() && player.GetComponent<CharacterController>().enabled)
                {
                    playerColliderHeight = player.GetComponent<CharacterController>().center.y;
                }
                else if (player.GetComponent<Rigidbody>())
                {
                    //if player has a capsule collider
                    if (player.GetComponent<CapsuleCollider>())
                    {
                        playerColliderHeight = player.GetComponent<CapsuleCollider>().center.y;
                    }
                    //if player has a sphere collider
                    else if (player.GetComponent<SphereCollider>())
                    {
                        playerColliderHeight = player.GetComponent<SphereCollider>().center.y;
                    }
                    //if player has a box collider
                    else if (player.GetComponent<BoxCollider>())
                    {
                        playerColliderHeight = player.GetComponent<BoxCollider>().center.y;
                    }
                }

                //setting playerOffset
                //if neither axis is locked
                if (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
                {
                    playerOffset = player.transform.position + (follow.cameraHeight - playerColliderHeight) * -player.transform.up;
                }
                //if x or z-axis is locked
                else
                {
                    playerOffset = player.transform.position + (follow.cameraHeight - playerColliderHeight) * -transform.forward;
                }

            }
        }
        playerPos = Vector3.zero;
        arrowPointerBounceHeightHalf = Mathf.Abs(lockingOn.lockingOnToTags.arrowPointerBounceHeight) / 2;

    }

    void LockOnValues()
    {

        LockOnVal1();
        LockOnVal2();
        LockOnVal3();
        LockOnVal4();

    }

    void LockOnVal1()
    {

        //find closest target if any
        if (lockingOn.lockingOnToTags.tagsToLockOnTo != null)
        {

            for (int i = targetableObjects.Count - 1; i > -1; i--)
            {
                if (targetableObjects[i] != null && !lockingOn.lockingOnToTags.tagsToLockOnTo.Contains(targetableObjects[i].tag))
                {
                    targetableObjects.RemoveAt(i);
                }
            }
            if (closestTarget != null && !lockingOn.lockingOnToTags.tagsToLockOnTo.Contains(closestTarget.tag))
            {
                closestTarget = null;
            }

            foreach (string tag in lockingOn.lockingOnToTags.tagsToLockOnTo)
            {

                if (tag != null && GameObject.FindGameObjectsWithTag(tag) != null)
                {
                    foreach (GameObject tagObject in GameObject.FindGameObjectsWithTag(tag))
                    {
                        if (!targetableObjects.Contains(tagObject))
                        {
                            targetableObjects.Add(tagObject);
                        }
                    }
                }

            }
        }
        for (int i = targetableObjects.Count - 1; i > -1; i--)
        {
            if (targetableObjects[i] == null)
            {
                targetableObjects.RemoveAt(i);
            }
        }

    }

    void LockOnVal2()
    {

        float distance = Mathf.Infinity;
        // Iterate through them and find the closest target
        foreach (GameObject targetableObject in targetableObjects)
        {
            curDistance = (targetableObject.transform.position - player.transform.position).sqrMagnitude;
            if (Vector3.Distance(targetableObject.transform.position, transform.position) * 4 >= 25)
            {
                dist = Vector3.Distance(targetableObject.transform.position, transform.position) * 4;
            }

            if (curDistance < distance && (!lockedOnToTarget || lockedOnToTarget && !lockingOn.lockingOnToTags.targetSameObjectWhileLockedOn))
            {
                closestTarget = targetableObject;
                distance = curDistance;
            }
        }
        if (closestTarget != null)
        {
            distanceFromClosestTarget = Vector3.Distance(player.transform.position, closestTarget.transform.position);
        }

        if (lockedOnToTarget && distanceFromClosestTarget > lockingOn.lockingOnToTags.lockOutDistance)
        {
            canLockOn = false;
        }
        else if (!Input.GetButton(lockingOn.lockOnInputButton) || Input.GetAxis(lockingOn.lockOnInputButton) == 0 || distanceFromClosestTarget <= lockingOn.lockingOnToTags.lockInDistance)
        {
            canLockOn = true;
        }

    }

    void LockOnVal3()
    {

        //getting half of the screen height
        halfScreenHeight = canvas.GetComponent<RectTransform>().rect.height / 2;

        //locking on
        if (lockingOn.allowLockingOn && canLockOn && (Input.GetButton(lockingOn.lockOnInputButton) || Input.GetAxis(lockingOn.lockOnInputButton) != 0))
        {

            barHeight = Mathf.SmoothStep(barHeight, halfScreenHeight * (lockingOn.barCoverage / 100), lockingOn.lockOnSpeed / 4f);
            lockedOn = true;
            following = false;

        }
        //following
        else
        {

            barHeight = Mathf.SmoothStep(barHeight, 0f, lockingOn.lockOnSpeed / 4f);
            if (lockedOn && (!Input.GetButton(lockingOn.lockOnInputButton) || Input.GetAxis(lockingOn.lockOnInputButton) == 0 || !canLockOn) || !lockingOn.allowLockingOn)
            {

                following = true;
                lockedOn = false;

            }

        }

    }

    void LockOnVal4()
    {

        //locking behind player while climbing
        if (follow.stayBehindPlayerWhileClimbingLedgeOrWall && (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis))
        {
            //getting player's transform.forward when he is turning on to a wall
            if (player.GetComponent<PlayerController>() && (player.GetComponent<PlayerController>().turnBack || player.GetComponent<PlayerController>().back2))
            {
                if (player.GetComponent<PlayerController>().rotationHit != Vector3.zero)
                {
                    playerForward = new Vector3(player.GetComponent<PlayerController>().rotationHit.x, lookDir.y, player.GetComponent<PlayerController>().rotationHit.z);
                }
                //if player has finished rotation, lerp playerForward to the actual player.transform.forward
                else if (Quaternion.Angle(player.transform.rotation, lastPlayerRot) <= 3f)
                {
                    playerForward = Vector3.Lerp(playerForward, player.transform.forward, 5 * Time.deltaTime);
                }
            }
            //getting player's transform.forward when he is turning on to a ledge
            else if (player.GetComponent<LedgeClimbController>() && (player.GetComponent<LedgeClimbController>().turnBack || player.GetComponent<LedgeClimbController>().back2))
            {
                if (player.GetComponent<LedgeClimbController>().backHit != Vector3.zero)
                {
                    playerForward = new Vector3(player.GetComponent<LedgeClimbController>().backHit.x, lookDir.y, player.GetComponent<LedgeClimbController>().backHit.z);
                }
                //if player has finished rotation, lerp playerForward to the actual player.transform.forward
                else if (Quaternion.Angle(player.transform.rotation, lastPlayerRot) <= 3f)
                {
                    playerForward = Vector3.Lerp(playerForward, player.transform.forward, 5 * Time.deltaTime);
                }
            }
            //getting player's transform.forward when he is not turning on to a wall or ledge
            else
            {
                playerForward = player.transform.forward;
            }
            lastPlayerRot = player.transform.rotation;
            //checking to see if player is currently climbing a wall or ledge
            if (player.GetComponent<PlayerController>() && (player.GetComponent<PlayerController>().currentlyClimbingWall && player.GetComponent<PlayerController>().finishedRotatingToWall || player.GetComponent<PlayerController>().turnBack || player.GetComponent<PlayerController>().back2 || player.GetComponent<PlayerController>().turningToWall && inFirstPersonMode)
            || player.GetComponent<LedgeClimbController>() && (player.GetComponent<LedgeClimbController>().grabbedOn || player.GetComponent<LedgeClimbController>().turnBack || player.GetComponent<LedgeClimbController>().back2))
            {
                playerClimbing = true;
            }
            else
            {
                playerClimbing = false;
                playerForward = -player.transform.forward;
            }
        }
        else
        {
            playerClimbing = false;
        }
        //looking in front of player if we are turning on to a wall for climbing (while in first person mode)
        if (inFirstPersonMode)
        {
            if (player.GetComponent<PlayerController>() && (player.GetComponent<PlayerController>().turnBack || player.GetComponent<PlayerController>().back2 || player.GetComponent<PlayerController>().turningToWall && inFirstPersonMode)
            || player.GetComponent<LedgeClimbController>() && (player.GetComponent<LedgeClimbController>().turnBack || player.GetComponent<LedgeClimbController>().back2))
            {
                turningToClimb = true;
            }
            else
            {
                turningToClimb = false;
            }
        }

    }

    void FollowingState()
    {

        Following1();
        LockingOn1();

    }

    void Following1()
    {

        //following player
        if (following && !playerClimbing)
        {

            lookAt = player.transform.position;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
            Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (directionVector != Vector3.zero)
            {
                // Get the length of the directon vector and then normalize it
                // Dividing by the length is cheaper than normalizing when we already have the length anyway
                float directionLength = directionVector.magnitude;
                directionVector = directionVector / directionLength;

                // Make sure the length is no bigger than 1
                directionLength = Mathf.Min(1, directionLength);

                // Make the input vector more sensitive towards the extremes and less sensitive in the middle
                // This makes it easier to control slow speeds when using analog sticks
                directionLength *= directionLength;

                // Multiply the normalized direction vector by the modified length
                directionVector *= directionLength;
            }

            if (directionVector.magnitude > 0.2f && Mathf.Abs(Input.GetAxis("Horizontal")) >= 0.01f
            || player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().inWater && Input.GetButton("Jump"))
            {

                //getting vector of player (while not swimming)
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {
                    vector2 = Vector3.Lerp(player.transform.right * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.forward * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.forward)));
                }
                //getting vector of player (while swimming)
                else
                {
                    vector2 = Vector3.Lerp(new Vector3(0, player.transform.right.y, 0) * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.forward * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.forward)));
                }
                lookDir = Vector3.Normalize(playerOffset - transform.position);
                lookDir.y = 0f;
                lookDir = Vector3.SmoothDamp(lookDir, vector2, ref velocityLookDir, follow.rotationDampening);

            }
            //if x-axis is locked
            if (follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
            {
                if (!follow.sideScrolling.flipAxis)
                {
                    //if not swimming
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        playerPos = playerOffset + player.transform.up * follow.cameraHeight - Vector3.Normalize(new Vector3(1, 0, 0)) * cameraDistance;
                    }
                    //if swimming
                    else
                    {
                        playerPos = new Vector3(player.transform.position.x, playerOffset.y, playerOffset.z) + new Vector3(0, transform.up.y, transform.up.z) * follow.cameraHeight - Vector3.Normalize(new Vector3(1, 0, 0)) * cameraDistance;
                    }
                }
                else
                {
                    //if not swimming
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        playerPos = playerOffset + player.transform.up * follow.cameraHeight - Vector3.Normalize(new Vector3(-1, 0, 0)) * cameraDistance;
                    }
                    //if swimming
                    else
                    {
                        playerPos = new Vector3(player.transform.position.x, playerOffset.y, playerOffset.z) + new Vector3(0, transform.up.y, transform.up.z) * follow.cameraHeight - Vector3.Normalize(new Vector3(-1, 0, 0)) * cameraDistance;
                    }
                }
            }
            //if z-axis is locked
            else if (follow.sideScrolling.lockZAxis && !follow.sideScrolling.lockXAxis)
            {
                if (!follow.sideScrolling.flipAxis)
                {
                    //if not swimming
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        playerPos = playerOffset + player.transform.up * follow.cameraHeight - Vector3.Normalize(new Vector3(0, 0, 1)) * cameraDistance;
                    }
                    //if swimming
                    else
                    {
                        playerPos = new Vector3(playerOffset.x, playerOffset.y, player.transform.position.z) + new Vector3(transform.up.x, transform.up.y, 0) * follow.cameraHeight - Vector3.Normalize(new Vector3(0, 0, 1)) * cameraDistance;
                    }
                }
                else
                {
                    //if not swimming
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        playerPos = playerOffset + player.transform.up * follow.cameraHeight - Vector3.Normalize(new Vector3(0, 0, -1)) * cameraDistance;
                    }
                    //if swimming
                    else
                    {
                        playerPos = new Vector3(playerOffset.x, playerOffset.y, player.transform.position.z) + new Vector3(transform.up.x, transform.up.y, 0) * follow.cameraHeight - Vector3.Normalize(new Vector3(0, 0, -1)) * cameraDistance;
                    }
                }
            }
            //if neither axis is locked
            else
            {
                //following player (while not swimming)
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {
                    playerPos = playerOffset + player.transform.up * follow.cameraHeight - Vector3.Normalize(lookDir) * cameraDistance;
                }
                //following player (while swimming)
                else
                {
                    //getting player's vertical rotation
                    playerYDeg = player.GetComponent<PlayerController>().yDeg;
                    if (player.GetComponent<PlayerController>().yDeg >= 90)
                    {
                        playerYDeg2 = player.GetComponent<PlayerController>().yDeg - 90;
                    }
                    else
                    {
                        playerYDeg2 = 0;
                    }
                    //getting player's horizontal rotation
                    if (playerYDeg <= 90f)
                    {
                        playerYRot = player.transform.eulerAngles.y;
                    }
                    else
                    {
                        playerYRot = Mathf.Abs(player.transform.eulerAngles.y + 180);
                    }
                    //getting camera's horizontal rotation
                    cameraYRot = transform.eulerAngles.y;

                    //getting the position to follow the player at
                    playerPos = (playerOffset + (transform.forward * (0.5f - (playerYDeg2 * 2) / 90 - Mathf.Abs(Mathf.DeltaAngle(cameraYRot, playerYRot)) / 90) + transform.up * (0.75f * (1 + (playerYDeg + (90 * (playerYDeg / 180))) / 180))) * follow.cameraHeight - Vector3.Normalize(lookDir) * cameraDistance);

                    //checking if camera has arrived at desired position yet
                    if (Vector3.Distance(transform.position, playerPos) < 0.5f)
                    {
                        atFPPoint = true;
                    }
                }
            }

        }
        //if the player is currently climbing a wall or ledge, and the camera is not locking on, follow the player
        else if (playerClimbing && follow.stayBehindPlayerWhileClimbingLedgeOrWall && (!lockedOn || !lockingOn.allowLockingOn) && (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis))
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
            lookDir = Vector3.Slerp(lookDir, playerForward, 6 * Time.deltaTime);
            playerPos = playerOffset + player.transform.up * follow.cameraHeight - lookDir * cameraDistance;
            lookAt = player.transform.position;
        }

    }

    void LockingOn1()
    {

        //locking on
        if (lockedOn && lockingOn.allowLockingOn)
        {

            //if we have a target
            if (closestTarget != null && (distanceFromClosestTarget <= lockingOn.lockingOnToTags.lockInDistance || lockedOnToTarget && distanceFromClosestTarget <= lockingOn.lockingOnToTags.lockOutDistance)
            && closestTarget.activeSelf && (closestTarget.GetComponentInChildren<Renderer>() == null || closestTarget.GetComponentInChildren<Renderer>() != null && closestTarget.GetComponentInChildren<Renderer>().enabled) && notBlocked)
            {

                //getting camera's initial rotation in first person mode so that we can slerp to the new rotation
                if (!lockedOnToTarget && inFirstPersonMode)
                {
                    lookAt = transform.position + transform.forward - new Vector3(0f, follow.playerHeight + 1f, 0f);
                }

                lockedOnToTarget = true;
                lookDir = Quaternion.LookRotation(new Vector3(closestTarget.transform.position.x, player.transform.position.y, closestTarget.transform.position.z) - player.transform.position) * Vector3.forward;

                //if we are not in first person mode
                if (!inFirstPersonMode)
                {
                    if (Vector3.Distance(lookAt, ((closestTarget.transform.position + transform.up * (25 / dist)) - new Vector3(0f, 2.5f, 0f))) > 0.1f)
                    {
                        lookAt = Vector3.Slerp(lookAt, ((closestTarget.transform.position + transform.up * (25 / dist) * height) - new Vector3(0f, 2.5f, 0f)), 8 * Time.deltaTime);
                    }
                    else
                    {
                        lookAt = (closestTarget.transform.position + transform.up * (25 / dist)) - new Vector3(0f, 2.5f, 0f);
                    }
                }
                //if we are in first person mode, use a different height to target the enemy
                else
                {
                    lookAt = Vector3.Slerp(lookAt, ((closestTarget.transform.position + transform.up * height) - new Vector3(0f, 2.5f, 0f)), 8 * Time.deltaTime);
                }

                //if we are not in first person mode, lock on to target and follow player accordingly
                if (!inFirstPersonMode)
                {

                    //locking on to player (while not swimming)
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        playerPos = player.transform.position - (lookDir * 0.4f) + (player.up * 1.85f * (1 + (25 / dist) / 25)) * follow.cameraHeight - lookDir * cameraDistance;
                    }
                    //locking on to player (while swimming)
                    else
                    {
                        playerPos = player.transform.position - (lookDir * 0.4f) + (-player.forward * 1.85f * (1 + (25 / dist) / 25)) * follow.cameraHeight - lookDir * cameraDistance;
                    }

                }
                //if we are in first person mode, lock on to target without changing camera position
                else
                {
                    //calculating camera position
                    Vector3 vTargetOffset;
                    vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                    playerPos = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
                    transform.position = playerPos;
                    transform.LookAt(lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));
                }

            }
            //if we are locking on without a target
            else
            {

                lockedOnToTarget = false;
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
                lookDir = player.transform.forward;
                //if we are not in first person mode, lock on to target and follow player accordingly
                if (!inFirstPersonMode)
                {
                    //locking on to player (while not swimming)
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {

                        playerPos = playerOffset + player.transform.up * follow.cameraHeight - lookDir * cameraDistance;

                    }
                    //locking on to player (while swimming)
                    else
                    {

                        playerPos = playerOffset - (player.transform.forward * follow.cameraHeight) - (player.transform.up * cameraDistance);
                        //setting values for after locking on
                        vector2 = Vector3.Lerp(new Vector3(0, player.transform.right.y, 0) * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.forward * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.forward)));
                        lookDir = Vector3.Normalize(playerOffset - transform.position);
                        lookDir.y = 0f;
                        lookDir = Vector3.SmoothDamp(lookDir, vector2, ref velocityLookDir, follow.rotationDampening);

                    }
                    lookAt = player.transform.position;
                }
                //if we are in first person mode, only lock on to target
                else
                {

                    //calculating camera position
                    Vector3 vTargetOffset;
                    vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                    //making sure the camera is not too rotated (or else it will flip over)
                    if (yFpDeg < 41 || yFpDeg > 44)
                    {
                        playerPos = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
                        transform.position = playerPos;
                    }

                    //calculating camera rotation
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                    {
                        transform.eulerAngles = player.transform.eulerAngles;
                    }
                    else
                    {
                        //making sure the camera is not too rotated (or else it will flip over)
                        if (yFpDeg < 41 || yFpDeg > 44)
                        {
                            if (yFpDeg < 42.56255f)
                            {
                                transform.eulerAngles = player.transform.eulerAngles - new Vector3(yFpDeg2 - yFpDeg, 0, 0);
                                oldEuler = transform.eulerAngles;
                            }
                            else
                            {
                                transform.eulerAngles = player.transform.eulerAngles - new Vector3(357.43745f + (yFpDeg2 - yFpDeg), 0, 0);
                                oldEuler = transform.eulerAngles;
                            }
                        }
                        else
                        {
                            transform.eulerAngles = oldEuler;
                        }
                    }

                }

            }

            //setting mouse orbit values for later when we are not locking on
            SetMouseOrbitValues();
            //setting first person mouse orbit values for later when we are not locking on
            if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
            {
                xFpDeg = player.transform.eulerAngles.y;
                yFpDeg = player.transform.eulerAngles.x;
            }
            else
            {
                xFpDeg = player.GetComponent<PlayerController>().xDeg;
                yFpDeg = player.GetComponent<PlayerController>().yDeg - player.GetComponent<PlayerController>().swimming.firstPerson.cameraAngleOffset - 90;
            }
            yFpDeg2 = yFpDeg;
            fpOrbitRotation = player.transform.rotation;

        }
        else
        {
            lockedOnToTarget = false;
        }

        CheckIfLockingOnIsBlocked();

        //exiting first person mode after locking on
        if (lockedOn && lockingOn.allowLockingOn)
        {
            inFirstPersonMode = false;
            firstPersonButtonPressed = false;
            firstPersonStart = false;
            if (player.GetComponent<PlayerController>())
            {
                player.GetComponent<PlayerController>().inFirstPersonMode = false;
                player.GetComponent<PlayerController>().firstPersonButtonPressed = false;
                player.GetComponent<PlayerController>().firstPersonStart = false;
            }
        }

    }

    void SetMouseOrbitValues()
    {

        //if we are using mouse orbit, set position and rotation to what the camera's currently is
        desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
        correctedDistance = desiredDistance;
        currentDistance = desiredDistance;
        xDeg = transform.eulerAngles.y;
        yDeg = transform.eulerAngles.x;
        rotation = transform.rotation;
        position = transform.position;

    }

    void CheckIfLockingOnIsBlocked()
    {

        //checking to see if something (such as a wall) is blocking the target when the camera is trying to lock on
        RaycastHit hit = new RaycastHit();
        if (closestTarget != null && !Physics.Linecast(transform.position, closestTarget.transform.position + new Vector3(0f, 1f * height, 0f), out hit, collisionLayers) && !Physics.Linecast(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z) - new Vector3(0f, 0.5f, 0f), closestTarget.transform.position + new Vector3(0f, 1f * height, 0f), out hit, collisionLayers) && (!Physics.Linecast(new Vector3(player.transform.position.x, closestTarget.transform.position.y, player.transform.position.z) + new Vector3(0f, 1f, 0f), closestTarget.transform.position + new Vector3(0f, 1f * height, 0f), out hit, collisionLayers) || !Physics.Linecast(player.transform.position + new Vector3(0f, 0.5f, 0f), new Vector3(closestTarget.transform.position.x, player.transform.position.y, closestTarget.transform.position.z) + new Vector3(0f, 0.5f * height, 0f), out hit, collisionLayers)))
        {
            notBlocked = true;
        }
        else
        {
            notBlocked = false;
        }

    }

    void LockOnUI()
    {

        ArrowPointer();

        LockOnBars();

    }

    void ArrowPointer()
    {

        //arrow pointer
        //getting the height of the closest target (for the arrow pointer)
        if (closestTarget != null)
        {
            if (closestTarget.GetComponent<CharacterController>())
            {
                feetPosition = closestTarget.GetComponent<CharacterController>().bounds.min.y;
                headPosition = closestTarget.GetComponent<CharacterController>().bounds.max.y;
                if (closestTarget.transform.localScale.y >= 1)
                {
                    height = (headPosition - feetPosition) * 1.4f;
                }
                else
                {
                    height = (closestTarget.transform.localScale.y * closestTarget.GetComponent<CharacterController>().height) * 1.4f;
                }
            }
            else if (closestTarget.GetComponent<CapsuleCollider>())
            {
                feetPosition = closestTarget.GetComponent<CapsuleCollider>().bounds.min.y;
                headPosition = closestTarget.GetComponent<CapsuleCollider>().bounds.max.y;
                if (closestTarget.transform.localScale.y >= 1)
                {
                    height = (headPosition - feetPosition) * 1.4f;
                }
                else
                {
                    height = (closestTarget.transform.localScale.y * closestTarget.GetComponent<CapsuleCollider>().height) * 1.4f;
                }
            }
            else if (closestTarget.GetComponent<SphereCollider>())
            {
                feetPosition = closestTarget.GetComponent<SphereCollider>().bounds.min.y;
                headPosition = closestTarget.GetComponent<SphereCollider>().bounds.max.y;
                if (closestTarget.transform.localScale.y >= 1)
                {
                    height = (headPosition - feetPosition) * 1.4f;
                }
                else
                {
                    height = ((closestTarget.transform.localScale.y / 1.83f * closestTarget.GetComponent<SphereCollider>().radius) * 4.2f) + 0.7f;
                }
            }
            else if (closestTarget.GetComponent<BoxCollider>())
            {
                feetPosition = closestTarget.GetComponent<BoxCollider>().bounds.min.y;
                headPosition = closestTarget.GetComponent<BoxCollider>().bounds.max.y;
                if (closestTarget.transform.localScale.y >= 1)
                {
                    height = (headPosition - feetPosition) * 1.4f;
                }
                else
                {
                    height = closestTarget.transform.localScale.y * closestTarget.GetComponent<BoxCollider>().size.y;
                }
            }
            else
            {
                height = closestTarget.transform.localScale.y;
            }


            if (lockingOn.lockingOnToTags.arrowPointer != null && (distanceFromClosestTarget <= lockingOn.lockingOnToTags.lockInDistance && notBlocked || lockedOnToTarget) && (closestTarget.GetComponent<Collider>() && closestTarget.GetComponent<Collider>().enabled || closestTarget.GetComponent<Renderer>() && closestTarget.GetComponent<Renderer>().enabled))
            {
                //creating arrow pointer
                if (pointer == null)
                {
                    arrowPointerBounceHeight2 = arrowPointerBounceHeightHalf;
                    pointer = GameObject.Instantiate(lockingOn.lockingOnToTags.arrowPointer);
                    pointer.gameObject.name = lockingOn.lockingOnToTags.arrowPointer.gameObject.name;
                }
                //positioning and rotating arrow pointer
                else
                {
                    if (arrowPointerBounceHeight2 - arrowBounciness < 0.1f * (arrowPointerBounceHeightHalf / 0.15f) && arrowPointerBounceHeight2 - arrowBounciness >= 0
                    || arrowPointerBounceHeight2 - arrowBounciness > -0.1f * (arrowPointerBounceHeightHalf / 0.15f) && arrowPointerBounceHeight2 - arrowBounciness <= 0)
                    {
                        arrowPointerBounceHeight2 = arrowPointerBounceHeightHalf * (-arrowPointerBounceHeight2 / Mathf.Abs(arrowPointerBounceHeight2));
                    }
                    arrowBounciness = Mathf.Lerp(arrowBounciness, arrowPointerBounceHeight2, lockingOn.lockingOnToTags.arrowPointerBounceSpeed * Time.deltaTime);
                    pointer.transform.localPosition = closestTarget.transform.position + new Vector3(0, height + lockingOn.lockingOnToTags.arrowPointerHeight + arrowBounciness, 0);
                    pointer.transform.rotation = Quaternion.LookRotation(new Vector3(transform.position.x, pointer.transform.position.y, transform.position.z) - pointer.transform.position) * lockingOn.lockingOnToTags.arrowPointer.transform.rotation;
                }
            }
            else if (pointer != null)
            {
                Destroy(pointer);
                pointer = null;
            }
        }
        else if (pointer != null)
        {
            Destroy(pointer);
            pointer = null;
        }

    }

    void LockOnBars()
    {

        //lock on bars
        if (lockingOn.barSprite != null)
        {

            //drawing top bar
            if (topBar == null)
            {
                topBar = new GameObject();
                topBar.gameObject.name = "TopBar";
                topBar.gameObject.layer = 5;
                topBar.AddComponent<Image>();
            }
            else
            {
                topBar.transform.SetParent(barHolder.transform);
                topBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, halfScreenHeight + (halfScreenHeight * (lockingOn.barCoverage / 100)) / 2 - barHeight);
                topBar.GetComponent<Image>().sprite = lockingOn.barSprite;
                topBar.GetComponent<RectTransform>().sizeDelta = new Vector2(canvas.GetComponent<RectTransform>().rect.width, halfScreenHeight * (lockingOn.barCoverage / 100));
                topBar.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }

            //drawing bottom bar
            if (bottomBar == null)
            {
                bottomBar = new GameObject();
                bottomBar.gameObject.name = "BottomBar";
                bottomBar.gameObject.layer = 5;
                bottomBar.AddComponent<Image>();
            }
            else
            {
                bottomBar.transform.SetParent(barHolder.transform);
                bottomBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -halfScreenHeight - (halfScreenHeight * (lockingOn.barCoverage / 100)) / 2 + barHeight);
                bottomBar.GetComponent<Image>().sprite = lockingOn.barSprite;
                bottomBar.GetComponent<RectTransform>().sizeDelta = new Vector2(canvas.GetComponent<RectTransform>().rect.width, halfScreenHeight * (lockingOn.barCoverage / 100));
                bottomBar.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }

        }

    }

    void FirstPersonState()
    {

        CheckingIfFirstPersonMode();

        SettingFirstPersonPosition();

        //enabling and disabling renderers (and warning the user if the renderer of any gameObjects they selected does not exist)
        ObjectEnablingDisabling();

        FirstPersonModeMouseOrbiting();

    }

    void CheckingIfFirstPersonMode()
    {

        //determining if we are using first person mode or not
        if ((firstPerson.alwaysUseFirstPerson && !follow.alwaysFollow && !mouseOrbit.alwaysMouseOrbit || firstPerson.switchToFirstPersonIfInputButtonPressed && firstPersonButtonPressed) && (!lockedOn || !lockingOn.allowLockingOn))
        {
            inFirstPersonMode = true;
        }
        else
        {
            inFirstPersonMode = false;
        }
        outOfFPInWaterTimer += 0.2f;

        //setting initial position for camera when transitioning from first person mode while swimming
        if (!inFirstPersonMode && firstPersonModeWasTrueLastUpdate)
        {
            if (player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().inWater)
            {
                outOfFPInWaterTimer = 0.0f;
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
                lookDir = player.transform.up;

                //not in first person mode values
                playerPos = playerOffset - player.transform.forward * follow.cameraHeight - lookDir * cameraDistance;
                lookAt = player.transform.position;

                //first person mode values
                //calculating camera position
                Vector3 vTargetOffset;
                vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                playerPos = player.position - ((player.transform.up * firstPerson.cameraDistance) + vTargetOffset);
                transform.position = playerPos;
                transform.eulerAngles = player.transform.eulerAngles;

                transform.position = playerPos;
                transform.LookAt(lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));

                //setting mouse orbit values
                desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
                correctedDistance = desiredDistance;
                currentDistance = desiredDistance;
                xDeg = transform.eulerAngles.y;
                yDeg = transform.eulerAngles.x;
                rotation = transform.rotation;
                position = transform.position;
            }
            firstPersonModeWasTrueLastUpdate = false;
        }
        if (inFirstPersonMode)
        {
            firstPersonModeWasTrueLastUpdate = true;
        }
        else
        {
            firstPersonModeWasTrueLastUpdate = false;
        }

        //transitioning from first person mode while swimming
        if (outOfFPInWaterTimer < 1)
        {

            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
            lookDir = player.transform.forward;
            //if we are not in first person mode, lock on to target and follow player accordingly
            if (!inFirstPersonMode)
            {
                //locking on to player (while not swimming)
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {

                    playerPos = playerOffset + player.transform.up * follow.cameraHeight - lookDir * cameraDistance;

                }
                //locking on to player (while swimming)
                else
                {

                    playerPos = playerOffset - (player.transform.forward * follow.cameraHeight) - (player.transform.up * cameraDistance);
                    //setting values for after locking on
                    vector2 = Vector3.Lerp(new Vector3(0, player.transform.right.y, 0) * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.forward * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.forward)));
                    lookDir = Vector3.Normalize(playerOffset - transform.position);
                    lookDir.y = 0f;
                    lookDir = Vector3.SmoothDamp(lookDir, vector2, ref velocityLookDir, follow.rotationDampening);

                }
                lookAt = player.transform.position;
            }
            //if we are in first person mode, only lock on to target
            else
            {

                //calculating camera position
                Vector3 vTargetOffset;
                vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                //making sure the camera is not too rotated (or else it will flip over)
                if (yFpDeg < 41 || yFpDeg > 44)
                {
                    playerPos = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
                    transform.position = playerPos;
                }

                //calculating camera rotation
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {
                    transform.eulerAngles = player.transform.eulerAngles;
                }
                else
                {
                    //making sure the camera is not too rotated (or else it will flip over)
                    if (yFpDeg < 41 || yFpDeg > 44)
                    {
                        if (yFpDeg < 42.56255f)
                        {
                            transform.eulerAngles = player.transform.eulerAngles - new Vector3(yFpDeg2 - yFpDeg, 0, 0);
                            oldEuler = transform.eulerAngles;
                        }
                        else
                        {
                            transform.eulerAngles = player.transform.eulerAngles - new Vector3(357.43745f + (yFpDeg2 - yFpDeg), 0, 0);
                            oldEuler = transform.eulerAngles;
                        }
                    }
                    else
                    {
                        transform.eulerAngles = oldEuler;
                    }
                }

            }

            //setting mouse orbit values for later when we are not locking on
            SetMouseOrbitValues();
            //setting first person mouse orbit values for later when we are not locking on
            if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
            {
                xFpDeg = player.transform.eulerAngles.y;
                yFpDeg = player.transform.eulerAngles.x;
            }
            else
            {
                xFpDeg = player.GetComponent<PlayerController>().xDeg;
                yFpDeg = player.GetComponent<PlayerController>().yDeg - player.GetComponent<PlayerController>().swimming.firstPerson.cameraAngleOffset - 90;
            }
            yFpDeg2 = yFpDeg;
            fpOrbitRotation = player.transform.rotation;

        }

    }

    void SettingFirstPersonPosition()
    {

        //first person mode
        if (inFirstPersonMode)
        {

            //calculating camera position
            Vector3 vTargetOffset;
            vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
            fpPosition = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
            transform.position = fpPosition;
            atFPPoint = false;

            //calculating camera rotation; non-mouse orbiting
            if (!firstPerson.mouseOrbiting.mouseOrbitInFirstPersonMode)
            {
                fpRotation = player.transform.eulerAngles;
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {
                    transform.eulerAngles = fpRotation;
                }
                else
                {
                    transform.eulerAngles = fpRotation + new Vector3(-45, 0, 0);
                }
            }

        }

    }

    void FollowingPlayerAndLockingOn()
    {

        //if we are not using the mouse to control the camera, follow and look at player
        if (follow.alwaysFollow && !mouseOrbitButtonPressed && !firstPersonButtonPressed || lockedOn)
        {
            //if no axis is locked, and we are not side-scrolling
            if (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
            {
                CompensateForWalls(playerOffset, ref playerPos);
            }

            //following
            if (!lockedOn && (outOfFPInWaterTimer >= 1 || outOfFPInWaterTimer <= 0.25f))
            {
                //if no axis is locked, and we are not side-scrolling
                if (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, playerPos, ref velocityCamSmooth, follow.movementDampening);
                }
                //if we are side-scrolling
                else
                {
                    //if player is fully out of the water
                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().outOfWaterTimer >= 8)
                    {
                        transform.position = new Vector3(playerPos.x, Mathf.SmoothDamp(transform.position.y, playerPos.y, ref velocityCamSmooth.y, follow.movementDampening), playerPos.z);
                    }
                    //if player has just exited the water
                    else
                    {
                        transform.position = Vector3.Slerp(transform.position, new Vector3(playerPos.x, Mathf.SmoothDamp(transform.position.y, playerPos.y, ref velocityCamSmooth.y, follow.movementDampening), playerPos.z), outOfWaterSlerpValue * Time.deltaTime);
                    }
                }
            }
            //locking on
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, playerPos, ref velocityCamSmooth, follow.movementDampening / (lockingOn.lockOnSpeed * 0.5f));
                atFPPoint = true;
            }
            //transitioning from first person while swimming
            if (outOfFPInWaterTimer < 1)
            {
                transform.position = Vector3.SmoothDamp(transform.position, playerPos, ref velocityCamSmooth, follow.movementDampening / 0.5f);
                atFPPoint = true;
            }

            //this is where we look at the player if we are not in first person mode
            if (!inFirstPersonMode)
            {

                //getting where to look
                //if completely camera is completely out of first person mode, and at playerPos
                if (atFPPoint || (follow.sideScrolling.lockXAxis || follow.sideScrolling.lockZAxis) && playerEnteredWaterOnceAlready)
                {

                    if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().outOfWaterTimer >= 8)
                    {
                        //if not in water
                        if (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis
                        || (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater))
                        {
                            transform.LookAt(lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));
                        }
                        //if in water
                        else
                        {
                            transform.LookAt(player.transform.position + new Vector3(0f, follow.playerHeight + 0.5f, 0f));
                        }

                        outOfWaterSlerpValue = 15;
                        landLookAt = (lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));
                    }
                    else
                    {
                        //transitioning from water to land camera view
                        outOfWaterSlerpValue += 0.5f;
                        landLookAt = Vector3.Slerp(landLookAt, (lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f)), outOfWaterSlerpValue * Time.deltaTime);
                        transform.LookAt(landLookAt);
                    }

                }
                //if camera is just exiting first person mode, and is not yet at playerPos
                else
                {

                    //if player is rotated up
                    if (player.GetComponent<PlayerController>().yDeg < 30f
                    //if neither axis is locked or we are not swimming
                    && (!follow.sideScrolling.lockXAxis && !follow.sideScrolling.lockZAxis || (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)))
                    {

                        transform.LookAt(lookAt + new Vector3(0f, follow.playerHeight + 1f, 0f));

                    }
                    //if player is rotated down
                    else
                    {

                        //if x and y rotation are both pointing in the right direction
                        if (Mathf.Abs(swimmingQuat.x - finalSwimQuat.x) < 45 && Mathf.Abs(swimmingQuat.y - finalSwimQuat.y) < 45)
                        {
                            transform.eulerAngles = new Vector3(Mathf.Lerp(transform.eulerAngles.x, finalSwimQuat.x, 12 * Time.deltaTime), finalSwimQuat.y, finalSwimQuat.z);
                        }
                        //if x rotation is pointing in the right direction, but y rotation is pointing backwards
                        else if (Mathf.Abs(swimmingQuat.x - finalSwimQuat.x) < 45 && Mathf.Abs(swimmingQuat.y - finalSwimQuat.y) >= 45)
                        {
                            transform.eulerAngles = new Vector3(Mathf.Lerp(transform.eulerAngles.x, finalSwimQuat.x, 12 * Time.deltaTime), swimmingQuat.y, swimmingQuat.z);
                        }
                        //if x rotation is pointing backwards, but y rotation is pointing in the right direction
                        else if (Mathf.Abs(swimmingQuat.x - finalSwimQuat.x) >= 45 && Mathf.Abs(swimmingQuat.y - finalSwimQuat.y) < 45)
                        {
                            transform.eulerAngles = new Vector3(swimmingQuat.x, finalSwimQuat.y, finalSwimQuat.z);
                        }
                        //if x and y rotation are both pointing backwards
                        else
                        {
                            transform.eulerAngles = swimmingQuat;
                        }

                    }

                }
                //checking if the player has already entered the water to determine the camera view
                if (player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().noCollisionWithWaterTimer < 5)
                {
                    playerEnteredWaterOnceAlready = true;
                }

            }
        }
        else
        {
            lookDir = player.transform.forward;
            playerPos = transform.position;
            lookAt = player.transform.position;
        }

    }

    void FirstPersonModeMouseOrbiting()
    {

        //first person mode; mouse orbiting
        if (inFirstPersonMode && firstPerson.mouseOrbiting.mouseOrbitInFirstPersonMode && (!lockedOn || !lockingOn.allowLockingOn) && !turningToClimb)
        {

            //getting degrees of rotation for camera
            xFpDeg += Input.GetAxis("Mouse X") * firstPerson.mouseOrbiting.xSpeed * 0.02f;
            yFpDeg -= Input.GetAxis("Mouse Y") * firstPerson.mouseOrbiting.ySpeed * 0.02f;

            //clamping value below 180
            if (yFpDeg >= 180)
            {
                yFpDeg = 360 - yFpDeg;
            }
            //if the player is not swimming, or if his movement/rotation is not locked while swimming (by the PlayerController)
            if (!player.GetComponent<PlayerController>()
            || player.GetComponent<PlayerController>() && (!player.GetComponent<PlayerController>().inWater || player.GetComponent<PlayerController>().swimming.firstPerson.allowMovement))
            {

                yFpDeg = ClampAngle(yFpDeg, firstPerson.mouseOrbiting.yMinLimit, firstPerson.mouseOrbiting.yMaxLimit);

                if (player.GetComponent<LedgeClimbController>() && player.GetComponent<LedgeClimbController>().grabbedOn)
                {

                    float xMinLimit = player.transform.eulerAngles.y - 90;
                    float xMaxLimit = player.transform.eulerAngles.y + 90;

                    //clamping values below 180
                    if (xMinLimit >= 180 && xMaxLimit >= 180)
                    {
                        xMinLimit = xMinLimit - 360;
                        xMaxLimit = xMaxLimit - 360;
                    }

                    xFpDeg = ClampAngle(xFpDeg, xMinLimit, xMaxLimit);

                }

                oldXFpDeg = xFpDeg;

            }
            //if the player is swimming, and his movement/rotation is locked (by the PlayerController)
            else
            {
                //clamping xFpDeg and yFpDeg between their minimum and maximum limits
                if (xFpDeg >= 180)
                {
                    xFpDeg = 360 - xFpDeg;
                }
                float xFpDegMin = oldXFpDeg + player.GetComponent<PlayerController>().swimming.firstPerson.rotationLimitsIfMovementIsNotAllowed.xMinLimit;
                float xFpDegMax = oldXFpDeg + player.GetComponent<PlayerController>().swimming.firstPerson.rotationLimitsIfMovementIsNotAllowed.xMaxLimit;
                float yFpDegMin = player.GetComponent<PlayerController>().swimming.firstPerson.rotationLimitsIfMovementIsNotAllowed.yMinLimit;
                float yFpDegMax = player.GetComponent<PlayerController>().swimming.firstPerson.rotationLimitsIfMovementIsNotAllowed.yMaxLimit;
                xFpDeg = ClampAngle(xFpDeg, xFpDegMin, xFpDegMax);
                yFpDeg = ClampAngle(yFpDeg, yFpDegMin, yFpDegMax);
            }

            //set camera rotation
            fpOrbitRotation = Quaternion.Euler(yFpDeg, xFpDeg, 0);
            transform.rotation = fpOrbitRotation;

            //set values for when we are not in first person mode
            lookDir = player.transform.forward;
            //calculating camera position
            playerYDeg = player.GetComponent<PlayerController>().yDeg;
            if (playerYDeg >= 90f)
            {
                playerOffset = player.transform.position + follow.cameraHeight * -player.transform.forward;
            }
            else
            {
                playerOffset = player.transform.position + follow.cameraHeight * player.transform.forward;
            }
            playerPos = playerOffset - (player.transform.forward * follow.cameraHeight) - (player.transform.up * cameraDistance);
            //setting values for after locking on
            vector2 = Vector3.Lerp(new Vector3(0, player.transform.right.y, 0) * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.up * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.up)));
            lookDir = Vector3.Normalize(playerOffset - transform.position);
            lookDir.y = 0f;
            lookDir = Vector3.SmoothDamp(lookDir, vector2, ref velocityLookDir, follow.rotationDampening);
            lookAt = player.transform.position;
            //setting mouse orbit values for later when we are not in first person mode
            SetMouseOrbitValues();

            //getting degrees of rotation for camera (unaffected by the player's beginningCameraAngle)
            yFpDeg2 -= Input.GetAxis("Mouse Y") * firstPerson.mouseOrbiting.ySpeed * 0.02f;
            //clamping yFpDeg2 between the minimum and maximum y limit
            if (yFpDeg2 >= 180)
            {
                yFpDeg2 = 360 - yFpDeg2;
            }
            yFpDeg2 = ClampAngle(yFpDeg2, firstPerson.mouseOrbiting.yMinLimit, firstPerson.mouseOrbiting.yMaxLimit);

            //the first person mode values have updated since entering first person mode
            goneThroughFirstPersonUpdate = true;

        }
        else
        {
            if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
            {
                xFpDeg = player.transform.eulerAngles.y;
                yFpDeg = player.transform.eulerAngles.x;
            }
            else
            {
                xFpDeg = player.GetComponent<PlayerController>().xDeg;
                yFpDeg = player.GetComponent<PlayerController>().yDeg - player.GetComponent<PlayerController>().swimming.firstPerson.cameraAngleOffset - 90;
            }
            yFpDeg2 = yFpDeg;
            goneThroughFirstPersonUpdate = false;
        }

    }

    void MouseOrbitMode()
    {

        GettingMouseOrbitDistances();

        //mouse orbiting
        if ((!follow.alwaysFollow && mouseOrbit.alwaysMouseOrbit || mouseOrbitButtonPressed) && !lockedOn && !inFirstPersonMode)
        {

            //setting following variables in case player switches the camera to follow
            lookDir = player.transform.forward;
            playerPos = transform.position;
            lookAt = player.transform.position;

            //creating a Vector3 to hold the camera's height
            Vector3 vTargetOffset;

            //getting degrees of rotation for camera
            xDeg += Input.GetAxis("Mouse X") * mouseOrbit.xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * mouseOrbit.ySpeed * 0.02f;

            //clamping yDeg between the minimum and maximum y limit
            if (yDeg >= 180)
            {
                yDeg = 360 - yDeg;
            }
            yDeg = ClampAngle(yDeg, mouseOrbit.yMinLimit, mouseOrbit.yMaxLimit);


            // set camera rotation
            rotation = Quaternion.Euler(yDeg, xDeg, 0);

            // calculate the desired distance
            //if we are allowed to use the scroll wheel, allow zooming in and out between the minimum and maximum distances
            if (mouseOrbit.allowZoomingWithScrollWheel)
            {
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * mouseOrbit.zoomSpeed * Mathf.Abs(desiredDistance);
                desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMinDistance, mouseOrbitMaxDistance);
            }
            //if we are not allowed to use the scroll wheel, only allow the maximum distance
            else
            {
                desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
            }
            correctedDistance = desiredDistance;

            // calculate desired camera position
            vTargetOffset = new Vector3(0, -mouseOrbit.cameraHeight, 0);
            position = player.position - (rotation * Vector3.forward * (desiredDistance) + vTargetOffset);

            // check for collision using the true target's desired registration point as set by user using height
            RaycastHit collisionHit;
            Vector3 trueTargetPosition = new Vector3(player.position.x, player.position.y + mouseOrbit.cameraHeight, player.position.z);

            // if there was a collision, correct the camera position and calculate the corrected distance
            bool isCorrected = false;
            if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers.value))
            {
                // calculate the distance from the original estimated position to the collision location,
                // subtracting out a safety "offset" distance from the object we hit.  The offset will help
                // keep the camera from being right on top of the surface we hit, which usually shows up as
                // the surface geometry getting partially clipped by the camera's front clipping plane.
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
            currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * mouseOrbit.zoomDampening) : correctedDistance;

            // keep within legal limits
            currentDistance = Mathf.Clamp(currentDistance, mouseOrbit.nearClippingPlane, mouseOrbitMaxDistance);

            // recalculate position based on the new currentDistance
            position = player.position - (rotation * Vector3.forward * (currentDistance) + vTargetOffset);

            transform.rotation = rotation;
            if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().outOfWaterTimer >= 8)
            {
                transform.position = position;
            }
            else
            {
                transform.position = Vector3.Slerp(transform.position, position, outOfWaterSlerpValue * Time.deltaTime);
            }

            //setting values for after mouse orbiting (if player is swimming)
            if (player.GetComponent<PlayerController>() && player.GetComponent<PlayerController>().inWater)
            {
                vector2 = Vector3.Lerp(new Vector3(0, player.transform.right.y, 0) * ((Input.GetAxis("Horizontal") >= 0f) ? -1f : 1f), player.transform.forward * ((Input.GetAxis("Vertical") >= 0f) ? 1f : -1f), Mathf.Abs(Vector3.Dot(transform.forward, player.transform.forward)));
                lookDir = Vector3.Normalize(playerOffset - transform.position);
                lookDir.y = 0f;
                lookDir = Vector3.SmoothDamp(lookDir, vector2, ref velocityLookDir, follow.rotationDampening);
            }
        }
        //since we are not orbiting right now, set position and rotation to what the camera's currently is
        else
        {
            desiredDistance = Mathf.Clamp(desiredDistance, mouseOrbitMaxDistance, mouseOrbitMaxDistance);
            correctedDistance = desiredDistance;
            currentDistance = desiredDistance;
            xDeg = transform.eulerAngles.y;
            yDeg = transform.eulerAngles.x;
            rotation = transform.rotation;
            position = transform.position;
        }

    }

    void GettingMouseOrbitDistances()
    {

        //adjusting distance from player relative to height
        float valueOne;
        float valueTwo;
        if (follow.cameraHeight <= 1)
        {
            valueOne = 0.4f;
            valueTwo = 0.6f;
        }
        else
        {
            valueOne = 0.65f;
            valueTwo = 0.35f;
        }

        //mouse orbit distances
        if (cameraDistance >= 0.6f)
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (1 + (1.5f / cameraDistance)) / 1.8577f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        else if (cameraDistance >= 0.4f)
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (1 + (cameraDistance)) / 0.84f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        else
        {
            mouseOrbitMaxDistance = (cameraDistance + (0.73f * (20 + (cameraDistance)) / 10.9338f) / Vector3.Distance(position, player.transform.position)) * (valueOne + follow.cameraHeight * valueTwo);
        }
        mouseOrbitMinDistance = mouseOrbit.minZoomInDistance;

    }

    void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        //compensate for walls between camera
        RaycastHit raycastHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out raycastHit, collisionLayers))
        {
            toTarget = raycastHit.point + raycastHit.normal * offsetFromWall;
        }

        //compensate for geometry intersecting with near clip plane
        Vector3 camPosition = GetComponent<Camera>().transform.position;
        GetComponent<Camera>().transform.position = toTarget;
        viewFrustum = CameraController.CalculateViewFrustum(GetComponent<Camera>(), ref nearClipDimensions);

        for (int i = 0; i < viewFrustum.Length / 2; i++)
        {

            RaycastHit raycastHit2 = new RaycastHit();
            RaycastHit raycastHit3 = new RaycastHit();
            while (Physics.Linecast(viewFrustum[i], viewFrustum[(i + 1) % (viewFrustum.Length / 2)], out raycastHit2) || Physics.Linecast(viewFrustum[(i + 1) % (viewFrustum.Length / 2)], viewFrustum[i], out raycastHit3))
            {

                Vector3 normal = raycastHit.normal;
                if (raycastHit.normal == Vector3.zero && raycastHit2.normal != Vector3.zero)
                {
                    normal = raycastHit2.normal;
                }

                toTarget += 0.2f * normal;
                GetComponent<Camera>().transform.position += toTarget;

                // Recalculate positions of near clip plane
                viewFrustum = CameraController.CalculateViewFrustum(GetComponent<Camera>(), ref nearClipDimensions);

            }

        }
        GetComponent<Camera>().transform.position = camPosition;
        viewFrustum = CameraController.CalculateViewFrustum(GetComponent<Camera>(), ref nearClipDimensions);

    }

    public static Vector3[] CalculateViewFrustum(Camera cam, ref Vector3 dimensions)
    {

        Vector3[] frustrum = new Vector3[8];

        //near clipping plane bounds
        frustrum[0] = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        frustrum[1] = cam.ViewportToWorldPoint(new Vector3(0f, 1f, cam.nearClipPlane));
        frustrum[3] = cam.ViewportToWorldPoint(new Vector3(1f, 0f, cam.nearClipPlane));
        frustrum[2] = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));

        //clipping planes (0 is left, 1 is right, 2 is bottom, 3 is top, 4 is near, and 5 is far)
        Plane[] frustrum2 = GeometryUtility.CalculateFrustumPlanes(cam);
        frustrum[4] = Vector3.Cross(frustrum2[0].normal, frustrum2[2].normal);
        frustrum[5] = Vector3.Cross(frustrum2[3].normal, frustrum2[0].normal);
        frustrum[6] = Vector3.Cross(frustrum2[1].normal, frustrum2[3].normal);
        frustrum[7] = Vector3.Cross(frustrum2[2].normal, frustrum2[1].normal);

        //dimensions
        dimensions.x = (frustrum[0] - frustrum[3]).magnitude;
        dimensions.y = (frustrum[1] - frustrum[0]).magnitude;
        dimensions.z = (frustrum[0] - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane))).magnitude;

        return frustrum;

    }

    void LateUpdate()
    {

        //first person mode
        if (inFirstPersonMode)
        {

            //calculating camera position
            Vector3 vTargetOffset;
            if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
            {
                vTargetOffset = new Vector3(0, -firstPersonCameraHeight, 0);
                fpPosition = player.position - ((player.transform.forward * firstPerson.cameraDistance) + vTargetOffset);
            }
            else
            {
                vTargetOffset = new Vector3(0, firstPerson.cameraDistance, 0);
                fpPosition = player.GetComponent<PlayerController>().colliderBottom + ((player.transform.up * firstPersonCameraHeight) + vTargetOffset);
            }
            transform.position = fpPosition;

            //calculating camera rotation
            if (!firstPerson.mouseOrbiting.mouseOrbitInFirstPersonMode || turningToClimb)
            {
                //setting first person mouse orbit degrees to our current rotation
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {

                    xFpDeg = player.transform.eulerAngles.y;
                    float xMinLimit = player.transform.eulerAngles.y - 90;
                    float xMaxLimit = player.transform.eulerAngles.y + 90;
                    //clamping value below 180
                    if (xMinLimit >= 180 && xMaxLimit >= 180)
                    {
                        xFpDeg = xFpDeg - 360;
                    }

                    yFpDeg = player.transform.eulerAngles.x;
                }
                else
                {
                    xFpDeg = player.GetComponent<PlayerController>().xDeg;
                    yFpDeg = player.GetComponent<PlayerController>().yDeg - player.GetComponent<PlayerController>().swimming.firstPerson.cameraAngleOffset - 90;
                }
                yFpDeg2 = yFpDeg;
                fpOrbitRotation = player.transform.rotation;
                //if player is not in water
                if (!player.GetComponent<PlayerController>() || player.GetComponent<PlayerController>() && !player.GetComponent<PlayerController>().inWater)
                {
                    fpRotation = player.transform.eulerAngles;
                }
                //if player is in water
                else if (player.GetComponent<PlayerController>().yDeg < 90 || player.transform.eulerAngles.x == 90)
                {
                    fpRotation = new Vector3(playerYDeg, player.transform.eulerAngles.y, player.transform.eulerAngles.z) + new Vector3(-60, 0, 0);
                }
                else
                {
                    fpRotation = new Vector3(185 - playerYDeg, player.transform.eulerAngles.y, player.transform.eulerAngles.z) + new Vector3(-60 + 115, 0, 0);
                }
                transform.eulerAngles = fpRotation;

            }

        }


        //checking to see if the mouse orbit button has been pressed
        if (Input.GetButtonDown(mouseOrbit.mouseOrbitInputButton) && mouseOrbit.switchToMouseOrbitIfInputButtonPressed)
        {
            if (mouseOrbitStart)
            {
                mouseOrbitStart = false;
            }
            else
            {
                mouseOrbitStart = true;
            }
        }
        else if (!mouseOrbit.switchToMouseOrbitIfInputButtonPressed)
        {
            mouseOrbitStart = false;
        }

        //checking to see if the first person button has been pressed
        if (Input.GetButtonDown(firstPerson.firstPersonInputButton) && firstPerson.switchToFirstPersonIfInputButtonPressed)
        {
            if (firstPersonStart)
            {
                firstPersonStart = false;
            }
            else
            {
                firstPersonStart = true;
            }
        }
        else if (!firstPerson.switchToFirstPersonIfInputButtonPressed)
        {
            firstPersonStart = false;
        }

        PositionBlackBarsInFirstPersonMode();

    }

    void PositionBlackBarsInFirstPersonMode()
    {

        //positioning black bars when mouse orbiting or in first person mode
        if (barHolder == null)
        {
            barHolder = new GameObject();
            barHolder.AddComponent<RectTransform>();
            barHolder.gameObject.name = "BarHolder";
            barHolder.gameObject.layer = 5;
        }
        else
        {

            barHolder.transform.SetParent(canvas.transform);
            barHolder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            barHolder.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            barHolder.transform.SetAsFirstSibling();

        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    void ObjectEnablingDisabling()
    {

        //enabling and disabling gameObjects while in first person mode
        if (inFirstPersonMode)
        {
            //if gameObjects have not been disabled/enabled yet
            if (!objectsFinished)
            {
                if (firstPerson.objectsToDisableInFirstPerson != null)
                {
                    foreach (GameObject obj in firstPerson.objectsToDisableInFirstPerson)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                        }
                        else if (!currentlyEnablingAndDisablingObjects)
                        {
                            objectWarning = true;
                        }
                    }
                }
                if (firstPerson.objectsToEnableInFirstPerson != null)
                {
                    foreach (GameObject obj in firstPerson.objectsToEnableInFirstPerson)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(true);
                        }
                        else if (!currentlyEnablingAndDisablingObjects)
                        {
                            objectWarning = true;
                        }
                    }
                }
                currentlyEnablingAndDisablingObjects = true;
            }
            objectsFinished = true;

        }
        //undoing enabling and disabling gameObjects when camera exits first person mode
        else
        {
            //if gameObjects have not been un-disabled/enabled yet
            if (objectsFinished)
            {
                if (firstPerson.objectsToDisableInFirstPerson != null)
                {
                    foreach (GameObject obj in firstPerson.objectsToDisableInFirstPerson)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(true);
                        }
                        else if (!currentlyEnablingAndDisablingObjects || inFirstPersonMode)
                        {
                            objectWarning = true;
                        }
                    }
                }
                if (firstPerson.objectsToEnableInFirstPerson != null)
                {
                    foreach (GameObject obj in firstPerson.objectsToEnableInFirstPerson)
                    {
                        if (obj != null)
                        {
                            obj.SetActive(false);
                        }
                        else if (!currentlyEnablingAndDisablingObjects || inFirstPersonMode)
                        {
                            objectWarning = true;
                        }
                    }
                }
                currentlyEnablingAndDisablingObjects = true;
            }
            objectsFinished = false;

        }

        //all loops that enable or disable gameObjects have finished, so we set currentlyEnablingAndDisablingObjects to false
        if (!inFirstPersonMode)
        {
            currentlyEnablingAndDisablingObjects = false;
        }
        //warns the user if any gameObject they selected does not exist
        if (objectWarning)
        {
            if (firstPerson.objectsToDisableInFirstPerson != null)
            {
                foreach (GameObject obj in firstPerson.objectsToDisableInFirstPerson)
                {
                    if (obj == null)
                    {
                        Debug.Log("<color=red>The gameObject: </color>\"" + obj + "\"<color=red> does not exist</color>");
                    }
                }
            }
            if (firstPerson.objectsToEnableInFirstPerson != null)
            {
                foreach (GameObject obj in firstPerson.objectsToEnableInFirstPerson)
                {
                    if (obj == null)
                    {
                        Debug.Log("<color=red>The gameObject: </color>\"" + obj + "\"<color=red> does not exist</color>");
                    }
                }
            }
            objectWarning = false;
        }

    }

}