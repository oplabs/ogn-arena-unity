using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

public class FlyCamera : MonoBehaviour
{

    float distance = 3.0f;
    public float xSpeed = 20.0f;
    public float ySpeed = 20.0f;
    public float scrollrate = 3.0f;
    float followRate = 0.5f;
    float yMinLimit = -60f;
    float yMaxLimit = 60f;

    const float distanceMin = 1f;
    const float distanceMax = 5f;
    public Vector3 Offset;
    public bool AlwaysOn = false;
    public float ZoomSensitivity = 0.02f;

    public bool singleTouchOrbiting = true;
    public bool pinchToZoom = true;

    private Vector2 lastTouchPosition;
    private bool isMobile;

    [DllImport("__Internal")]
    private static extern bool IsMobile();

    public bool isWebMobile()
    {
 #if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
 #else
        return false;
 #endif
    }


    // Use this for initialization
    void Start()
    {
        isMobile = (SystemInfo.deviceType == DeviceType.Handheld) || isWebMobile();
        Debug.Log("Is Mobile device:" + isMobile);
    }

    public void Reset()
    {
    }

    Vector3 getTarget()
    {
        GameObject target = GameObject.Find("hero").transform.GetChild(0).gameObject;
        return target.transform.GetChild(0).gameObject.transform.position + new Vector3(0, 1.0f, 0);
    }

    

    void LateUpdate()
    {
        Update();
    }

    void Update()
    {
        GameObject light = GameObject.Find("Directional Light");
        GameObject parent = GameObject.Find("hero");
        Renderer renderTarget = parent.transform.GetChild(0).gameObject.GetComponent<Renderer>();

        Vector3 tgt = renderTarget.bounds.center + new Vector3(0, 0.5f, 0);
        //Debug.Log("Target is:" + tgt);
        Quaternion rotation = transform.rotation;
        //Debug.Log("Is Mobile device:" + (SystemInfo.deviceType == DeviceType.Handheld));
        
        // if it's hand held then no mouse check
        bool IsMouseOverGameWindow = !(isMobile || 0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y);
        //DOS Modified tweaked this to be selectable
        if (IsMouseOverGameWindow && Input.touchCount == 0)
        {
            float targetX = (Input.mousePosition.x / Screen.width - 0.5f) * 290f;
            float targetY = (Input.mousePosition.y / Screen.height - 0.5f) * 70f; // don't go all the way up
   

            //rotation = Quaternion.Euler(targetY, targetX, 0);
            //move toward target rotation
            rotation = Quaternion.Lerp(rotation, Quaternion.Euler(targetY, targetX, 0), 0.5f);
        }
        else if (Input.touchCount == 1)
        {
            if (AlwaysOn || EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) == false)
            {
                Vector3 tRay = tgt - transform.position;
                Quaternion _r = Quaternion.LookRotation(tRay, Vector3.up);
                Vector3 angles = _r.eulerAngles;
                //Debug.Log("Euler is:" + angles);
                float x = angles.y;
                float y = angles.x;

                if (y > 180f)
                {
                    //Debug.Log("overflow on:" + y);
                    y = y - 360f;
                }

                if (x > 180f)
                {
                    x = x - 360f;
                }

                Touch touchZero = Input.GetTouch(0);

                if (touchZero.phase == TouchPhase.Moved && touchZero.position != lastTouchPosition)
                {
                    // don't take the delta if it's too large a time
                    x += touchZero.deltaPosition.x * xSpeed * 0.04f;
                    y -= touchZero.deltaPosition.y * ySpeed * 0.02f;
 
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                    //Debug.Log("PostClamp Y is:" + y);


                    Quaternion _toR = Quaternion.Euler(y, x, 0);
                    rotation = Quaternion.Lerp(rotation, _toR, Time.deltaTime * 5f);

                    lastTouchPosition = touchZero.position;
             
                }
            }
        }

 

        if (Input.touchCount == 2 && pinchToZoom)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Moved)
            {
                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame. Flip it so it goes the right way
                float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) * ZoomSensitivity;
                distance = Mathf.Clamp(distance - (deltaMagnitudeDiff / 10.0f), distanceMin, distanceMax);
            }
        }
        else
        {
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollrate, distanceMin, distanceMax);
        }


        float currentDistance = Vector3.Distance(tgt, transform.position);

        //Debug.Log("Current distance is:" + currentDistance);
        if (distance > currentDistance + followRate)
        {
            currentDistance += followRate;

        }
        else if (distance < currentDistance - followRate)
        {
            currentDistance -= followRate;
        } else
        {
            currentDistance = distance;
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + tgt;

        transform.rotation = rotation;
        transform.position = position;

        // light follow from above
        light.transform.position = position + new Vector3(0, 1.5f, 0);
        light.transform.LookAt(tgt - new Vector3(0, 0.5f, 0));
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        // These must be "while" loops. It's possible at low framerate that we have moved more than 360 degrees.
        while (angle < -360F)
            angle += 360F;
        while (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
