﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using WebDataParser;


namespace DataSetGenerator {

    static class DataGenerator {

        static string TestFileDirectory { get { return ".\\..\\..\\..\\Testlog/"; } }

        public static void GetUserInfoTechnique() {
            using (StreamWriter datawriter = new StreamWriter("user_technique_data.csv")) {
                string[] files = Directory.GetFiles(TestFileDirectory);
                foreach (var file in files) {
                    string id = file.Split('/').Last().Split('.')[0];
                    Test t = new Test(new StreamReader(file), id);
                    foreach(var gesture in t.Attempts) {
                        string time = (t.Attempts[gesture.Key].Last().Time - t.TestStart[gesture.Key]).TotalSeconds.ToString();
                        float hitPercentage = Test.GetHitsPerTry(t.Attempts[gesture.Key]).Last() * 100f;
                        string totalHit = hitPercentage.ToString();
                        string totalError = (100f - hitPercentage).ToString();
                        datawriter.WriteLine(t.ID + ", " + gesture.Key + ", " + time + ", " + totalHit + ", " + totalError);
                    }
                }
            }
        }
    }
}