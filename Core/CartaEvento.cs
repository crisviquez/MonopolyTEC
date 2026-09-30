public abstract class CartaEvento
{
    public int Id { get; set; }
    public string Descripcion { get; set; }
    public int Valor { get; set; }

    public CartaEvento(int id, string descripcion, int valor)
    {
        Id = id;
        Descripcion = descripcion;
        Valor = valor;
    }

    public abstract void Aplicar(Jugador jugador, Juego juego);
}
