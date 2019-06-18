using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class Tundra
    {
        public static float TargetTemperature { get; } = -5f;
        public static float TargetHumidity { get; } = 25f;

        // Terrain
        public static BlockType DefaultBlocktype = BlockType.rock;
        public static BlockType UnderLayerType { get; set; } = BlockType.snow;
        public static float TerrainAmplitude { get; set; } = 1.2f;
        public static BlockType TopLayerType { get; set; } = BlockType.snow;
        public static int TopLayerThickness { get; set; } = 2;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = "res://models/trees/pine_snow1.tres";
        public static float TreeRate { get; set; } = 1.06f;
        public static string DecorationModel { get; set; } = "res://models/decorations/puddles1.tres";
        public static float DecorationRate { get; set; } = 0;
    }
}
