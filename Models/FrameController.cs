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
                    var frame = package.Frame;

                    bool canInsert = frame.FreeRects.Any(free =>
                        (free.Width >= detail.Width && free.Height >= detail.Height) ||
                        (free.Width >= detail.Height && free.Height >= detail.Width));

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
                detail.MoveDetail(position.X, position.Y);
            }
            else if (package.Frame.BoundingBox.MinX == position.X)
            {
                detail.MoveDetail(position.X, position.Y - detail.Pading);
            }
            else if (package.Frame.BoundingBox.MinY == position.Y)
            {
                detail.MoveDetail(position.X + detail.Pading, position.Y);
            }
            else
            {
                detail.MoveDetail(position.X + detail.Pading, position.Y - detail.Pading);
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

            detail.MoveDetail(pos.X, pos.Y);

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

            return result;
        }
    }

    public class FramePackage
    {
        public Frame Frame;
        public List<Detail> Details;

        public FramePackage()
        {
            Details = new List<Detail>();
        }
    }
}
