using System;
using System.Collections.Generic;
using System.Linq;
using api.coleta.Utils;
using Xunit;

namespace api.coleta.Tests
{
    public class NutrienteKeyMappingIntegrityTests
    {
        [Fact]
        public void All_NutrientKeyMapping_Targets_Should_Exist_In_DefaultConfig_Soja()
        {
            // arrange
            var soja = (Dictionary<string, object>)NutrienteConfig.DefaultNutrienteConfig["soja"];
            var mappingTargets = NutrienteConfig.NutrientKeyMapping.Values.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

            // act
            var missingTargets = mappingTargets.Where(t => !soja.ContainsKey(t)).ToList();

            // assert
            Assert.True(missingTargets.Count == 0, $"Mapped nutrient keys are missing in DefaultNutrienteConfig['soja']: {string.Join(", ", missingTargets)}");
        }

        [Fact]
        public void NutrientKeyMapping_Should_Not_Map_Two_ShortKeys_To_DifferentFullKeys_By_Accident()
        {
            // Optional check: detect duplicates values mapping
            var mapping = NutrienteConfig.NutrientKeyMapping;
            var duplicates = mapping.GroupBy(x => x.Value)
                                    .Where(g => g.Select(kvp => kvp.Key).Count() > 1)
                                    .Select(g => new { Full = g.Key, ShortKeys = g.Select(kvp => kvp.Key).ToArray() })
                                    .ToList();

            // This is informational— fail only if duplicates point to different features unintendedly.
            // If duplicates are intentional, this assertion can be relaxed or explicitly handled.
            Assert.True(duplicates.Count == 0, $"Duplicate mapping targets found - multiple short keys map to the same full attribute: {string.Join("; ", duplicates.Select(d => d.Full + " => (" + string.Join(",", d.ShortKeys) + ")"))}");
        }
    }
}
