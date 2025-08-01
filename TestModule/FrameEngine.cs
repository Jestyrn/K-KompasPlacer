using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    internal class FrameEngine
    {
        /*
         * 
         * 1. Вызов TryPlacePart()
         * Попытка разместить без поворота
         * Попытка разместить с 90 градусами
         * (Вызов TryPlace)
         * 
         * 2. TryPlace()
         * Поиск свободного пространства(цикл)
         * Смотрим по эвристике(Best Short Side Fit)
         * Находим лучшее совпадение
         * Размещаем в Frame
         * Делаем Split()
         *
         * 3. Split()
         * Делим область на вертикаль
         * Делим область на горизонталь
         * Урезаем лишнее(поглощенные)
         * MergeFreeSpace()
         * 
         * 4. MergeFreeSpace()
         * Удаляем поглощение
         * 
         */
        private List<Detail> PlacedDetails = new List<Detail>();
        private List<Detail> SourceDetails = new List<Detail>();
        private List<CustRectangle> FreeRects = new List<CustRectangle>();
        private CustRectangle SourceRect;
        private Detail detail;

        public FrameEngine(FrameBuilder frame, Detail detail)
        {
            FreeRects.Clear();
            FreeRects.Add(new CustRectangle(
                frame.BoundingBox.MinX,
                frame.BoundingBox.MinY,
                frame.BoundingBox.Width,
                frame.BoundingBox.Height
                ));

            SourceRect = FreeRects[0];
            this.detail = detail;
        }

        public bool TryPlaceDetail(Detail detail)
        {
            if (TryPlace(detail.Width, detail.Height, out var rect))
            {
                PlacedDetails.Add(detail);
                return true;
            }

            if (TryPlace(detail.Height, detail.Width, out rect))
            {
                detail.RotateDetail(90);
                PlacedDetails.Add(detail);
                return true;
            }

            Console.WriteLine("Не получилось разместить");
            return false;
        }

        private bool TryPlace(double width, double height, out CustRectangle placement)
        {
            placement = null;
            CustRectangle bestRect = null;
            double bestScore = int.MaxValue;

            // Ищем лучший свободный прямоугольник
            foreach (var freeRect in FreeRects)
            {
                if (freeRect.Width >= width && freeRect.Height >= height)
                {
                    // Эвристика: Best Short Side Fit
                    double leftoverHoriz = freeRect.Width - width;
                    double leftoverVert = freeRect.Height - height;
                    double score = Math.Min(leftoverHoriz, leftoverVert);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestRect = freeRect;
                    }
                }
            }

            if (bestRect == null) return false;

            // Размещаем в левый нижний угол
            placement = new CustRectangle(bestRect.X, bestRect.Y, width, height);
            detail.MoveDetail(bestRect.X, bestRect.Y);

            // Обновляем список свободных областей
            SplitFreeRects(placement);
            return true;
        }

        private void SplitFreeRects(CustRectangle placedRect)
        {
            List<CustRectangle> newRects = new List<CustRectangle>();

            foreach (var freeRect in FreeRects)
            {
                if (!freeRect.Intersects(placedRect))
                {
                    newRects.Add(freeRect);
                    continue;
                }

                // Разделение по вертикали
                if (placedRect.X > freeRect.X)
                {
                    newRects.Add(new CustRectangle(
                        freeRect.X, freeRect.Y,
                        placedRect.X - freeRect.X, freeRect.Height));
                }

                if (placedRect.Right < freeRect.Right)
                {
                    newRects.Add(new CustRectangle(
                        placedRect.Right, freeRect.Y,
                        freeRect.Right - placedRect.Right, freeRect.Height));
                }

                // Разделение по горизонтали
                if (placedRect.Y > freeRect.Y)
                {
                    newRects.Add(new CustRectangle(
                        freeRect.X, freeRect.Y,
                        freeRect.Width, placedRect.Y - freeRect.Y));
                }

                if (placedRect.Bottom < freeRect.Bottom)
                {
                    newRects.Add(new CustRectangle(
                        freeRect.X, placedRect.Bottom,
                        freeRect.Width, freeRect.Bottom - placedRect.Bottom));
                }
            }

            FreeRects = newRects;
            MergeFreeRects();
        }

        private void MergeFreeRects()
        {
            // Удаляем полностью поглощённые прямоугольники
            FreeRects.RemoveAll(rect1 =>
                FreeRects.Any(rect2 =>
                    rect1 != rect2 &&
                    rect2.X <= rect1.X &&
                    rect2.Y <= rect1.Y &&
                    rect2.Right >= rect1.Right &&
                    rect2.Bottom >= rect1.Bottom));
        }
    }

    public class CustRectangle
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }

        public CustRectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(CustRectangle other)
        {
            return X < other.X + other.Width &&
                   X + Width > other.X &&
                   Y < other.Y + other.Height &&
                   Y + Height > other.Y;
        }
    }
}
