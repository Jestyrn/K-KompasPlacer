using netDxf;
using netDxf.Entities;

namespace Models
{
    public class TryRewrite
    {
        private string Ex = "";

        private List<FramePackage> FramePackages;
        private List<Insert> Result;
        private List<Detail> Details;

        private BoundingBox InnerSize;
        private BoundingBox OuterSize;

        private int DetailPading;
        private int FramesPading;
        private int InnerPading;

        public TryRewrite(List<Detail> dets, double width, double height, int dPad = 0, int iPad = 0, int fPad = 0)
        {
            DetailPading = dPad;
            FramesPading = fPad;
            InnerPading = iPad;

            Details = new List<Detail>(dets);
            FramePackages = new List<FramePackage>();
            Result = new List<Insert>();

            OuterSize = new BoundingBox(
                0,0,
                0 + width,
                0-height);

            InnerSize = new BoundingBox(
                0, 0,
                0 - width - iPad,
                0 + height + iPad);
        }
    
        public List<Insert> Execute()
        {
            Result.Clear();

            foreach (var detail in Details)
            {
                bool placed = false;

                foreach (var pack in FramePackages)
                {
                    var frame = pack.Frame;

                    if (pack.isFull) continue;
                    if (frame.Capacity < detail.Area) continue;
                    bool canInsert = frame.FreeRects.Any(f =>
                        (f.Width >= detail.Width && f.Height >= detail.Height) ||
                        (f.Width >= detail.Height && f.Height >= detail.Width));

                    if (!canInsert) continue;

                    Vector2 pos = FindPosition(frame.FreeRects, detail.Bounds, out bool willPlaced, out bool willRotaed);
                    if (!willPlaced) continue;

                    if (!TryPlace(pack, detail, pos, willRotaed)) continue;

                    break;
                }

                if (!placed)
                {
                    if (InnerSize.CanInsert(detail.Bounds))
                    {
                        Ex = $"Выбран слишком маленький размер\n" +
                            $"Frame: {Frame.Bounds.Width}, {Frame.Bounds.Height}\n" +
                            $"Detail: {detail.Bounds.Width}, {detail.Bounds.Height}";

                        return null;
                    }

                    AddNewFrame(detail);
                }
            }

            foreach (var pack in FramePackages)
            {
                Result.Add(pack.Frame.Insert);
                Result.AddRange(pack.Details.Select(d => d.Insert));
            }

            FramePackages.Clear();
            return Result;
        }

        private bool TryPlace(FramePackage pack, Detail detail, Vector2 pos, bool willRotaed)
        {
            var frame = pack.Frame;
            var bounds = detail.Bounds;

            var futureBox = new BoundingBox(
                pos.X,
                pos.Y,
                pos.X + bounds.Width,
                pos.Y - bounds.Height);

            double newX1 = futureBox.MinX;
            double newY1 = futureBox.MinY;
            double newX2 = futureBox.MaxX + DetailPading > InnerSize.MaxX ? futureBox.MaxX + (InnerSize.MaxX - futureBox.MaxX) : futureBox.MaxX + DetailPading;
            double newY2 = futureBox.MaxY + DetailPading > InnerSize.MaxY ? futureBox.MaxY - (InnerSize.MaxY + futureBox.MaxY) : futureBox.MaxY + DetailPading;

            var paddedBox = new BoundingBox(newX1, newX2, newY1, newY2);
            if (frame.BoundingBox.InsideBounds(paddedBox)) return false;

            if (willRotaed) detail.RotateDetail(90);

            detail.MoveTo(paddedBox.MinX, paddedBox.MinY);

            frame.Capacity -= paddedBox.Area;
            frame.FreeRects = UpdateFreeRects(frame.FreeRects, paddedBox);
            return true;
        }

        private Vector2 FindPosition(List<BoundingBox> rects, BoundingBox bounds, out bool willPlaced, out bool willRotaed)
        {
            willPlaced = false;
            willRotaed = false;

            double bestScore = int.MaxValue;
            BoundingBox bestBounds = null;

            foreach (var free in rects)
            {
                double score = free.Area - (bounds.Width * bounds.Height);

                if (free.Width >= bounds.Width && free.Height >= bounds.Height)
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBounds = free;
                        willRotaed = false;
                        willPlaced = true;
                    }
                }

                if (free.Width >= bounds.Height && free.Height >= bounds.Width)
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBounds = free;
                        willRotaed = true;
                        willPlaced = true;
                    }
                }
            }

            return bestBounds == null ? Vector2.Zero : new Vector2(bestBounds.MinX, bestBounds.MinY);
        }

        private void AddNewFrame(Detail detail)
        {
            var pos = FramePackages.Count == 0 ?
                new Vector2(0, 0) :
                new Vector2(FramePackages.Last().Frame.BoundingBox.MaxX + FramesPading + (InnerPading * 2), 0);

            detail.MoveTo(pos.X, pos.Y);

            var newPack = new FramePackage
            {
                Frame = new Frame(pos, InnerSize.Width, InnerSize.Height),
                Details = new List<Detail> { detail }
            };

            var det = new BoundingBox(
                pos.X,
                pos.Y,
                pos.X + DetailPading,
                pos.Y - DetailPading);

            newPack.Frame.Capacity -= detail.Area;
            newPack.Frame.FreeRects = UpdateFreeRects(newPack.Frame.FreeRects, detail.Bounds);

            FramePackages.Add(newPack);
        }

        private List<BoundingBox> UpdateFreeRects(List<BoundingBox> rects, BoundingBox taken)
        {
            var result = new List<BoundingBox>();

            foreach (var free in rects)
            {
                if (!free.Intersects(taken))
                {
                    result.Add(free);
                    continue;
                }

                double width = free.Width - taken.Width;
                double height = free.Height - taken.Height;
                bool vertCut = width > height;

                if (vertCut)
                {
                    // вертикально
                    result.Add(new BoundingBox(
                        free.MinX,
                        taken.MaxX,
                        taken.MaxY,
                        free.MaxY
                        ));

                    result.Add(new BoundingBox(
                        taken.MaxX,
                        free.MaxX,
                        free.MinY,
                        free.MaxY
                        ));
                }
                else
                {
                    result.Add(new BoundingBox(
                        taken.MaxX, 
                        free.MaxX,
                        free.MinY,
                        taken.MaxY
                        ));

                    result.Add(new BoundingBox(
                        free.MinX,
                        free.MaxX,
                        taken.MaxY,
                        free.MaxY
                        ));
                }
            }

            // Внести оптимизации

            return result;
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
