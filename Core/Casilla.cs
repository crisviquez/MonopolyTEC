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
