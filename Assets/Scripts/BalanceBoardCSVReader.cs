using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BalanceBoardCSVReader : MonoBehaviour
{
    public string fileName;
    string fileLocation;

    //StreamWriter writer = new StreamWriter(Application.dataPath + "/Resources/EyeTrackingTempData.csv", false);
    //writer.WriteLine("Timestamp,Participant,Game,Total Time Played,Session Number,Session Time Played,Map Number,Level Number,Wave Number,Event,Object,X Coordinates,Y Coordinates,Jump Status");
    //writer.Close();

    StreamReader reader;
    TextAsset boardCSV;

    // Start is called before the first frame update
    void Start()
    {
        boardCSV = Resources.Load<TextAsset>(fileName);
        //fileLocation = Application.res
        reader = new StreamReader(fileName);
    }

    // Update is called once per frame
    void Update()
    {
        //reader = new StreamReader(boardCSV);

        
    }

    private void OnApplicationQuit()
    {
        reader.Close();
    }


}
/*
{
    StreamReader reader = new StreamReader(abilitiesFileLocation);
    bool endOfFile = false;

    reader.ReadLine();

    while (!endOfFile)
    //for (int f = 1; f < )
    {
        string dataString = reader.ReadLine();
        // Debug.Log(dataString);

        if (dataString == null)
        {
            endOfFile = true;
            break;
        }

        var dataValues = dataString.Split(',');

        if (dataValues[1] != null && dataValues[1] != "" && dataValues[1] != "TBD")
        {
            string abilityName = dataValues[1].ToString();

            float[][] values = new float[9][];
            values[0] = new float[7];
            for (int v = 0; v < 7; v++)
            {
                if (dataValues[v + 3] != null && dataValues[v + 3] != "") values[0][v] = float.Parse(dataValues[v + 3]);
                else values[0][v] = -99;
            }

            for (int f = 1; f < 9; f++)
            {
                //Debug.Log(abilityName + f);
                dataString = reader.ReadLine();
                dataValues = dataString.Split(',');
                values[f] = new float[7];
                for (int v = 0; v < 7; v++)
                {
                    if (dataValues[v + 3] != null && dataValues[v + 3] != "") values[f][v] = float.Parse(dataValues[v + 3]);
                    else values[f][v] = -99;
                }
            }

            abilities.Add(abilityName, values);
            //abilityList.Add(abilityName + ": " + values[0]);
        }
    }

    reader.Close();
}
*/
