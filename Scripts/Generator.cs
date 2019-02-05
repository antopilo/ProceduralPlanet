using Godot;
using System.Collections.Generic;



public class Generator : Node
{
    [Export] int CurrentSeed = 0;
    // Noises
    private OpenSimplexNoise Noise = new OpenSimplexNoise();
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Camera Camera;
    SurfaceTool SurfaceTool;
    private SpatialMaterial VoxMaterial;
    private SpatialMaterial WaterMaterial;

    private Vector3 ChunkSize = new Vector3(16, 128, 16);

    [Export] private int RenderDistance = 8;
    [Export] private int PreloadDistance = 12;

    // Chunks
    private Dictionary<Vector2, Chunk> LoadedChunks = new Dictionary<Vector2, Chunk>();
    private Dictionary<Vector2, Chunk> PreloadedChunks = new Dictionary<Vector2, Chunk>();

    private Thread Thread;

    // Water
    private MeshInstance Water;
    private PlaneMesh WaterMesh;

    private float WaterOffset = 0.5f;
    private int WaterLevel = 70;

    float tick = 0;
    [Export] float UpdateRate = 0.1f;
    private Vector3[] Vertices = { new Vector3(0, 0, 0),
                                   new Vector3(1, 0, 0),
                                   new Vector3(1, 0, 1),
                                   new Vector3(0, 0, 1),
                                   new Vector3(0, 1, 0),
                                   new Vector3(1, 1, 0),
                                   new Vector3(1, 1, 1),
                                   new Vector3(0, 1, 1) };


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
        Camera = GetNode("../Camera") as Camera;
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;
        //WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        Water = new MeshInstance();
        WaterMesh = new PlaneMesh();
        WaterMesh.Size = new Vector2(ChunkSize.x * RenderDistance * 32, ChunkSize.z * RenderDistance * 32);
        WaterMesh.SetMaterial(WaterMaterial);
        Water.SetMesh(WaterMesh);
        Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        AddChild(Water);

        
        Thread = new Godot.Thread();
        Preload();


        //new Chunk(new Vector2(0, 0), PreloadedChunks, Noise, Thread1);
    }

    public void Preload()
    {
        int count = 0;
        for (int i = -PreloadDistance; i < PreloadDistance; i++)
            for (int j = -PreloadDistance; j < PreloadDistance; j++)
            {
                count++;
                GD.Print("Preloading World : " + (count) + " / " + (PreloadDistance * 2) * PreloadDistance * 2 + "done.");
                new Chunk(new Vector2(i, j), PreloadedChunks, Noise);
            }
    }


    public override void _Process(float delta)
    {
        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        //for (int x = -RenderDistance + camX; x < RenderDistance + camX; x++)
        //    for (int z = -RenderDistance + camZ; z < RenderDistance + camZ; z++)
        //        if (!LoadedChunks.ContainsKey(new Vector2(x, z)) )
        //              new Chunk(new Vector2(x, z), PreloadedChunks, Noise);
   
        if (tick < UpdateRate)
        {
            tick += delta;
            return;
        }
        tick = 0;

        if (PreloadedChunks.Count < 0)
            return;

        List<Vector2> keys = new List<Vector2>(PreloadedChunks.Keys);
        Render(PreloadedChunks[keys[0]]);
        PreloadedChunks.Remove(keys[0]);
        
    }


    /// <summary>
    ///  Create a random seed.
    /// </summary>
    private void GenerateSeed()
    {
        if(CurrentSeed != 0)
            return;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;

        // Noise.Lacunarity = 0.25f;
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 5;
        Noise.Period = 1250;
        Noise.Persistence = 0.75f;
    }

  
    private void Render(Chunk pChunk)
    {
        SurfaceTool = new SurfaceTool();
        SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        SurfaceTool.SetMaterial(VoxMaterial);
       
        foreach (Voxel vox in pChunk.Voxels.Values)
            CreateVoxel(SurfaceTool, vox, pChunk);

        MeshInstance chunk = new MeshInstance
        {
            Mesh = SurfaceTool.Commit(),
            Name = pChunk.Offset.ToString(),
            Translation = new Vector3(pChunk.Offset.x * pChunk.ChunkSize.x, 0, pChunk.Offset.y * pChunk.ChunkSize.z)
        };

        //chunk.CreateTrimeshCollision(); // Collisions generation.
        chunk.AddToGroup("Chunk");
        // Pop animation

        AddChild(chunk);

        //Tween t = new Tween();
        //AddChild(t);
        //t.InterpolateProperty(chunk, "scale", new Vector3(0, 0, 0), new Vector3(1, 1, 1), 2f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        //t.InterpolateProperty(chunk, "translation", chunk.Translation + new Vector3(0,255 * 2,0), chunk.Translation, 2f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        //chunk.Scale = new Vector3(0, 0, 0);
        //t.Start();

        if (!LoadedChunks.ContainsKey(pChunk.Offset))
            LoadedChunks.Add(pChunk.Offset, pChunk);

        SurfaceTool.Clear();
    }


    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private void CreateVoxel(SurfaceTool pSurfaceTool, Voxel pVoxel, Chunk pChunk)
    {
        Vector3 pPosition = pVoxel.Position;
        
        // Colors
        switch (pVoxel.type)
        {
            case BlockType.grass: // Grass
                pSurfaceTool.AddColor(new Color("3e8948").Darkened(rng.RandfRange(0f, 0.1f)));
                break;
            case BlockType.rock: // Rock
                pSurfaceTool.AddColor(new Color("5a6988"));
                break;
            case BlockType.sand: // Sand
                pSurfaceTool.AddColor(new Color("fee761"));
                break;
            case BlockType.wood: // Wood
                pSurfaceTool.AddColor(new Color("3e2731"));
                break;
            case BlockType.leaves:
                pSurfaceTool.AddColor(new Color("63c74d"));
                break;
            default:
                pSurfaceTool.AddColor(new Color("76552b"));
                break;
        }

        bool left = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(-1, 0, 0));
        bool right = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(1, 0, 0));
        bool front = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, 1));
        bool back = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, -1));
        bool top = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 1, 0));
        bool bottom = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, -1, 0));

        // If block is hidden.
        //if (left && right && front && back && top && bottom || pPosition.y < 0)
        //    return;

        if (top) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
        }
        if (right) // Right
        {
            pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //if (pPosition.x != ChunkSize.x - 1)
            //{
            //        pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //}
            //else if(PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(1, 0)))
            //{
            //    Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(1, 0)] as Chunk;
            //    if (!nb.Voxels.ContainsKey(new Vector3(0, pPosition.y, pPosition.z)))
            //    {
            //        pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //    }
            //}
        }
        if (left) // Left
        {
            pSurfaceTool.AddNormal(new Vector3(-1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
        }
        if (front) // Front
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //if (pPosition.z != ChunkSize.z - 1)
            //{
            //    pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            //    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //}
            //else if (PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(0, 1)))
            //{
            //    Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(0, 1)] as Chunk;
            //    if (!nb.Voxels.ContainsKey(new Vector3(pPosition.x, pPosition.y, 0)))
            //    {
            //        pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            //        pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //    }
            //}
        }
        if (back) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, -1));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
        }
        if (bottom)
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
        }
    }

}
