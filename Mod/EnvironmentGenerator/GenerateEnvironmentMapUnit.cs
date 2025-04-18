using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Nocturne.Core.Class;

public class GenerateEnvironmentMapUnit : Unit<Image, Dictionary<Vector2I, EnumMaterial>>
{
    public override Dictionary<Vector2I, EnumMaterial> Execute(Image informationMaps)
    {
        int range = informationMaps.GetWidth();
        Dictionary<Vector2I, EnumMaterial> enviromentDic = new Dictionary<Vector2I, EnumMaterial>();

        Parallel.For(0, range, x => {
        Parallel.For(0, range, y =>
        {
            Vector2I pixel = new Vector2I(x, y);
            EnumMaterial material = EnumMaterial.Air; // Default material
            float height = informationMaps.GetPixelv(pixel).R * 10f;
            float humidity = informationMaps.GetPixelv(pixel).G * 100f;
            float temperature = informationMaps.GetPixelv(pixel).B * 1000f - 140f;

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

            enviromentDic.Add(pixel, material);
        });});

        return enviromentDic;
    }
}