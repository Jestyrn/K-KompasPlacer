using netDxf.Entities;
using Vector2 = netDxf.Vector2;
using System.Windows;

namespace TestModule
{
    public class FrameController
    {
        private List<FramePackage> framesPackages;
        private List<Insert> Result;
        private List<Detail> details;

        private double width;
        private double height;

        private int Pading;
        public string Ex = "";

        public FrameController(List<Detail> details, double width, double height, int pading = 0, int insPading = 0)
        {
            this.details = details;
            framesPackages = new List<FramePackage>();
            this.width = width;
            this.height = height;

            Pading = pading;
            Frame.Pading = insPading;
        }

        public List<Insert> TakeFilled()
        {
            Result = new List<Insert>();

            foreach (var detail in details)
            {
                bool detailPlaced = false;

                foreach (var package in framesPackages)
                {
                    if (package.isFull && (framesPackages.IndexOf(package) != framesPackages.Count)) continue;

                    int cnt = framesPackages.Count;

                    if (cnt > 80 && (framesPackages.IndexOf(package) > 80))
                    {

                    }

                    var frame = package.Frame;

                    bool canInsert = frame.FreeRects.Any(free =>
                        (free.Width >= detail.Width && free.Height >= detail.Height) ||
                        (free.Width >= detail.Height && free.Height >= detail.Width));

                    // --- //

                    var mostMinDetail = details.OrderByDescending(x => x.Area).Last();
                    double detailMinWidth = mostMinDetail.Width;
                    double detailMinHeight = mostMinDetail.Height;

                    var bigFreeRect = frame.FreeRects.OrderByDescending(x => x.Area).First();
                    bool normConfirm = (bigFreeRect.Width < detailMinWidth) && (bigFreeRect.Height < detailMinHeight);
                    bool rotateConfirm = (bigFreeRect.Width < detailMinHeight) && (bigFreeRect.Height < detailMinWidth);

                    if (normConfirm || rotateConfirm)
                    {
                        package.isFull = true;
                        continue;
                    }

                    // --- //

                    if (!canInsert)
                    {
                        continue;
                    }

                    Vector2 newPos = FindBestPosition(frame, detail, out bool foundPosition);
                    if (!foundPosition) continue;

                    if (!PlaceDetail(package, detail, newPos)) continue;
                    detailPlaced = true;

                    break;
                }

                if (!detailPlaced)
                {
                    if (!Frame.Bounds.CanInsert(detail.Bounds))
                    {
                        Ex = $"Выбран слишком маленький размер\n" +
                                            $"Frame: {Frame.Bounds.Width}, {Frame.Bounds.Height}\n" +
                                            $"Detail: {detail.Bounds.Width}, {detail.Bounds.Height}";

                        return null;
                    }

                    AddNewFrameWithDetail(detail);
                }
            }

            foreach (var package in framesPackages)
            {
                Result.Add(package.Frame.Insert);
                Result.AddRange(package.Details.Select(d => d.Insert));
            }

            framesPackages.Clear();
            return Result;
        }

        private bool PlaceDetail(FramePackage package, Detail detail, Vector2 position)
        {
            if (package.Frame.BoundingBox.MinX == position.X && package.Frame.BoundingBox.MinY == position.Y)
            {
                detail.MoveTo(position.X, position.Y);
            }
            else if (package.Frame.BoundingBox.MinX == position.X)
            {
                detail.MoveTo(position.X, position.Y - detail.Padding);
            }
            else if (package.Frame.BoundingBox.MinY == position.Y)
            {
                detail.MoveTo(position.X + detail.Padding, position.Y);
            }
            else
            {
                detail.MoveTo(position.X + detail.Padding, position.Y - detail.Padding);
            }

            if (package.Frame.BoundingBox.Intersects(detail.Bounds))
                if (!package.Frame.BoundingBox.InsideBounds(detail.Bounds))
                    return false;
            
            package.Details.Add(detail);
            package.Frame.Capacity -= detail.Area;
            package.Frame.FreeRects = UpdateFreeRects(package.Frame.FreeRects, detail.Bounds);
            return true;
        }

        private void AddNewFrameWithDetail(Detail detail)
        {
            Vector2 pos = framesPackages.Count == 0
                ? new Vector2(0, 0)
                : new Vector2(framesPackages.Last().Frame.BoundingBox.MaxX + Pading + Frame.Pading, 0);

            detail.MoveTo(pos.X + Frame.Pading, pos.Y - Frame.Pading);

            var newPackage = new FramePackage
            {
                Frame = new Frame(pos, width, height),
                Details = new List<Detail> { detail }
            };

            newPackage.Frame.Capacity -= detail.Area;
            newPackage.Frame.FreeRects = UpdateFreeRects(newPackage.Frame.FreeRects, detail.Bounds);

            framesPackages.Add(newPackage);
        }

        private Vector2 FindBestPosition(Frame frame, Detail detail, out bool placed)
        {
            placed = false;
            double bestScore = double.MaxValue;
            BoundingBox bestBox = null;
            bool shouldRotate = false;

            foreach (var freePlace in frame.FreeRects)
            {
                if (freePlace.Width >= detail.Width && freePlace.Height >= detail.Height)
                {
                    double gapWidth = freePlace.Width - detail.Width;
                    double gapHeight = freePlace.Height - detail.Height;

                    double score = Math.Min(gapWidth, gapHeight);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = freePlace;
                        shouldRotate = false;
                    }
                }

                if (freePlace.Width >= detail.Height && freePlace.Height >= detail.Width)
                {
                    double gapWidth = freePlace.Width - detail.Height;
                    double gapHeight = freePlace.Height - detail.Width;

                    double score = Math.Min(gapWidth, gapHeight);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBox = freePlace;
                        shouldRotate = true;
                    }
                }
            }

            if (bestBox != null)
            {
                if (shouldRotate)
                    detail.RotateDetail(90);
                placed = true;
                return new Vector2(bestBox.MinX, bestBox.MinY);
            }

            return Vector2.Zero;
        }

        private List<BoundingBox> UpdateFreeRects(List<BoundingBox> freeRects, BoundingBox takenBox)
        {
            var result = new List<BoundingBox>();

            foreach (var bounds in freeRects)
            {
                if (!bounds.Intersects(takenBox))
                {
                    result.Add(bounds);
                    continue;
                }

                double widthScope, heightScope;
                widthScope = Math.Abs(takenBox.Width / takenBox.Height);
                heightScope = Math.Abs(takenBox.Height / takenBox.Width);

                if (widthScope > 1.5)
                {
                    // Правая верняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        takenBox.MaxY
                        ));

                    // Нижняя большая часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        bounds.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));
                }
                else if (heightScope > 1.5)
                {
                    // Левая нижняя часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        takenBox.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));

                    // Правая большая часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        bounds.MaxY
                        ));
                }
                else
                {
                    // Правая верняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        bounds.MinY,
                        takenBox.MaxY
                        ));

                    // Левая нижняя часть
                    result.Add(new BoundingBox(
                        bounds.MinX,
                        takenBox.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));

                    // Правая нижняя часть
                    result.Add(new BoundingBox(
                        takenBox.MaxX,
                        bounds.MaxX,
                        takenBox.MaxY,
                        bounds.MaxY
                        ));
                }
            }

            result = OptimizeSplit( result );

            return result;
        }

        private List<BoundingBox> OptimizeSplit(List<BoundingBox> rects)
        {
            // Делаем два сценария
            var splitX = NormalizeAndMerge(rects, splitByX: true);
            var splitY = NormalizeAndMerge(rects, splitByX: false);

            // Считаем площадь
            double areaX = splitX.Sum(r => r.Area);
            double areaY = splitY.Sum(r => r.Area);

            // Можно вводить более сложный критерий:
            // например, выбрать по максимальной площади одного прямоугольника
            double maxRectX = splitX.Max(r => r.Area);
            double maxRectY = splitY.Max(r => r.Area);

            // Тут простая логика: выбираем тот вариант, где общая площадь больше
            if (areaX > areaY) return splitX;
            if (areaY > areaX) return splitY;

            // Если площади одинаковы — выбираем тот, где меньше прямоугольников
            if (splitX.Count < splitY.Count) return splitX;
            if (splitY.Count < splitX.Count) return splitY;

            // Если всё одинаково — выбираем по самому крупному прямоугольнику
            return maxRectX >= maxRectY ? splitX : splitY;
        }

        private List<BoundingBox> NormalizeAndMerge(List<BoundingBox> rects, bool splitByX)
        {
            var coords = splitByX
                ? rects.SelectMany(r => new[] { r.MinX, r.MaxX }).Distinct().OrderBy(x => x).ToList()
                : rects.SelectMany(r => new[] { r.MinY, r.MaxY }).Distinct().OrderBy(y => y).ToList();

            var otherCoords = splitByX
                ? rects.SelectMany(r => new[] { r.MinY, r.MaxY }).Distinct().OrderBy(y => y).ToList()
                : rects.SelectMany(r => new[] { r.MinX, r.MaxX }).Distinct().OrderBy(x => x).ToList();

            var cells = new List<BoundingBox>();

            for (int i = 0; i < coords.Count - 1; i++)
            {
                for (int j = 0; j < otherCoords.Count - 1; j++)
                {
                    BoundingBox cell;
                    if (splitByX)
                        cell = new BoundingBox(coords[i], coords[i + 1], otherCoords[j], otherCoords[j + 1]);
                    else
                        cell = new BoundingBox(otherCoords[j], otherCoords[j + 1], coords[i], coords[i + 1]);

                    if (rects.Any(r => cell.Intersects(r)))
                    {
                        cells.Add(cell);
                    }
                }
            }

            return AdvancedMerge(cells);
        }

        private List<BoundingBox> AdvancedMerge(List<BoundingBox> rects)
        {
            bool merged;
            do
            {
                merged = false;

                // Удаляем вложенные
                for (int i = 0; i < rects.Count; i++)
                {
                    for (int j = rects.Count - 1; j >= 0; j--)
                    {
                        if (i == j) continue;
                        if (rects[j].Intersects(rects[i]))
                        {
                            rects.RemoveAt(j);
                            merged = true;
                        }
                    }
                }

                // Склеивание полос (по X или по Y)
                for (int i = 0; i < rects.Count; i++)
                {
                    var a = rects[i];

                    // Ищем все совпадающие по высоте (одинаковые MinY/MaxY)
                    var sameRow = rects.Where(r => r.MinY == a.MinY && r.MaxY == a.MaxY).OrderBy(r => r.MinX).ToList();
                    if (sameRow.Count > 1)
                    {
                        // Проверим, идут ли они подряд без разрывов
                        bool contiguous = true;
                        for (int k = 0; k < sameRow.Count - 1; k++)
                        {
                            if (sameRow[k].MaxX != sameRow[k + 1].MinX)
                            {
                                contiguous = false;
                                break;
                            }
                        }

                        if (contiguous)
                        {
                            var mergedBox = new BoundingBox(
                                sameRow.First().MinX,
                                sameRow.Last().MaxX,
                                a.MinY,
                                a.MaxY
                            );
                            rects.RemoveAll(r => sameRow.Contains(r));
                            rects.Add(mergedBox);
                            merged = true;
                            break;
                        }
                    }

                    // То же самое для колонок (по X)
                    var sameCol = rects.Where(r => r.MinX == a.MinX && r.MaxX == a.MaxX).OrderBy(r => r.MinY).ToList();
                    if (sameCol.Count > 1)
                    {
                        bool contiguous = true;
                        for (int k = 0; k < sameCol.Count - 1; k++)
                        {
                            if (sameCol[k].MaxY != sameCol[k + 1].MinY)
                            {
                                contiguous = false;
                                break;
                            }
                        }
                        if (contiguous)
                        {
                            var mergedBox = new BoundingBox(
                                a.MinX,
                                a.MaxX,
                                sameCol.First().MinY,
                                sameCol.Last().MaxY
                            );
                            rects.RemoveAll(r => sameCol.Contains(r));
                            rects.Add(mergedBox);
                            merged = true;
                            break;
                        }
                    }
                }

            } while (merged);

            return rects;
        }

    }

    public class FramePackage
    {
        public Frame Frame;
        public List<Detail> Details;
        public bool isFull;

        public FramePackage()
        {
            Details = new List<Detail>();
            isFull = false;
        }
    }
}
