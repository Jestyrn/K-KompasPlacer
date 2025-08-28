using netDxf.Entities;
using Vector2 = netDxf.Vector2;
using System.Windows;
using System.Text;
using System.Net.Http.Headers;

namespace TestModule
{
    public class FrameController
    {
        public string Ex = "";

        private List<FramePackage> FramePackages;
        private List<Insert> Result;
        private List<Detail> Details;

        private BoundingBox InnerSize;
        private BoundingBox OuterSize;

        private int DetailPading;
        private int FramesPading;
        private int InnerPading;

        public FrameController(List<Detail> dets, double width, double height, int iPad = 0, int fPad = 0, int dPad = 0)
        {
            InnerPading = iPad;
            FramesPading = fPad;
            DetailPading = dPad;

            Details = new List<Detail>(dets);
            FramePackages = new List<FramePackage>();
            Result = new List<Insert>();

            OuterSize = new BoundingBox(0, width, 0,-height);

            InnerSize = new BoundingBox(
                OuterSize.MinX + InnerPading,
                OuterSize.MaxX - InnerPading,
                OuterSize.MinY - InnerPading,
                OuterSize.MaxY + InnerPading
                );
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

                    if (!TryPlace(pack, detail, pos, willRotaed, out placed)) continue;

                    break;
                }

                if (!placed)
                {
                    if (!InnerSize.CanInsert(detail.Bounds))
                    {
                        Ex = $"Выбран слишком маленький размер\n" +
                            $"Frame: {InnerSize.Width}, {InnerSize.Height}\n" +
                            $"Detail: {detail.Bounds.Width + DetailPading}, {detail.Bounds.Height + DetailPading}\n" +
                            $"Показаны размеры с учетом отступов";

                        return null;
                    }

                    AddNewFrame(detail);
                }
            }

            foreach (var pack in FramePackages)
            {
                var startAxle = new Vector2(pack.Frame.BoundingBox.MinX - InnerPading, pack.Frame.BoundingBox.MinY + InnerPading);
                var fr = new Frame(startAxle, OuterSize.Width, OuterSize.Height);

                Result.Add(fr.Insert);
                Result.AddRange(pack.Details.Select(d => d.Insert));
            }

            FramePackages.Clear();
            return Result;
        }

        private bool TryPlace(FramePackage pack, Detail detail, Vector2 pos, bool willRotaed, out bool placed)
        {
            placed = false;
            var frame = pack.Frame;
            var bounds = detail.Bounds;

            var futureBox = new BoundingBox(
                pos.X,
                pos.X + bounds.Width,
                pos.Y,
                pos.Y - bounds.Height);

            double newX1 = futureBox.MinX;
            double newY1 = futureBox.MinY;
            double newX2 = futureBox.MaxX + DetailPading > frame.BoundingBox.MaxX ? -1 : futureBox.MaxX + DetailPading;
            double newY2 = futureBox.MaxY - DetailPading < frame.BoundingBox.MaxY ? 1 : futureBox.MaxY - DetailPading;

            if (newX2 == -1 || newY2 == 1) return false;

            var paddedBox = new BoundingBox(newX1, newX2, newY1, newY2);
            if (!frame.BoundingBox.InsideBounds(paddedBox)) return false;

            if (willRotaed) detail.RotateDetail(90);

            detail.MoveTo(paddedBox.MinX, paddedBox.MinY);

            frame.Capacity -= paddedBox.Area;
            pack.Details.Add(detail);
            frame.FreeRects = UpdateFreeRects(frame.FreeRects, paddedBox);

            placed = true;
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
                new Vector2(InnerSize.MinX, InnerSize.MinY) :
                new Vector2(FramePackages.Last().Frame.BoundingBox.MaxX + FramesPading + (InnerPading * 2), InnerSize.MinY);

            detail.MoveTo(pos.X, pos.Y);

            var newPack = new FramePackage
            {
                Frame = new Frame(pos, InnerSize.Width, InnerSize.Height),
                Details = new List<Detail> { detail }
            };

            var det = new BoundingBox(
                pos.X,
                pos.X + detail.Bounds.Width + DetailPading,
                pos.Y,
                pos.Y - detail.Bounds.Height - DetailPading);

            newPack.Frame.Capacity -= detail.Area;
            newPack.Frame.FreeRects = UpdateFreeRects(newPack.Frame.FreeRects, det);

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
                    // горизонтально
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

            result = MergeRects(result);

            return result;
        }

        private List<BoundingBox> MergeRects(List<BoundingBox> result)
        {
            result = PruneContainedRects(result);
            result = MergeAdjacentRects(result);
            // result = SimpleMerge(result);
            return result;
        }

        private List<BoundingBox> SimpleMerge(List<BoundingBox> result)
        {
            List<BoundingBox> concatX = new List<BoundingBox>();
            List<BoundingBox> concatY = new List<BoundingBox>();

            for (int i = 0; i < result.Count; i++)
            {
                var a = result[i];

                concatX.AddRange(result.Where(r => (r.MinX == a.MinX) && (r.MaxX == a.MaxX)));
                concatY.AddRange(result.Where(r => (r.MinY == a.MinY) && (r.MaxY == a.MaxY)));

                if (concatX.Count > 0)
                {
                    result.Add(new BoundingBox(a.MinX, a.MaxX, concatX.Max(y1 => y1.MinY), concatX.Min(y2 => y2.MaxY)));

                    result.Remove(a);

                    for (int j = 0; j < concatX.Count; j++)
                    {
                        result.Remove(concatX[j]);
                    }

                    concatX.Clear();
                }

                if (concatY.Count > 0)
                {
                    result.Add(new BoundingBox(concatY.Min(x1 => x1.MinX), concatY.Max(x2 => x2.MaxX), a.MinY, a.MaxY));

                    result.Remove(a);

                    for (int j = 0; j < concatY.Count; j++)
                    {
                        result.Remove(concatY[j]);
                    }

                    concatY.Clear();
                }
            }

            return result;
        }

        private List<BoundingBox> PruneContainedRects(List<BoundingBox> rects)
        {
            var outList = new List<BoundingBox>(rects);
            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = 0; j < rects.Count; j++)
                {
                    if (i == j) continue;
                    var a = rects[i];
                    var b = rects[j];
                    if (a == null || b == null) continue;
                    if (IsContainedIn(a, b))
                    {
                        outList.Remove(a);
                        break;
                    }
                }
            }
            return outList;
        }

        private bool IsContainedIn(BoundingBox a, BoundingBox b)
        {
            return a.MinX >= b.MinX - 1e-9 && a.MaxX <= b.MaxX + 1e-9 && a.MinY >= b.MinY - 1e-9 && a.MaxY <= b.MaxY + 1e-9;
        }

        private List<BoundingBox> MergeAdjacentRects(List<BoundingBox> rects)
        {
            var list = new List<BoundingBox>(rects);
            bool mergedAny;

            do
            {
                mergedAny = false;
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        var a = list[i]; var b = list[j]; if (a == null || b == null) continue; 
                        
                        // если вертикально соседствуют и по X границы совпадают
                        if (Math.Abs(a.MinX - b.MinX) < 1e-9 && Math.Abs(a.MaxX - b.MaxX) < 1e-9)
                        {
                            // если a.MaxY == b.MinY или b.MaxY == a.MinY — можно объединить по Y
                            if (Math.Abs(a.MaxY - b.MinY) < 1e-9)
                            {
                                var merged = new BoundingBox(a.MinX, a.MaxX, a.MinY, b.MaxY);
                                list[i] = merged;
                                list.RemoveAt(j);
                                mergedAny = true;
                                break;
                            }

                            if (Math.Abs(b.MaxY - a.MinY) < 1e-9)
                            {
                                var merged = new BoundingBox(a.MinX, a.MaxX, b.MinY, a.MaxY);
                                list[i] = merged;
                                list.RemoveAt(j);
                                mergedAny = true;
                                break;
                            }
                        }

                        // если горизонтально соседствуют и по Y границы совпадают
                        if (Math.Abs(a.MinY - b.MinY) < 1e-9 && Math.Abs(a.MaxY - b.MaxY) < 1e-9)
                        {
                            if (Math.Abs(a.MaxX - b.MinX) < 1e-9)
                            {
                                var merged = new BoundingBox(a.MinX, b.MaxX, a.MinY, a.MaxY);
                                list[i] = merged;
                                list.RemoveAt(j);
                                mergedAny = true;
                                break;
                            }
                            if (Math.Abs(b.MaxX - a.MinX) < 1e-9)
                            {
                                var merged = new BoundingBox(b.MinX, a.MaxX, a.MinY, a.MaxY);
                                list[i] = merged;
                                list.RemoveAt(j);
                                mergedAny = true;
                                break;
                            }
                        }
                    }

                    if (mergedAny) break;
                }

            } while (mergedAny);

            return list;
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
