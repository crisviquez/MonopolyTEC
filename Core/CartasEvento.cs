public class CartaRecibirDinero : CartaEvento
{
    public CartaRecibirDinero(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        juego.DarDineroPorEvento(jugador, this);
    }
}

public class CartaPagarDinero : CartaEvento
{
    public CartaPagarDinero(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        juego.CobrarPorEvento(jugador, this);
    }
}

public class CartaAvanzar : CartaEvento
{
    public CartaAvanzar(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        juego.MoverPorEvento(jugador, Valor);
    }
}

public class CartaRetroceder : CartaEvento
{
    public CartaRetroceder(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        juego.MoverPorEvento(jugador, 0 - Valor);
    }
}

public class CartaPerderTurno : CartaEvento
{
    public CartaPerderTurno(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        jugador.TurnosPorPerder = jugador.TurnosPorPerder + Valor;
    }
}

public class CartaIrACasilla : CartaEvento
{
    public CartaIrACasilla(int id, string descripcion, int valor)
        : base(id, descripcion, valor)
    {
    }

    public override void Aplicar(Jugador jugador, Juego juego)
    {
        juego.IrACasillaPorEvento(jugador, Valor);
    }
}
