using UnityEngine;
using DG.Tweening;

/*
 * Generalized Camera Behavior script to attach to a camera. Simple and easy to use.
 * TODO:
 */
public class CameraBehavior : MonoBehaviour
{
    //for the sake of iterative development, pointing to the PowerManager

    private const int LMB = 0;
    private const int RMB = 1;
    private const int MMB = 2;

    //Control and User-Interface objects and variables

    [HideInInspector]
    public bool cameraLocked;
    [HideInInspector]
    public bool cameraUsedOnce; //used to trigger the next step in the game
    [HideInInspector]
    public bool isPaused;

    private int currentLevel;

    //debug mode allows the user to type a key for whatever power they want to use

    //Camera references and variables
    [HideInInspector]
    public float defaultFieldOfView = 30.0f;

    //still used by FTUE. TODO: Try to remove it.
    public GameObject emptyGameObject; //reference for an empty game object, which is used as the point that the camera always looks at

    //Variables for camerashake
    private Vector3 originPosition;
    private Quaternion originRotation;
    private float shake_decay;
    private float shake_intensity;

    //new camera methods' variables
    public Transform target;
    private Vector3 targetOriginalPosition;
    private float originalXDeg, originalYDeg, originalCurrentDistance, originalDesiredDistance;
    public GameObject cameraRig;

    [Header("Camera Control Options")]
    public int defaultRotationSpeed = 5;
    public float defaultPanSpeed = 5f;
    public int defaultZoomSpeed = 5;

    [Header("Camera Movement Variables")]
    public float distance = 5.0f;
    public float maxXZPanDistance = 700f;
    public float minXZPanDistance = 0f;
    public float maxYPanDistance = 150f;
    public float minYPanDistance = 20f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;

    //camera coordinates
    private float xDeg;
    private float yDeg;
    private float currentDistance;
    private float desiredDistance;

    //camera starting position for every level
    [Header("Starting Camera Position Variables")]
    public float startingYDeg = 4.2f; //4.2f
    public float startingXDeg = 243.675f;
    public float startingDistance = 580f; //new potential value: 600?

    private bool oneClick;
    private bool timerRunning;
    private float timerForDoubleClick;
    private float delay;

    public enum CameraState
    {
        FREE, // under complete user control. The default state
        LOCKED, // the user cannot control the camera
        COMPUTER_CONTROLLED,    //state when camera is automated. User cannot control camera
        TOURING     //start of new era, camera WILL rotate around island to see all its tree clumps, temples, and villages
    };
    [HideInInspector]
    public CameraState cameraState;

    private bool isLockedOnVolcano;
    private Tween volcanoFocusTween;
    private Quaternion originalFocus;
    public float cameraFocusOnVolcanoTweenDuration = 1.0f;

    void Awake()
    {
        //Debug.Log("CameraBehavior now retrieving FTUE");
        currentLevel = 0;

        //UI initialization
        cameraLocked = false;

        isPaused = false;

        cameraRig = new GameObject();
        cameraRig.name = "Main Camera Rig";
        cameraRig.transform.position = transform.position;
        transform.SetParent(cameraRig.transform);

    }

    void Start()
    {
        print("Beginning CameraBehavior.Start()");
        //FocusCameraOnVolcano();
        this.GetComponent<Camera>().fieldOfView = defaultFieldOfView;

        currentDistance = distance;
        desiredDistance = distance;
        cameraRig.transform.rotation = transform.rotation;
        cameraRig.transform.position = transform.position;

        timerForDoubleClick = 0f;
        oneClick = false;
        timerRunning = false;
        delay = .5f;
        targetOriginalPosition = target.position;
        print("Camera Rig position: " + cameraRig.transform.position);
        print("Camera Euler values: " + transform.rotation.eulerAngles);

        cameraState = CameraState.FREE;

        originalYDeg = startingYDeg;
        originalXDeg = startingXDeg;
        originalCurrentDistance = startingDistance;
        originalDesiredDistance = startingDistance;

        xDeg = originalXDeg;
        yDeg = originalYDeg;
        currentDistance = originalCurrentDistance;
        desiredDistance = originalDesiredDistance;
        CalculateNewCameraAngle();
    }

    void Update()
    {

        if (Input.GetKeyUp(KeyCode.X) && Application.isEditor)
        {
            print("=========================\nLOCATION\n");
            print("(xDeg/yDeg): (" + xDeg + " , " + yDeg + ")\nDesired Distance:\t" + desiredDistance);
        }
        if (isPaused) return;
        if (shake_intensity > 0)
        {
            transform.localPosition = originPosition + Random.insideUnitSphere * shake_intensity;
            shake_intensity -= shake_decay;
        }
        //this is how long in seconds to allow for a double click
        //found here: http://answers.unity3d.com/questions/331545/double-click-mouse-detection-.html
        if (Input.GetMouseButtonDown(RMB))
        {
            if (!oneClick) // first click no previous clicks
            {
                oneClick = true;
                timerForDoubleClick = Time.time; // save the current time
            }
            else
            {
                oneClick = false; // found a double click, now reset
                TweenResetCameraPosition();
                //FocusCameraOnVolcano();

            }
        }
        if (oneClick)
        {
            // if the time now is delay seconds more than when the first click started. 
            if ((Time.time - timerForDoubleClick) > delay)
            {
                //basically if thats true its been too long and we want to reset so the next click is simply a single click and not a double click.
                oneClick = false;
            }
        }
    }

    void LateUpdate()
    {
        // Don't do anything if target is not defined
        if (!target || isPaused)
            return;


        switch (cameraState)
        {
            case CameraState.FREE:
                CalculateFreeInput();
                break;

            case CameraState.LOCKED:
                break;

            case CameraState.COMPUTER_CONTROLLED:
                CalculateNewCameraAngle();
                break;

            case CameraState.TOURING:
                CalculateNewCameraAngle();
                break;
        }

    }

    #region Mouse Camera Controls
    private void CalculateFreeInput()
    {
        bool playerInput = false;
        Quaternion oldRotation = cameraRig.transform.rotation;
        Vector3 oldPos = cameraRig.transform.position;
        Vector3 oldTargetPos = target.position;
        float oldCurrentDistance = currentDistance;
        float oldDesiredDistance = desiredDistance;
        float oldYDeg = 0f;

        // If middle mouse is selected? ORBIT
        if (Input.GetMouseButton(RMB))
        {
            playerInput = true;
            CalculateOrbitalInput(out oldYDeg);
        }
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
        else if (Input.GetMouseButton(MMB))
        {
            //playerInput = true;
            CalculatePanningInput();

        }
        // affect the desired Zoom distance if we roll the scrollwheel
        else if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            playerInput = true;
            CalculateZoomInput();
        }

        CalculateNewCameraAngle();

        if (CameraIntersectsGround())
        {
            yDeg = oldYDeg;
            cameraRig.transform.position = oldPos;
            currentDistance = oldCurrentDistance;
            desiredDistance = oldDesiredDistance;
            target.position = oldTargetPos;
            CalculateNewCameraAngle();
        }
    }

    private void CalculateOrbitalInput(out float oldYDeg)
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        xDeg += x * defaultRotationSpeed * 0.6f;
        oldYDeg = yDeg;
        yDeg -= y * defaultRotationSpeed * 0.6f;

    }

    private void CalculatePanningInput()
    {
        //grab the rotation of the camera
        target.rotation = cameraRig.transform.rotation; //I don't know if this line is still needed
        target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * defaultPanSpeed);
        target.Translate(transform.up * -Input.GetAxis("Mouse Y") * defaultPanSpeed, Space.World);
        Vector3 clampedPos = this.ClampIn3DSpace();
        target.localPosition = clampedPos;
    }

    private void CalculateZoomInput()
    {
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * defaultZoomSpeed * Mathf.Abs(desiredDistance);
    }

    private void CalculateNewCameraAngle()
    {
        Quaternion rotation = Quaternion.identity;
        Vector3 position = Vector3.zero;
        //clamp the zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

        //Clamp the vertical axis for the orbit
        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

        // set camera rotation
        rotation = Quaternion.Euler(yDeg, xDeg, 0);

        //used to smooth zoom on this line by lerping the distance, but we don't want that in our game.
        currentDistance = desiredDistance;

        // keep within the minimum and maximum distance we've declared
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // calculate position based on the new currentDistance
        position = target.position - (rotation * Vector3.forward * currentDistance);

        cameraRig.transform.rotation = rotation;
        cameraRig.transform.position = position;
    }

    private Vector3 ClampIn3DSpace()
    {
        Vector3 position = target.position;
        position.x = Mathf.Clamp(position.x, minXZPanDistance, maxXZPanDistance);
        position.y = Mathf.Clamp(position.y, minYPanDistance, maxYPanDistance);
        position.z = Mathf.Clamp(position.z, minXZPanDistance, maxXZPanDistance);
        return position;
    }

    private bool CameraIntersectsGround()
    {
        Vector3 currentPosition = cameraRig.transform.position;
        if (currentPosition.y <= 0f)
            return true;
        else
            return false;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    #endregion

    #region Pause Management
    //PAUSE FUNCTIONS
    void ManagePause()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void PauseGame()
    {
        print("Pausing Game!");
        isPaused = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        print("Resuming game!");
        isPaused = false;
        Time.timeScale = 1f;
    }
    #endregion

    public void Shake(int eraNumber)
    {
        originPosition = transform.localPosition;
        shake_intensity = 2.0f + 1.0f * eraNumber;
        shake_decay = 0.02f + 0.01f * eraNumber;
    }

    public float EarthquakeShake(int eraNumber)
    {
        originPosition = transform.localPosition;
        shake_intensity = 5.0f + 1.2f * eraNumber;
        shake_decay = 0.03f;
        return shake_intensity / shake_decay;
    }

    RaycastHit GrabCursorCollision()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
        }
        return hit;
    }

    public void LockCamera()
    {
        cameraLocked = true;
    }

    public void UnlockCamera()
    {
        cameraLocked = false;
    }

    //============
    //Functions used for automated camera behavior
    //============

    public void TweenResetCameraPosition()
    {
        TweenResetCameraPosition(1f);
    }

    //double right click method
    public void TweenResetCameraPosition(float duration)
    {
        cameraState = CameraState.COMPUTER_CONTROLLED;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => yDeg, y => yDeg = y, originalYDeg, duration)) //rotate y position
                .Join(target.DOMove(targetOriginalPosition, duration)) //move target back to original position
                .Join(DOTween.To(() => currentDistance, x => currentDistance = x, originalCurrentDistance, duration)) //zoom current
                .Join(DOTween.To(() => desiredDistance, x => desiredDistance = x, originalDesiredDistance, duration)) //zoom desired
                .AppendCallback(ResetCameraState);
    }

    private void ResetCameraState()
    {
        cameraState = CameraState.FREE;
    }

    //TODO: only use this method. Replace all other uses of similar methods.
    public void ResetCameraLocation()
    {
        //SET CAMERA POSITION TO LOCATION WHERE ENTIRE ISLAND CAN BE VIEWED

        yDeg = originalYDeg;
        xDeg = originalXDeg;
        currentDistance = originalCurrentDistance;
        desiredDistance = originalDesiredDistance;
        target.position = targetOriginalPosition;

        CalculateNewCameraAngle(); //run this to refresh the camera position
    }

}
