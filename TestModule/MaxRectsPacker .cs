namespace TestModule
{
    public class MaxRectsPacker
    {
        public List<Rectangle> FreeRects { get; private set; }
        public List<PlacedPart> PlacedParts { get; private set; }
        public double SheetWidth { get; private set; }
        public double SheetHeight { get; private set; }

        public MaxRectsPacker(double sheetWidth, double sheetHeight)
        {
            SheetWidth = sheetWidth;
            SheetHeight = sheetHeight;
            FreeRects = new List<Rectangle> { new Rectangle(0, 0, sheetWidth, sheetHeight) };
            PlacedParts = new List<PlacedPart>();
        }

        public bool TryPlacePart(Part part)
        {
            // 1. Пробуем разместить без поворота
            if (TryPlace(part.Width, part.Height, out var rect))
            {
                PlacedParts.Add(new PlacedPart { Part = part, X = rect.X, Y = rect.Y });
                return true;
            }

            // 2. Пробуем с поворотом (если допустимо)
            if (TryPlace(part.Height, part.Width, out rect))
            {
                PlacedParts.Add(new PlacedPart { Part = part, X = rect.X, Y = rect.Y });
                return true;
            }

            return false;
        }

        private bool TryPlace(double width, double height, out Rectangle placement)
        {
            placement = null;
            Rectangle bestRect = null;
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
            placement = new Rectangle(bestRect.X, bestRect.Y, width, height);

            // Обновляем список свободных областей
            SplitFreeRects(placement);
            return true;
        }

        private void SplitFreeRects(Rectangle placedRect)
        {
            List<Rectangle> newRects = new List<Rectangle>();

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
                    newRects.Add(new Rectangle(
                        freeRect.X, freeRect.Y,
                        placedRect.X - freeRect.X, freeRect.Height));
                }

                if (placedRect.Right < freeRect.Right)
                {
                    newRects.Add(new Rectangle(
                        placedRect.Right, freeRect.Y,
                        freeRect.Right - placedRect.Right, freeRect.Height));
                }

                // Разделение по горизонтали
                if (placedRect.Y > freeRect.Y)
                {
                    newRects.Add(new Rectangle(
                        freeRect.X, freeRect.Y,
                        freeRect.Width, placedRect.Y - freeRect.Y));
                }

                if (placedRect.Bottom < freeRect.Bottom)
                {
                    newRects.Add(new Rectangle(
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

    // Класс для хранения данных о детали
    public class Part
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int Id { get; set; } // Опционально, для идентификации
    }

    public class PlacedPart
    {
        public Part Part { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Rectangle
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Top { get; private set; }
        public double Bottom { get; private set; }

        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(Rectangle other)
        {
            return X < other.X + other.Width &&
                   X + Width > other.X &&
                   Y < other.Y + other.Height &&
                   Y + Height > other.Y;
        }
    }
}
