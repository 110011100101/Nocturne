using Godot;
using Godot.Collections;
using Nocturne.Core.Class;

public class GenerateHumidityMapUnit : Unit<Array<Image>, Array<Image>>
{
    public override Array<Image> Execute(Array<Image> informationMaps)
    {
        Image heightMap = informationMaps[0];
        Image humidityMap = Image.CreateEmpty(heightMap.GetWidth(), heightMap.GetWidth(), false, Image.Format.Rgba8);

        FastNoiseLite noise = new FastNoiseLite()
        {
            Seed = (int)GD.Randi(),                             // 随机种子
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Frequency = 0.02f,                                  // 控制湿度分布的范围
            FractalOctaves = 3,                                 // 增加分形层数
            FractalLacunarity = 2.0f,                           // 增强层间差异
            FractalGain = 0.5f                                  // 控制振幅衰减
        };

        for (int x = 0; x < humidityMap.GetWidth(); x++)
        for (int y = 0; y < humidityMap.GetWidth(); y++)
        {
            float noiseValue = noise.GetNoise2D(x, y);

            int height = (int)heightMap.GetPixelv(new Vector2I(x, y)).R;

            float humidityValue = Mathf.Clamp(((noiseValue + 1) * 100) + height * noiseValue * 10, 0, 100);
            
            humidityMap.SetPixelv(new Vector2I(x, y), new Color(humidityValue, humidityValue, humidityValue));
        }

        informationMaps.Add(humidityMap);

        return informationMaps;
    }
}