using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVWriter : MonoBehaviour
{
    string filename = "";

    // Start is called before the first frame update

    public class HistStats
    {
        public int episodeNumber;
        public float totalReward;
        public float totalSteps;
    }

    void Start()
    {
        filename = Application.dataPath + "/demoStats.csv";   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WriteCSV(List<HistStats> x)
    {
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Episode Number, Total Reward, Total Steps, ");
        tw.Close();

        tw = new StreamWriter(filename, true);

        foreach (HistStats item in x)
        {
            tw.WriteLine(item.episodeNumber +","+item.totalReward+","+item.totalSteps);
        }
        tw.Close();
    }
}
