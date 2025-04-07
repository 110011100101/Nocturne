using Godot;
using Godot.Collections;
using Nocturne.Core.Class;

public class GenerateTemperatureMap : Unit<Array<Image>, Array<Image>>
{
    public override Array<Image> Execute(Array<Image> informationMaps)
    {
        Image heightMap = informationMaps[0];
        Image humidityMap = informationMaps[1];
        Image temperatureMap = Image.CreateEmpty(heightMap.GetWidth(), heightMap.GetWidth(), false, Image.Format.Rgba8);

        FastNoiseLite noise = new FastNoiseLite()
        {
            Seed = (int)GD.Randi(),                             // 随机种子
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Frequency = 0.03f,                                  // 控制温度分布的范围
            FractalOctaves = 3,                                 // 增加分形层数
            FractalLacunarity = 2.0f,                           // 增强层间差异
            FractalGain = 0.5f                                  // 控制振幅衰减
        };

        for (int x = 0; x < temperatureMap.GetWidth(); x++)
        for (int y = 0; y < temperatureMap.GetWidth(); y++)
        {
            float noiseValue = noise.GetNoise2D(x, y);

            int height = (int)heightMap.GetPixelv(new Vector2I(x, y)).R;
            float humidity = humidityMap.GetPixelv(new Vector2I(x, y)).R;



            // 调整温度计算逻辑
            float temperature = Mathf.Clamp((-20 + y - (height * 5) - humidity * noiseValue) * 0.5f, -40, 60);

            temperatureMap.SetPixelv(new Vector2I(x, y), new Color(temperature, temperature, temperature));
        }

        informationMaps.Add(temperatureMap);

        return informationMaps;
    }
}