using System;
using api.coleta.Utils;
using Xunit;

namespace api.coleta.Tests
{
    public class NutrienteConfigDependentesTests
    {
        [Fact]
        public void CaClassification_With_CTC_6_5_Should_Return_Medio()
        {
            // arrange
            string shortKey = "Ca";
            double averageValue = 2.9; // falls into Médio for intervalo_ctc 6.0-7.0
            double referenceValue = 6.5; // CTC interval 6.0 - 7.0

            // act
            var result = NutrienteConfig.GetNutrientClassification(shortKey, averageValue, referenceValue, "CTC");

            // assert
            Assert.Equal("Médio", result.Classificacao);
        }

        [Fact]
        public void CaClassification_With_CTC_10_5_Should_Return_Alto()
        {
            // arrange
            string shortKey = "Ca";
            double averageValue = 5.5; // falls into Alto for intervalo_ctc >= 10
            double referenceValue = 10.5; // CTC interval >=10

            // act
            var result = NutrienteConfig.GetNutrientClassification(shortKey, averageValue, referenceValue, "CTC");

            // assert
            Assert.Equal("Alto", result.Classificacao);
        }

        [Fact]
        public void MgClassification_With_CTC_6_5_Should_Return_Medio()
        {
            // arrange
            string shortKey = "Mg";
            double averageValue = 0.95; // falls into Médio for intervalo_ctc 6.0-7.0
            double referenceValue = 6.5; // CTC interval 6.0 - 7.0

            // act
            var result = NutrienteConfig.GetNutrientClassification(shortKey, averageValue, referenceValue, "CTC");

            // assert
            Assert.Equal("Médio", result.Classificacao);
        }

        [Fact]
        public void Phosphorus_Mehlich_With_Argila_500_Should_Return_Medio()
        {
            // arrange
            string shortKey = "PMELICH 1"; // maps to "Fósforo Mehlich " (Argila dependent)
            double averageValue = 7.0; // falls into Médio for Argila interval null-600
            double referenceValue = 500.0; // Argila interval null-600

            // act
            var result = NutrienteConfig.GetNutrientClassification(shortKey, averageValue, referenceValue, "Argila");

            // assert
            Assert.Equal("Médio", result.Classificacao);
        }

        [Fact]
        public void All_MappedNutrients_Should_Have_Classifications()
        {
            var soja = (Dictionary<string, object>)NutrienteConfig.DefaultNutrienteConfig["soja"];
            var mapping = NutrienteConfig.NutrientKeyMapping;
            var missingClassification = new System.Collections.Generic.List<string>();

            foreach (var kvp in mapping)
            {
                var shortKey = kvp.Key;
                var fullKey = kvp.Value;

                if (!soja.ContainsKey(fullKey))
                {
                    missingClassification.Add($"{shortKey} => {fullKey} (no soy config)");
                    continue;
                }

                var nutrient = (dynamic)soja[fullKey];
                string reference = null;
                double referenceValue = 0.0;
                double averageValue = 0.0;

                try
                {
                    if (nutrient.dependencia != null)
                    {
                        dynamic dep = nutrient.dependencia;
                        if (dep.tipo == "config_dependentes")
                        {
                            reference = dep.referencia;
                            // pick a reference interval from config_dependentes to compute a valid referenceValue and averageValue
                            if (NutrienteConfig.config_dependentes.ContainsKey(fullKey))
                            {
                                dynamic refConfig = NutrienteConfig.config_dependentes[fullKey];
                                dynamic refGroup = reference == "CTC" ? refConfig.CTC : refConfig.Argila;
                                var intervalList = (System.Collections.IEnumerable)refGroup.intervalos;
                                // pick the first interval group
                                var firstIntervalGroup = intervalList.GetEnumerator();
                                firstIntervalGroup.MoveNext();
                                var group = (System.Collections.Generic.Dictionary<string, object>)firstIntervalGroup.Current;
                                dynamic intervalRef = group[reference == "CTC" ? "intervalo_ctc" : "intervalo_argila"];
                                double? minRef = null; double? maxRef = null;
                                try { if (intervalRef.min != null) minRef = (double?)intervalRef.min; } catch { }
                                try { if (intervalRef.max != null) maxRef = (double?)intervalRef.max; } catch { }
                                referenceValue = ChooseMidPoint(minRef, maxRef);

                                // find classification 'Médio' if present, else pick first classification
                                double? minClass = null; double? maxClass = null;
                                foreach (var kv in group)
                                {
                                    if (kv.Key == "intervalo_ctc" || kv.Key == "intervalo_argila") continue;
                                    if (kv.Key == "Médio")
                                    {
                                        dynamic cls = kv.Value;
                                        try { if (cls.min != null) minClass = (double?)cls.min; } catch { }
                                        try { if (cls.max != null) maxClass = (double?)cls.max; } catch { }
                                        break;
                                    }
                                }
                                if (minClass == null && maxClass == null)
                                {
                                    // try to pick first numeric classification entry
                                    foreach (var kv in group)
                                    {
                                        if (kv.Key == "intervalo_ctc" || kv.Key == "intervalo_argila") continue;
                                        dynamic cls = kv.Value;
                                        try { if (cls.min != null || cls.max != null) { if (minClass == null) minClass = (double?)cls.min; if (maxClass == null) maxClass = (double?)cls.max; break; } } catch { }
                                    }
                                }
                                averageValue = ChooseMidPoint(minClass, maxClass);
                            }
                        }
                    }
                    else if (nutrient.intervalos != null)
                    {
                        var intervals = (System.Collections.IEnumerable)nutrient.intervalos;
                        // pick the 'Médio' interval if present
                        dynamic chosen = null;
                        foreach (dynamic interval in intervals)
                        {
                            try { if (interval.classificacao == "Médio") { chosen = interval; break; } } catch { }
                        }
                        if (chosen == null)
                        {
                            // pick middle interval
                            var arr = System.Linq.Enumerable.ToArray(intervals.Cast<dynamic>());
                            chosen = arr[arr.Length / 2];
                        }
                        double? min = null; double? max = null;
                        try { if (chosen.min != null) min = (double?)chosen.min; } catch { }
                        try { if (chosen.max != null) max = (double?)chosen.max; } catch { }
                        averageValue = ChooseMidPoint(min, max);
                    }
                    else
                    {
                        // No classification defined
                        missingClassification.Add($"{shortKey} => {fullKey} (no intervals or dependents)");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    missingClassification.Add($"{shortKey} => {fullKey} (error preparing values: {ex.Message})");
                    continue;
                }

                var result = NutrienteConfig.GetNutrientClassification(shortKey, averageValue, referenceValue, reference);
                if (result == null || result.Intervalos == null || result.Intervalos.Count == 0 || string.IsNullOrEmpty(result.Classificacao))
                {
                    missingClassification.Add($"{shortKey} => {fullKey} (no classification returned)");
                }
            }

            Assert.True(missingClassification.Count == 0, $"Some nutrients couldn't be classified or their mapping is missing: {string.Join(", ", missingClassification)}");
        }

        [Fact]
        public void Should_Differentiate_Similar_Short_Keys_Ca_CaCTC_CTC()
        {
            var mapping = NutrienteConfig.NutrientKeyMapping;
            Assert.True(mapping.ContainsKey("Ca"));
            Assert.True(mapping.ContainsKey("Ca/CTC"));
            Assert.True(mapping.ContainsKey("CTC"));

            var fullCa = mapping["Ca"];
            var fullCaCTC = mapping["Ca/CTC"];
            var fullCTC = mapping["CTC"];

            // ensure they map to distinct full keys
            Assert.False(string.Equals(fullCa, fullCaCTC, StringComparison.InvariantCultureIgnoreCase), "Ca and Ca/CTC must map to different full attributes");
            Assert.False(string.Equals(fullCa, fullCTC, StringComparison.InvariantCultureIgnoreCase), "Ca and CTC must map to different full attributes");
            Assert.False(string.Equals(fullCaCTC, fullCTC, StringComparison.InvariantCultureIgnoreCase), "Ca/CTC and CTC must map to different full attributes");

            // ensure classifications are returned for each
            var resCa = NutrienteConfig.GetNutrientClassification("Ca", 3.0, 6.5, "CTC");
            var resCaCTC = NutrienteConfig.GetNutrientClassification("Ca/CTC", 3.5, 0.0, null);
            var resCTC = NutrienteConfig.GetNutrientClassification("CTC", 12.0, 0.0, null);

            Assert.True(resCa.Intervalos.Count > 0 && !string.IsNullOrEmpty(resCa.Classificacao));
            Assert.True(resCaCTC.Intervalos.Count > 0 && !string.IsNullOrEmpty(resCaCTC.Classificacao));
            Assert.True(resCTC.Intervalos.Count > 0 && !string.IsNullOrEmpty(resCTC.Classificacao));
        }

        private static double ChooseMidPoint(double? min, double? max)
        {
            if (min == null && max == null) return 1.0;
            if (min == null) return Math.Max(0.0, (max.Value / 2.0));
            if (max == null) return min.Value + 1.0;
            return (min.Value + max.Value) / 2.0;
        }
    }
}
