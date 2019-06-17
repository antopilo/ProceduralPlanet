


/*

     100%   +-------------------------------------------+
            |          |          |         |           |
            |          |          |         |           |
            |          |          |         |           |
    H  75%  |          |          +---------+           |
    U       |          |          |         |           |
    M       |          |          |         +-----------+
    I       |          |          |         |           |
    D  50%  |          +--------------------+           |
    I       |          |                    |           |
    T       |          |                    +-----------+
    Y       |          |                    |           |
   (%) 25%  +----------+--------------------+           |
            |          |                    |           |
            |          |                    |           |
            |          |                    |           |
        0%  +-------------------------------------------+
           -10c        0c         10c       20c         30c
                           Temperature(celcius)

 */

using ProceduralPlanet.Scripts.Biomes;

public static class BiomeManager
{
    public static Biomes GetBiome(float temperature, float humidity)
    {
        bool isTundra = temperature >= -10 && temperature < 0;
        bool isBorealForest = temperature >= 0 && temperature < 10 && humidity >= 50 && humidity < 100;
        bool isWoodlands = temperature >= 0 && temperature < 20 && humidity >= 25 && humidity < 50;
        bool isColdDesert = temperature >= 0 && temperature < 20 && humidity >= 0 && humidity < 25;
        bool isDesert = temperature >= -10 && temperature < 0 && humidity >= 0 && humidity < 25;
        bool isSavanna = temperature >= 20 && temperature < 30 && humidity >= 25 && humidity < 75;
        bool isRainForest = temperature >= 10 && temperature < 20 && humidity >= 75 && humidity < 100;
        bool isTropicRainForest = temperature >= 20 && temperature < 30 && humidity >= 75 && humidity < 100;
        bool isForest = temperature >= 10 && temperature < 20 && humidity >= 50 && humidity < 75;

        if (isTundra)
            return Biomes.Tundra;
        if (isBorealForest)
            return Biomes.BorealForest;
        else if (isWoodlands)
            return Biomes.Woodlands;
        else if (isColdDesert)
            return Biomes.ColdDesert;
        else if (isDesert)
            return Biomes.Desert;
        else if (isSavanna)
            return Biomes.Savanna;
        else if (isRainForest)
            return Biomes.RainForest;
        else if (isTropicRainForest)
            return Biomes.TropicRainForest;
        else
            return Biomes.Forest; // Default 
    }

    public static void SetBiomeSettings(Biomes biome)
    {
        switch (biome)
        {
            case Biomes.Tundra:
                BiomeSettings.DefaultBlocktype = Tundra.DefaultBlocktype;
                BiomeSettings.TopLayerType = Tundra.TopLayerType;
                BiomeSettings.UnderLayerType = Tundra.UnderLayerType;
                BiomeSettings.Mountains = Tundra.Mountains;
                BiomeSettings.TerrainAmplitude = Tundra.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = Tundra.TopLayerThickness;
                BiomeSettings.TreeModel = Tundra.TreeModel;
                BiomeSettings.TreeRate = Tundra.TreeRate;
                BiomeSettings.DecorationModel = Tundra.DecorationModel;
                BiomeSettings.DecorationRate = Tundra.DecorationRate;
                break;
            case Biomes.BorealForest:
                BiomeSettings.DefaultBlocktype = BorealForest.DefaultBlocktype;
                BiomeSettings.TopLayerType = BorealForest.TopLayerType;
                BiomeSettings.UnderLayerType = BorealForest.UnderLayerType;
                BiomeSettings.Mountains = BorealForest.Mountains;
                BiomeSettings.TerrainAmplitude = BorealForest.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = BorealForest.TopLayerThickness;
                BiomeSettings.TreeModel = BorealForest.TreeModel;
                BiomeSettings.TreeRate = BorealForest.TreeRate;
                BiomeSettings.DecorationModel = BorealForest.DecorationModel;
                BiomeSettings.DecorationRate = BorealForest.DecorationRate;
                break;
            case Biomes.Woodlands:
                BiomeSettings.DefaultBlocktype = Woodlands.DefaultBlocktype;
                BiomeSettings.TopLayerType = Woodlands.TopLayerType;
                BiomeSettings.UnderLayerType = Woodlands.UnderLayerType;
                BiomeSettings.Mountains = Woodlands.Mountains;
                BiomeSettings.TerrainAmplitude = Woodlands.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = Woodlands.TopLayerThickness;
                BiomeSettings.TreeModel = Woodlands.TreeModel;
                BiomeSettings.TreeRate = Woodlands.TreeRate;
                BiomeSettings.DecorationModel = Woodlands.DecorationModel;
                BiomeSettings.DecorationRate = Woodlands.DecorationRate;
                break;
            case Biomes.ColdDesert:
                BiomeSettings.DefaultBlocktype = ColdDesert.DefaultBlocktype;
                BiomeSettings.TopLayerType = ColdDesert.TopLayerType;
                BiomeSettings.UnderLayerType = ColdDesert.UnderLayerType;
                BiomeSettings.Mountains = ColdDesert.Mountains;
                BiomeSettings.TerrainAmplitude = ColdDesert.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = ColdDesert.TopLayerThickness;
                BiomeSettings.TreeModel = ColdDesert.TreeModel;
                BiomeSettings.TreeRate = ColdDesert.TreeRate;
                BiomeSettings.DecorationModel = ColdDesert.DecorationModel;
                BiomeSettings.DecorationRate = ColdDesert.DecorationRate;
                break;
            case Biomes.Desert:
                BiomeSettings.DefaultBlocktype = Desert.DefaultBlocktype;
                BiomeSettings.TopLayerType = Desert.TopLayerType;
                BiomeSettings.UnderLayerType = Desert.UnderLayerType;
                BiomeSettings.Mountains = Desert.Mountains;
                BiomeSettings.TerrainAmplitude = Desert.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = Desert.TopLayerThickness;
                BiomeSettings.TreeModel = Desert.TreeModel;
                BiomeSettings.TreeRate = Desert.TreeRate;
                BiomeSettings.DecorationModel = Desert.DecorationModel;
                BiomeSettings.DecorationRate = Desert.DecorationRate;
                break;
            case Biomes.Savanna:
                BiomeSettings.DefaultBlocktype = Savanna.DefaultBlocktype;
                BiomeSettings.TopLayerType = Savanna.TopLayerType;
                BiomeSettings.UnderLayerType = Savanna.UnderLayerType;
                BiomeSettings.Mountains = Savanna.Mountains;
                BiomeSettings.TerrainAmplitude = Savanna.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = Savanna.TopLayerThickness;
                BiomeSettings.TreeModel = Savanna.TreeModel;
                BiomeSettings.TreeRate = Savanna.TreeRate;
                BiomeSettings.DecorationModel = Savanna.DecorationModel;
                BiomeSettings.DecorationRate = Savanna.DecorationRate;
                break;
            case Biomes.RainForest:
                BiomeSettings.DefaultBlocktype = RainForest.DefaultBlocktype;
                BiomeSettings.TopLayerType = RainForest.TopLayerType;
                BiomeSettings.UnderLayerType = RainForest.UnderLayerType;
                BiomeSettings.Mountains = RainForest.Mountains;
                BiomeSettings.TerrainAmplitude = RainForest.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = RainForest.TopLayerThickness;
                BiomeSettings.TreeModel = RainForest.TreeModel;
                BiomeSettings.TreeRate = RainForest.TreeRate;
                BiomeSettings.DecorationModel = RainForest.DecorationModel;
                BiomeSettings.DecorationRate = RainForest.DecorationRate;
                break;
            case Biomes.TropicRainForest:
                BiomeSettings.DefaultBlocktype = TropicRainForest.DefaultBlocktype;
                BiomeSettings.TopLayerType = TropicRainForest.TopLayerType;
                BiomeSettings.UnderLayerType = TropicRainForest.UnderLayerType;
                BiomeSettings.Mountains = TropicRainForest.Mountains;
                BiomeSettings.TerrainAmplitude = TropicRainForest.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = TropicRainForest.TopLayerThickness;
                BiomeSettings.TreeModel = TropicRainForest.TreeModel;
                BiomeSettings.TreeRate = TropicRainForest.TreeRate;
                BiomeSettings.DecorationModel = TropicRainForest.DecorationModel;
                BiomeSettings.DecorationRate = TropicRainForest.DecorationRate;
                break;
            case Biomes.Forest:
                BiomeSettings.DefaultBlocktype = Forest.DefaultBlocktype;
                BiomeSettings.TopLayerType = Forest.TopLayerType;
                BiomeSettings.UnderLayerType = Forest.UnderLayerType;
                BiomeSettings.Mountains = Forest.Mountains;
                BiomeSettings.TerrainAmplitude = Forest.TerrainAmplitude;
                BiomeSettings.TopLayerThickness = Forest.TopLayerThickness;
                BiomeSettings.TreeModel = Forest.TreeModel;
                BiomeSettings.TreeRate = Forest.TreeRate;
                BiomeSettings.DecorationModel = Forest.DecorationModel;
                BiomeSettings.DecorationRate = Forest.DecorationRate;
                break;
        }
    }

    
}