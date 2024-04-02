using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class EyeTrackingRecorder : MonoBehaviour
{
    public static EyeTrackingRecorder instance;

    [SerializeField] bool writeWithUpdate = false;

    [SerializeField] bool sendToCustomFile = false;
    [SerializeField] string customFilePath = "N/A";

    //SaveData playerSaveData;

    /*string timestamp = "";
    string participantID = "";
    string gameName = "Tower Defense VR";
    float totalTimePlayed = 0f;
    int sessionNumber = 0;
    float currentSessionTimePlayed = 0f;
    int mapNumber = 0;
    int levelNumber = 0;
    int waveNumber = 0;
    string occuringEvent = "";
    string occuringObject = "";
    Vector3 occuringObjectPosition = Vector3.zero;
    bool jumpStatus = false;
    */

    public GameObject seenObjectL = null, seenObjectR = null;
    //Vector3 seenObjectsPosition = Vector3.zero;

    string newDataLine = "";
    //string folderID = "00";

    string filePath = "";
    string seenPosFormatted = "";

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        //folderID = System.DateTime.Now.ToString("MM-dd/hh:mm:ss");
        Debug.Log(Application.dataPath + "/Resources/EyeTrackingTempData.csv");

        //playerSaveData = GameManager.instance.PlayerData();

        //if we're running the program in the Eeditor, delete the contents of the EyeTrackingTempData file
        //otherwise, create a new folder based on the time & date
#if UNITY_EDITOR
        StreamWriter writer = new StreamWriter(Application.dataPath + "/Resources/EyeTrackingTempData.csv", false);
        //writer.WriteLine("Timestamp,Participant,Game,Total Time Played,Session Number,Session Time Played,Map Number,Level Number,Wave Number,Event,Object,X Coordinates,Y Coordinates,Jump Status");
        writer.WriteLine("Timestamp,Seen GameObject,Seen Object Coordinates");
        writer.Close();
#else
        string folderID = System.DateTime.Now.ToString("MM-dd/hh:mm:ss");
        StreamWriter writer = new StreamWriter(Application.dataPath + "/TDEyeData_" + folderID + ".csv", false);
        //writer.Write("First Line,Use this for Headers");
        writer.WriteLine("Timestamp,Seen GameObject,Seen Object Coordinates");
        writer.Close();
#endif

        if (sendToCustomFile ) { filePath = customFilePath; }
        else { filePath = Application.dataPath + "/Resources/EyeTrackingTempData.csv"; }
    }

    // Update is called once per frame
    void Update()
    {
        if (writeWithUpdate)
        {
            newDataLine = "";
            seenPosFormatted = "";

            StreamWriter writer;
            writer = new StreamWriter(filePath, true);

            newDataLine = System.DateTime.Now.ToString("HH.mm.ss.fff");

            if (seenObjectL != null)
            {
                Vector3 pos = seenObjectL.transform.position;
                //seenPosFormatted = string.Format("{0,3:f3};{1,3:f3};{2,3:f3}", pos.x, pos.y, pos.z);
                seenPosFormatted = pos.x + " | " + pos.y + " | " + pos.z;
                newDataLine += "," + seenObjectL.name + "," + seenPosFormatted;
            }

            else if (seenObjectR != null)
            {
                Vector3 pos = seenObjectR.transform.position;
                //seenPosFormatted = string.Format("{0,3:f3};{1,3:f3};{2,3:f3}", pos.x, pos.y, pos.z);
                seenPosFormatted = pos.x + " | " + pos.y + " | " + pos.z;
                newDataLine += "," + seenObjectR.name + "," + seenPosFormatted;
            }

            else if (seenObjectL == null && seenObjectR == null)
            {
                newDataLine += ",N/A,N/A";
            }

            writer.WriteLine(newDataLine);

            seenObjectL = null;
            seenObjectR = null;
            writer.Close();
        }
    }

    public static void NewFocus(GameObject newFocusObject)
    {

    }
}
