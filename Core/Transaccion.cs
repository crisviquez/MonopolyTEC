public enum TipoTransaccion
{
    COMPRA_PROPIEDAD,
    PAGO_ALQUILER,
    PAGO_BANCO,
    PAGO_ENTRE_JUGADORES,
    GANANCIA_EVENTO,
    PERDIDA_EVENTO,
    PREMIO_INICIO
}

public class Transaccion
{
    // El Id 0 se reserva para el Banco (los jugadores empiezan en 1)
    public const int ID_BANCO = 0;
    public const string NOMBRE_BANCO = "Banco";

    public int Id { get; private set; }
    public DateTime FechaHora { get; private set; }
    public int Turno { get; private set; }
    public TipoTransaccion Tipo { get; private set; }
    public int IdOrigen { get; private set; }
    public string NombreOrigen { get; private set; }
    public int IdDestino { get; private set; }
    public string NombreDestino { get; private set; }
    public int Monto { get; private set; }
    public string Descripcion { get; private set; }

    public Transaccion(
        int id,
        int turno,
        TipoTransaccion tipo,
        int idOrigen,
        string nombreOrigen,
        int idDestino,
        string nombreDestino,
        int monto,
        string descripcion)
    {
        Id = id;
        FechaHora = DateTime.Now;
        Turno = turno;
        Tipo = tipo;
        IdOrigen = idOrigen;
        NombreOrigen = nombreOrigen;
        IdDestino = idDestino;
        NombreDestino = nombreDestino;
        Monto = monto;
        Descripcion = descripcion;
    }

    public bool InvolucraJugador(int idJugador)
    {
        if (IdOrigen == idJugador)
        {
            return true;
        }
        if (IdDestino == idJugador)
        {
            return true;
        }
        return false;
    }

    public string FechaHoraTexto()
    {
        return FechaHora.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string ComoTexto()
    {
        return "#" + Id
            + " | " + FechaHoraTexto()
            + " | Turno " + Turno
            + " | " + Tipo
            + " | " + NombreOrigen + " -> " + NombreDestino
            + " | " + Monto
            + " | " + Descripcion;
    }
}