﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace SW9_Project {
    static class ShapeFactory {

        public static Shape CreateShape(string shape, double size) {
            switch (shape) {
                case "circle": return CreateCircle(size * 0.8);
                case "square": return CreateSquare(size * 0.8);
                case "triangle": return CreateTriangle(size);
                case "pentagon": return CreatePentagon(size);
                default: return CreateStar(size, 5);
            }
        }

        public static UIElement CreatePointer() {
            Image pointer = new Image();
            pointer.Source = new BitmapImage(new Uri("resources/pointer.png", UriKind.RelativeOrAbsolute));
            return pointer;
        }

        public static Rectangle CreateGridCell(double width, double height) {
            Rectangle rectangle = new Rectangle();
            rectangle.StrokeThickness = 1;
            rectangle.Fill = Brushes.White;
            rectangle.Stroke = Brushes.Black;
            rectangle.Height = height;
            rectangle.Width = width;

            return rectangle;
        }

        public static Ellipse CreateCircle(double size) {
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = Brushes.Indigo;
            ellipse.StrokeThickness = 1;
            ellipse.Stroke = Brushes.Black;
            ellipse.Height = size;
            ellipse.Width = size;

            return ellipse;
        }

        public static Rectangle CreateSquare(double size) {

            Rectangle square = new Rectangle();
            square.StrokeThickness = 1;
            square.Fill = Brushes.Blue;
            square.Stroke = Brushes.Black;
            square.Height = size;
            square.Width = size;

            return square;
        }

        public static Polygon CreateTriangle(double size) {

            Polygon triangle = new Polygon();
            triangle.Stroke = Brushes.Black;
            triangle.Fill = Brushes.LightSeaGreen;
            triangle.StrokeThickness = 1;
            triangle.Points = new PointCollection{ new Point(0,0), new Point(size, 0), new Point(size / 2, -size) };
            return triangle;
        }

        public static Polygon CreateStar(double size, int num_points) {
            Polygon star = new Polygon();
            star.Stroke = Brushes.Black;
            star.Fill = Brushes.Purple;
            star.StrokeThickness = 1;

            // Make room for the points.
            Point[] pts = new Point[num_points];

            double rx = size / 2;
            double ry = size / 2;
            double cx = 0 + rx;
            double cy = 0 + ry;

            // Start at the top.
            double theta = -Math.PI / 2;
            double dtheta = 4 * Math.PI / num_points;
            for (int i = 0; i < num_points; i++) {
                star.Points.Add(new Point(
                    (float)(cx + rx * Math.Cos(theta)),
                    (float)(cy + ry * Math.Sin(theta))));
                star.Fill = Brushes.Purple;
                theta += dtheta;
            }

            return star;
        }

        public static Polygon CreatePentagon(double size) {
            Polygon pentagon = new Polygon();

            pentagon.Stroke = Brushes.Black;
            pentagon.Fill = Brushes.Maroon;
            pentagon.StrokeThickness = 1;
            PointCollection collection = new PointCollection();
            collection.Add(new Point(0.2 * size, 0));
            collection.Add(new Point(-(size * 0.05), -(size * 0.5)));
            collection.Add(new Point(size / 2, -size));
            collection.Add(new Point(size + (size * 0.05), -(size * 0.5)));
            collection.Add(new Point(size * 0.8, 0));
            pentagon.Points = collection;
            return pentagon;
        }
    }
}
