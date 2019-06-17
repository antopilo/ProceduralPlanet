using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProceduralPlanet.Scripts.Biomes;


public class BiomeSettings
{
    public float TargetTemperature { get; set; }
    public float TargetHumidity { get; set; }

    public BlockType DefaultBlocktype { get; set; }
    public BlockType UnderLayerType { get; set; } 
    public float TerrainAmplitude { get; set; }
    public BlockType TopLayerType { get; set; }
    public int TopLayerThickness { get; set; } 
    public bool Mountains { get; set; } = true;

    // Vegetation
    public string TreeModel { get; set; }
    public float TreeRate { get; set; }

    public string DecorationModel { get; set; } 
    public int DecorationRate { get; set; }

    public BiomeSettings()
    {
        this.TargetTemperature = 0f;
        this.TargetHumidity = 0f;
        this.DefaultBlocktype = BlockType.rock;
        this.TopLayerType = BlockType.rock;
        this.UnderLayerType = BlockType.rock;
        this.Mountains = false;
        this.TerrainAmplitude = 1f;
        this.TopLayerThickness = 1;
        this.TreeModel = "";
        this.TreeRate = 0;
        this.DecorationModel = "";
        this.DecorationRate = 0;
    }

    public BiomeSettings(Biomes biome)
    {
        switch (biome)
        {
            case Biomes.Tundra:

                this.TargetTemperature = Tundra.TargetTemperature;
                this.TargetHumidity = Tundra.TargetHumidity;
                this.DefaultBlocktype = Tundra.DefaultBlocktype;
                this.TopLayerType = Tundra.TopLayerType;
                this.UnderLayerType = Tundra.UnderLayerType;
                this.Mountains = Tundra.Mountains;
                this.TerrainAmplitude = Tundra.TerrainAmplitude;
                this.TopLayerThickness = Tundra.TopLayerThickness;
                this.TreeModel = Tundra.TreeModel;
                this.TreeRate = Tundra.TreeRate;
                this.DecorationModel = Tundra.DecorationModel;
                this.DecorationRate = Tundra.DecorationRate;
                break;
            case Biomes.BorealForest:
                this.TargetTemperature = BorealForest.TargetTemperature;
                this.TargetHumidity = BorealForest.TargetHumidity;
                this.DefaultBlocktype = BorealForest.DefaultBlocktype;
                this.TopLayerType = BorealForest.TopLayerType;
                this.UnderLayerType = BorealForest.UnderLayerType;
                this.Mountains = BorealForest.Mountains;
                this.TerrainAmplitude = BorealForest.TerrainAmplitude;
                this.TopLayerThickness = BorealForest.TopLayerThickness;
                this.TreeModel = BorealForest.TreeModel;
                this.TreeRate = BorealForest.TreeRate;
                this.DecorationModel = BorealForest.DecorationModel;
                this.DecorationRate = BorealForest.DecorationRate;
                break;
            case Biomes.Woodlands:
                this.TargetTemperature = Woodlands.TargetTemperature;
                this.TargetHumidity = Woodlands.TargetHumidity;
                this.DefaultBlocktype = Woodlands.DefaultBlocktype;
                this.TopLayerType = Woodlands.TopLayerType;
                this.UnderLayerType = Woodlands.UnderLayerType;
                this.Mountains = Woodlands.Mountains;
                this.TerrainAmplitude = Woodlands.TerrainAmplitude;
                this.TopLayerThickness = Woodlands.TopLayerThickness;
                this.TreeModel = Woodlands.TreeModel;
                this.TreeRate = Woodlands.TreeRate;
                this.DecorationModel = Woodlands.DecorationModel;
                this.DecorationRate = Woodlands.DecorationRate;
                break;
            case Biomes.ColdDesert:
                this.TargetTemperature = ColdDesert.TargetTemperature;
                this.TargetHumidity = ColdDesert.TargetHumidity;
                this.DefaultBlocktype = ColdDesert.DefaultBlocktype;
                this.TopLayerType = ColdDesert.TopLayerType;
                this.UnderLayerType = ColdDesert.UnderLayerType;
                this.Mountains = ColdDesert.Mountains;
                this.TerrainAmplitude = ColdDesert.TerrainAmplitude;
                this.TopLayerThickness = ColdDesert.TopLayerThickness;
                this.TreeModel = ColdDesert.TreeModel;
                this.TreeRate = ColdDesert.TreeRate;
                this.DecorationModel = ColdDesert.DecorationModel;
                this.DecorationRate = ColdDesert.DecorationRate;
                break;
            case Biomes.Desert:
                this.TargetTemperature = Desert.TargetTemperature;
                this.TargetHumidity = Desert.TargetHumidity;
                this.DefaultBlocktype = Desert.DefaultBlocktype;
                this.TopLayerType = Desert.TopLayerType;
                this.UnderLayerType = Desert.UnderLayerType;
                this.Mountains = Desert.Mountains;
                this.TerrainAmplitude = Desert.TerrainAmplitude;
                this.TopLayerThickness = Desert.TopLayerThickness;
                this.TreeModel = Desert.TreeModel;
                this.TreeRate = Desert.TreeRate;
                this.DecorationModel = Desert.DecorationModel;
                this.DecorationRate = Desert.DecorationRate;
                break;
            case Biomes.Savanna:
                this.TargetTemperature = Savanna.TargetTemperature;
                this.TargetHumidity = Savanna.TargetHumidity;
                this.DefaultBlocktype = Savanna.DefaultBlocktype;
                this.TopLayerType = Savanna.TopLayerType;
                this.UnderLayerType = Savanna.UnderLayerType;
                this.Mountains = Savanna.Mountains;
                this.TerrainAmplitude = Savanna.TerrainAmplitude;
                this.TopLayerThickness = Savanna.TopLayerThickness;
                this.TreeModel = Savanna.TreeModel;
                this.TreeRate = Savanna.TreeRate;
                this.DecorationModel = Savanna.DecorationModel;
                this.DecorationRate = Savanna.DecorationRate;
                break;
            case Biomes.RainForest:
                this.TargetTemperature = RainForest.TargetTemperature;
                this.TargetHumidity = RainForest.TargetHumidity;
                this.DefaultBlocktype = RainForest.DefaultBlocktype;
                this.TopLayerType = RainForest.TopLayerType;
                this.UnderLayerType = RainForest.UnderLayerType;
                this.Mountains = RainForest.Mountains;
                this.TerrainAmplitude = RainForest.TerrainAmplitude;
                this.TopLayerThickness = RainForest.TopLayerThickness;
                this.TreeModel = RainForest.TreeModel;
                this.TreeRate = RainForest.TreeRate;
                this.DecorationModel = RainForest.DecorationModel;
                this.DecorationRate = RainForest.DecorationRate;
                break;
            case Biomes.TropicRainForest:
                this.TargetTemperature = BorealForest.TargetTemperature;
                this.TargetHumidity = BorealForest.TargetHumidity;
                this.DefaultBlocktype = TropicRainForest.DefaultBlocktype;
                this.TopLayerType = TropicRainForest.TopLayerType;
                this.UnderLayerType = TropicRainForest.UnderLayerType;
                this.Mountains = TropicRainForest.Mountains;
                this.TerrainAmplitude = TropicRainForest.TerrainAmplitude;
                this.TopLayerThickness = TropicRainForest.TopLayerThickness;
                this.TreeModel = TropicRainForest.TreeModel;
                this.TreeRate = TropicRainForest.TreeRate;
                this.DecorationModel = TropicRainForest.DecorationModel;
                this.DecorationRate = TropicRainForest.DecorationRate;
                break;
            case Biomes.Forest:
                this.TargetTemperature = Forest.TargetTemperature;
                this.TargetHumidity = Forest.TargetHumidity;
                this.DefaultBlocktype = Forest.DefaultBlocktype;
                this.TopLayerType = Forest.TopLayerType;
                this.UnderLayerType = Forest.UnderLayerType;
                this.Mountains = Forest.Mountains;
                this.TerrainAmplitude = Forest.TerrainAmplitude;
                this.TopLayerThickness = Forest.TopLayerThickness;
                this.TreeModel = Forest.TreeModel;
                this.TreeRate = Forest.TreeRate;
                this.DecorationModel = Forest.DecorationModel;
                this.DecorationRate = Forest.DecorationRate;
                break;
        }
    }

    
}

