using Monopoly.Estructuras;

public class Jugador
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Posicion { get; set; }
    public int Saldo { get; set; }
    public bool EnBancarrota { get; set; }
    public int TurnosPorPerder { get; set; }
    public ListaSimple<Propiedad> Propiedades { get; set; }

    public Jugador(int id, string nombre, int saldo)
    {
        Id = id;
        Nombre = nombre;
        Posicion = 0;
        Saldo = saldo;
        EnBancarrota = false;
        TurnosPorPerder = 0;
        Propiedades = new ListaSimple<Propiedad>();
    }
}