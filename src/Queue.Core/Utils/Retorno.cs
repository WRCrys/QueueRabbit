namespace Queue.Core.Utils;

public struct Retorno
{
    public bool Tipo { get; set; }
    public string Mensagem { get; set; }
    public object Data { get; set; }

    public Retorno(bool tipo = true, string mensagem = "", object data = null)
    {
        Tipo = tipo;
        Mensagem = mensagem;
        Data = data;
    }

    public Retorno(bool tipo = true, string mensagem = "")
    {
        Tipo = tipo;
        Mensagem = mensagem;
        Data = null;
    }

    public Retorno(bool tipo = true, object data = null)
    {
        Tipo = tipo;
        Mensagem = "";
        Data = data;
    }
}