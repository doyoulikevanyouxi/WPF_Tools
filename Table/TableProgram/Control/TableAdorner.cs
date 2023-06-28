using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Table.Control
{
    enum AdornerMouseAction
    {
        Click = 0,
        DoubleClick = 1,
        MouseMove = 2,
        MouseOver = 3
    }
    class TableAdorner: Adorner
    {
        private VisualCollection visualC;
        private ContentPresenter contentP;
        private UIElement iElement;
        private Path path;
        private PathGeometry pathGeometry;

        private Point startPoint1;
        private PathFigure pathFigure;
        private LineSegment lineSegment;
        private ArcSegment arcSegment;
        private LineSegment lineSegment2;
        //十字
        private Point startPoint2;
        private Point startPoint3;
        private PathFigure pathFigure1;
        private PathFigure pathFigure2;
        private LineSegment lineSegment3;
        private LineSegment lineSegment4;

        private double borderThickness = 1;
        protected bool IsHorizential;


        public double offsetX = 0f;
        public double offsetY = 0f;
        public int Row { get; set; }
        public int Column { get; set; }

        public delegate void AdornerEvent(AdornerMouseAction adornerMouseAction, object param);
        public AdornerEvent Click;

        public TableAdorner(FrameworkElement adornedElement, bool IsHorizential = false) : base(adornedElement)
        {
            Row = 0;
            Column = 0;
            this.IsHorizential = IsHorizential;
            iElement = adornedElement;
            visualC = new VisualCollection(this);
            contentP = new ContentPresenter();

            path = new Path() { Stroke = new SolidColorBrush(Colors.LightGray), StrokeThickness = 1, Fill = new SolidColorBrush(Colors.White) };
            pathGeometry = new PathGeometry();
            PathSegmentCollection pathSegments = new PathSegmentCollection();
            if (IsHorizential)
            {
                startPoint1 = new Point(adornedElement.ActualWidth + borderThickness / 2f, 2.5);
                lineSegment = new LineSegment(new Point(-5, 2.5), true);
                arcSegment = new ArcSegment() { Point = new Point(-5, -2.5), IsLargeArc = true, Size = new Size(10, 10), SweepDirection = SweepDirection.Clockwise };
                lineSegment2 = new LineSegment(new Point(adornedElement.ActualWidth + borderThickness, -2.5), true);

                //十字部分
                Point circlCenter = new Point(-5 - Math.Sqrt((10 - 2.5) * (10 + 2.5)), 0);
                startPoint2 = new Point(circlCenter.X, -5);
                lineSegment3 = new LineSegment(new Point(circlCenter.X, 5), true);
                startPoint3 = new Point(circlCenter.X - 5, 0);
                lineSegment4 = new LineSegment(new Point(circlCenter.X + 5, 0), true);
            }
            else
            {
                startPoint1 = new Point(-2.5, adornedElement.ActualHeight + borderThickness / 2f);
                lineSegment = new LineSegment(new Point(-2.5, -5), true);
                arcSegment = new ArcSegment() { Point = new Point(2.5, -5), IsLargeArc = true, Size = new Size(10, 10), SweepDirection = SweepDirection.Clockwise };
                lineSegment2 = new LineSegment(new Point(2.5, adornedElement.ActualHeight + borderThickness / 2f), true);


                Point circlCenter = new Point(0, -5 - Math.Sqrt((10 - 2.5) * (10 + 2.5)));
                startPoint2 = new Point(-5, circlCenter.Y);
                lineSegment3 = new LineSegment(new Point(5, circlCenter.Y), true);
                startPoint3 = new Point(0, circlCenter.Y - 5);
                lineSegment4 = new LineSegment(new Point(0, circlCenter.Y + 5), true);

            }


            pathFigure = new PathFigure() { StartPoint = startPoint1 };
            pathFigure1 = new PathFigure() { StartPoint = startPoint2 };
            pathFigure2 = new PathFigure() { StartPoint = startPoint3 };

            pathFigure.Segments.Add(lineSegment);
            pathFigure.Segments.Add(arcSegment);
            pathFigure.Segments.Add(lineSegment2);

            //十字
            pathFigure1.Segments.Add(lineSegment3);
            pathFigure2.Segments.Add(lineSegment4);

            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Figures.Add(pathFigure1);
            pathGeometry.Figures.Add(pathFigure2);
            path.Data = pathGeometry;
            contentP.Content = path;
            visualC.Add(contentP);
        }
        public object Content
        {
            get => contentP.Content;
            set => contentP.Content = value;
        }
        protected override int VisualChildrenCount => visualC.Count;
        protected override Visual GetVisualChild(int index)
        {
            return visualC[index];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentP.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return contentP.RenderSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            contentP.Measure(constraint);
            return contentP.DesiredSize;
        }

        public void SetOffset(Point point)
        {
            offsetX = point.X;
            offsetY = point.Y;
            pathFigure.StartPoint = new Point(pathFigure.StartPoint.X + point.X, pathFigure.StartPoint.Y + point.Y);
            pathFigure1.StartPoint = new Point(pathFigure1.StartPoint.X + point.X, pathFigure1.StartPoint.Y + point.Y);
            pathFigure2.StartPoint = new Point(pathFigure2.StartPoint.X + point.X, pathFigure2.StartPoint.Y + point.Y);

            lineSegment.Point = new Point(lineSegment.Point.X + point.X, lineSegment.Point.Y + point.Y);
            arcSegment.Point = new Point(arcSegment.Point.X + point.X, arcSegment.Point.Y + point.Y);
            lineSegment2.Point = new Point(lineSegment2.Point.X + point.X, lineSegment2.Point.Y + point.Y);

            lineSegment3.Point = new Point(lineSegment3.Point.X + point.X, lineSegment3.Point.Y + point.Y);
            lineSegment4.Point = new Point(lineSegment4.Point.X + point.X, lineSegment4.Point.Y + point.Y);
        }
        public void SetOffsetX(double value)
        {
            offsetX = value;
            pathFigure.StartPoint = new Point(pathFigure.StartPoint.X + value, pathFigure.StartPoint.Y);
            pathFigure1.StartPoint = new Point(pathFigure1.StartPoint.X + value, pathFigure1.StartPoint.Y);
            pathFigure2.StartPoint = new Point(pathFigure2.StartPoint.X + value, pathFigure2.StartPoint.Y);

            lineSegment.Point = new Point(lineSegment.Point.X + value, lineSegment.Point.Y);
            arcSegment.Point = new Point(arcSegment.Point.X + value, arcSegment.Point.Y);
            lineSegment2.Point = new Point(lineSegment2.Point.X + value, lineSegment2.Point.Y);

            lineSegment3.Point = new Point(lineSegment3.Point.X + value, lineSegment3.Point.Y);
            lineSegment4.Point = new Point(lineSegment4.Point.X + value, lineSegment4.Point.Y);
        }
        public void SetOffsetY(double value)
        {
            offsetY = value;
            pathFigure.StartPoint = new Point(pathFigure.StartPoint.X, pathFigure.StartPoint.Y + value);
            pathFigure1.StartPoint = new Point(pathFigure1.StartPoint.X, pathFigure1.StartPoint.Y + value);
            pathFigure2.StartPoint = new Point(pathFigure2.StartPoint.X, pathFigure2.StartPoint.Y + value);

            lineSegment.Point = new Point(lineSegment.Point.X, lineSegment.Point.Y + value);
            arcSegment.Point = new Point(arcSegment.Point.X, arcSegment.Point.Y + value);
            lineSegment2.Point = new Point(lineSegment2.Point.X, lineSegment2.Point.Y + value);

            lineSegment3.Point = new Point(lineSegment3.Point.X, lineSegment3.Point.Y + value);
            lineSegment4.Point = new Point(lineSegment4.Point.X, lineSegment4.Point.Y + value);
        }




        protected override void OnMouseEnter(MouseEventArgs e)
        {
            path.Stroke = new SolidColorBrush(Color.FromRgb(0x2D, 0x9B, 0xFB));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            path.Stroke = new SolidColorBrush(Colors.LightGray);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount != 1)
                return;

            if (IsHorizential)
                Click(AdornerMouseAction.Click, new ValuePair<int, bool>(Row, true));
            else
                Click(AdornerMouseAction.Click, new ValuePair<int, bool>(Column, false));

        }
    }
}
