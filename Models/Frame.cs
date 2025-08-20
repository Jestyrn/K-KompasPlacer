using netDxf;
using netDxf.Blocks;
using netDxf.Entities;

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

        FreeRects = new List<BoundingBox> { BoundingBox };
        Capacity = BoundingBox.Area;
    }
}