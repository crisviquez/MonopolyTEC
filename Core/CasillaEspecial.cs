public abstract class Casilla
{
    public int Id { get; set; }
    public string Nombre { get; set; }

    public Casilla(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }

    public abstract void Ejecutar(Jugador jugador, Juego juego);
}
public class CasillaEspecial : Casilla
{
    public string Tipo { get; set; }

    public CasillaEspecial(int id, string nombre, string tipo)
        : base(id, nombre)
    {
        Tipo = tipo;
    }

    public override void Ejecutar()
    {
        Console.WriteLine("Casilla especial: " + Nombre);
        Console.WriteLine("Tipo: " + Tipo);
    }
}
