public class Dado
{
    public int Valor { get; private set; }

    public Dado()
    {
        Valor = 0;
    }

    
    public bool AsignarValor(int valor)
    {
        if (valor < 1 || valor > 6)
        {
            return false;
        }

        Valor = valor;
        return true;
    }
}
