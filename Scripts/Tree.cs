using Godot;
using System.Collections.Generic;

public class Tree
{
    public int Height, Width; // Height and witdh of th trunk
    public Dictionary<Vector3, Voxel> Voxels = new Dictionary<Vector3, Voxel>(); // Voxels
    private RandomNumberGenerator rng; // Generator.

    public Tree(RandomNumberGenerator prng)
    {
        rng = prng;
        // Randomizing the seed.
        rng.Randomize();
        
        // Getting the height of the trunk.
        Height = rng.RandiRange(8, 16);
        Width = rng.RandiRange(0, 1);

        // Start the procedure
        // MakeTrunk();
    }
    
    /// <summary>
    /// Place the trunk, then leaves.
    /// </summary>
    //private void MakeTrunk()
    //{
    //    // Loop up the trunk.
    //    for (int y = 0; y < Height; y++)
    //    {
    //        if(Width == 0) // if the random width of the trunk is 0, assume it's 1.
    //        {
    //            Voxel voxel = new Voxel()
    //            {
    //                Position = new Vector3(0, y, 0),
    //                type = BlockType.wood
    //            };

    //            // Add to the dictionary.
    //            Voxels.Add(voxel.Position, voxel);
    //        }
    //        else // if the trunk is bigger than 1.
    //        {
    //            for (int x = -Width; x <= Width; x++) // x
    //                for (int z = -Width; z <= Width; z++)
    //                {
    //                    // Make voxel.
    //                    Voxel voxel = new Voxel()
    //                    {
    //                        Position = new Vector3(x, y, z),
    //                        type = BlockType.wood
    //                    };

    //                    // Add to the dictionary.
    //                    Voxels.Add(voxel.Position, voxel);
    //                }
    //        }
    //    }

    //    // Make a random number of leaves spheres. from 0 to 4.
    //    for (int i = 0; i < rng.RandiRange(0,4); i++)
    //    {
    //        int x = rng.RandiRange(1, 5);
    //        int y = Height - rng.RandiRange(1, 5); // Offset from the top of the trunk.
    //        int z = rng.RandiRange(1, 5);

    //        PlaceLeaves(new Vector3(x, y, z));
    //    }
       
    //    // Always place some leaves to the top of the trunk.
    //    // We don't want "stick" trees.
    //    PlaceLeaves(new Vector3(0,Height,0));
    //}

    ///// <summary>
    /////  Make a sphere at the top of the trunk.
    ///// </summary>
    //public void PlaceLeaves(Vector3 pOffset)
    //{
    //    // Random Circle width.
    //    int leaveWidth = rng.RandiRange(4, 6);

    //    // Iterating through a box of the size of the leaveWidth.
    //    // Checking if the distance from i,k,j to the center only if
    //    // it's lower than the f the cube. Resulting in a perfect sphere.
    //    for (int i = -leaveWidth; i < leaveWidth; i++)
    //        for (int k = -leaveWidth; k < leaveWidth; k++)
    //            for (int j = -leaveWidth / 2; j < leaveWidth / 2; j++)
    //            {
    //                // If the position is outside the range of the circle skip.
    //                if ( !(( new Vector3(i, j * 2, k) - new Vector3(0, 0, 0) ).Length() < leaveWidth) )
    //                    continue;

    //                // Placing the voxel.
    //                Vector3 voxPosition = new Vector3(i + pOffset.x, j + pOffset.y, k + pOffset.z);
    //                if (!Voxels.ContainsKey(voxPosition)) // Checking if there is already a block there.
    //                {
    //                    // new Voxel.
    //                    Voxel newVoxel = new Voxel()
    //                    {
    //                        Position = voxPosition,
    //                        type = BlockType.leaves
    //                    };
    //                    Voxels.Add(voxPosition, newVoxel); // Placing it.
    //                }
    //            }
    //}
}

