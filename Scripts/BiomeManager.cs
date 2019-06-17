
public enum Biomes
{
    Tundra,
    BorealForest,
    Woodlands,
    ColdDesert,
    Desert,
    Savanna,
    RainForest,
    TropicRainForest,
    Forest,
}

/*

    100+                                     +-----------+
          |                                  |           |
          |                                  |           |
          |                                  |           |
          |                                  |           |
          |                                  |           |
    H 75  |                        +---------------------+
    U     |                        |         |           |
    M     |                        |         |           |
    I     |                        |         |           |
    D 50  |             +--------------------+           |
    I     |             |          |         |           |
    t     |             |          |         |           |
    Y     |             |          |         |           |
      25  +------------------------+---------------------+
          |             |                    |           |
          |             |                    |           |
          |             +--------------------+           |
          |             |                    |           |
          |             |                    |           |
       0  +----------------------------------------------^
         -10            0          10        20         30
                           Temperature(celcius)

 */

public static class BiomeManager
{
    public static Biomes GetBiome(float temperature, float humidity)
    {
        bool isTundra           = temperature >= -10  && temperature <  0 && humidity >=  0 && humidity < 25;
        bool isBorealForest     = temperature >=   0  && temperature < 10 && humidity >= 25 && humidity < 50;
        bool isWoodlands        = temperature >=   0  && temperature < 20 && humidity >= 15 && humidity < 25;
        bool isColdDesert       = temperature >=   0  && temperature < 20 && humidity >=  0 && humidity < 15;
        bool isDesert           = temperature >= -10  && temperature <  0 && humidity >=  0 && humidity < 25;
        bool isSavanna          = temperature >=  20  && temperature < 30 && humidity >= 25 && humidity < 75;
        bool isRainForest       = temperature >=  10  && temperature < 20 && humidity >= 50 && humidity < 75;
        bool isTropicRainForest = temperature >=  20  && temperature < 30 && humidity >= 75 && humidity < 100;
        bool isForest           = temperature >=  10  && temperature < 20 && humidity >= 25 && humidity < 50;

        if(isTundra)
            return Biomes.Tundra;
        else if(isBorealForest)
            return Biomes.BorealForest;
        else if(isWoodlands)
            return Biomes.Woodlands;
        else if(isColdDesert)
            return Biomes.ColdDesert;
        else if(isDesert)
            return Biomes.Desert;
        else if(isSavanna)
            return Biomes.Savanna;
        else if(isRainForest)
            return Biomes.RainForest;
        else if(isTropicRainForest)
            return Biomes.TropicRainForest;
        else 
            return Biomes.Forest; // Default 
    }
}