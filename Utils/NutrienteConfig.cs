#nullable disable
class  NutrienteConfig
{
    public static readonly Dictionary<string, object> DefaultNutrienteConfig = new Dictionary<string, object>
    {
        ["soja"] = new Dictionary<string, object>
        {
            ["pH"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 4.4, classificacao = "Muito Baixo" },
                    new { min = 4.4, max = 4.9, classificacao = "Baixo" },
                    new { min = 4.9, max = 5.4, classificacao = "Médio" },
                    new { min = 5.4, max = 6.0, classificacao = "Adequado" },
                    new { min = 6.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["pH (H2O)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 5.1, classificacao = "Muito Baixo" },
                    new { min = 5.1, max = 5.6, classificacao = "Baixo" },
                    new { min = 5.6, max = 6.1, classificacao = "Médio" },
                    new { min = 6.1, max = 6.8, classificacao = "Adequado" },
                    new { min = 6.8, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Cálcio - Ca (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Magnésio - Mg (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Potássio - K (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Potássio - K (mg/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["SB (cmolc/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2, classificacao = "Muito Baixo" },
                    new { min = 2, max = 4, classificacao = "Baixo" },
                    new { min = 4, max = 6, classificacao = "Médio" },
                    new { min = 6, max = 12, classificacao = "Adequado" },
                    new { min = 12, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Alumínio - Al (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Alumínio Al"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Acidez Potencial (H+Al) (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Ca + Mg (cmolc/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "CTC" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando CTC média do talhão."
            },
            ["Fósforo - P Mehlich-1 (mg/dm³)"] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "Argila" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando Argila média do talhão."
            },
            ["Fósforo Mehlich "] = new
            {
                intervalos = (object[])null,
                dependencia = new { tipo = "config_dependentes", referencia = "Argila" },
                descricao = "Classificação baseada em intervalos do config_dependentes usando Argila média do talhão."
            },
            ["Fósforo - P Resina (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 12, classificacao = "Muito Baixo" },
                    new { min = 12, max = 20, classificacao = "Baixo" },
                    new { min = 20, max = 30, classificacao = "Médio" },
                    new { min = 30, max = 45, classificacao = "Adequado" },
                    new { min = 45, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Matéria Orgânica - MO (g/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 15, classificacao = "Muito Baixo" },
                    new { min = 15, max = 25, classificacao = "Baixo" },
                    new { min = 25, max = 35, classificacao = "Médio" },
                    new { min = 35, max = 50, classificacao = "Adequado" },
                    new { min = 50, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Carbono Orgânico - C (g/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 8.7, classificacao = "Muito Baixo" },
                    new { min = 8.7, max = 14.5, classificacao = "Baixo" },
                    new { min = 14.5, max = 20.3, classificacao = "Médio" },
                    new { min = 20.3, max = 29.0, classificacao = "Adequado" },
                    new { min = 29.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["CTC Efetiva (t)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2, classificacao = "Muito Baixo" },
                    new { min = 2, max = 4, classificacao = "Baixo" },
                    new { min = 4, max = 6, classificacao = "Médio" },
                    new { min = 6, max = 12, classificacao = "Adequado" },
                    new { min = 12, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["CTC a pH 7 (T)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 4, classificacao = "Muito Baixo" },
                    new { min = 4, max = 10, classificacao = "Baixo" },
                    new { min = 10, max = 15, classificacao = "Médio" },
                    new { min = 15, max = 20, classificacao = "Adequado" },
                    new { min = 20, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["V%"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 45, classificacao = "Muito Baixo" },
                    new { min = 45, max = 50, classificacao = "Baixo" },
                    new { min = 50, max = 65, classificacao = "Médio" },
                    new { min = 65, max = 75, classificacao = "Adequado" },
                    new { min = 75, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["m%"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 5, classificacao = "Muito Baixo", cor = "#317c53" },
                    new { min = 5, max = 10, classificacao = "Baixo", cor = "#90FF4C" },
                    new { min = 10, max = 20, classificacao = "Médio", cor = "#E1E86E" },
                    new { min = 20, max = 40, classificacao = "Adequado", cor = "#EB883C" },
                    new { min = 40, max = (double?)null, classificacao = "Muito Alto", cor = "#EB3F3F" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Ca/CTC (%)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 30, classificacao = "Muito Baixo" },
                    new { min = 30, max = 40, classificacao = "Baixo" },
                    new { min = 40, max = 50, classificacao = "Médio" },
                    new { min = 50, max = 60, classificacao = "Adequado" },
                    new { min = 60, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Mg/CTC (%)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 8, classificacao = "Muito Baixo" },
                    new { min = 8, max = 10, classificacao = "Baixo" },
                    new { min = 10, max = 15, classificacao = "Médio" },
                    new { min = 15, max = 20, classificacao = "Adequado" },
                    new { min = 20, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["K/CTC (%)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2, classificacao = "Muito Baixo" },
                    new { min = 2, max = 3, classificacao = "Baixo" },
                    new { min = 3, max = 4, classificacao = "Médio" },
                    new { min = 4, max = 5, classificacao = "Adequado" },
                    new { min = 5, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["H+Al/CTC (%)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 20, classificacao = "Muito Baixo", cor = "#317c53" },
                    new { min = 20, max = 30, classificacao = "Adequado", cor = "#90FF4C" },
                    new { min = 30, max = 40, classificacao = "Médio", cor = "#E1E86E" },
                    new { min = 40, max = 50, classificacao = "Alto", cor = "#EB883C" },
                    new { min = 50, max = (double?)null, classificacao = "Muito Alto", cor = "#EB3F3F" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Al/CTC (%)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 5, classificacao = "Muito Baixo", cor = "#317C53" },
                    new { min = 5, max = 9, classificacao = "Adequado", cor = "#90FF4C" },
                    new { min = 9, max = 14, classificacao = "Médio", cor = "#E1E86E" },
                    new { min = 14, max = 20, classificacao = "Alto", cor = "#EB883C" },
                    new { min = 20, max = (double?)null, classificacao = "Muito Alto", cor = "#EB3F3F" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Ca/Mg"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2, classificacao = "Muito Baixo" },
                    new { min = 2, max = 3, classificacao = "Baixo" },
                    new { min = 3, max = 4, classificacao = "Médio" },
                    new { min = 4, max = 5, classificacao = "Adequado" },
                    new { min = 5, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Ca/K"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 9, classificacao = "Muito Baixo" },
                    new { min = 9, max = 10, classificacao = "Baixo" },
                    new { min = 10, max = 12, classificacao = "Médio" },
                    new { min = 12, max = 16, classificacao = "Adequado" },
                    new { min = 16, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Mg/K"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2, classificacao = "Muito Baixo" },
                    new { min = 2, max = 3, classificacao = "Baixo" },
                    new { min = 3, max = 4, classificacao = "Médio" },
                    new { min = 4, max = 5, classificacao = "Adequado" },
                    new { min = 5, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["B (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 0.25, classificacao = "Muito Baixo" },
                    new { min = 0.25, max = 0.4, classificacao = "Baixo" },
                    new { min = 0.4, max = 0.6, classificacao = "Médio" },
                    new { min = 0.6, max = 0.8, classificacao = "Adequado" },
                    new { min = 0.8, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Cu (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 0.2, classificacao = "Muito Baixo" },
                    new { min = 0.2, max = 0.3, classificacao = "Baixo" },
                    new { min = 0.3, max = 0.5, classificacao = "Médio" },
                    new { min = 0.5, max = 1.0, classificacao = "Adequado" },
                    new { min = 1.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Zn (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 0.6, classificacao = "Muito Baixo" },
                    new { min = 0.6, max = 0.8, classificacao = "Baixo" },
                    new { min = 0.8, max = 1.2, classificacao = "Médio" },
                    new { min = 1.2, max = 5.0, classificacao = "Adequado" },
                    new { min = 5.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Mn (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 1.0, classificacao = "Muito Baixo" },
                    new { min = 1.0, max = 2.0, classificacao = "Baixo" },
                    new { min = 2.0, max = 5.0, classificacao = "Médio" },
                    new { min = 5.0, max = 20.0, classificacao = "Adequado" },
                    new { min = 20.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["Fe (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 2.5, classificacao = "Muito Baixo" },
                    new { min = 2.5, max = 5.0, classificacao = "Baixo" },
                    new { min = 5.0, max = 10.0, classificacao = "Médio" },
                    new { min = 10.0, max = 30.0, classificacao = "Adequado" },
                    new { min = 30.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["C"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 10, classificacao = "Muito Baixo" },
                    new { min = 10, max = 20, classificacao = "Baixo" },
                    new { min = 20, max = 30, classificacao = "Médio" },
                    new { min = 30, max = 40, classificacao = "Adequado" },
                    new { min = 40, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            },
            ["S (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 5, classificacao = "Muito Baixo" },
                    new { min = 5, max = 10, classificacao = "Baixo" },
                    new { min = 10, max = 20, classificacao = "Médio" },
                    new { min = 20, max = 30, classificacao = "Adequado" },
                    new { min = 30, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação e cor próprias."
            }
            ,
            ["Na (mg/dm³)"] = new
            {
                intervalos = new object[]
                {
                    new { min = (double?)null, max = 0.2, classificacao = "Muito Baixo" },
                    new { min = 0.2, max = 0.5, classificacao = "Baixo" },
                    new { min = 0.5, max = 1.0, classificacao = "Médio" },
                    new { min = 1.0, max = 2.0, classificacao = "Adequado" },
                    new { min = 2.0, max = (double?)null, classificacao = "Muito Alto" }
                },
                dependencia = (object)null,
                descricao = "Classificação de Sódio (Na) em mg/dm³ - valores de exemplo; ajuste conforme necessidade de domínio."
            }
        }
    };

    public static readonly Dictionary<string, string> NutrientKeyMapping = new Dictionary<string, string>
    {
        ["pH"] = "pH",
        ["pH (CaCl2)"] = "pH",
        ["pH CaCl2"] = "pH",
        ["pH (H2O)"] = "pH (H2O)",
        ["pH H2O"] = "pH (H2O)",
        ["Ca"] = "Cálcio - Ca (cmolc/dm³)",
        ["Cálcio"] = "Cálcio - Ca (cmolc/dm³)",
        ["Mg"] = "Magnésio - Mg (cmolc/dm³)",
        ["Magnésio"] = "Magnésio - Mg (cmolc/dm³)",
        ["K"] = "Potássio - K (cmolc/dm³)",
        ["Potássio"] = "Potássio - K (cmolc/dm³)",
        ["Al"] = "Alumínio Al",
        ["Alumínio"] = "Alumínio Al",
        ["H+Al"] = "Acidez Potencial (H+Al) (cmolc/dm³)",
        ["Ca+Mg"] = "Ca + Mg (cmolc/dm³)",
        ["Ca + Mg"] = "Ca + Mg (cmolc/dm³)",
        ["CTC"] = "CTC a pH 7 (T)",
        ["V"] = "V%",
        ["V%"] = "V%",
        ["m"] = "m%",
        ["m%"] = "m%",
        ["Ca/CTC"] = "Ca/CTC (%)",
        ["Ca/CTC (%)"] = "Ca/CTC (%)",
        ["Mg/CTC"] = "Mg/CTC (%)",
        ["Mg/CTC (%)"] = "Mg/CTC (%)",
        ["K/CTC"] = "K/CTC (%)",
        ["K/CTC (%)"] = "K/CTC (%)",
        ["H+Al/CTC"] = "H+Al/CTC (%)",
        ["H+Al/CTC (%)"] = "H+Al/CTC (%)",
        ["Al/CTC"] = "Al/CTC (%)",
        ["Al/CTC (%)"] = "Al/CTC (%)",
        ["Ca/Mg"] = "Ca/Mg",
        ["Ca/K"] = "Ca/K",
        ["Mg/K"] = "Mg/K",
        ["P MELICH 1"] = "Fósforo Mehlich ",
        ["PMELICH 1"] = "Fósforo Mehlich ",
        ["P Mehlich"] = "Fósforo Mehlich ",
        ["Fósforo"] = "Fósforo Mehlich ",
        ["P RESINA"] = "Fósforo - P Resina (mg/dm³)",
        ["P Resina"] = "Fósforo - P Resina (mg/dm³)",
        ["Mat. Org."] = "Matéria Orgânica - MO (g/dm³)",
        ["MO"] = "Matéria Orgânica - MO (g/dm³)",
        ["Matéria Orgânica"] = "Matéria Orgânica - MO (g/dm³)",
        ["C. organico"] = "Carbono Orgânico - C (g/dm³)",
        ["C Orgânico"] = "Carbono Orgânico - C (g/dm³)",
        ["B"] = "B (mg/dm³)",
        ["Cu"] = "Cu (mg/dm³)",
        ["Zn"] = "Zn (mg/dm³)",
        ["Mn"] = "Mn (mg/dm³)",
        ["Fe"] = "Fe (mg/dm³)",
        ["S"] = "S (mg/dm³)",
        ["Enxofre"] = "S (mg/dm³)",
        ["Na"] = "Na (mg/dm³)",
        ["Soma de bases"] = "SB (cmolc/dm³)",
        ["SB"] = "SB (cmolc/dm³)",
        ["CTC efetiva"] = "CTC Efetiva (t)",
        ["t"] = "CTC Efetiva (t)"
    };


    public static readonly Dictionary<string, object> config_dependentes = new Dictionary<string, object>
    {
        ["Cálcio - Ca (cmolc/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.5 },
                        ["Baixo"] = new { min = 1.5, max = 2.0 },
                        ["Médio"] = new { min = 2.0, max = 2.5 },
                        ["Alto"] = new { min = 2.5, max = 3.0 },
                        ["Muito Alto"] = new { min = 3.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 2.1 },
                        ["Baixo"] = new { min = 2.1, max = 2.8 },
                        ["Médio"] = new { min = 2.8, max = 3.5 },
                        ["Alto"] = new { min = 3.5, max = 4.2 },
                        ["Muito Alto"] = new { min = 4.2, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 2.4 },
                        ["Baixo"] = new { min = 2.4, max = 3.2 },
                        ["Médio"] = new { min = 3.2, max = 4.0 },
                        ["Alto"] = new { min = 4.0, max = 4.8 },
                        ["Muito Alto"] = new { min = 4.8, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 2.7 },
                        ["Baixo"] = new { min = 2.7, max = 3.6 },
                        ["Médio"] = new { min = 3.6, max = 4.5 },
                        ["Alto"] = new { min = 4.5, max = 5.4 },
                        ["Muito Alto"] = new { min = 5.4, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 3.0 },
                        ["Baixo"] = new { min = 3.0, max = 4.0 },
                        ["Médio"] = new { min = 4.0, max = 5.0 },
                        ["Alto"] = new { min = 5.0, max = 6.0 },
                        ["Muito Alto"] = new { min = 6.0, max = (double?)null }
                    }
                }
            }
        },
        ["Magnésio - Mg (cmolc/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.4 },
                        ["Baixo"] = new { min = 0.4, max = 0.6 },
                        ["Médio"] = new { min = 0.6, max = 0.9 },
                        ["Alto"] = new { min = 0.9, max = 1.0 },
                        ["Muito Alto"] = new { min = 1.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.6 },
                        ["Baixo"] = new { min = 0.6, max = 0.9 },
                        ["Médio"] = new { min = 0.9, max = 1.1 },
                        ["Alto"] = new { min = 1.1, max = 1.4 },
                        ["Muito Alto"] = new { min = 1.4, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.7 },
                        ["Baixo"] = new { min = 0.7, max = 1.0 },
                        ["Médio"] = new { min = 1.0, max = 1.3 },
                        ["Alto"] = new { min = 1.3, max = 1.6 },
                        ["Muito Alto"] = new { min = 1.6, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.8 },
                        ["Baixo"] = new { min = 0.8, max = 1.1 },
                        ["Médio"] = new { min = 1.1, max = 1.5 },
                        ["Alto"] = new { min = 1.5, max = 1.8 },
                        ["Muito Alto"] = new { min = 1.8, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.9 },
                        ["Baixo"] = new { min = 0.9, max = 1.2 },
                        ["Médio"] = new { min = 1.2, max = 1.6 },
                        ["Alto"] = new { min = 1.6, max = 2.0 },
                        ["Muito Alto"] = new { min = 2.0, max = (double?)null }
                    }
                }
            }
        },
        ["Potássio - K (cmolc/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.1 },
                        ["Baixo"] = new { min = 0.1, max = 0.15 },
                        ["Médio"] = new { min = 0.15, max = 0.2 },
                        ["Alto"] = new { min = 0.2, max = 0.25 },
                        ["Muito Alto"] = new { min = 0.25, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.14 },
                        ["Baixo"] = new { min = 0.14, max = 0.21 },
                        ["Médio"] = new { min = 0.21, max = 0.28 },
                        ["Alto"] = new { min = 0.28, max = 0.35 },
                        ["Muito Alto"] = new { min = 0.35, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.16 },
                        ["Baixo"] = new { min = 0.16, max = 0.24 },
                        ["Médio"] = new { min = 0.24, max = 0.32 },
                        ["Alto"] = new { min = 0.32, max = 0.4 },
                        ["Muito Alto"] = new { min = 0.4, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.18 },
                        ["Baixo"] = new { min = 0.18, max = 0.27 },
                        ["Médio"] = new { min = 0.27, max = 0.36 },
                        ["Alto"] = new { min = 0.36, max = 0.45 },
                        ["Muito Alto"] = new { min = 0.45, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.2 },
                        ["Baixo"] = new { min = 0.2, max = 0.3 },
                        ["Médio"] = new { min = 0.3, max = 0.4 },
                        ["Alto"] = new { min = 0.4, max = 0.5 },
                        ["Muito Alto"] = new { min = 0.5, max = (double?)null }
                    }
                }
            }
        },
        ["Potássio - K (mg/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 39.0 },
                        ["Baixo"] = new { min = 39.0, max = 58.0 },
                        ["Médio"] = new { min = 58.0, max = 78.0 },
                        ["Alto"] = new { min = 78.0, max = 97.0 },
                        ["Muito Alto"] = new { min = 97.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 54.0 },
                        ["Baixo"] = new { min = 54.0, max = 81.0 },
                        ["Médio"] = new { min = 81.0, max = 109.0 },
                        ["Alto"] = new { min = 109.0, max = 136.0 },
                        ["Muito Alto"] = new { min = 136.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 62.0 },
                        ["Baixo"] = new { min = 62.0, max = 93.0 },
                        ["Médio"] = new { min = 93.0, max = 124.0 },
                        ["Alto"] = new { min = 124.0, max = 156.0 },
                        ["Muito Alto"] = new { min = 156.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 70.0 },
                        ["Baixo"] = new { min = 70.0, max = 105.0 },
                        ["Médio"] = new { min = 105.0, max = 140.0 },
                        ["Alto"] = new { min = 140.0, max = 175.0 },
                        ["Muito Alto"] = new { min = 175.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 78.0 },
                        ["Baixo"] = new { min = 78.0, max = 117.0 },
                        ["Médio"] = new { min = 117.0, max = 156.0 },
                        ["Alto"] = new { min = 156.0, max = 195.0 },
                        ["Muito Alto"] = new { min = 195.0, max = (double?)null }
                    }
                }
            }
        },
        ["Acidez Potencial (H+Al) (cmolc/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.0, cor = "#317C53" },
                        ["Baixo"] = new { min = 1.0, max = 1.5, cor = "#90FF4C" },
                        ["Médio"] = new { min = 1.5, max = 2.0, cor = "#E1E86E" },
                        ["Alto"] = new { min = 2.0, max = 2.5, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 2.5, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.4, cor = "#317C53" },
                        ["Baixo"] = new { min = 1.4, max = 2.1, cor = "#90FF4C" },
                        ["Médio"] = new { min = 2.1, max = 2.8, cor = "#E1E86E" },
                        ["Alto"] = new { min = 2.8, max = 3.5, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 3.5, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.6, cor = "#317C53" },
                        ["Baixo"] = new { min = 1.6, max = 2.4, cor = "#90FF4C" },
                        ["Médio"] = new { min = 2.4, max = 3.2, cor = "#E1E86E" },
                        ["Alto"] = new { min = 3.2, max = 4.0, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 4.0, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.8, cor = "#317C53" },
                        ["Baixo"] = new { min = 1.8, max = 2.7, cor = "#90FF4C" },
                        ["Médio"] = new { min = 2.7, max = 3.6, cor = "#E1E86E" },
                        ["Alto"] = new { min = 3.6, max = 4.5, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 4.5, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 2.0, cor = "#317C53" },
                        ["Baixo"] = new { min = 2.0, max = 3.0, cor = "#90FF4C" },
                        ["Médio"] = new { min = 3.0, max = 4.0, cor = "#E1E86E" },
                        ["Alto"] = new { min = 4.0, max = 5.0, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 5.0, max = (double?)null, cor = "#EB3F3F" }
                    }
                }
            }
        },
        ["Ca + Mg (cmolc/dm³)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 1.9 },
                        ["Baixo"] = new { min = 1.9, max = 2.6 },
                        ["Médio"] = new { min = 2.6, max = 3.4 },
                        ["Alto"] = new { min = 3.4, max = 4.0 },
                        ["Muito Alto"] = new { min = 4.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 2.7 },
                        ["Baixo"] = new { min = 2.7, max = 3.7 },
                        ["Médio"] = new { min = 3.7, max = 4.6 },
                        ["Alto"] = new { min = 4.6, max = 5.6 },
                        ["Muito Alto"] = new { min = 5.6, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 3.1 },
                        ["Baixo"] = new { min = 3.1, max = 4.2 },
                        ["Médio"] = new { min = 4.2, max = 5.3 },
                        ["Alto"] = new { min = 5.3, max = 6.4 },
                        ["Muito Alto"] = new { min = 6.4, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 3.5 },
                        ["Baixo"] = new { min = 3.5, max = 4.7 },
                        ["Médio"] = new { min = 4.7, max = 6.0 },
                        ["Alto"] = new { min = 6.0, max = 7.2 },
                        ["Muito Alto"] = new { min = 7.2, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 3.9 },
                        ["Baixo"] = new { min = 3.9, max = 5.2 },
                        ["Médio"] = new { min = 5.2, max = 6.6 },
                        ["Alto"] = new { min = 6.6, max = 8.0 },
                        ["Muito Alto"] = new { min = 8.0, max = (double?)null }
                    }
                }
            }
        },
        ["Mg/CTC (%)"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.4 },
                        ["Baixo"] = new { min = 0.4, max = 0.6 },
                        ["Médio"] = new { min = 0.6, max = 0.9 },
                        ["Alto"] = new { min = 0.9, max = 1.0 },
                        ["Muito Alto"] = new { min = 1.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.6 },
                        ["Baixo"] = new { min = 0.6, max = 0.9 },
                        ["Médio"] = new { min = 0.9, max = 1.1 },
                        ["Alto"] = new { min = 1.1, max = 1.4 },
                        ["Muito Alto"] = new { min = 1.4, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.7 },
                        ["Baixo"] = new { min = 0.7, max = 1.0 },
                        ["Médio"] = new { min = 1.0, max = 1.3 },
                        ["Alto"] = new { min = 1.3, max = 1.6 },
                        ["Muito Alto"] = new { min = 1.6, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.8 },
                        ["Baixo"] = new { min = 0.8, max = 1.1 },
                        ["Médio"] = new { min = 1.1, max = 1.5 },
                        ["Alto"] = new { min = 1.5, max = 1.8 },
                        ["Muito Alto"] = new { min = 1.8, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.9 },
                        ["Baixo"] = new { min = 0.9, max = 1.2 },
                        ["Médio"] = new { min = 1.2, max = 1.6 },
                        ["Alto"] = new { min = 1.6, max = 2.0 },
                        ["Muito Alto"] = new { min = 2.0, max = (double?)null }
                    }
                }
            }
        },
        ["Alumínio Al"] = new
        {
            CTC = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = (double?)null, max = 6.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.25, cor = "#317C53" },
                        ["Baixo"] = new { min = 0.25, max = 0.5, cor = "#90FF4C" },
                        ["Médio"] = new { min = 0.5, max = 0.75, cor = "#E1E86E" },
                        ["Alto"] = new { min = 0.75, max = 1.0, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 1.0, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 6.0, max = 7.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.35, cor = "#317C53" },
                        ["Baixo"] = new { min = 0.35, max = 0.7, cor = "#90FF4C" },
                        ["Médio"] = new { min = 0.7, max = 1.0, cor = "#E1E86E" },
                        ["Alto"] = new { min = 1.0, max = 1.4, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 1.4, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 7.0, max = 8.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.4, cor = "#317C53" },
                        ["Baixo"] = new { min = 0.4, max = 0.8, cor = "#90FF4C" },
                        ["Médio"] = new { min = 0.8, max = 1.2, cor = "#E1E86E" },
                        ["Alto"] = new { min = 1.2, max = 1.6, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 1.6, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 8.0, max = 10.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.45, cor = "#317C53" },
                        ["Baixo"] = new { min = 0.45, max = 0.9, cor = "#90FF4C" },
                        ["Médio"] = new { min = 0.9, max = 1.3, cor = "#E1E86E" },
                        ["Alto"] = new { min = 1.3, max = 1.8, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 1.8, max = (double?)null, cor = "#EB3F3F" }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_ctc"] = new { min = 10.0, max = (double?)null },
                        ["Muito Baixo"] = new { min = (double?)null, max = 0.5, cor = "#317C53" },
                        ["Baixo"] = new { min = 0.5, max = 1.0, cor = "#90FF4C" },
                        ["Médio"] = new { min = 1.0, max = 1.5, cor = "#E1E86E" },
                        ["Alto"] = new { min = 1.5, max = 2.0, cor = "#EB883C" },
                        ["Muito Alto"] = new { min = 2.0, max = (double?)null, cor = "#EB3F3F" }
                    }
                }
            }
        },
        ["Fósforo Mehlich "] = new
        {
            Argila = new
            {
                intervalos = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["intervalo_argila"] = new { min = (double?)null, max = 600.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 4.0 },
                        ["Baixo"] = new { min = 4.0, max = 6.0 },
                        ["Médio"] = new { min = 6.0, max = 8.0 },
                        ["Alto"] = new { min = 8.0, max = 12.0 },
                        ["Muito Alto"] = new { min = 12.0, max = (double?)null }
                    },
                    new Dictionary<string, object>
                    {
                        ["intervalo_argila"] = new { min = 600.0, max = 350.0 },
                        ["Muito Baixo"] = new { min = (double?)null, max = 5.0 },
                        ["Baixo"] = new { min = 5.0, max = 8.0 },
                        ["Médio"] = new { min = 8.0, max = 12.0 },
                        ["Alto"] = new { min = 12.0, max = 15.0 },
                        ["Muito Alto"] = new { min = 15.0, max = (double?)null }
                    }
                }
            }
        },
        ["Fósforo Resina"] = new
        {
            intervalos = new object[]
            {
                new { min = (double?)null, max = 10.0, classificacao = "Muito Baixo" },
                new { min = 10.0, max = 20.0, classificacao = "Baixo" },
                new { min = 20.0, max = 30.0, classificacao = "Médio" },
                new { min = 30.0, max = 40.0, classificacao = "Alto" },
                new { min = 40.0, max = (double?)null, classificacao = "Muito Alto" }
            }
        }
    };

    public static readonly Dictionary<string, string> cores_classificacao = new Dictionary<string, string>
    {
        ["Muito Baixo"] = "#EB3F3F",
        ["Baixo"] = "#EB883C",
        ["Médio"] = "#E1E86E",
        ["Adequado"] = "#2CBE56",
        ["Alto"] = "#90FF4C",
        ["Muito Alto"] = "#317C53"
    };

    public class NutrientResult
    {
        public string Classificacao { get; set; }
        public string Cor { get; set; }
        public object Intervalo { get; set; }
    }

    public class IntervaloInfo
    {
        public string Classificacao { get; set; }
        public string Cor { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
    }

    public class NutrientFullResult
    {
        public double ValorMedio { get; set; }
        public string Classificacao { get; set; }
        public string Cor { get; set; }
        public List<IntervaloInfo> Intervalos { get; set; }
    }

    // Cache estático de intervalos para evitar recálculos
    private static readonly Dictionary<string, List<IntervaloInfo>> _intervalosCache = new Dictionary<string, List<IntervaloInfo>>();
    private static readonly object _cacheLock = new object();

    public static NutrientFullResult GetNutrientClassification(string attribute, double averageValue, double referenceValue, string reference = null)
    {
        // Treat null or invalid values as 0
        if (double.IsNaN(averageValue) || double.IsInfinity(averageValue))
        {
            averageValue = 0.0;
        }

        // Map short key to full key if exists
        string fullAttribute = NutrientKeyMapping.ContainsKey(attribute) ? NutrientKeyMapping[attribute] : attribute;

        var result = new NutrientFullResult
        {
            ValorMedio = averageValue,
            Intervalos = new List<IntervaloInfo>()
        };
        
        // Usar cache para intervalos se dispon\u00edvel
        string cacheKey = $"{fullAttribute}_{reference}_{referenceValue}";
        List<IntervaloInfo> intervalosCache = null;
        lock (_cacheLock)
        {
            if (_intervalosCache.TryGetValue(cacheKey, out intervalosCache))
            {
                result.Intervalos = intervalosCache;
                // Encontrar classifica\u00e7\u00e3o do valor atual
                foreach (var intervalo in intervalosCache)
                {
                    if ((intervalo.Min == null || averageValue >= intervalo.Min) &&
                        (intervalo.Max == null || averageValue < intervalo.Max))
                    {
                        result.Classificacao = intervalo.Classificacao;
                        result.Cor = intervalo.Cor;
                        return result;
                    }
                }
                return result;
            }
        }

        var soja = (Dictionary<string, object>)DefaultNutrienteConfig["soja"];
        if (soja.ContainsKey(fullAttribute))
        {
            dynamic nutrient = soja[fullAttribute];
            if (nutrient.dependencia != null)
            {
                dynamic dep = nutrient.dependencia;
                if (dep.tipo == "config_dependentes" && dep.referencia == reference)
                {
                    if (config_dependentes.ContainsKey(fullAttribute))
                    {
                        dynamic configDep = config_dependentes[fullAttribute];
                        if (reference == "CTC" || reference == "Argila")
                        {
                            dynamic refConfig = null;
                            if (reference == "CTC")
                            {
                                refConfig = configDep.CTC;
                            }
                            else if (reference == "Argila")
                            {
                                refConfig = configDep.Argila;
                            }
                            if (refConfig != null)
                            {
                                var intervalos = (System.Collections.IEnumerable)refConfig.intervalos;
                                foreach (System.Collections.Generic.Dictionary<string, object> interval in intervalos)
                                {
                                    dynamic intervaloRef = interval[reference == "CTC" ? "intervalo_ctc" : "intervalo_argila"];
                                    
                                    double? minRef = null;
                                    double? maxRef = null;
                                    
                                    try { if (intervaloRef.min != null) minRef = (double?)intervaloRef.min; } catch {}
                                    try { if (intervaloRef.max != null) maxRef = (double?)intervaloRef.max; } catch {}
                                    
                                    // Check if reference value falls in this interval range
                                    bool referenciaCorreta = (minRef == null || referenceValue >= minRef) &&
                                                           (maxRef == null || referenceValue < maxRef);
                                    
                                    if (referenciaCorreta)
                                    {
                                        // Collect all levels in this interval
                                        foreach (var kvp in interval)
                                        {
                                            if (kvp.Key != "intervalo_ctc" && kvp.Key != "intervalo_argila")
                                            {
                                                dynamic level = kvp.Value;
                                                string classificacao = kvp.Key;
                                                string cor = cores_classificacao.ContainsKey(classificacao) ? cores_classificacao[classificacao] : null;
                                                try { if (level.cor != null) cor = level.cor; } catch { }
                                                result.Intervalos.Add(new IntervaloInfo
                                                {
                                                    Classificacao = classificacao,
                                                    Cor = cor,
                                                    Min = level.min,
                                                    Max = level.max
                                                });
                                                // Check if this is the matching classification
                                                if ((level.min == null || averageValue >= level.min) &&
                                                    (level.max == null || averageValue < level.max))
                                                {
                                                    result.Classificacao = classificacao;
                                                    result.Cor = cor;
                                                }
                                            }
                                        }
                                        break; // Found the correct reference interval
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (nutrient.intervalos != null)
            {
                foreach (dynamic interv in nutrient.intervalos)
                {
                    string classificacao = interv.classificacao;
                    string cor = cores_classificacao.ContainsKey(classificacao) ? cores_classificacao[classificacao] : null;
                    try { if (interv.cor != null) cor = interv.cor; } catch { }
                    result.Intervalos.Add(new IntervaloInfo
                    {
                        Classificacao = classificacao,
                        Cor = cor,
                        Min = interv.min,
                        Max = interv.max
                    });
                    // Check if this is the matching classification
                    if ((interv.min == null || averageValue >= interv.min) &&
                        (interv.max == null || averageValue < interv.max))
                    {
                        result.Classificacao = classificacao;
                        result.Cor = cor;
                    }
                }
            }
        }
        
        // Armazenar em cache para reutiliza\u00e7\u00e3o
        if (result.Intervalos.Count > 0)
        {
            lock (_cacheLock)
            {
                if (!_intervalosCache.ContainsKey(cacheKey))
                {
                    _intervalosCache[cacheKey] = result.Intervalos;
                }
            }
        }
        
        return result;
    }
}