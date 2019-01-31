using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Generator : Node
{
    [Export] int CurrentSeed = 0;

    // Noises
    private OpenSimplexNoise Noise = new OpenSimplexNoise();

    private Camera Camera;
    SurfaceTool SurfaceTool;
    private SpatialMaterial VoxMaterial;
    private SpatialMaterial WaterMaterial;

    private Vector3 ChunkSize = new Vector3(16, 128, 16);
    private int RenderDistance = 4;
    private float VoxelSize = 1f;

    // Chunks
    private Dictionary VoxInChunks = new Dictionary();
    private List<Node> LoadedChunks = new List<Node>();

    // Water
    private MeshInstance Water;
    private PlaneMesh WaterMesh;

    private float WaterOffset = 0.5f;
    private int WaterLevel = 60;

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
        Camera = GetNode("../Camera") as Camera;
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;
        WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        Water = new MeshInstance();
        WaterMesh = new PlaneMesh();
        WaterMesh.Size = new Vector2((VoxelSize * ChunkSize.x ) * RenderDistance * 4 , (VoxelSize * ChunkSize.z) * RenderDistance * 4) ;
        WaterMesh.SetMaterial(WaterMaterial);
        Water.SetMesh(WaterMesh);
        Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        AddChild(Water);

        for (int i = -RenderDistance; i < RenderDistance; i++)
            for (int j = -RenderDistance; j < RenderDistance; j++)
                LoadChunk(new Vector2(i, j));
    }

    private void GenerateSeed()
    {
        if(CurrentSeed != 0)
            return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;
        
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 5;
        Noise.Period = 500;
        Noise.Persistence = 0.8f;
    }

    /// <summary>
    /// Preload Chunks data inside an array without rendering it yet.
    /// This is useful to warm up the chunk before rendering it.
    /// </summary>
    /// <param name="cPosition">Chunk position.</param>
    private void PreloadChunk(Vector2 cPosition)
    {
        for (int z = 0; z < ChunkSize.z; z++)
            for (int x = 0; x < ChunkSize.x; x++)
            {
                var height = Mathf.Stepify((Noise.GetNoise2d(cPosition.x + x, cPosition.y + z) + 1) * ChunkSize.y / 2, 1);
                VoxInChunks.Add( new Vector3(x, height, z).ToString(), new Vector3(x, height, z));

                for (int y = 0; y < ChunkSize.y; y++)
                {
                    var pos = new Vector3(x + cPosition.x, y, z + cPosition.y);
                    if (Noise.GetNoise3dv(pos) - ((y / ChunkSize.y) / 2.5f) >= 0)
                    {
                        height += 1;
                        VoxInChunks.Add(new Vector3(x, height, z).ToString(), new Vector3(x, height, z));
                    }    
                }
            }
    }

    /// <summary>
    /// Renders a chunk.
    /// </summary>
    /// <param name="cPosition">Global Position.</param>
    private void LoadChunk(Vector2 pPosition)
    {
        
        var cPosition = new Vector2(Mathf.Floor(pPosition.x) * ChunkSize.x, Mathf.Floor(pPosition.y) * ChunkSize.z);
        SurfaceTool = new SurfaceTool();
        SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        
        PreloadChunk(cPosition);

        foreach (Vector3 voxPos in VoxInChunks.Values)
            CreateVoxel(SurfaceTool, voxPos, cPosition);

        var chunk = new MeshInstance();
        chunk.Mesh = SurfaceTool.Commit();
        chunk.Name = cPosition.ToString();
        //chunk.CreateTrimeshCollision(); // Collisions generation.
        chunk.AddToGroup("Chunk");
        AddChild(chunk);

        LoadedChunks.Add(chunk); // Keeping track of generated chunks.

        SurfaceTool.Clear();

        VoxInChunks.Clear();
        GD.Print("Chunk Done: " + pPosition.ToString());
    }


    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private void CreateVoxel(SurfaceTool pSurfaceTool, Vector3 pPosition, Vector2 cPosition)
    {
        // Global position of the voxel.
        var VoxelPos = new Vector3(pPosition.x + cPosition.x, pPosition.y, pPosition.z + cPosition.y);
        

        if (CanCreateFace(pPosition.x, pPosition.y + 1, pPosition.z)) // Above
        {

            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[4] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[6] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            //SurfaceTool.AddColor(new Color("7CFC00"));
        }
        if (CanCreateFace(pPosition.x + 1, pPosition.y , pPosition.z)) // Right
        {
            pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[2] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[1] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[2] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[6] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos); 
        }
        if (CanCreateFace(pPosition.x - 1, pPosition.y , pPosition.z)) // Left
        {
            pSurfaceTool.AddNormal(new Vector3(-1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[0] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[3] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[0] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[4] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos); 

        }
        if (CanCreateFace(pPosition.x, pPosition.y, pPosition.z + 1)) // Front
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            pSurfaceTool.AddVertex(Vertices[3] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[6] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[2] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[3] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[6] + VoxelPos); 
        }
        if (CanCreateFace(pPosition.x, pPosition.y, pPosition.z - 1)) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, -1));
            pSurfaceTool.AddVertex(Vertices[0] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[1] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[4] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[0] + VoxelPos);
        }
    }

    // Check if there is a voxel at X Y Z position in the current preloaded chunk.
    private bool CanCreateFace(float x, float y, float z)
    {
        return !( VoxInChunks.ContainsKey( new Vector3(x, y, z).ToString() )  );//|| 
            //( (x % (ChunkSize.x ) == 0 || z % (ChunkSize.z) == 0 || x <= 1 || z <= 1 ) && VoxInChunks.ContainsKey(new Vector3(x, y + 1, z).ToString()) );
    }
}
