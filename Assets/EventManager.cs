using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
//using System.Diagnostics.Eventing.Reader;
using LSL;

public class EventManager : MonoBehaviour//Singleton<Analytics>
{
    public static EventManager instance;

    /*
    public InputField partID;
    public InputField partAge;
    public InputField partGender;
    public InputField condition;
    public InputField trial;
    public InputField group;
    public Toggle compeditive;

    public string sessionTime;
    public GameObject VRcamHeadPos;
    public GameObject rightHand;
    public GameObject handRayCast;
    public GameObject Partner_handRayCast;
    public GameObject Partner_rightHand;
    public GameObject Partner_VRcamHeadPos;

    private Vector3[] RayPoints = new Vector3[2];
    private Vector3[] Partner_RayPoints = new Vector3[2];

    public bool assignPartner = false;

    */
    GameObject eyeTracker;

    //LSL 
    string StreamName = "LSL4Unity.Samples.LoomTest1";
    string StreamType = "Markers";
    private StreamOutlet outlet;
    private string[] sample = new string[30];





    string fileLocation = "";

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        //SwitchFileLocations("Main Menu Events");
    }

    void Start()
    {
        //eyeTracker = GameObject.FindGameObjectWithTag("eye tracker");
        // TODO: Refactor to add sub-folder according to participant id for better segregation
        //savePath = Application.dataPath + "/Events/";

        SwitchFileLocations("Main Menu Events");

        //eventCSVPath = savePath + 

        //filePath = savePath + "analytics.json";
        //csvPath = savePath + "analytics.csv";
        //filePath2 = savePath + "analytics2.json";
        //csvPath2 = savePath + "analytics2.csv";
        //filePath3 = savePath + "analytics3.json";
        //csvPath3 = savePath + "analytics3.csv";
        //filePath4 = savePath + "analytics4.json";
        //csvPath4 = savePath + "analytics4.csv";

        /*if (!File.Exists(savePath))
        {
            Debug.Log("making new analytics forlder");
            Directory.CreateDirectory(savePath);
        }
        try
        {
            if (!File.Exists(csvPath))
                File.WriteAllText(csvPath, "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, eyePosX, eyePosY, eyePosZ, headPosX, HeadPosY, HeadPosZ, HeadRotX, HeadRotY, HeadRotZ, HandPosX, HandPosY, HandPosZ, HandRotX, HandRotY, HandRotZ, currentGazeTarget, EnvironmentGazeTarget, Handedness, RightPupil, LeftPupil, Group");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }*/
        //SyncManager.shared.initialSetup();
        //sessionTime = TimerScript.instance.currentTime.ToString();

        //LSL
        //var hash = new Hash128();
        //hash.Append(StreamName);
        //hash.Append(StreamType);
        //hash.Append(gameObject.GetInstanceID());

        //LSL varibale declaration.

       // StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, 1, LSL.LSL.IRREGULAR_RATE,
       //     channel_format_t.cf_string, hash.ToString());
        //outlet = new StreamOutlet(streamInfo);


        //sample[0] = "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, eyePosX, eyePosY, eyePosZ, Group";
        //outlet.push_sample(sample);
    }

    public void SwitchFileLocations(string newLocation = "N/A")
    {
        string newFileLocation = "";
        Debug.Log(Application.dataPath);

        if (newLocation == "N/A")
        {
            SaveData playerData = GameManager.instance.PlayerData;
            newFileLocation = Application.dataPath + "/Events/" + playerData.PlayerID + "/";
        }
        else
        {
            newFileLocation = Application.dataPath + "/Events/" + newLocation + "/";
        }

        if (!File.Exists(newFileLocation))
        {
            Directory.CreateDirectory(newFileLocation);
        }

        newFileLocation += GameManager.instance.SessionStartTime.ToString("MM-yy - HH.mm.ss") + ".csv";
        File.WriteAllText(newFileLocation, "Timestamp,Participant,Game,Total Time Played,Session Number,Session Time Played,Map Number,Level Number,Wave Number,Event,Gameobject,Gameobject Coordinates,Jump Status\n");
        fileLocation = newFileLocation;

        Debug.Log("Swapped Files.");
    }

    public void RecordNewEvent(DataEvent newEvent)
    {
        Debug.Log("Recording Event...");
        string newEventString = string.Join(",", newEvent.GetDataStringArray());

        StreamWriter writer;
        writer = new StreamWriter(fileLocation, true);

        writer.WriteLine(newEventString);

        writer.Close();

        Debug.Log("Event Recorded!");
    }

    // Update is called once per frame
    /*void Update()
    {
        if (assignPartner)
        {
            // AssignPartnerObjects
        }


        if (GameManager.instance.eyeTracking)
        {
            DataPoint data = new DataPoint();
            data.timestamp = DateTime.Now.Ticks.ToString();
            data.participant = PlayerPrefs.GetString("ParticipantID");
            data.condition = PlayerPrefs.GetString("ParticipantCondition");
            data.trial = PlayerPrefs.GetString("trial");
            data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
            data.gender = PlayerPrefs.GetString("ParticipantGender");
            data.group = PlayerPrefs.GetString("group");
            data.task = "Loom";
            data.handedness = "Right";
            data.eventName = "playing";

            data.currentGazeTraget = EyeTracker.instance.gazeTarget;
            data.currentGazeArea = EyeTracker.instance.gazeArea;

            if (EyeTracker.instance.gazeTarget != "none")
            {
                data.eyePosX = EyeTracker.instance.hit1.point.x.ToString();
                data.eyePosY = EyeTracker.instance.hit1.point.y.ToString();
                data.eyePosZ = EyeTracker.instance.hit1.point.z.ToString();

            }
            else if (EyeTracker.instance.gazeArea != "none")
            {
                data.eyePosX = EyeTracker.instance.hit2.point.x.ToString();
                data.eyePosY = EyeTracker.instance.hit2.point.y.ToString();
                data.eyePosZ = EyeTracker.instance.hit2.point.z.ToString();
            }
            else
            {
                data.eyePosX = "N/A";
                data.eyePosY = "N/A";
                data.eyePosZ = "N/A";
            }

            data.eyeAngleX = EyeTracker.instance.gazePoint.x.ToString();
            data.eyeAngleY = EyeTracker.instance.gazePoint.y.ToString();
            data.eyeAngleZ = EyeTracker.instance.gazePoint.z.ToString();
            data.headPosX = VRcamHeadPos.transform.position.x.ToString();
            data.headPosY = VRcamHeadPos.transform.position.y.ToString();
            data.headPosZ = VRcamHeadPos.transform.position.z.ToString();
            data.headRotX = VRcamHeadPos.transform.rotation.x.ToString();
            data.headRotY = VRcamHeadPos.transform.rotation.y.ToString();
            data.headRotZ = VRcamHeadPos.transform.rotation.z.ToString();
            data.handPosX = rightHand.transform.position.x.ToString();
            data.handPosY = rightHand.transform.position.y.ToString();
            data.handPosZ = rightHand.transform.position.z.ToString();
            data.handRotX = rightHand.transform.rotation.x.ToString();
            data.handRotY = rightHand.transform.rotation.y.ToString();
            data.handRotZ = rightHand.transform.rotation.z.ToString();

            Vector3 rayCastStartPos = GetRaycastStartPos(handRayCast.GetComponent<LineRenderer>(), RayPoints);
            data.rayCastStartPosX = rayCastStartPos.x.ToString();
            data.rayCastStartPosY = rayCastStartPos.y.ToString();
            data.rayCastStartPosZ = rayCastStartPos.z.ToString();

            Vector3 rayCastEndPos = GetRaycastEndPos(handRayCast.GetComponent<LineRenderer>(), RayPoints);
            data.rayCastEndPosX = rayCastEndPos.x.ToString();
            data.rayCastEndPosY = rayCastEndPos.y.ToString();
            data.rayCastEndPosZ = rayCastEndPos.z.ToString();
 



            data.rigthPupil = eyeTracker.GetComponent<EyeTracker>().rightEyePupil_diameter.ToString();
            data.leftPupil = eyeTracker.GetComponent<EyeTracker>().leftEyePupil_diameter.ToString();
            string csvstring = String.Join(",", GetDataArray(data));
            //LSL output.
            sample[0] = csvstring;
            outlet.push_sample(sample);

            //File.AppendAllText(csvPath, "\n");
            //File.AppendAllText(csvPath, csvstring);

        }
        /*
        string csvstring1 = String.Join(",", "Abhinav");
        sample[0] = csvstring1;
        File.AppendAllText(csvPath, "\n");
        File.AppendAllText(csvPath, csvstring1);
        outlet.push_sample(sample);
        }*/
    

   /* public void writeEvent(string eventString)
    {
        DataPoint data = new DataPoint();
        data.timestamp = DateTime.Now.Ticks.ToString();
        data.participant = PlayerPrefs.GetString("ParticipantID");
        data.condition = PlayerPrefs.GetString("ParticipantCondition");
        data.trial = PlayerPrefs.GetString("trial");
        data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
        data.gender = PlayerPrefs.GetString("ParticipantGender");
        data.group = PlayerPrefs.GetString("group");
        data.task = "Loom";
        //data.currentGazeTraget = EyeTracker.instance.gazeTarget;
        //data.currentGazeArea = EyeTracker.instance.gazeArea;
        // Handedness is Right by default
        data.handedness = "Right";
        data.eventName = eventString;
/*
        if (EyeTracker.instance.gazeTarget != "none")
        {
            data.eyePosX = EyeTracker.instance.hit1.point.x.ToString();
            data.eyePosY = EyeTracker.instance.hit1.point.y.ToString();
            data.eyePosZ = EyeTracker.instance.hit1.point.z.ToString();

        }
        else if (EyeTracker.instance.gazeArea != "none")
        {
            data.eyePosX = EyeTracker.instance.hit2.point.x.ToString();
            data.eyePosY = EyeTracker.instance.hit2.point.y.ToString();
            data.eyePosZ = EyeTracker.instance.hit2.point.z.ToString();
        }
        else
        {
            data.eyePosX = "N/A";
            data.eyePosY = "N/A";
            data.eyePosZ = "N/A";
        }

        data.eyeAngleX = EyeTracker.instance.gazePoint.x.ToString();
        data.eyeAngleY = EyeTracker.instance.gazePoint.y.ToString();
        data.eyeAngleZ = EyeTracker.instance.gazePoint.z.ToString();
        data.headPosX = VRcamHeadPos.transform.position.x.ToString();
        data.headPosY = VRcamHeadPos.transform.position.y.ToString();
        data.headPosZ = VRcamHeadPos.transform.position.z.ToString();
        data.headRotX = VRcamHeadPos.transform.rotation.x.ToString();
        data.headRotY = VRcamHeadPos.transform.rotation.y.ToString();
        data.headRotZ = VRcamHeadPos.transform.rotation.z.ToString();
        data.handPosX = rightHand.transform.position.x.ToString();
        data.handPosY = rightHand.transform.position.y.ToString();
        data.handPosZ = rightHand.transform.position.z.ToString();
        data.handRotX = rightHand.transform.rotation.x.ToString();
        data.handRotY = rightHand.transform.rotation.y.ToString();
        data.handRotZ = rightHand.transform.rotation.z.ToString();

        Vector3 rayCastStartPos = GetRaycastStartPos(handRayCast.GetComponent<LineRenderer>(), RayPoints);
        data.rayCastStartPosX = rayCastStartPos.x.ToString();
        data.rayCastStartPosY = rayCastStartPos.y.ToString();
        data.rayCastStartPosZ = rayCastStartPos.z.ToString();

        Vector3 rayCastEndPos = GetRaycastEndPos(handRayCast.GetComponent<LineRenderer>(), RayPoints);
        data.rayCastEndPosX = rayCastEndPos.x.ToString();
        data.rayCastEndPosX = rayCastEndPos.y.ToString();
        data.rayCastEndPosZ = rayCastEndPos.z.ToString();




        data.rigthPupil = eyeTracker.GetComponent<EyeTracker>().rightEyePupil_diameter.ToString();
        data.leftPupil = eyeTracker.GetComponent<EyeTracker>().leftEyePupil_diameter.ToString();


        string csvstring = String.Join(",", GetDataArray(data));
        //LSL output.
        sample[0] = csvstring;


        outlet.push_sample(sample);
        File.AppendAllText(csvPath, "\n");
        File.AppendAllText(csvPath, csvstring);
    }

    public void WriteData(string eventString, string participant, string sessionTime, string testX, string testY, string testZ)
    {
        DataPoint data = new DataPoint();
        data.timestamp = DateTime.Now.Ticks.ToString();
        data.participant = PlayerPrefs.GetString("ParticipantID");
        data.task = PlayerPrefs.GetString("ParticipantCondition");
        data.trial = PlayerPrefs.GetString("trial");
        data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
        data.gender = PlayerPrefs.GetString("ParticipantGender");
        data.eventName = eventString;
        /*        data.testX = testX;
                data.testY = testY;
                data.testZ = testZ;*/
        /*data.group = PlayerPrefs.GetString("group");


        string jsonString = JsonUtility.ToJson(data);
        string csvstring = String.Join(",", GetDataArray(data));

        sample[0] = csvstring;
        outlet.push_sample(sample);
        //File.AppendAllText(filePath, "\n");
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, eyePosX, eyePosY, eyePosZ, Group");
        }
        File.AppendAllText(csvPath, "\n");
        File.AppendAllText(filePath, jsonString);
        File.AppendAllText(csvPath, csvstring);
        
    }
    public void WriteData2(string eventString, string participant, string sessionTime, string testX, string testY, string testZ)
    {
        DataPoint data = new DataPoint();
        data.timestamp = DateTime.Now.Ticks.ToString();
        data.participant = PlayerPrefs.GetString("ParticipantID");
        data.task = PlayerPrefs.GetString("ParticipantCondition");
        data.trial = PlayerPrefs.GetString("trial");
        data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
        data.gender = PlayerPrefs.GetString("ParticipantGender");
        data.eventName = eventString;
        /*        data.testX = testX;
                data.testY = testY;
                data.testZ = testZ;*/
       /* data.group = PlayerPrefs.GetString("group");



        string jsonString = JsonUtility.ToJson(data);
        string csvstring = String.Join(",", GetDataArray(data));
        //LSL output.
        sample[0] = csvstring;

        outlet.push_sample(sample);
        //File.AppendAllText(filePath, "\n");
        if (!File.Exists(csvPath2))
        {
            File.WriteAllText(csvPath2, "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, xPos, yPos, zPos, Group");
        }
        File.AppendAllText(csvPath2, "\n");
        File.AppendAllText(filePath2, jsonString);
        File.AppendAllText(csvPath2, csvstring);
    }

    public void WriteData3(string eventString, string participant, string sessionTime, string testX, string testY, string testZ)
    {
        DataPoint data = new DataPoint();
        data.timestamp = DateTime.Now.Ticks.ToString();
        data.participant = PlayerPrefs.GetString("ParticipantID");
        data.task = PlayerPrefs.GetString("ParticipantCondition");
        data.trial = PlayerPrefs.GetString("trial");
        data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
        data.gender = PlayerPrefs.GetString("ParticipantGender");
        data.eventName = eventString;
        /*data.testX = testX;
        data.testY = testY;
        data.testZ = testZ;*/
       /* data.group = PlayerPrefs.GetString("group");



        string jsonString = JsonUtility.ToJson(data);
        string csvstring = String.Join(",", GetDataArray(data));

        sample[0] = csvstring;

        outlet.push_sample(sample);
        //File.AppendAllText(filePath, "\n");
        if (!File.Exists(csvPath3))
        {
            File.WriteAllText(csvPath3, "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, rightEye, leftEye, zPos, Group");
        }
        File.AppendAllText(csvPath3, "\n");
        File.AppendAllText(filePath3, jsonString);
        File.AppendAllText(csvPath3, csvstring);
    }
    public void WriteData4(string eventString, string participant, string sessionTime, string xRow, string yRow, string zRow, string xPos, string yPos, string zPos)
    {
        DataPoint data = new DataPoint();
        data.timestamp = DateTime.Now.Ticks.ToString();
        data.participant = PlayerPrefs.GetString("ParticipantID");
        data.task = PlayerPrefs.GetString("ParticipantCondition");
        data.trial = PlayerPrefs.GetString("trial");
        data.Age = PlayerPrefs.GetString("ParticipantAge").ToString();
        data.gender = PlayerPrefs.GetString("ParticipantGender");
        data.eventName = eventString;
        /*data.testX = xRow;
        data.testY = yRow;
        data.testZ = zRow;
        data.x = xPos;
        data.y = yPos;
        data.z = zPos;*/
       /* data.group = PlayerPrefs.GetString("group");


        string jsonString = JsonUtility.ToJson(data);
        string csvstring = String.Join(",", GetDataArray(data));
        //LSL output.
        sample[0] = csvstring;

        outlet.push_sample(sample);
        //File.AppendAllText(filePath, "\n");
        if (!File.Exists(csvPath4))
        {
            File.WriteAllText(csvPath4, "TimeStamp,participant,Condition,Tiral,Age,Gender,SessionTime,Event, xRotation, yRotation, zRotation, xPos, yPos, Zpos, Group");
        }
        File.AppendAllText(csvPath4, "\n");
        File.AppendAllText(filePath4, jsonString);
        File.AppendAllText(csvPath4, csvstring);
    }

    string[] GetDataArray(DataPoint data)
    {
        List<string> stringlist = new List<string>();
        stringlist.Add(data.timestamp);
        stringlist.Add(data.participant);
        stringlist.Add(data.Age);
        stringlist.Add(data.gender);
        stringlist.Add(data.handedness);
        stringlist.Add(data.task);
        stringlist.Add(data.group);
        stringlist.Add(data.condition);
        stringlist.Add(data.trial);
        stringlist.Add(data.eventName);
        stringlist.Add(data.currentGazeTraget);
        stringlist.Add(data.currentGazeArea);
        stringlist.Add(data.eyePosX);
        stringlist.Add(data.eyePosY);
        stringlist.Add(data.eyePosZ);
        stringlist.Add(data.eyeAngleX);
        stringlist.Add(data.eyeAngleY);
        stringlist.Add(data.eyeAngleZ);
        stringlist.Add(data.headPosX);
        stringlist.Add(data.headPosY);
        stringlist.Add(data.headPosZ);
        stringlist.Add(data.headRotX);
        stringlist.Add(data.headRotY);
        stringlist.Add(data.headRotZ);
        stringlist.Add(data.handPosX);
        stringlist.Add(data.handPosY);
        stringlist.Add(data.handPosZ);
        stringlist.Add(data.handRotX);
        stringlist.Add(data.handRotY);
        stringlist.Add(data.handRotZ);
        stringlist.Add(data.rayCastStartPosX);
        stringlist.Add(data.rayCastStartPosY);
        stringlist.Add(data.rayCastStartPosZ);
        stringlist.Add(data.rayCastEndPosX);
        stringlist.Add(data.rayCastEndPosY);
        stringlist.Add(data.rayCastEndPosZ);
        stringlist.Add(data.Partner_headPosX);
        stringlist.Add(data.Partner_headPosY);
        stringlist.Add(data.Partner_headPosZ);
        stringlist.Add(data.Partner_headRotX);
        stringlist.Add(data.Partner_headRotY);
        stringlist.Add(data.Partner_headRotZ);
        stringlist.Add(data.Partner_handPosX);
        stringlist.Add(data.Partner_handPosY);
        stringlist.Add(data.Partner_handRotX);
        stringlist.Add(data.Partner_handRotY);
        stringlist.Add(data.Partner_handRotZ);
        stringlist.Add(data.Partner_handPosZ);
        stringlist.Add(data.Partner_rayCastStartPosX);
        stringlist.Add(data.Partner_rayCastStartPosY);
        stringlist.Add(data.Partner_rayCastStartPosZ);
        stringlist.Add(data.Partner_rayCastEndPosX);
        stringlist.Add(data.Partner_rayCastEndPosY);
        stringlist.Add(data.Partner_rayCastEndPosZ);
        stringlist.Add(data.rigthPupil);
        stringlist.Add(data.leftPupil);

        return stringlist.ToArray();
    }

    public void SetParticipantID(string id)
    {
        PlayerPrefs.SetString("ParticipantID", id);
    }
    public void SetParticipantAge(string age)
    {
        PlayerPrefs.SetString("ParticipantAge", age);
    }
    public void SetParticipantCondition(string con)
    {
        PlayerPrefs.SetString("ParticipantCondition", con);
    }
    public void Settrial(string trial)
    {
        PlayerPrefs.SetString("trial", trial);
    }
    public void SetParticipantGender(string gen)
    {
        PlayerPrefs.SetString("ParticipantGender", gen);
    }
    public void SetParticipantGroup(string group)
    {
        PlayerPrefs.SetString("group", group);
    }
    public void SetCompetitive(Toggle tog)
    {
        if (tog.isOn)
        {
            Debug.Log("set competitive on");
            PlayerPrefs.SetInt("Competitive", 1);
        }
        else
        {
            Debug.Log("set competitive off");
            PlayerPrefs.SetInt("Competitive", 0);
        }
    }


    public void SetPartVariables()
    {
        SetParticipantID(partID.text);
        SetParticipantAge(partAge.text);
        SetParticipantCondition(condition.text);
        SetParticipantGender(partGender.text);
        Settrial(trial.text);
        SetParticipantGroup(group.text);
        SetCompetitive(compeditive);
    }

    public Vector3 GetRaycastStartPos(LineRenderer lr, Vector3[] array)
    {
        lr.GetPositions(array);
        return array[0];
    }
    public Vector3 GetRaycastEndPos(LineRenderer lr, Vector3[] array)
    {
        lr.GetPositions(array);
        return array[1];
    }*/

    
}



[System.Serializable]
public class DataEvent
{
    public string timestamp;
    public string participantID;
    public string gameName;
    public string totalTimePlayed;
    public string sessionNumber;
    public string currentSessionTime;
    public string mapNumber;
    public string levelNumber;
    public string waveNumber;
    public string eventName;
    public string gameObjectName;
    public string eventCoordinates;
    public string jumpStatus;

    public DataEvent(string newEventName, GameObject newGO, Vector3 newEventCoords, bool isJumped)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the SaveData class (in GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        participantID = playerData.PlayerID;
        gameName = Application.productName;
        totalTimePlayed = playerData.GetTotalTimePlayed();
        sessionNumber = playerData.TimesFileOpened.ToString();
        currentSessionTime = playerData.GetCurrentSessionPlaytime();

        mapNumber = gm.currMapName;
        levelNumber = gm.currPathNum.ToString();
        waveNumber = gm.currWaveNum.ToString();

        eventName = newEventName;
        gameObjectName = newGO.name;
        eventCoordinates = new string ("X: " + newEventCoords.x + "Y: " + newEventCoords.y + "Z: " + newEventCoords.z);
        jumpStatus = isJumped.ToString() ;
    }

    public DataEvent(string newEventName, string newGOName, string newEventCoords, string isJumped)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the SaveData class (in GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        participantID = playerData.PlayerID;
        gameName = Application.productName;
        totalTimePlayed = playerData.GetTotalTimePlayed();
        sessionNumber = playerData.TimesFileOpened.ToString();
        currentSessionTime = playerData.GetCurrentSessionPlaytime();

        mapNumber = gm.currMapName;
        levelNumber = gm.currPathNum.ToString();
        waveNumber = gm.currWaveNum.ToString();

        eventName = newEventName;
        gameObjectName = newGOName;
        eventCoordinates = newEventCoords;
        jumpStatus = isJumped;
    }

    public DataEvent(string newEventName)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the Player File (at GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        gameName = Application.productName;

        if (playerData != null)
        {
            participantID = playerData.PlayerID;
            totalTimePlayed = playerData.GetTotalTimePlayed();
            sessionNumber = playerData.TimesFileOpened.ToString();
            currentSessionTime = playerData.GetCurrentSessionPlaytime();
        }
        else
        {
            participantID = "MainMenuScene";
            totalTimePlayed = "N/A";
            sessionNumber = "N/A";
            currentSessionTime = "N/A";
        }

        mapNumber = "N/A";
        levelNumber = "N/A";
        waveNumber = "N/A";

        eventName = newEventName;
        gameObjectName = "N/A";
        eventCoordinates = "N/A";
        jumpStatus = "N/A";
    }



    public string[] GetDataStringArray()
    {
        List<string> newEventStringList = new List<string>();

        newEventStringList.Add(timestamp);
        newEventStringList.Add(participantID);
        newEventStringList.Add(gameName);
        newEventStringList.Add(totalTimePlayed);
        newEventStringList.Add(sessionNumber);
        newEventStringList.Add(currentSessionTime);
        newEventStringList.Add(mapNumber);
        newEventStringList.Add(levelNumber);
        newEventStringList.Add(waveNumber);
        newEventStringList.Add(eventName);
        newEventStringList.Add(gameObjectName);
        newEventStringList.Add(eventCoordinates);
        newEventStringList.Add(jumpStatus);

        return newEventStringList.ToArray();
    }
}

