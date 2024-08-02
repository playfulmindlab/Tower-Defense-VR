using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System;
using System.IO;

namespace LSL4Unity.Samples.SimpleInlet
{
    // You probably don't need this namespace. We do it to avoid contaminating the global namespace of your project.
    public class SimpleInletBalanceBoard : MonoBehaviour
    {
        /*
         * This example shows the minimal code required to get an LSL inlet running
         * without leveraging any of the helper scripts that come with the LSL package.
         * This behaviour uses LSL.cs only. There is little-to-no error checking.
         * See Resolver.cs and BaseInlet.cs for helper behaviours to make your implementation
         * simpler and more robust.
         */

        // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
        public string StreamName;
        ContinuousResolver resolver;
        float[] coordValues = new float[3];
        public Vector2 CoordValues { get { return new Vector2(coordValues[0], coordValues[1]); } set { } }
        public Vector2 rotationValues = Vector2.zero;

        double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

        // We need to keep track of the inlet once it is resolved.
        private StreamInlet inlet;

        // We need buffers to pass to LSL when pulling data.
        private string[] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
        private double timestamp_buffer;

        private bool isReady = false;
        //public bool IsReady
        //{
        //    get { return isReady; }
        //    set { }
        //}

        string fileLocation = "";

        int timer = 0;

        void Start()
        {
            if (!StreamName.Equals(""))
                resolver = new ContinuousResolver("name", StreamName);
            else
            {
                Debug.LogError("Object must specify a name for resolver to lookup a stream.");
                this.enabled = false;
                return;
            }
            CreateNewFile("BalanceBoardTestValues");
            StartCoroutine(ResolveExpectedStream());
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();
            //isReady = true;
            while (results.Length == 0 && timer < 300)
            {
                Debug.Log("Detecting results");
                yield return new WaitForSeconds(0.1f);
                timer++;
                results = resolver.results();
            }

            if (results.Length <= 0)
            {
                Debug.LogError("ERROR: Could not find Balance Board. Ending search for StreamInlet.");
                StopCoroutine("ResolveExpectedStream");
                yield break;
            }

            isReady = true;
            Debug.Log("Found results : " + results.Length);
            inlet = new StreamInlet(results[0]);

            Debug.Log(inlet.ToString() + " // " + inlet.info().type());

            // Prepare pull_chunk buffer
            int buf_samples = (int)Mathf.Ceil((float)(inlet.info().nominal_srate() * max_chunk_duration));
            Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");

            data_buffer = new string[1];
            timestamp_buffer = 0.0;
        }

        private void OnApplicationQuit()
        {
            if (inlet != null)
                inlet.Close();
            else
                Debug.LogError("No StreamInlet detected during gameplay.");
        }

        // Update is called once per frame
        void Update()
        {
            if (inlet != null)
            {
                inlet.pull_sample(data_buffer, timestamp_buffer);

                //Debug.Log("Samples returned: " + data_buffer[0] + " // Data Buffer: " + data_buffer.Length + " // Timestamps: " + timestamp_buffer);

                if (data_buffer[0] != null)
                {
                    /*float[] */coordValues = ConvertStringToFloat(data_buffer[0]);

                    rotationValues = new Vector2((-coordValues[0] * 1.5f), (coordValues[1]));

                    //[0] = X, [1] = Y, [2] = Magnitude
                }

            }
            //TODO: if you are in an environment where you CANNOT use a Balance Board,
            //simply cycle on sin()/cos() values. This should only be used for testing,
            //to make sure that rotations will adjust accordingly
            else
            {
                float sinValue = Mathf.PingPong(Time.time *  15f, 90f) - 45 ;
                rotationValues = new Vector2(-sinValue * 1.5f, sinValue);
                coordValues[0] = -sinValue;
                coordValues[1] = -sinValue;
            }

            if (isReady)
            {
                RecordNewBalanceBoardValues(new Vector3(coordValues[0], coordValues[1], coordValues[2]), rotationValues);
            }
        }

        float[] ConvertStringToFloat(string str)
        {
            string dataString = str.ToString();
            string[] delimiters = new string[] { "COP Measurement -> X: ", "Y: ", "Mag: " };
            string[] splitData = dataString.Split(delimiters, StringSplitOptions.None);

            float[] values = new float[3];

            if (splitData.Length > 3)// && float.TryParse(splitData[1], out float result1) && float.TryParse(splitData[2], out float result2))
            {
                //NOTE: splitData[0] is only "", and is ignored when recording values
                Debug.Log("Split Data: " + splitData[1] + " | " + splitData[2] + " | " + splitData[3] + " | " + splitData.Length);

                values[0] = float.Parse(splitData[1]);
                values[1] = float.Parse(splitData[2]);
                values[2] = float.Parse(splitData[3]);
            }

            return values;
        }

        public void CreateNewFile(string newLocation = "N/A")
        {
            string newFileLocation = "";
            Debug.Log(Application.dataPath);

            if (newLocation == "N/A")
            {
                SaveData playerData = GameManager.instance.PlayerData;
                newFileLocation = Application.dataPath + "/Balance Board/" + playerData.PlayerID + "/";
            }
            else
            {
                newFileLocation = Application.dataPath + "/Balance Board/" + newLocation + "/";
            }

            if (!File.Exists(newFileLocation))
            {
                Directory.CreateDirectory(newFileLocation);
            }

            newFileLocation += GameManager.instance.SessionStartTime.ToString("MM-yy - HH.mm.ss") + ".csv";
            File.WriteAllText(newFileLocation, "Timestamp,Participant,Session Number,Raw X,Raw Y,Raw Z,Mod X,Mod Y, Mod Z\n");
            fileLocation = newFileLocation;

            Debug.Log("Swapped Files - Balance Board.");
        }

        void RecordNewBalanceBoardValues(Vector3 newBBValues, Vector3 moddedBBValues)
        {
            Debug.Log("Balance Board...");
            string newReadingsString = "";

            GameManager gm = GameManager.instance;
            SaveData playerData = gm.PlayerData;

            newReadingsString = DateTime.Now.ToString("HH.mm.ss.fff") + ",";
            newReadingsString += playerData.PlayerID + ",";
            newReadingsString += playerData.TimesFileOpened.ToString() + ",";

            newReadingsString += newBBValues.x.ToString() + ",";
            newReadingsString += newBBValues.y.ToString() + ",";
            newReadingsString += newBBValues.z.ToString() + ",";
            newReadingsString += moddedBBValues.x.ToString() + ",";
            newReadingsString += moddedBBValues.y.ToString() + ",";
            newReadingsString += moddedBBValues.z.ToString();

            StreamWriter writer;
            writer = new StreamWriter(fileLocation, true);

            writer.WriteLine(newReadingsString);

            writer.Close();

            Debug.Log("Balance Board Recorded!");
        }
    }
}
