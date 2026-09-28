public class CasillaEvento : Casilla
{
    private CartaEvento? carta;

    public CasillaEvento(int id, string nombre)
        : base(id, nombre)
    {
        carta = null;
    }

    public void AsignarCarta(CartaEvento nuevaCarta)
    {
        carta = nuevaCarta;
    }

    public CartaEvento? ObtenerCarta()
    {
        return carta;
    }

    public override void Ejecutar(Jugador jugador, Juego juego)
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