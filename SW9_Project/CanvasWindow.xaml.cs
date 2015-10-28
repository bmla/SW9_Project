using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SW9_Project {
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window, IDrawingBoard {

        Shape pointingCircle;

        KinectManager kinectManager;

        Rectangle[,] grid;
        int gridHeight = 10, gridWidth = 10;
        double squareHeight = 0, squareWidth = 0;

        Point pointFromMid = new Point();

        public CanvasWindow() {
            InitializeComponent();
            kinectManager = new KinectManager(this);
        }

        private void CreateGrid() {
            CreateGrid(gridHeight, gridWidth);

            Shape t = ShapeFactory.CreateCircle(100);
            canvas.Children.Add(t);
            Canvas.SetBottom(t, (canvas.ActualHeight / 2) - (100 / 2));
            Canvas.SetLeft(t, (canvas.ActualWidth / 2) - (100 / 2));
        }

        private void CreateGrid(int width, int height) {
            gridHeight = height;
            gridWidth = width;
            squareHeight = canvas.ActualHeight / height;
            squareWidth = canvas.ActualWidth / width;

            grid = new Rectangle[width, height];

            for(int i = 0; i < width; i++) {
                for(int j = 0; j < height; j++) {
                    grid[i, j] = ShapeFactory.CreateGridCell(squareWidth, squareHeight);
                    canvas.Children.Add(grid[i, j]);
                    Canvas.SetBottom(grid[i, j], j * squareHeight);
                    Canvas.SetLeft(grid[i, j], i * squareWidth);
                }
            }
        }

        private Rectangle GetCell(Point p) {


            int x = (int)Math.Floor(p.X / squareWidth);
            int y = (int)Math.Floor(p.Y / squareHeight);

            if (x >= gridWidth) { x = gridWidth - 1; }
            if (y >= gridHeight) { y = gridHeight - 1; }

            return grid[x, y]; 

        }

        Rectangle currentCell;
        private void ColorCell(Point toColor) {
            
            if (currentCell != null) {
                currentCell.Fill = Brushes.Transparent;
            }
            currentCell = GetCell(toColor);
            currentCell.Fill = Brushes.Yellow;
        }

        public void PointAt(double xFromMid, double yFromMid) {

            if (pointingCircle == null) {
                pointingCircle = ShapeFactory.CreatePointer();
            }

            pointFromMid = GetPoint(xFromMid, yFromMid);

            
            MoveShape(pointingCircle, pointFromMid);
            ColorCell(pointFromMid);
            
        }

        public void PullShape(double xFromMid, double yFromMid) {
            throw new NotImplementedException();
        }

        public void ReceiveShape(Shape shapeToMove, double x, double y) {
            throw new NotImplementedException();
        }

        private void MoveShape(Shape shapeToMove, Point p) {

            double x = p.X - (shapeToMove.Width / 2);
            double y = p.Y - (shapeToMove.Height / 2);
            
            Canvas.SetLeft(shapeToMove, x);
            Canvas.SetBottom(shapeToMove, y);
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e) {
            if(canvas.Children.Count != 0) {
                canvas.Children.RemoveRange(0, canvas.Children.Count);
            }
            CreateGrid();
        }

        public Point GetPoint(double xFromMid, double yFromMid)
        {
            double x = Scale(canvas.ActualWidth, .25f, xFromMid);
            double y = Scale(canvas.ActualHeight, .26f, yFromMid);
            Point p = new Point(x, y);

            return p;
        }
        
        private static double Scale(double maxPixel, float maxSkeleton, double position)
        {
            double value = ((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));
            if (value > maxPixel)
                return maxPixel;
            if (value < 0)
                return 0;
            return value;
        }

        private void canvas_Loaded(object sender, RoutedEventArgs e) {
            CreateGrid();
        }
        
    }
}
