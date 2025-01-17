using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System;
using System.IO;
using System.Linq;
using Unity.Transforms;
using Meta.WitAi.Data;

namespace LSL4Unity.Samples.SimpleInlet
{
    public class SimpleInletBalanceBoard : MonoBehaviour
    {
        // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
        public string StreamName;
        ContinuousResolver resolver;
        float[] coordValues = new float[3];
        public Vector2 CoordValues { get { return new Vector2(coordValues[0], coordValues[1]); } }
        public float MagValue { get { return coordValues[2] / 2f; } }
        public Vector3 CoordValues3 { get { return new Vector3(coordValues[0], coordValues[1], coordValues[2]); } }

        double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

        // We need to keep track of the inlet once it is resolved.
        private StreamInlet inlet;

        // We need buffers to pass to LSL when pulling data.
        //private string[] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
        private float[] new_data_buffer;
        private double timestamp_buffer;

        private bool isReady = false;

        string fileLocation = "";

        float timer = 0;
        float[] lastValues = new float[3] {0, 0, 0};
        List<Vector2> mostRecentReadings = new List<Vector2>();
        Vector2 avgReading = Vector2.zero;
        int maxAverageReadingSample = 20;

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

            timer = 0f;
            isReady = true;
            Debug.Log("Found results : " + results.Length);
            inlet = new StreamInlet(results[0]);

            Debug.Log(inlet.ToString() + " // " + inlet.info().type());

            // Prepare pull_chunk buffer
            int buf_samples = (int)Mathf.Ceil((float)(inlet.info().nominal_srate() * max_chunk_duration));
            Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");

            new_data_buffer = new float[3];
            timestamp_buffer = 0.08;

        }

        private void OnApplicationQuit()
        {
            if (inlet != null)
                inlet.Close();
            else
                Debug.LogError("No StreamInlet detected during gameplay.");
        }

        //public float[] coordHolder = new float[3] { 0, 0, 0 };

        // Update is called once per frame
        void Update()
        {

            if (inlet != null)
            {
                inlet.pull_sample(new_data_buffer, timestamp_buffer);

                if (new_data_buffer != null)
                {
                    AddNewReadingToRecentReadings(new_data_buffer);

                    //coordValues[0] = X, coordValues[1] = Y, coordValues[2] = Magnitude
                }
            }
            //TODO: if you are in an environment where you CANNOT use a Balance Board,
            //simply cycle on sin()/cos() values. This should only be used for testing,
            //to make sure that rotations will adjust accordingly
            else
            {
                float sinValue = Mathf.PingPong(Time.time * 15f, 90f) - 45;

                AddNewReadingToRecentReadings(new float[] {sinValue, sinValue, 0f});
            }

            avgReading = GetAverageReading();
            coordValues = new float[] {avgReading.x, avgReading.y, avgReading.x};

            if (isReady)
            {
                RecordNewBalanceBoardValues(mostRecentReadings[mostRecentReadings.Count - 1], avgReading);
            }
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

        void AddNewReadingToRecentReadings(float[] newCoordValues)
        {
            Vector3 newPosition = new Vector3(newCoordValues[0], newCoordValues[1], newCoordValues[2]);

            mostRecentReadings.Add(newPosition);

            if (mostRecentReadings.Count > maxAverageReadingSample)
            {
                mostRecentReadings.RemoveAt(0);
            }
        }

        Vector3 GetAverageReading()
        {
            Vector2 newAverage = Vector3.zero;

            int trim = Mathf.CeilToInt(mostRecentReadings.Count * .2f);
            List<Vector2> trimmedList = mostRecentReadings.OrderBy(x => x.sqrMagnitude).Skip(trim).Take(mostRecentReadings.Count - trim * 2).ToList();

            for (int i = 0; i < trimmedList.Count; i++)
            {
                newAverage += trimmedList[i];
            }

            newAverage /= trimmedList.Count;
            return newAverage;
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

        //------------------------------------------------------------------------------
        //------------------------------------------------------------------------------

        //this function was used when we passed in a string from the balance board, and converted it
        //into float values. Currently, we are now passing in floats through 3 channels, so this function
        //is only here now as a reference - and in case somethign goes wrong with the floats
        float[] ConvertStringToFloat(string str)
        {
            string dataString = str.ToString();
            //string[] delimiters = new string[] { "COP Measurement -> X: ", "Y: ", "Mag: " };
            string[] splitData = dataString.Split(" ->", StringSplitOptions.None);

            float[] values = new float[3];
            if (splitData.Length > 3)
            {
                //NOTE: splitData[0] is only "", and is ignored when recording values
                Debug.Log("Split Data: " + splitData[1] + " | " + splitData[2] + " | " + splitData[3] + " | " + splitData.Length);

                values[0] = float.Parse(splitData[1]);
                values[1] = float.Parse(splitData[2]);
                values[2] = float.Parse(splitData[3]);
            }

            if (float.IsNaN(values[0]) || float.IsNaN(values[1])) { return lastValues; }
            else { lastValues = values; }

            return values;
        }
    }
}
