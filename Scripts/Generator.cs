using Godot;
using Godot.Collections;

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
    private float VoxelSize = 1f;

    // Chunks
    private Dictionary LoadedChunks = new Dictionary();
    private Dictionary PreloadedChunks = new Dictionary();

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
        WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        Water = new MeshInstance();
        WaterMesh = new PlaneMesh();
        WaterMesh.Size = new Vector2(VoxelSize * ChunkSize.x * RenderDistance * 32, VoxelSize * ChunkSize.z * RenderDistance * 32);
        WaterMesh.SetMaterial(WaterMaterial);
        Water.SetMesh(WaterMesh);
        Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        AddChild(Water);

        int max = PreloadDistance ;

        for (int i = -PreloadDistance / 2; i < PreloadDistance / 2; i++)
            for (int j = -PreloadDistance / 2; j < PreloadDistance / 2; j++)
            {
                GD.Print("Preloading World : " + ((i + PreloadDistance + j + PreloadDistance) / max) * 100 + "% done.");
                new Chunk(new Vector2(i, j), PreloadedChunks, Noise);
            }
    }
    public override void _Process(float delta)
    {
        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        for (int x = -RenderDistance + camX; x < RenderDistance + camX; x++)
            for (int z = -RenderDistance + camZ; z < RenderDistance + camZ; z++)
                if (!LoadedChunks.ContainsKey(new Vector2(x, z)))
                    new Chunk(new Vector2(x, z), PreloadedChunks, Noise);
        if (tick < UpdateRate)
        {
            tick += delta;
            return;
        }
        tick = 0;
        
        if (PreloadedChunks.Count > 0)
            foreach (Chunk c in PreloadedChunks.Values)
            {
                ThreadPreload(c);
                PreloadedChunks.Remove(c.Offset);
            }

        


        
    }


    private void GenerateSeed()
    {
        if(CurrentSeed != 0)
            return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;

        // Noise.Lacunarity = 0.25f;
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 5;
        Noise.Period = 500;
        Noise.Persistence = 0.75f;
    }

  
    private void ThreadPreload(Chunk pChunk)
    {
        SurfaceTool = new SurfaceTool();
        SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        SurfaceTool.SetMaterial(VoxMaterial);
       
        foreach (Vector3 vox in pChunk.Voxels.Values)
        {
            CreateVoxel(SurfaceTool, vox, pChunk);
        }

        var chunk = new MeshInstance
        {
            Mesh = SurfaceTool.Commit(),
            Name = pChunk.Offset.ToString(),
            Translation = new Vector3(pChunk.Offset.x * pChunk.ChunkSize.x, 0, pChunk.Offset.y * pChunk.ChunkSize.z)
        };
        //chunk.CreateTrimeshCollision(); // Collisions generation.
        chunk.AddToGroup("Chunk");
        AddChild(chunk);

        if (!LoadedChunks.ContainsKey(pChunk.Offset))
            LoadedChunks.Add(pChunk.Offset, pChunk);

        SurfaceTool.Clear();
    }


    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private void CreateVoxel(SurfaceTool pSurfaceTool, Vector3 pPosition, Chunk pChunk)
    {

        pSurfaceTool.AddColor(new Color("76552b").Blend(new Color("59637d").Darkened(pPosition.y / pChunk.ChunkSize.y / 2)));

        bool left, right, front, back, top, bottom = false;

        left = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(-1, 0, 0));
        right = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(1, 0, 0));
        front = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, 1));
        back = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, -1));
        top = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 1, 0));
        bottom = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, -1, 0));

        if (left  && right && front && back && top && bottom || pPosition.y < 0) return;
        // Color
        if ( top || top && (left || right || front|| back))
            pSurfaceTool.AddColor(new Color("66CD00"));

        if (pPosition.y < WaterLevel + rng.RandiRange(1, 2))
        {
            pSurfaceTool.AddColor(new Color("EDC9AF"));
        }

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
            if (pPosition.x != ChunkSize.x - 1)
            {
                    pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
                    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[5] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[1] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            }
            else if(PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(1, 0)))
            {
                Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(1, 0)] as Chunk;
                if (!nb.Voxels.ContainsKey(new Vector3(0, pPosition.y, pPosition.z)))
                {
                    pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
                    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[5] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[1] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[5] + pPosition);
                }
            }
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
            if (pPosition.z != ChunkSize.z - 1)
            {
                pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
                pSurfaceTool.AddVertex(Vertices[3] + pPosition);
                pSurfaceTool.AddVertex(Vertices[6] + pPosition);
                pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                pSurfaceTool.AddVertex(Vertices[3] + pPosition);
                pSurfaceTool.AddVertex(Vertices[7] + pPosition);
                pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            }
            else if (PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(0, 1)))
            {
                Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(0, 1)] as Chunk;
                if (!nb.Voxels.ContainsKey(new Vector3(pPosition.x, pPosition.y, 0)))
                {
                    pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
                    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[7] + pPosition);
                    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
                }
            }
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
        if (bottom && pPosition.y != 0)
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

    // Check if there is a voxel at X Y Z position in the current preloaded chunk.
    private bool CanCreateFace(float x, float y, float z, Chunk pChunk)
    {
        return !pChunk.Voxels.ContainsKey(new Vector3(x, y, z));//|| 

    }
}
