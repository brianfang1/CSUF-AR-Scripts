using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//ARFoundationとARCoreExtensions関連を使用する
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace AR_Fukuoka
{
    public class SampleScript : MonoBehaviour
    {
        [SerializeField]
        ARRaycastManager m_RaycastManager;
        List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
        [SerializeField]
        Camera arCam;

        //GeospatialAPI
        public AREarthManager EarthManager;
        //GeospatialAPI ARCore
        public VpsInitializer Initializer;
        //UI 
        public Text OutputText;
        
        public double HeadingThreshold = 25; //Allowable accuracy of azimuth [ORIGINAL]
        public double HorizontalThreshold = 20; //Allowable accuracy of horizontal position [ORIGINAL]
        

        private struct prefabLocation
        {
            public double Latitude;
            public double Longitude;

            public prefabLocation( double Latitude, double Longitude) 
            {
                this.Latitude = Latitude;
                this.Longitude = Longitude;
            }
        }

        private prefabLocation myHomePrefab = new prefabLocation(33.9843667988129, -117.899593302068);
        private prefabLocation recCenterPrefab = new prefabLocation(33.88271273750798, -117.88765951159485);
        private prefabLocation titanShopsPrefab = new prefabLocation(33.88154974343308, -117.88678588803086);
        private prefabLocation tsuPrefab = new prefabLocation(33.881619769735295, -117.88743881625606);
        private prefabLocation pollakLibraryPrefab = new prefabLocation(33.88152899488228, -117.88577994118933);
        private prefabLocation healthCenterPrefab = new prefabLocation(33.88272091885041, -117.88424348643913);
        private prefabLocation ecsPrefab = new prefabLocation(33.88232971232807, -117.88295649608438);
        private prefabLocation humanitiesPrefab = new prefabLocation(33.88045607974854, -117.88448836035454);

        public GameObject myHomeObj, recCenterObj, titanShopsObj, tsuObj, pollakLibraryObj, healthCenterObj, ecsObj,
        humanitiesObj;
        private bool isCsufPrefabInstantiated = false;
        public double Latitude;  //Latitude to place the object
        public double Longitude; //Longtitude to place the object
        public double Altitude; //Height to place the object
        public double Heading; //Object Orientation (N=0)
        public GameObject ContentPrefab; //Original Data of display object
        public ARAnchorManager AnchorManager; //Used to create anchors

        // Start is called before the first frame update
        void Start()
        {
            myHomeObj = null;
            recCenterObj = null;
            titanShopsObj = null;
            tsuObj = null;
            pollakLibraryObj = null;
            arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
            m_RaycastManager=GetComponent<ARRaycastManager>();
        }
        // Update is called once per frame
        void Update()
        {
            string status = "";
            // If initialization fails or you do not want to track, do nothing and return
            if (!Initializer.IsReady || EarthManager.EarthTrackingState != TrackingState.Tracking)
            {
                return;
            }
            // Get tracking results
            GeospatialPose pose = EarthManager.CameraGeospatialPose;

            // Describe here the behavior according to tracking accuracy
            //Tracking accuracy is worse than the schedule
            if (pose.HeadingAccuracy > HeadingThreshold ||
                 pose.HorizontalAccuracy > HorizontalThreshold)
            {
                status = "Low tracking accuracy";
            }
            else //Tracking accuracy is above threshold 
            {
                status = "High tracking accuracy";
                instantiatePrefab(ref recCenterObj, recCenterPrefab, pose);
                instantiatePrefab(ref titanShopsObj, titanShopsPrefab, pose);
                instantiatePrefab(ref tsuObj, tsuPrefab, pose);
                instantiatePrefab(ref pollakLibraryObj, pollakLibraryPrefab, pose);
                instantiatePrefab(ref healthCenterObj, healthCenterPrefab, pose);
                instantiatePrefab(ref ecsObj, ecsPrefab, pose);
                instantiatePrefab(ref humanitiesObj, humanitiesPrefab, pose);

                isCsufPrefabInstantiated = true;
            }
            
            RaycastHit hit;
            Ray ray = arCam.ScreenPointToRay(Input.GetTouch(0).position);
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            string objName = "TestTeASDADASasdfsfFFSFFSFSFSFSasdaFS"; 

            //check for touch to toggleOutline
            if (Input.touchCount == 0)
            {
                ShowTrackingInfo(status, pose, 0, "");
                return;
            }

            if(Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                ShowTrackingInfo(status, pose, Input.touchCount, touch.position.ToString());
                if(Physics.Raycast(raycast, out hit) && touch.phase == TouchPhase.Began){
                    OutputText.text = OutputText.text + "HIT \n";
                    if(hit.collider.tag == "TouchPrefab") {
                        GameObject myObj = hit.collider.gameObject;
                        OutputText.text = OutputText.text + "You touched " + myObj.name + " \n";
                        myObj.GetComponent<Outline>().enabled = !myObj.GetComponent<Outline>().enabled;
                        toggleDialogueBox();
                    }
                } else {
                    OutputText.text = OutputText.text + "MISS \n";
                }
            }
        }       

        void ShowTrackingInfo(string status, GeospatialPose pose, int touchCount = 0, string touchPosition = "")
        {
            // Displays Lat, Lng, Alt and their position
            OutputText.text = string.Format(
                "Latitude/Longitude: {0}°, {1}°\n" +
                "Horizontal Accuracy: {2}m\n" +
                "Altitude: {3}m\n" +
                "Vertical Accuracy: {4}m\n" +
                "Heading: {5}°\n" +
                "Heading Accuracy: {6}°\n" +
                "{7} \n" +
                "Touch Count: {8} \n" +
                "Touch Position: {9} \n"
                ,
                pose.Latitude.ToString("F6"),  //{0}
                pose.Longitude.ToString("F6"), //{1}
                pose.HorizontalAccuracy.ToString("F6"), //{2}
                pose.Altitude.ToString("F2"),  //{3}
                pose.VerticalAccuracy.ToString("F2"),  //{4}
                pose.Heading.ToString("F1"),   //{5}
                pose.HeadingAccuracy.ToString("F1"),   //{6}
                status, //{7},
                touchCount.ToString(),
                touchPosition
            );
        }

        void toggleOutline()
        {
            if (Input.touchCount == 0)
            {
                return;
            }
            RaycastHit hit;
            Ray ray = arCam.ScreenPointToRay(Input.GetTouch(0).position);

            if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
            {
                if(Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if(Physics.Raycast(ray, out hit))
                    {
                        if(hit.collider.gameObject.tag == "TouchPrefab")
                        {
                            var myObj = hit.collider.gameObject;
                            myObj.GetComponent<Outline>().enabled = !myObj.GetComponent<Outline>().enabled;
                        }
                        // Don't need this. This will spawn the prefab. Taken from tutorial but is not useful for our usecase.
                        // else 
                        // {
                        //     SpawnPrefab(m_Hits[0].pose.position);
                        // }
                    }
                }
            }
        }

        void toggleDialogueBox()
        {
            // Create different dialogue each prefab location. We can choose which dialogue box to display based on name of prefab
            GameObject.Find("Canvas").GetComponent<Canvas>().transform.Find("DialogueBox").gameObject.SetActive(true);
            // GameObject myCanvas = GameObject.Find("Canvas").GetComponent<Canvas>().gameObject;

            // GameObject dialogueBox = myCanvas.transform.Find("DialogueBox").gameObject;
            // // dialogueBox.SetActive(!dialogueBox.activeInHierarchy);
            // dialogueBox.SetActive(true);

        }

        void instantiatePrefab(ref GameObject displayObject, prefabLocation prefabInfo, GeospatialPose pose)
        {
            if(displayObject == null)
            {
                //Height of the phone - 1.5m to be approximately the height of the ground
                Altitude = pose.Altitude - 1.5f;

                // Angle correction (Because the anchor generation function assumes South=0)
                Quaternion quaternion = Quaternion.AngleAxis(180f - (float)Heading, Vector3.up);

                // Create anchors at specified position and orientation.
                ARGeospatialAnchor Anchor = AnchorManager.AddAnchor(prefabInfo.Latitude, prefabInfo.Longitude, Altitude, quaternion);
        
                // Materialize the object if the anchor is correctly created
                if(Anchor != null)
                {
                    displayObject = Instantiate(ContentPrefab, Anchor.transform);
                    
                    // Add outline
                    var outline = displayObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineColor = Color.yellow;
                    outline.OutlineWidth = 5f;
                    outline.enabled = false;
                }
            }
        }
    }
}


