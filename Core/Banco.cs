using System;
using Monopoly.Estructuras;

public class Banco
{
    public const int SALDO_INICIAL = 1500;
    public const int PREMIO_INICIO = 200;

    private HistorialTransacciones historial;

    public Banco()
    {
        historial = new HistorialTransacciones();
    }

    public HistorialTransacciones ObtenerHistorial()
    {
        return historial;
    }

    // Cobra la propiedad y se la asigna al jugador. Devuelve false si no se puede
    public bool ComprarPropiedad(Jugador jugador, Propiedad propiedad, int turno)
    {
        if (propiedad.Propietario != null)
        {
            return false;
        }
        if (jugador.Saldo < propiedad.Precio)
        {
            return false;
        }

        jugador.Saldo = jugador.Saldo - propiedad.Precio;
        propiedad.Propietario = jugador;
        jugador.Propiedades.Agregar(propiedad);

        Registrar(turno, TipoTransaccion.COMPRA_PROPIEDAD,
            jugador.Id, jugador.Nombre,
            Transaccion.ID_BANCO, Transaccion.NOMBRE_BANCO,
            propiedad.Precio, jugador.Nombre + " compro " + propiedad.Nombre);
        return true;
    }

    // Paga la deuda al acreedor (o al banco si Acreedor es null). Devuelve false si no alcanza el saldo
    public bool PagarDeuda(Jugador deudor, DeudaPendiente deuda, int turno)
    {
        if (deudor.Saldo < deuda.Monto)
        {
            return false;
        }

        deudor.Saldo = deudor.Saldo - deuda.Monto;

        int idDestino = Transaccion.ID_BANCO;
        string nombreDestino = Transaccion.NOMBRE_BANCO;
        if (deuda.Acreedor != null)
        {
            deuda.Acreedor.Saldo = deuda.Acreedor.Saldo + deuda.Monto;
            idDestino = deuda.Acreedor.Id;
            nombreDestino = deuda.Acreedor.Nombre;
        }

        Registrar(turno, deuda.Tipo,
            deudor.Id, deudor.Nombre,
            idDestino, nombreDestino,
            deuda.Monto, deuda.Descripcion);
        return true;
    }

    // El banco le da dinero al jugador (premio por inicio, ganancia por evento)
    public void Depositar(Jugador jugador, int monto, TipoTransaccion tipo, string descripcion, int turno)
    {
        jugador.Saldo = jugador.Saldo + monto;

        Registrar(turno, tipo,
            Transaccion.ID_BANCO, Transaccion.NOMBRE_BANCO,
            jugador.Id, jugador.Nombre,
            monto, descripcion);
    }

    // Elimina al jugador: entrega lo que le queda a quien le debia y libera sus propiedades
    public void LiquidarBancarrota(Jugador jugador, DeudaPendiente? deuda, int turno)
    {
        if (deuda != null && jugador.Saldo > 0)
        {
            int restante = jugador.Saldo;
            jugador.Saldo = 0;

            int idDestino = Transaccion.ID_BANCO;
            string nombreDestino = Transaccion.NOMBRE_BANCO;
            TipoTransaccion tipo = TipoTransaccion.PAGO_BANCO;

            if (deuda.Acreedor != null)
            {
                deuda.Acreedor.Saldo = deuda.Acreedor.Saldo + restante;
                idDestino = deuda.Acreedor.Id;
                nombreDestino = deuda.Acreedor.Nombre;
                tipo = TipoTransaccion.PAGO_ENTRE_JUGADORES;
            }

            Registrar(turno, tipo,
                jugador.Id, jugador.Nombre,
                idDestino, nombreDestino,
                restante, "Pago parcial de " + jugador.Nombre + " por bancarrota");
        }

        foreach (Propiedad p in jugador.Propiedades)
        {
            p.Propietario = null;
        }
        jugador.Propiedades = new ListaSimple<Propiedad>();

        jugador.Saldo = 0;
        jugador.EnBancarrota = true;
    }

    // Patrimonio = saldo + valor de las propiedades
    public int CalcularPatrimonio(Jugador jugador)
    {
        int total = jugador.Saldo;
        foreach (Propiedad p in jugador.Propiedades)
        {
            total = total + p.Precio;
        }
        return total;
    }

    private void Registrar(int turno, TipoTransaccion tipo, int idOrigen, string nombreOrigen,
        int idDestino, string nombreDestino, int monto, string descripcion)
    {
        Transaccion transaccion = new Transaccion(
            historial.ObtenerSiguienteId(), turno, tipo,
            idOrigen, nombreOrigen, idDestino, nombreDestino,
            monto, descripcion);
        historial.Agregar(transaccion);
    }
}