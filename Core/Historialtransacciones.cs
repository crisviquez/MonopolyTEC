using Monopoly.Estructuras;

public class HistorialTransacciones
{
    private ListaDoble<Transaccion> transacciones;
    private int ultimoId;

    public HistorialTransacciones()
    {
        transacciones = new ListaDoble<Transaccion>();
        ultimoId = 0;
    }

    public int Cantidad
    {
        get { return transacciones.Cantidad; }
    }

    // Da el Id que debe llevar la proxima transaccion
    public int ObtenerSiguienteId()
    {
        return ultimoId + 1;
    }

    public void Agregar(Transaccion transaccion)
    {
        transacciones.Agregar(transaccion);
        ultimoId = transaccion.Id;
    }

    // De la mas antigua a la mas reciente
    public ListaSimple<Transaccion> ObtenerDesdeAntigua()
    {
        ListaSimple<Transaccion> resultado = new ListaSimple<Transaccion>();
        foreach (Transaccion t in transacciones)
        {
            resultado.Agregar(t);
        }
        return resultado;
    }

    // De la mas reciente a la mas antigua
    public ListaSimple<Transaccion> ObtenerDesdeReciente()
    {
        ListaSimple<Transaccion> resultado = new ListaSimple<Transaccion>();
        foreach (Transaccion t in transacciones.ObtenerEnOrdenInverso())
        {
            resultado.Agregar(t);
        }
        return resultado;
    }

    // Devuelve las transacciones donde el jugador es origen o destino
    public ListaSimple<Transaccion> BuscarPorJugador(int idJugador)
    {
        ListaSimple<Transaccion> resultado = new ListaSimple<Transaccion>();
        foreach (Transaccion t in transacciones)
        {
            if (t.InvolucraJugador(idJugador) == true)
            {
                resultado.Agregar(t);
            }
        }
        return resultado;
    }

    public ListaSimple<Transaccion> BuscarPorTipo(TipoTransaccion tipo)
    {
        ListaSimple<Transaccion> resultado = new ListaSimple<Transaccion>();
        foreach (Transaccion t in transacciones)
        {
            if (t.Tipo == tipo)
            {
                resultado.Agregar(t);
            }
        }
        return resultado;
    }

    public void ImprimirTodas()
    {
        foreach (Transaccion t in transacciones)
        {
            Console.WriteLine(t.ComoTexto());
        }
    }

    public void ImprimirDesdeReciente()
    {
        foreach (Transaccion t in transacciones.ObtenerEnOrdenInverso())
        {
            Console.WriteLine(t.ComoTexto());
        }
    }

    // Escribe el reporte TXT de la partida. Devuelve false si no se pudo escribir
    public bool ExportarTxt(string ruta)
    {
        try
        {
            StreamWriter escritor = new StreamWriter(ruta);

            escritor.WriteLine("=== HISTORIAL DE TRANSACCIONES - MONOPOLY TEC ===");
            escritor.WriteLine("Total de transacciones: " + transacciones.Cantidad);
            escritor.WriteLine("");
            escritor.WriteLine("Nro | Turno | Tipo | Origen | Destino | Monto | Descripcion");
            escritor.WriteLine("----------------------------------------------------------");

            foreach (Transaccion t in transacciones)
            {
                escritor.WriteLine(
                    t.Id + " | "
                    + t.Turno + " | "
                    + t.Tipo + " | "
                    + t.NombreOrigen + " | "
                    + t.NombreDestino + " | "
                    + t.Monto + " | "
                    + t.Descripcion);
            }

            escritor.Close();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}