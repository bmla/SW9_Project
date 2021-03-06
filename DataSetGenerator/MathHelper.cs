﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetGenerator {
    public static class MathHelper {

        public static double GetDistance(Attempt attempt) {
            return attempt.Source == DataSource.Accuracy ? DistanceToTargetCenter(attempt) : DistanceToTargetCell(attempt);
        }

        private static double DistanceSquare(Point v, Point w) {
            return Math.Pow(v.X - w.X, 2) + Math.Pow(v.Y - w.Y, 2);
        }

        private static double DistanceToSegmentSquared(Point p, Point v, Point w) {
            double l2 = DistanceSquare(v, w);
            if (l2 == 0) { return DistanceSquare(p, v); }
            double t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
            if (t < 0) { return DistanceSquare(p, v); }
            if (t > 1) { return DistanceSquare(p, w); }
            Point n = new Point(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y));
            return DistanceSquare(p, n);
        }

        private static double DistanceToSegment(Point p, Point ls, Point le) {
            return Math.Sqrt(DistanceToSegmentSquared(p, ls, le));
        }

        public static double GetScale(GridSize size) {
            return size == GridSize.Large ? 122.0f : 61.0f;
        }

        private static double DistanceToTargetCenter(Attempt attempt) {
            double scale = GetScale(attempt.Size);
            Point c = new Point((attempt.TargetCell.X * scale) + (scale * 0.5), (attempt.TargetCell.Y * scale) + (scale * 0.5));
            return Math.Sqrt(DistanceSquare(c, attempt.Pointer));

        }

        private static double DistanceToTargetCell(Attempt attempt) {
            if(attempt.Hit) { return 0; }
            double scale = GetScale(attempt.Size);
            List<Tuple<Point, Point>> lineSegments = new List<Tuple<Point, Point>>();
            Point t = new Point(attempt.TargetCell.X * scale, attempt.TargetCell.Y * scale);
            Point u = new Point(t.X, t.Y + scale);
            Point v = new Point(t.X + scale, t.Y + scale);
            Point w = new Point(t.X + scale, t.Y);
            lineSegments.Add(new Tuple<Point, Point>(t, u));
            lineSegments.Add(new Tuple<Point, Point>(t, w));
            lineSegments.Add(new Tuple<Point, Point>(u, v));
            lineSegments.Add(new Tuple<Point, Point>(v, w));
            List<double> distances = new List<double>();
            foreach (var line in lineSegments) {
                distances.Add(DistanceToSegment(attempt.Pointer, line.Item1, line.Item2));
            }

            return distances.Min();
        }

        public static Tuple<double, double> GetXYDistance(Attempt attempt) {
            double scale = GetScale(attempt.Size);
            double x = attempt.TargetCell.X * scale, y = attempt.TargetCell.Y * scale;
            double xDistance = 0, yDistance = 0;

            if (attempt.Pointer.X < x) {
                xDistance = x - attempt.Pointer.X;
            }
            else if (attempt.Pointer.X > x + scale) {
                xDistance = attempt.Pointer.X - (x + scale);
            }

            if (attempt.Pointer.Y < y) {
                yDistance = y - attempt.Pointer.Y;
            }
            else if (attempt.Pointer.Y > y + scale) {
                yDistance = attempt.Pointer.Y - (y + scale);
            }

            return new Tuple<double, double>(xDistance, yDistance);
        }

        public static float[] GetHitsPerTry(List<Attempt> attempts) {

            int hits = 0; float[] hitsAtTries = new float[attempts.Count]; int currentAttempt = 0;
            foreach (var attempt in attempts) {
                if (attempt.Hit) {
                    hits++;
                }
                hitsAtTries[currentAttempt++] = (float)hits / ((float)currentAttempt);
            }


            return hitsAtTries;
        }

        public static float[] GetTimePerTarget(List<Attempt> attempts) {

            float[] timeAtTries = new float[attempts.Count]; int currentAttempt = 0;
            foreach (var attempt in attempts) {

                timeAtTries[currentAttempt++] = (float)attempt.Time.TotalSeconds;
            }

            return timeAtTries;
        }
    }
}
