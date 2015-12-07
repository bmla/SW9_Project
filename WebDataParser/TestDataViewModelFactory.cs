﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

using SW9_Project;

using WebDataParser.Models;

namespace WebDataParser {
    public static class TestDataViewModelFactory {

        static Dictionary<string, Test> Tests = new Dictionary<string, Test>();
        static Dictionary<string, TestDataViewModel> TestViewModels = new Dictionary<string, TestDataViewModel>();
        static List<GestureType> AllGestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

        public static int GetTotalTestCount(HttpServerUtilityBase utility) {
            return Directory.GetFiles(utility.MapPath("~/Testlog/"), "*.test").Count();
        }
        private static TestDataViewModel GetTest(HttpServerUtilityBase utlity) {

            if (TestViewModels.ContainsKey("average")) {
                return TestViewModels["average"];
            }
            TestViewModels.Add("average", new TestDataViewModel("average"));

            string directory = utlity.MapPath("~/Testlog/");
            string[] files = Directory.GetFiles(directory, "*.test");
            if (Tests.Count != files.Count()) {
                Tests.Clear();
                foreach (var s in files) {
                    string id = s.Split('/').Last().Split('.').First();
                    StreamReader sr = new StreamReader(s);
                    Tests.Add(id, new Test(sr, id));
                }
            }

            var tests = Tests.Values.ToList();

            var averageHitPercentagePerGesture = GetAverageHitPercentagePerTurn(tests);
            var averageTimePerTargetPerGesture = GetAverageTimePerTarget(tests);


            foreach (var gesture in AllGestures) {
                GestureInfo info = new GestureInfo();
                info.HitData = GetJSPercentageArray(averageHitPercentagePerGesture[gesture], gesture);
                info.TimeData = GetJSTimeArray(averageTimePerTargetPerGesture[gesture], gesture);
                info.HitPercentage = averageHitPercentagePerGesture[gesture].Last() * 100f;
                info.PracticeTime = (int)GetAveragePracticeTimePerGesture(tests)[gesture].TotalSeconds;
                info.TotalTime = (int)(GetAverageTestTimePerGesture(tests)[gesture]).TotalSeconds;

                List<Attempt> attempts = new List<Attempt>();
                foreach(var test in tests) {
                    attempts.AddRange(test.Attempts[gesture]);
                }
                info.Img = DrawHitBox(attempts);

                TestViewModels["average"].GestureInformation[GetGestureTypeString(gesture)] = info;

            }

            return TestViewModels["average"];
        }

        public static MemoryStream GetHitbox(string id, string type) {
            return TestViewModels[id].GestureInformation[type].Img;
        }

        public static TestDataViewModel GetTest(HttpServerUtilityBase utility, string id) {

            if(id == "average") {
                return GetTest(utility);
            }

            if (TestViewModels.ContainsKey(id)) {
                return TestViewModels[id];
            }
            TestViewModels.Add(id, new TestDataViewModel(id));
            TestViewModels[id].GestureInformation = new Dictionary<string, GestureInfo>();

             
            if (!Tests.ContainsKey(id)) {
                StreamReader reader = new StreamReader(utility.MapPath("~/Testlog/" + id + ".test"));
                Tests[id] = new Test(reader, id);
            }
            
            foreach (var gesture in AllGestures) {
                GestureInfo info = new GestureInfo();
                var hitsPerTry = GetHitsPerTry(Tests[id].Attempts[gesture]);
                info.HitData = GetJSPercentageArray(hitsPerTry, gesture);
                info.TimeData = GetJSTimeArray(GetTimePerTarget(Tests[id].Attempts[gesture], Tests[id].TestStart[gesture]), gesture);
                info.HitPercentage = hitsPerTry.Last() * 100f;
                info.PracticeTime = (int)Tests[id].PracticeTime[gesture].TotalSeconds;
                info.TotalTime = (int)(Tests[id].Attempts[gesture].Last().Time - Tests[id].TestStart[gesture]).TotalSeconds;
                info.Img = DrawHitBox(Tests[id].Attempts[gesture]);

                TestViewModels[id].GestureInformation[GetGestureTypeString(gesture)] = info;
                
            }


            return TestViewModels[id];

        }

        private static string GetGestureTypeString(GestureType type) {
            switch (type) {
                case GestureType.Pinch: return "pinch"; 
                case GestureType.Swipe: return "swipe"; 
                case GestureType.Throw: return "throw";
                case GestureType.Tilt: return "tilt";
            }
            return "exception";
        }

        private static Dictionary<GestureType, float[]> GetAverageHitPercentagePerTurn(List<Test> tests) {

            List<GestureType> gestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

            Dictionary<GestureType, float[]> averageHitPercentagePerGesture = new Dictionary<GestureType, float[]>();

            foreach (var gesture in gestures) {

                float[] avgPercentage = new float[tests[0].Attempts[0].Count];
                List<float[]> percentages = new List<float[]>();

                foreach (var test in tests) {
                    percentages.Add(GetHitsPerTry(test.Attempts[gesture]));
                }

                for (int i = 0; i < avgPercentage.Length; i++) {
                    foreach (var percentage in percentages) {
                        avgPercentage[i] += percentage[i];
                    }
                    avgPercentage[i] /= (float)percentages.Count;
                }
                averageHitPercentagePerGesture.Add(gesture, avgPercentage);
            }

            return averageHitPercentagePerGesture;

        }
        private static Dictionary<GestureType, float[]> GetAverageTimePerTarget(List<Test> tests) {
            List<GestureType> gestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

            Dictionary<GestureType, float[]> averageTimePerGesture = new Dictionary<GestureType, float[]>();

            foreach (var gesture in gestures) {

                float[] averageTime = new float[tests[0].Attempts[0].Count];
                List<float[]> times = new List<float[]>();

                foreach (var test in tests) {
                    times.Add(GetTimePerTarget(test.Attempts[gesture], test.TestStart[gesture]));
                }

                for (int i = 0; i < averageTime.Length; i++) {
                    foreach (var time in times) {
                        averageTime[i] += time[i];
                    }
                    averageTime[i] /= (float)times.Count;
                }
                averageTimePerGesture.Add(gesture, averageTime);
            }

            return averageTimePerGesture;
        }

        private static Dictionary<GestureType, TimeSpan> GetAverageTestTimePerGesture(List<Test> tests) {
            Dictionary<GestureType, TimeSpan> temp = new Dictionary<GestureType, TimeSpan>();
            Dictionary<GestureType, TimeSpan> avgTime = new Dictionary<GestureType, TimeSpan>();

            foreach (var test in tests) {
                foreach (var gesture in test.Attempts) {
                    if (!temp.ContainsKey(gesture.Key)) {
                        temp.Add(gesture.Key, gesture.Value.Last().Time - test.TestStart[gesture.Key]);
                    } else {
                        temp[gesture.Key] += gesture.Value.Last().Time - test.TestStart[gesture.Key];
                    }
                }
            }

            foreach (var time in temp) {
                avgTime.Add(time.Key, TimeSpan.FromSeconds(time.Value.TotalSeconds / tests.Count));
            }

            return avgTime;

        }

        private static Dictionary<GestureType, TimeSpan> GetAveragePracticeTimePerGesture(List<Test> tests) {
            Dictionary<GestureType, TimeSpan> temp = new Dictionary<GestureType, TimeSpan>();
            Dictionary<GestureType, TimeSpan> avgTime = new Dictionary<GestureType, TimeSpan>();

            foreach (var test in tests) {
                foreach (var time in test.PracticeTime) {
                    if (!temp.ContainsKey(time.Key)) {
                        temp.Add(time.Key, time.Value);
                    } else {
                        temp[time.Key] += time.Value;
                    }
                }
            }

            foreach (var time in temp) {
                avgTime.Add(time.Key, TimeSpan.FromSeconds(time.Value.TotalSeconds / tests.Count));
            }

            return avgTime;
        }

        private static MemoryStream DrawHitBox(List<Attempt> attempts) {

            //61 pixel sized squares, makes it better to look at
            int cellSize = 61;
            int bmsize = cellSize * 3;

            Bitmap hitbox = new Bitmap(bmsize, bmsize);
            Graphics hBGraphic = Graphics.FromImage(hitbox);
            hBGraphic.FillRectangle(Brushes.White, 0, 0, bmsize, bmsize);
            hBGraphic.DrawRectangle(new Pen(Brushes.Black, 1.0f), cellSize, cellSize, cellSize, cellSize);

            foreach (var attempt in attempts) {
                Brush brush = attempt.Size == GridSize.Large ? Brushes.Red : Brushes.Green;
                float scale = attempt.Size == GridSize.Large ? 122.0f : 61.0f;
                Point p = new Point(attempt.TargetCell.X, attempt.TargetCell.Y);
                p.X = p.X * scale; p.Y = p.Y * scale;
                p.X = attempt.Pointer.X - p.X;
                p.Y = attempt.Pointer.Y - p.Y;
                if (attempt.Size == GridSize.Large) {
                    p.X /= 2;
                    p.Y /= 2;
                }

                p.X += cellSize;
                p.Y += cellSize;

                if (!((p.X < 0) && (p.X >= bmsize)) || !((p.Y < 0) && (p.Y >= bmsize))) {
                    hBGraphic.FillRectangle(brush, (float)p.X, (float)p.Y, 2, 2);
                }
            }

            hBGraphic.Save();


            MemoryStream ms = new MemoryStream();

            hitbox.Save(ms, ImageFormat.Png);

            hBGraphic.Dispose();
            hitbox.Dispose();

            return ms;


            //Changed grid size.Grid height: 10 Grid width: 20 Cell height: 61.4 Cell width: 60.7
            //Changed grid size.Grid height: 5 Grid width: 10 Cell height: 122.8 Cell width: 121.4

        }

        private static float[] GetHitsPerTry(List<Attempt> attempts) {

            int hits = 0; float[] hitsAtTries = new float[attempts.Count]; int currentAttempt = 0;
            foreach (var attempt in attempts) {
                if (attempt.Hit) {
                    hits++;
                }
                hitsAtTries[currentAttempt++] = (float)hits / ((float)currentAttempt);
            }


            return hitsAtTries;
        }

        private static float[] GetTimePerTarget(List<Attempt> attempts, TimeSpan start) {

            float[] timeAtTries = new float[attempts.Count]; int currentAttempt = 0;
            foreach (var attempt in attempts) {
                float timeAtTarget = (float)(attempt.Time.TotalSeconds - start.TotalSeconds);

                timeAtTries[currentAttempt++] = timeAtTarget;
                start = attempt.Time;
            }

            return timeAtTries;
        }

        private static string GetJSPercentageArray(float[] percentages, GestureType type) {

            //var data = [ [[0, 0], [1, 1], [1,0]] ];

            string array = " [ ";
            for (int i = 0; i < percentages.Length; i++) {
                float percentage = (float)percentages[i] * 100.0f;
                string sPercentage = percentage.ToString().Replace(',', '.');
                array += "[" + (i + 1) + ", " + sPercentage + "], ";
            }

            array = array.Remove(array.Length - 2);
            array += " ];\n";

            return "var " + type + "Data = " + array;
        }

        private static string GetJSTimeArray(float[] times, GestureType type) {

            //var data = [ [[0, 0], [1, 1], [1,0]] ];

            string array = " [ ";
            for (int i = 0; i < times.Length; i++) {
                float time = (float)times[i];
                string sTime = time.ToString().Replace(',', '.');
                array += "[" + (i + 1) + ", " + sTime + "], ";
            }

            array = array.Remove(array.Length - 2);
            array += " ];\n";

            return "var Time" + type + "Data = " + array;
        }
    }
}