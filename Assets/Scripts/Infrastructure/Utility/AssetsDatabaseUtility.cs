using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Utility
{
    public static class AssetsDatabaseUtility
    {
        public static T[] GetAssetsAtPath<T>(string path) where T : Object
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
            var foundAssets = new List<T>();

            foreach (var guid in assets)
            {
                foundAssets.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return foundAssets.ToArray();
        }

        public static void CreateAssetAtPath<T>(T asset, string path) where T : Object
        {
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();
        }
    }
}