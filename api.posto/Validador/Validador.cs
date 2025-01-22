public class ValidadorDeEntidade
{
    public static void ValidarSeIgual(object objeto1, object objeto2, string mensagem)
    {
        if (!objeto1.Equals(objeto2))
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarSeDiferente(object objeto1, object objeto2, string mensagem)
    {
        if (objeto1.Equals(objeto2))
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarMaximo(string valor, int maximo, string mensagem)
    {
        var length = valor?.Trim()?.Length ?? 0;

        if (length > maximo)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarMaximo(decimal valor, int maximo, string mensagem)
    {
        if (valor > maximo)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarMinimo(string valor, int minimo, string mensagem)
    {
        var length = valor?.Trim()?.Length ?? 0;

        if (length < minimo)
        {
            throw new Exception(mensagem);
        }
    }


    public static void ValidarMinimo(decimal valor, decimal minimo, string mensagem)
    {
        if (valor < minimo)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarTamanhoRequerido(string valor, int tamanhoRequerido, string mensagem)
    {
        var length = valor?.Trim()?.Length ?? 0;

        if (length != tamanhoRequerido)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarSeVazioOuNulo(string valor, string mensagem)
    {
        if (string.IsNullOrEmpty(valor))
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarSeEhNulo(object objeto, string mensagem)
    {
        if (objeto == null)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarNaoEhSeNulo(object objeto, string mensagem)
    {
        if (objeto != null)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarMinimoMaximo(string valor, int minimo, int maximo, string mensagem)
    {
        var length = valor?.Trim()?.Length ?? 0;

        if (length < minimo || length > maximo)
        {
            throw new Exception(mensagem);
        }
    }
    public static void ValidarMinimoMaximo(decimal valor, decimal minimo, decimal maximo, string mensagem)
    {
        if (valor < minimo || valor > maximo)
        {
            throw new Exception(mensagem);
        }
    }
    public static void ValidarMinimoMaximo(int valor, decimal minimo, int maximo, string mensagem)
    {
        if (valor < minimo || valor > maximo)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarSeMenorIgualMinimo(decimal valor, decimal minimo, string mensagem)
    {
        if (valor <= minimo)
        {
            throw new Exception(mensagem);
        }
    }
    public static void ValidarSeMenorIgualMinimo(int valor, int minimo, string mensagem)
    {
        if (valor <= minimo)
        {
            throw new Exception(mensagem);
        }
    }


    public static void ValidarSeFalso(bool boolValor, string mensagem)
    {
        if (!boolValor)
        {
            throw new Exception(mensagem);
        }
    }
    public static void ValidarSeVerdadeiro(bool boolValor, string mensagem)
    {
        if (boolValor)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarDateTime(DateTime dateTime, string mensagem)
    {
        // Verifica se o ano está dentro do intervalo suportado pelo DateTime (1601-9999)
        if (dateTime.Year < 1601 || dateTime.Year > 9999)
        {
            throw new Exception(mensagem);
        }

        try
        {
            DateTime newDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarNegativo(decimal valor, string mensagem)
    {
        if (valor < 0)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarMenorIgualZero(decimal? valor, string mensagem)
    {
        if ((valor ?? 0) <= 0)
        {
            throw new Exception(mensagem);
        }
    }

    public static void ValidarCpf(string cpf, string mensagem)
    {
        if (!ValidadorCPF(cpf))
        {
            throw new Exception(mensagem);
        }
    }

    private static bool ValidadorCPF(string cpf)
    {
        if (string.IsNullOrEmpty(cpf))
        {
            return false;
        }

        cpf = cpf.Trim();
        cpf = cpf.Replace(".", "").Replace("-", "");

        if (cpf.Length != 11)
        {
            return false;
        }

        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        int[] multiplicador1 = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        }

        int resto = soma % 11;
        if (resto < 2)
        {
            resto = 0;
        }
        else
        {
            resto = 11 - resto;
        }

        string digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        }

        resto = soma % 11;
        if (resto < 2)
        {
            resto = 0;
        }
        else
        {
            resto = 11 - resto;
        }

        digito = digito + resto.ToString();

        return cpf.EndsWith(digito);
    }
}