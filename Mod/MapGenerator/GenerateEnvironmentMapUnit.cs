using Godot;
using Godot.Collections;
using Nocturne.Core.Class;

public class GenerateEnvironmentMapUnit : Unit<Array<Image>, Array<Image>>
{
    public override Array<Image> Execute(Array<Image> informationMaps)
    {
        Image heightMap = informationMaps[0];
        Image humidityMap = informationMaps[1];
        Image temperatureMap = informationMaps[2];
        Image environmentMap = Image.CreateEmpty(heightMap.GetWidth(), heightMap.GetWidth(), false, Image.Format.Rgba8);

        for (int x = 0; x < environmentMap.GetWidth(); x++)
        for (int y = 0; y < environmentMap.GetWidth(); y++)
        {
            int height = (int)heightMap.GetPixelv(new Vector2I(x, y)).R;
            float humidity = humidityMap.GetPixelv(new Vector2I(x, y)).R;
            float temperature = temperatureMap.GetPixelv(new Vector2I(x, y)).R;

            EnumMaterial material = EnumMaterial.Air; // Default material

            // 这个逻辑是基于EnumMaterial的, 除了C#约定你必须在新增材质的时候先引入枚举, 其他方面约等于完全独立
            // 你可以自由搭配前置的材质包mod(这个材质包是真真正正的材质包,不是纹理材质)
            if (humidity <= 10)
            {
                material = EnumMaterial.Stone;
            }
            else if (humidity <= 30)
            {
                material = temperature < -10 ? EnumMaterial.Stone : EnumMaterial.Soil;
            }
            else if (humidity <= 80)
            {
                material = temperature < -10 ? EnumMaterial.Snow : temperature < 20 ? EnumMaterial.Soil : EnumMaterial.Grass;
            }
            else if (humidity <= 100)
            {
                switch (height)
                {
                    case 0:
                        material = temperature < 0 ? EnumMaterial.Ice : EnumMaterial.Water;
                        break;
                    case 1:
                        material = temperature < 0 ? EnumMaterial.Ice : EnumMaterial.Water;
                        break;
                    case 2:
                        material = temperature < 0 ? EnumMaterial.Stone : EnumMaterial.Grass;
                        break;
                    case 3:
                        material = temperature < 0 ? EnumMaterial.Snow : EnumMaterial.Soil;
                        break;
                    case 4:
                        material = temperature < 0 ? EnumMaterial.Snow : EnumMaterial.Stone;
                        break;
                }
            }

            environmentMap.SetPixelv(new Vector2I(x, y), new Color((int)material, (int)material, (int)material));
        }

        informationMaps.Add(environmentMap);

        return informationMaps;
    }
}