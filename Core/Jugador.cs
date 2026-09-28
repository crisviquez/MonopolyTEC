public class Jugador
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Posicion { get; set; }
    public int Saldo { get; set; }
    public bool EnBancarrota { get; set; }

    public Jugador(int id, string nombre, int saldo)
    {
        Id = id;
        Nombre = nombre;
        Posicion = 0;
        Saldo = saldo;
        EnBancarrota = false;
    }
}

// Version minima para que Compile