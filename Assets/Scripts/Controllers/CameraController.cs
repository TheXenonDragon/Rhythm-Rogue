using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //  Keys
    private KeyCode rotateCameraLeftKey = KeyCode.Q;
    private KeyCode rotateCameraRightKey = KeyCode.E;

    //  Angles
    private Vector3 destinationAngle = Vector3.zero;            //  Pivot rotation
    private float destinationAngleTracker = 0f;                 //  Pivot rotation
    private Vector3 defaultHingeAngle = Vector3.right * 60f;    //  Hinge rotation
    private Vector3 blockedHingeAngle = Vector3.right * 90f;    //  Hinge rotation

    //  Positions
    private Vector3 defaultHingePosition = new Vector3(0f, 10f, -5f);   //  Hinge position
    private Vector3 blockedHingePosition = new Vector3(0f, 10f, 0f);    //  Hinge position

    //  Rotation and Movement management
    private bool isRotatingPivot = false;
    private bool setHingeToDefault = true;
    private float rotationSpeed = 3.5f;
    private float movementSpeed = 2.0f;

    //  Raycast
    public LayerMask layerMask;
    private string playerLayer = "Player";
    private RaycastHit hit;
    private GameObject raycastOrigin;                   //  Origin Position to Check for Obstructions

    //  Camera Orientation
    private GameObject cameraOrientation;

    //  Player position
    public Transform playerTransform;

    //  Hinge GameObject
    public GameObject cameraHinge;
    



    // Start is called before the first frame update
    void Start()
    {
        cameraOrientation = new GameObject("CameraOrientation");

        raycastOrigin = new GameObject("CameraRaycastOrigin");
        raycastOrigin.transform.position = Camera.main.transform.position;
        raycastOrigin.transform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        //  Pivot
        GetUserInput();
        UpdatePivotRotation();
        UpdatePivotPosition();

        //  Hinge
        CheckForViewObstruction();
        UpdateHingeRotation();
        UpdateHingePosition();
    }

    public Transform GetTransform(){
        //  This method is used to get the direction of the camera for the playerController.
        //  The purpose of this is so that the forward key will always be the forward direction
        //  of the camera.

        cameraOrientation.transform.eulerAngles = destinationAngle;
        return cameraOrientation.transform;
    }

    private void GetUserInput(){
        if(Input.GetKeyDown(rotateCameraLeftKey)){
            destinationAngleTracker += 90f;
            destinationAngle = new Vector3(destinationAngle.x, destinationAngleTracker, destinationAngle.z);
            isRotatingPivot = true;
        }
        else if(Input.GetKeyDown(rotateCameraRightKey)){
            destinationAngleTracker -= 90f;
            destinationAngle = new Vector3(destinationAngle.x, destinationAngleTracker, destinationAngle.z);
            isRotatingPivot = true;
        }
    }

    private void UpdatePivotPosition(){
        float threshold = 0.01f;

        if(playerTransform != null){
            if(Vector3.Distance(transform.position, playerTransform.position) > threshold) {
                transform.position = Vector3.Lerp(transform.position, playerTransform.position, movementSpeed * Time.deltaTime);
            }
            else {    
                transform.position = playerTransform.position;
            }
        }
    }

    private void UpdatePivotRotation(){
        float threshold = 0.01f;

        if(isRotatingPivot){
            if(Quaternion.Angle(transform.rotation, Quaternion.Euler(destinationAngle)) > threshold) {
                transform.eulerAngles = new Vector3(0f, Mathf.LerpAngle(transform.eulerAngles.y, destinationAngle.y, rotationSpeed * Time.deltaTime), 0f);
            }
            else {
                transform.eulerAngles = destinationAngle;
                isRotatingPivot = false;
            }
        }
    }

    private void CheckForViewObstruction(){
        if(Physics.Raycast(raycastOrigin.transform.position, (playerTransform.position - raycastOrigin.transform.position).normalized, out hit, 100f, layerMask) && (!hit.transform.gameObject.layer.Equals(LayerMask.NameToLayer(playerLayer)))){
            setHingeToDefault = false;
        }
        else{
            setHingeToDefault = true;
        }
    }

    private void UpdateHingeRotation(){
        if(setHingeToDefault){
            cameraHinge.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(cameraHinge.transform.eulerAngles.x, defaultHingeAngle.x, rotationSpeed * Time.deltaTime), 0f, 0f);
        }
        else{
            cameraHinge.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(cameraHinge.transform.eulerAngles.x, blockedHingeAngle.x, rotationSpeed * Time.deltaTime), 0f, 0f);
        }
    }

    private void UpdateHingePosition(){
        if(setHingeToDefault){
            cameraHinge.transform.localPosition = Vector3.Lerp(cameraHinge.transform.localPosition, defaultHingePosition, rotationSpeed * Time.deltaTime);
        }
        else{
            cameraHinge.transform.localPosition = Vector3.Lerp(cameraHinge.transform.localPosition, blockedHingePosition, rotationSpeed * Time.deltaTime);
        }
    }
}