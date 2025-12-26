namespace api.coleta.Utils
{
    public static class ProfundidadeFormatter
    {
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
                "ZeroACem" => "0-100",
                "DezAVinte" => "10-20",
                "VinteATrinta" => "20-30",
                "TrintaAQuarenta" => "30-40",
                "QuarentaACinquenta" => "40-50",
                "CinquentaASetenta" => "50-70",
                _ => profundidadeEnum // fallback: retorna o valor original se não for reconhecido
            };
        }
    }
}
