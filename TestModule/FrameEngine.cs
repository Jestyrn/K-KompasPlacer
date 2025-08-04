using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using netDxf;

namespace TestModule
{
    public class FrameEngine
    {
        private List<BoundingBox> FreeRects = new List<BoundingBox>();
        private BoundingBox ReturnedBounds;

        private double MinX = 0;
        private double MinY = 0;
        private double Width = 0;
        private double Height = 0;
        private double Area = 0;

        public double FreeArea = 0;


        public FrameEngine(BoundingBox frame)
        {
            FreeRects.Add(frame);
            MinX = frame.MinX;
            MinY = frame.MinY;
            Width = frame.Width;
            Height = frame.Height;
            Area = frame.Area;
            FreeArea = Area;

            ReturnedBounds = new BoundingBox(0, 0, 0, 0);
        }

        public void UpdateFrame(BoundingBox newFrame)
        {
            FreeRects.Clear();
            FreeRects.Add(newFrame);
            MinX = newFrame.MinX;
            MinY = newFrame.MinY;
            Width = newFrame.Width;
            Height = newFrame.Height;
            Area = newFrame.Area;
            FreeArea = Area;
        }

        public bool TryPlaceDetail(Detail detail, out bool needRotate, out BoundingBox place)
        {
            place = new BoundingBox(0,0,0,0);
            needRotate = false;

            if (detail.Area > Area) return false;

            if (TryInsert(detail.Width, detail.Height))
            {
                place = ReturnedBounds;
                return true;
            }
            else if(TryInsert(detail.Height, detail.Width))
            {
                place = ReturnedBounds;
                needRotate = true;
                return true;
            }

            Console.WriteLine("Деталька не встала");
            return false;
        }

        private bool TryInsert(double width, double height)
        {
            double bestScore = int.MaxValue;
            BoundingBox bestBox = null;

            foreach (var rect in FreeRects)
            {
                if (rect.Width >= width & rect.Height >= height)
                {
                    double leftoverHoriz = rect.Width - width;
                    double leftoverVert = rect.Height - height;
                    double score = Math.Min(leftoverHoriz, leftoverVert);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = rect;
                    }
                }
            }

            if (bestBox == null) return false;
            var takenPlace = new BoundingBox(new Vector2(bestBox.MinX, bestBox.MinY), width, height);
            ReturnedBounds = takenPlace;

            AddNewFreeSpaces(takenPlace);
            MergeRects();

            FreeArea = 0;
            foreach (var rect in FreeRects)
                FreeArea += rect.Area;

            return true;
        }

        private void AddNewFreeSpaces(BoundingBox takenPlace)
        {
            var newRects = new List<BoundingBox>();

            foreach (var rect in FreeRects)
            {
                if (!rect.Intersects(takenPlace))
                {
                    newRects.Add(rect);
                    continue;
                }

                if (takenPlace.MinX > rect.MinX)
                {
                    newRects.Add(new BoundingBox(
                        rect.MinX, rect.MaxY,
                        takenPlace.MinX - rect.MinX, rect.Height));
                }

                if (takenPlace.MaxX < rect.MaxX)
                {
                    newRects.Add(new BoundingBox(
                        takenPlace.MaxX, rect.MaxY,
                        rect.MaxX - takenPlace.MaxX, rect.Height));
                }

                if (takenPlace.MaxY > rect.MaxY)
                {
                    newRects.Add(new BoundingBox(
                        rect.MinX, rect.MinY,
                        rect.Width, takenPlace.MaxY - rect.MaxY));
                }

                if (takenPlace.MinY < rect.MinY)
                {
                    newRects.Add(new BoundingBox(
                        rect.MinX, takenPlace.MinY,
                        rect.Width, rect.MinY - takenPlace.MinY));
                }
            }

            FreeRects = newRects;
        }

        private void MergeRects()
        {
            FreeRects.RemoveAll(rect1 =>
            FreeRects.Any(rect2 =>
                rect1 != rect2 &&
                rect2.MinX <= rect1.MinX &&
                rect2.MinY <= rect1.MinY &&
                rect2.MaxX >= rect1.MaxX &&
                rect2.MaxY >= rect1.MinY));
        }
    }
}
