using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class BorealForest : Biome
    {
        public static float TargetTemperature { get; } = 2f;
        public static float TargetHumidity { get; } = 2f;

        // Terrain
        public static BlockType DefaultBlocktype = BlockType.grass;
        public static BlockType UnderLayerType { get; set; } = BlockType.dirt;
        public static float TerrainAmplitude { get; set; } = 0.5f;
        public static BlockType TopLayerType { get; set; } = BlockType.grass;
        public static int TopLayerThickness { get; set; } = 2;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = "res://models/trees/pine1.tres";
        public static int TreeRate { get; set; } = 1;
        public static string DecorationModel { get; set; } = "res://models/decorations/grass.tres";
        public static int DecorationRate { get; set; } = 10;
    }
}
