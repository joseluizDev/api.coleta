using api.coleta.Models.Entidades;

namespace api.coleta.Utils
{
    public static class ProfundidadeFormatter
    {
        public static string Formatar(Profundidade profundidade)
        {
            return Formatar(profundidade.ToString());
        }

        /// <summary>
        /// Formata o enum de profundidade para um formato legível (ex.: "ZeroAVinte" -> "0-20")
        /// </summary>
        /// <param name="profundidadeEnum">Nome do enum como string</param>
        /// <returns>String formatada em formato humano</returns>
        public static string Formatar(string profundidadeEnum)
        {
            if (string.IsNullOrWhiteSpace(profundidadeEnum))
                return "N/A";

            return profundidadeEnum switch
            {
                "ZeroADez" => "0-10",
                "ZeroAVinte" => "0-20",
                "ZeroATrinta" => "0-30",
                "ZeroAQuarenta" => "0-40",
                "ZeroACinquenta" => "0-50",
                "ZeroASetenta" => "0-70",

                "DezAVinte" => "10-20",
                "DezATrinta" => "10-30",
                "DezAQuarenta" => "10-40",
                "DezACinquenta" => "10-50",
                "DezASetenta" => "10-70",

                "VinteATrinta" => "20-30",
                "VinteAQuarenta" => "20-40",
                "VinteACinquenta" => "20-50",
                "VinteASetenta" => "20-70",

                "TrintaAQuarenta" => "30-40",
                "TrintaACinquenta" => "30-50",
                "TrintaASetenta" => "30-70",

                "QuarentaACinquenta" => "40-50",
                "QuarentaASetenta" => "40-70",

                "CinquentaASetenta" => "50-70",

                "ZeroACem" => "0-100",
                _ => profundidadeEnum
            };
        }
    }
}
