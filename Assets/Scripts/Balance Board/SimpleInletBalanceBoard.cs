using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System;

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

        double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

        // We need to keep track of the inlet once it is resolved.
        private StreamInlet inlet;

        // We need buffers to pass to LSL when pulling data.
        private string[] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
        private double timestamp_buffer;


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
            StartCoroutine(ResolveExpectedStream());
        }

        IEnumerator ResolveExpectedStream()
        {

            var results = resolver.results();
            while (results.Length == 0)
            {
                Debug.Log("Detecting results");
                yield return new WaitForSeconds(.1f);
                results = resolver.results();
            }

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
            inlet.Close();
        }

        // Update is called once per frame
        void Update()
        {
            if (inlet != null)
            {
                inlet.pull_sample(data_buffer, timestamp_buffer);

                Debug.Log("Samples returned: " + data_buffer[0] + " // Data Buffer: " + data_buffer.Length + " // Timestamps: " + timestamp_buffer);

                if (data_buffer[0] != null)
                {
                    Debug.Log("Convert String to Viable Code here");

                    float[] coordValues = ConvertStringToFloat(data_buffer[0].ToString());

                    Debug.Log("Maggies: " + coordValues[2]);

                    //string[] splitData = dataString.Split(dataString, char.Parse("X: "));
                }
                //if (samples_returned > 0)
                //{
                // There are many things you can do with the incoming chunk to make it more palatable for Unity.
                // Note that if you are going to do significant processing and feature extraction on your signal,
                // it makes much more sense to do that in an external process then have that process output its
                // result to yet another stream that you capture in Unity.
                // Most of the time we only care about the latest sample to get a visual representation of the latest
                // state, so that's what we do here: take the last sample only and use it to udpate the object scale.
                // float x = data_buffer[samples_returned - 1, 0];
                //float y = data_buffer[samples_returned - 1, 1];
                //float z = data_buffer[samples_returned - 1, 2];
                //var new_scale = new Vector3(x, y, z);
                // Debug.Log("Setting cylinder scale to " + new_scale);
                //gameObject.transform.localScale = new_scale;
                //}
            }
        }

        float[] ConvertStringToFloat(string str)
        {
            string dataString = str.ToString();
            string[] delimiters = new string[] { "COP Measurement -> X: ", "Y: ", "Mag: " };
            string[] splitData = dataString.Split(delimiters, StringSplitOptions.None);

            float[] values = new float[3];

            if (splitData.Length > 3 && float.TryParse(splitData[1], out float result1) && float.TryParse(splitData[2], out float result2))
            {
                //NOTE: splitData[] is only "", and is ignored when recording values
                Debug.Log("Split Data: " + splitData[1] + " | " + splitData[2] + " | " + splitData[3] + " | " + splitData.Length);

                values[0] = float.Parse(splitData[1]);
                values[1] = float.Parse(splitData[2]);
                values[2] = float.Parse(splitData[3]);
            }

            return values;

        }
    }
}
