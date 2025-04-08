// using Godot;
// using Godot.Collections;
// using System;
// using Nocturne.Core.Class;

// /// <summary>
// /// 接收一个图组,返回
// /// </summary>
// public class RefineMapUnit : Unit<Array<Image>, Image>
// {
//     public override Image Execute(Array<Image> informationMaps)
//     {
//         if (informationMaps.Count != 4) throw new Exception("RefineMapUnit: informationMaps.Count != 4");

//         Image heightMap;
//         Image humidityMap;
//         Image temperatureMap;
//         Image environmentMap;

//         foreach(Image map in informationMaps)
//         {
//             switch(map.ResourceName)
//             {
//                 case "heightMap":
//                     heightMap = map;
//                     break;
//                 case "humidityMap":
//                     humidityMap = map;
//                     break;
//                 case "temperatureMap":
//                     temperatureMap = map;
//                     break;
//                 case "environmentMap":
//                     environmentMap = map;
//                     break;
//                 default:
//                     throw new Exception("RefineMapUnit: map not found");
//             }
//         }

//         // 这里引入精细化放大的逻辑,等我拉完屎回来写, 我知道你在看, 别畸吧动我代码
//         int detailedPlateSize = refinedEnvironmentMap.GetWidth() * 10;
//         int detailedLevelRange = detailedPlateSize / 5;
//         Image detailedHeightMap = Image.CreateEmpty(detailedPlateSize, detailedPlateSize, false, Image.Format.Rgba8);
//         Image refinedEnvironmentMap = Image.CreateEmpty(detailedPlateSize, detailedPlateSize, false, Image.Format.Rgba8);
//         Image baseHeightMap = Image.CreateEmpty(detailedPlateSize, detailedPlateSize, false, Image.Format.Rgba8);

//         FastNoiseLite noise = new FastNoiseLite()
//         {
//             Seed = (int)GD.Randi(),                             // 随机种子
//             NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
//             Frequency = 0.0495f,                                // 这个值越小地图越大块
//             FractalOctaves = 3,                                 // 增加分形层数
//             FractalLacunarity = 2.7f,                           // 增强层间差异(值越大层间边缘越破碎)
//             FractalGain = 0.27f,                                // 控制振幅衰减(值越大地图越稀碎)
//         };

//         // 生成高颗粒度高度图
//         for (int x = 0; x < detailedPlateSize; x++)
//         for (int y = 0; y < detailedPlateSize; y++)
//         {
//             float noiseValue = noise.GetNoise2D(x, y);
//             int height = (int)((noiseValue + 1.0f) * detailedLevelRange);       // [-1,1] => [0.00, detailedLevelRange]

//             detailedHeightMap.SetValue(new Vector2I(x, y), height);
//         }

//         // 构建基准高度图
//         for (int x = 0; x < plateSize; x++)
//         for (int y = 0; y < plateSize; y++)
//         {
//             for (int i = x * subdivisionFactor ; i < ((x + 1) * subdivisionFactor); i++)
//             for (int j = y * subdivisionFactor ; j < ((y + 1) * subdivisionFactor); j++)
//             {
//                 baseHeightMap.SetValue(new Vector2I(i, j), detailedHeightMap.GetValue(new Vector2I(i, j)) * detailedLevelRange);
//             }
//         }

//         // 将基准高度图叠加到高颗粒度图上
//         detailedHeightMap = detailedHeightMap.AddMatrix(baseHeightMap);

//         return detailedHeightMap;
//     }
// }