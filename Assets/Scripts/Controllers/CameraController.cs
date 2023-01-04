using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //  Keys
    private KeyCode rotateCameraLeftKey = KeyCode.Q;
    private KeyCode rotateCameraRightKey = KeyCode.E;

    //  Angles
    private Vector3 destinationAngle = new Vector3(0f, 0f, 0f);
    
    private float[] angleList = {0f, 90f, 180f, 270f};
    private int angleListIndex = 0; 

    //  Rotation and Movement management
    private bool isRotating = false;
    private float rotationSpeed = 2.5f;
    private float movementSpeed = 2.0f;

    //  Camera Orientation
    private GameObject cameraOrientation;

    //  Player position
    public Transform playerTransform;



    // Start is called before the first frame update
    void Start()
    {
        cameraOrientation = new GameObject("CameraOrientation");
    }

    // Update is called once per frame
    void Update()
    {
        GetUserInput();
        RotateCameraToDestinationAngle();
        UpdatePosition();
    }

    public Transform GetTransform(){
        cameraOrientation.transform.eulerAngles = destinationAngle;
        return cameraOrientation.transform;
    }

    private void UpdatePosition(){
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

    private void GetUserInput(){
        if(Input.GetKeyDown(rotateCameraLeftKey)){
            angleListIndex++;
            if(angleListIndex == 4){
                angleListIndex = 0;
            }

            destinationAngle = new Vector3(destinationAngle.x, angleList[angleListIndex], destinationAngle.z);
            isRotating = true;
        }
        else if(Input.GetKeyDown(rotateCameraRightKey)){
            angleListIndex--;
            if(angleListIndex == -1){
                angleListIndex = 3;
            }

            destinationAngle = new Vector3(destinationAngle.x, angleList[angleListIndex], destinationAngle.z);
            isRotating = true;
        }
    }

    private void RotateCameraToDestinationAngle(){
        float threshold = 0.01f;

        if(isRotating){
            if(Vector3.Distance(transform.eulerAngles, destinationAngle) > threshold) {
                transform.eulerAngles = Vector3.Lerp(transform.rotation.eulerAngles, destinationAngle, rotationSpeed * Time.deltaTime);
            }
            else {    
                transform.eulerAngles = destinationAngle;
                isRotating = false;
            }
        }
    }
}
