public class CartaEvento
{
    public int Id { get; set; }
    public string Descripcion { get; set; }
    public string TipoEvento { get; set; }
    public int Valor { get; set; }

    public CartaEvento(
        int id,
        string descripcion,
        string tipoEvento,
        int valor
    )
    {
        Id = id;
        Descripcion = descripcion;
        TipoEvento = tipoEvento;
        Valor = valor;
    }

    public void Ejecutar()
    {
        Console.WriteLine(Descripcion);

        switch (TipoEvento)
        {
            case "RecibirDinero":
                Console.WriteLine("Recibe: ₡" + Valor);
                break;

            case "PagarDinero":
                Console.WriteLine("Paga: ₡" + Valor);
                break;

            case "Avanzar":
                Console.WriteLine("Avanza " + Valor + " posiciones.");
                break;

            case "Retroceder":
                Console.WriteLine("Retrocede " + Valor + " posiciones.");
                break;

            case "PerderTurno":
                Console.WriteLine("Pierde el turno.");
                break;

            case "IrACasilla":
                Console.WriteLine("Va a la casilla " + Valor);
                break;
        }
    }
}
