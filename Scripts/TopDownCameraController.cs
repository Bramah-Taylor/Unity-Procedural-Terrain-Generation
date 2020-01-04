using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    public float CameraSpeed = 1.0f;
    public float VerticalSpeedModifier = 1.0f;
    public float HorizontalSpeedModifier = 1.0f;
    public float MaxZoomDistance = 70.0f;
    public float MinZoomDistance = 15.0f;
    public float MouseSensitivity = 1800.0f;
    public float ZoomSensitivity = 200.0f;
    public float PanSensitivity = 60.0f;

    private float MinZoomSlackDistance;
    private float OldMouseXPosition;
    private float DefaultCameraDistance;

    public Camera CameraComponent;
    public Transform ScaleRefObject;

    // TODO:
    //      - Add zoom acceleration based on distance
    //      - Potentially use sphere tracing to prevent the camera from going through things?

    private void Start()
    {
        Cursor.visible = false;
        MinZoomSlackDistance = (MinZoomDistance * 2.0f) / 3.0f;
        DefaultCameraDistance = Vector3.Distance(transform.position, CameraComponent.transform.position);
    }

    void Update()
    {
        // Move the camera using the player's input axes
        float verticalMove = Input.GetAxis("Vertical") * CameraSpeed * VerticalSpeedModifier;
        float horizontalMove = Input.GetAxis("Horizontal") * CameraSpeed * HorizontalSpeedModifier;

        // Calculate a false forward vector for the camera by rotating the right transform vector by -90 degrees
        // We need to do this as the camera's true forward vector is facing into the scene, and altering this would zoom the camera instead of moving it
        Quaternion zToXQuat = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        Vector3 cameraFalseForward = zToXQuat * CameraComponent.transform.right;

        // Now perform the actual camera movement
        Vector3 desiredMove = cameraFalseForward * verticalMove + CameraComponent.transform.right * horizontalMove;
        transform.Translate(desiredMove * PanSensitivity * Time.deltaTime);

        // Zoom the camera in and out using mouse scroll delta
        float desiredZoomMagnitude = Input.mouseScrollDelta.y;

        // Get the centre of the screen - we'll use this to make sure the camera is still above the terrain
        Vector3 ScreenCentre = new Vector3((float)Screen.width / 2.0f, (float)Screen.height / 2.0f, 0.0f);

        // Transform the camera's centre in pixel coords (screen space) to a ray in world space
        Ray screenCentreRay = CameraComponent.ScreenPointToRay(ScreenCentre);
        RaycastHit hit;

        if (desiredZoomMagnitude - Mathf.Epsilon != 0.0f)
        {
            // Check that the camera is above the terrain by casting a ray into the scene down the camera's forward vector
            if (Physics.Raycast(screenCentreRay, out hit))
            {
                float PointDistance = Vector3.Distance(CameraComponent.transform.position, hit.point);

                // Check that the camera isn't getting too close to scene geometry
                // If it is, smoothly zoom out
                if (PointDistance < MinZoomSlackDistance)
                {
                    desiredZoomMagnitude = -0.1f * ((MinZoomSlackDistance - PointDistance) * (MinZoomSlackDistance - PointDistance));
                    PointDistance = MinZoomDistance + 1.0f;
                }
                // Do the reverse if the camera is getting too far away from the scene
                else if (PointDistance > MaxZoomDistance)
                {
                    desiredZoomMagnitude = 0.01f * (PointDistance - MaxZoomDistance);
                    PointDistance = MaxZoomDistance - 1.0f;
                }

                // Ensure that the camera is not still trying to get out of the desired scene bounds
                if (!(PointDistance < MinZoomDistance && desiredZoomMagnitude > 0.0f) &&
                !(PointDistance > MaxZoomDistance && desiredZoomMagnitude < 0.0f))
                {
                    // Now move the camera's position to the new desired position
                    desiredZoomMagnitude *= ZoomSensitivity * Time.deltaTime;
                    Vector3 desiredZoom = CameraComponent.transform.forward * desiredZoomMagnitude;
                    Vector3 copiedLocalPosition = CameraComponent.transform.localPosition;
                    copiedLocalPosition.y += desiredZoom.y;
                    copiedLocalPosition.z += desiredZoom.z;
                    CameraComponent.transform.localPosition = copiedLocalPosition;
                }
            }
            // If we didn't hit anything, we are underneath the scene
            // In this case, we want to snap out and above the geometry that's behind us
            else
            {

            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cursor.visible = !Cursor.visible;
        }

        if (Cursor.visible)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                SetRefObjectPosition();
            }
        }
        
        if (Input.GetKey(KeyCode.Mouse2))
        {
            float NewMouseXPosition = Input.GetAxis("Mouse X");
            if (OldMouseXPosition - Mathf.Epsilon == 0.0f)
            {
                OldMouseXPosition = Input.GetAxis("Mouse X");
            }
            else
            {
                if (Physics.Raycast(screenCentreRay, out hit))
                {
                    float mouseXDelta = (NewMouseXPosition - OldMouseXPosition) / (float)Screen.width;
                    CameraComponent.transform.RotateAround(hit.point, Vector3.up, 360.0f * mouseXDelta * MouseSensitivity * Time.deltaTime);
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            OldMouseXPosition = 0.0f;
        }
    }

    void SetRefObjectPosition()
    {
        Ray ray = CameraComponent.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 newPosition = hit.point;
            newPosition.y += 1.0f;
            ScaleRefObject.transform.position = newPosition;
        }
    }
}
