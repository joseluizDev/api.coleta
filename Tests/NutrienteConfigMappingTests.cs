using System;
using System.Collections.Generic;
using System.Linq;
using api.coleta.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace api.coleta.Tests
{
    public class NutrienteConfigMappingTests
    {
        [Fact]
        public void SampleRelatorioKeys_ShouldMatchConfiguredNutrientKeys()
        {
            // arrange
            // read full sample from resource file (keeps inline code readable)
            var samplePath = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Tests", "Resources", "relatorio-sample.json");
            var sample = System.IO.File.ReadAllText(samplePath);
            var array = JArray.Parse(sample);
            var knownNonNutrientKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "LAB",
                "ID",
                "Talhão ",
                "prof.",
                "Talhão"
            };

            var soja = (Dictionary<string, object>)NutrienteConfig.DefaultNutrienteConfig["soja"];

            var missing = new List<string>();
            // act & assert
            foreach (var token in array.Cast<JObject>())
            {
                foreach (var prop in token.Properties())
                {
                    var key = prop.Name;
                    // Map using NutrientKeyMapping if exists
                    var mappedKey = NutrienteConfig.NutrientKeyMapping.ContainsKey(key) ? NutrienteConfig.NutrientKeyMapping[key] : key;

                    // If it's a known non-nutrient field (meta information), continue
                    if (knownNonNutrientKeys.Contains(key))
                        continue;

                    // Otherwise, ensure the mapped key exists in the config
                    if (!soja.ContainsKey(mappedKey))
                    {
                        missing.Add($"{key} => {mappedKey}");
                    }
                }
            }
            // Only allow certain missing nutrient keys that are intentionally not defined in the config
            var allowedMissing = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                // No allowed missing nutrient keys for this test; all keys from the sample should be mapped to a 'soja' config key
            };

            var missingNotAllowed = missing.Where(m => !allowedMissing.Contains(m.Split(" => ")[1])).ToList();
            Assert.True(missingNotAllowed.Count == 0, $"Found attributes not allowed to be missing from 'soja' config: {string.Join(", ", missingNotAllowed)}");
        }
    }
}
