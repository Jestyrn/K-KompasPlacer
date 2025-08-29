using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System.Collections.Generic;

public class Frame
{
    public static int Id { get; private set; } = 0;
    public static BoundingBox Bounds { get; private set; }
    public static int Pading { get; set; }


    public BoundingBox BoundingBox { get; private set; }
    public Insert Insert { get; private set; }
    public List<EntityObject> Entities { get; private set; }
    public List<BoundingBox> FreeRects { get; set; }

    public double Capacity { get; set; }

    private Vector2 Axle;
    private double Width;
    private double Height;

    public Frame(Vector2 axle, double width, double height)
    {
        Bounds = new BoundingBox(axle, width, height);

        Axle = new Vector2(axle.X + Pading, axle.Y - Pading);
        Width = width - (Pading * 2);
        Height = height - (Pading * 2);

        CreateBounds();
        CreateDxf();
    }

    public List<Insert> TakeFreeRectsDxf()
    {
        List<EntityObject> ents = new List<EntityObject>();
        List<Insert> ins = new List<Insert>(); 

        foreach (var free in FreeRects)
        {
            ents.Add(new Line(new Vector2(free.MinX, free.MinY), new Vector2(free.MaxX, free.MinY)));
            ents.Add(new Line(new Vector2(free.MaxX, free.MinY), new Vector2(free.MaxX, free.MaxY)));
            ents.Add(new Line(new Vector2(free.MaxX, free.MaxY), new Vector2(free.MinX, free.MaxY)));
            ents.Add(new Line(new Vector2(free.MinX, free.MaxY), new Vector2(free.MinX, free.MinY)));

            ins.Add(new Insert(new Block($"Frame_Free{Id}-res", ents)));
            ents.Clear();
        }

        return ins;
    }

    private void CreateDxf()
    {
        Block block = new Block($"Frame{Id}");
        block.Entities.Add(new Line(new Vector2(Bounds.MinX, Bounds.MinY), new Vector2(Bounds.MaxX, Bounds.MinY)));
        block.Entities.Add(new Line(new Vector2(Bounds.MaxX, Bounds.MinY), new Vector2(Bounds.MaxX, Bounds.MaxY)));
        block.Entities.Add(new Line(new Vector2(Bounds.MaxX, Bounds.MaxY), new Vector2(Bounds.MinX, Bounds.MaxY)));
        block.Entities.Add(new Line(new Vector2(Bounds.MinX, Bounds.MaxY), new Vector2(Bounds.MinX, Bounds.MinY)));

        Insert = new Insert(block);

        Id++;
    }

    private void CreateBounds()
    {
        BoundingBox = new BoundingBox(Axle, Width, Height);

        FreeRects = new List<BoundingBox>();
        FreeRects.Add(BoundingBox);
        Capacity = BoundingBox.Area;
    }
}