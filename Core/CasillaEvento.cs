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
public class CasillaEvento : Casilla
{
    private CartaEvento carta;

    public CasillaEvento(int id, string nombre)
        : base(id, nombre)
    {
        carta = null;
    }

    public void AsignarCarta(CartaEvento nuevaCarta)
    {
        carta = nuevaCarta;
    }

    public CartaEvento ObtenerCarta()
    {
        return carta;
    }

    public override void Ejecutar()
    {
        if (carta != null)
        {
            carta.Ejecutar();
        }
        else
        {
            Console.WriteLine("No hay una carta disponible.");
        }
    }
}
