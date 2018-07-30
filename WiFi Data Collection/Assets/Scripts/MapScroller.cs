using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class MapScroller : MonoBehaviour {


    Touch touch;
    public float speed = 0.5f;
    public float perspectiveZoomSpeed = 0.1f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.1f;              // The rate of change of the orthographic size in orthographic mode.
    [SerializeField] private Text coordinateText;
    [SerializeField] private Camera m_camera;
    [SerializeField] private Image scanner_circle;

    float maxZoom = 100.0f;
    float minZoom = 20.0f;

    public GameObject cross;

    

    AndroidJavaClass pluginMainClass;
    AndroidJavaObject activityContext;

    


    // Use this for initialization
    void Start () {

        Screen.orientation = ScreenOrientation.Portrait;

#if UNITY_ANDROID
        pluginMainClass = new AndroidJavaClass("com.example.unitywifiplugin.WiFiPlugin");
        pluginMainClass.CallStatic("initialize");
#endif
        InvokeRepeating("checkStatus", 1f, 0.25f);

    }


    // Update is called once per frame
    void Update () {

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
            if (m_camera.orthographic)
            {
                m_camera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
                m_camera.orthographicSize = Mathf.Max(m_camera.orthographicSize, 0.1f);
            }
            else
            {  
                m_camera.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
                m_camera.fieldOfView = Mathf.Clamp(m_camera.fieldOfView, minZoom, maxZoom);
            }
        }


        if (Input.touchCount > 0)
        {

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                m_camera.transform.Translate(touchDeltaPosition.x * speed * Time.deltaTime, touchDeltaPosition.y * speed * Time.deltaTime, 0);
            }
        }

        coordinateText.text= "coordinates: ( " + cross.transform.position.x + " , " + cross.transform.position.y + " )";

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    public void ButtonOnClick()
    {
        pluginMainClass.CallStatic("takeMeasurement", new object[] { cross.transform.position.x, cross.transform.position.y});

 
    }

    public void onSaveButtonClick()
    {
        pluginMainClass.CallStatic("finishAndSave");
    }

    private bool checkStatus() {

        bool isScannerReady = pluginMainClass.CallStatic<bool>("isWiFiScannerReady");

        if (isScannerReady)
        {
            scanner_circle.color = Color.green;
        }
        else
        {
            scanner_circle.color = Color.red;
        }
        return isScannerReady;

    }
}
