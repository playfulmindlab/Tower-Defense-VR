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

    SaveData playerSaveData;

    string timestamp = "";
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

    string newDataLine = "";
    //string folderID = "00";

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        //folderID = System.DateTime.Now.ToString("MM-dd/hh:mm:ss");
        Debug.Log(Application.dataPath + "/Data/EyeTrackingTempData.csv");

        playerSaveData = GameManager.instance.PlayerData();

        //if we're running the program in the Eeditor, delete the contents of the EyeTrackingTempData file
        //otherwise, create a new folder based on the time & date
#if UNITY_EDITOR
        StreamWriter writer = new StreamWriter(Application.dataPath + "/Resources/EyeTrackingTempData.csv", false);       
        writer.WriteLine("Timestamp,Participant,Game,Total Time Played,Session Number,Session Time Played,Map Number,Level Number,Wave Number,Event,Object,X Coordinates,Y Coordinates,Jump Status");
        writer.Close();
#else
        string folderID = System.DateTime.Now.ToString("MM-dd/hh:mm:ss");
        StreamWriter writer = new StreamWriter(Application.dataPath + "/TDEyeData_" + folderID + ".csv", false);
        writer.Write("First Line,Use this for Headers");
        writer.Close();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (writeWithUpdate)
        {
            newDataLine = "";

            StreamWriter writer;
            if (sendToCustomFile)
            {
                writer = new StreamWriter(customFilePath, true);
            }
            else
            {
                writer = new StreamWriter(Application.dataPath + "/Resources/EyeTrackingTempData.csv", true);
            }

            timestamp = System.DateTime.Now.ToString("HH.mm.ss.fff") + ",";

            newDataLine = System.DateTime.Now.ToString("HH.mm.ss.fff") + "," + "DataValue";

            writer.WriteLine(newDataLine);

            writer.Close();
        }
    }

    public static void NewFocus(GameObject newFocusObject)
    {

    }
}
