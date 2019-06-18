using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class ColdDesert 
    {
        public static float TargetTemperature { get; } = 10f;
        public static float TargetHumidity { get; } = 12.5f;

        // Terrain
        public static BlockType DefaultBlocktype = BlockType.rock;
        public static BlockType UnderLayerType { get; set; } = BlockType.dirt;
        public static float TerrainAmplitude { get; set; } = 0.25f;
        public static BlockType TopLayerType { get; set; } = BlockType.grass;
        public static int TopLayerThickness { get; set; } = 2;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = null;
        public static float TreeRate { get; set; } = 0;
        public static string DecorationModel { get; set; } = "res://models/decorations/grass.tres";
        public static float DecorationRate { get; set; } = 50;
    }
}
