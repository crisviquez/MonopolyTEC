using System;
using Monopoly.Estructuras;

public class Juego
{
    public const int MAX_JUGADORES = 4;
    public const int MAX_TURNOS_POR_DEFECTO = 60;
    public const int IMPUESTO = 100;
        public const int MINIMO_CASILLAS = 40;
    public const int POSICION_CARCEL = 10;
    public const int TURNOS_EN_CARCEL = 1;

    public int NumeroCasillas { get; set; }

    private Tablero tablero;
    private Banco banco;
    private Dado dado1;
    private Dado dado2;
    private ListaSimple<Jugador> jugadores;
    private ColaCircular<Jugador> turnos;
    private ColaCircular<CartaEvento> mazo;
    private ListaSimple<string> eventos;

    private int siguienteIdJugador;
    private int numeroTurno;
    private int maxTurnos;
    private bool iniciado;
    private bool terminado;
    private int idGanador;
    private bool dadosLanzados;
    private bool resolviendoCarta;
    private Propiedad? ofertaPendiente;
    private DeudaPendiente? deudaPendiente;

    public Juego(int numeroCasillas)
        : this(numeroCasillas, MAX_TURNOS_POR_DEFECTO)
    {
    }

    public Juego(int numeroCasillas, int maxTurnos)
    {
        if (numeroCasillas < MINIMO_CASILLAS)
        {
            numeroCasillas = MINIMO_CASILLAS;
        }

        NumeroCasillas = numeroCasillas;
        this.maxTurnos = maxTurnos;

        tablero = new Tablero(numeroCasillas);
        banco = new Banco();
        dado1 = new Dado();
        dado2 = new Dado();
        jugadores = new ListaSimple<Jugador>();
        turnos = new ColaCircular<Jugador>();
        mazo = CrearMazo();
        eventos = new ListaSimple<string>();

        siguienteIdJugador = 1; // el Id 0 es del banco
        numeroTurno = 0;
        iniciado = false;
        terminado = false;
        idGanador = 0;
        dadosLanzados = false;
        resolviendoCarta = false;
        ofertaPendiente = null;
        deudaPendiente = null;
    }

    // ---------- Consultas ----------

    public bool HaIniciado()
    {
        return iniciado;
    }

    public bool EstaTerminado()
    {
        return terminado;
    }

    public int ObtenerIdGanador()
    {
        return idGanador;
    }

    public ListaSimple<Jugador> ObtenerJugadores()
    {
        return jugadores;
    }

    // Solo se puede llamar despues de Iniciar()
    public Jugador ObtenerJugadorActual()
    {
        return turnos.VerSiguiente();
    }

    public Propiedad? ObtenerOfertaPendiente()
    {
        return ofertaPendiente;
    }

    public int ObtenerDado1()
    {
        return dado1.Valor;
    }

    public int ObtenerDado2()
    {
        return dado2.Valor;
    }

    public HistorialTransacciones ObtenerHistorial()
    {
        return banco.ObtenerHistorial();
    }

    // Entrega los mensajes acumulados y deja la lista vacia
    public ListaSimple<string> TomarEventos()
    {
        ListaSimple<string> resultado = eventos;
        eventos = new ListaSimple<string>();
        return resultado;
    }

    public void AgregarEvento(string texto)
    {
        eventos.Agregar(texto);
    }

    // ---------- Sala de espera ----------

    public Jugador? AgregarJugador(string nombre)
    {
        if (iniciado == true)
        {
            return null;
        }
        if (jugadores.Cantidad >= MAX_JUGADORES)
        {
            return null;
        }

        Jugador nuevo = new Jugador(siguienteIdJugador, nombre, Banco.SALDO_INICIAL);
        siguienteIdJugador = siguienteIdJugador + 1;
        jugadores.Agregar(nuevo);
        return nuevo;
    }

    // Devuelve "" si todo salio bien, o el texto del error
    public string Iniciar()
    {
        if (iniciado == true)
        {
            return "La partida ya inicio";
        }
        if (jugadores.Cantidad < 2)
        {
            return "Se necesitan al menos 2 jugadores para iniciar";
        }
        string faltantes = NombresSinTarjeta();
        if (faltantes != "")
        {
            return "No se puede iniciar, faltan tarjetas RFID de: " + faltantes;
        }

        foreach (Jugador j in jugadores)
        {
            turnos.Encolar(j);
        }

        iniciado = true;
        numeroTurno = 1;
        AgregarEvento("La partida comienza. Turno de " + ObtenerJugadorActual().Nombre);
        return "";
    }

    // Sale de la sala (antes de iniciar) o abandona la partida (ya iniciada)
    public void QuitarJugador(Jugador jugador)
    {
        if (iniciado == false)
        {
            jugadores.Eliminar(jugador);
            AgregarEvento(jugador.Nombre + " salio de la sala");
            return;
        }

        if (terminado == true || jugador.EnBancarrota == true)
        {
            return;
        }

        AgregarEvento(jugador.Nombre + " abandono la partida");
        EliminarJugador(jugador, null);
    }

    // ---------- Tarjetas RFID ----------

    public Jugador? BuscarJugadorPorTarjeta(string idTarjeta)
    {
        if (idTarjeta == "")
        {
            return null;
        }

        foreach (Jugador j in jugadores)
        {
            if (j.IdTarjeta == idTarjeta)
            {
                return j;
            }
        }
        return null;
    }

    private Jugador? BuscarJugadorPorNombre(string nombre)
    {
        foreach (Jugador j in jugadores)
        {
            if (j.Nombre.ToLower() == nombre.Trim().ToLower())
            {
                return j;
            }
        }
        return null;
    }

        public string NombresSinTarjeta()
    {
        string nombres = "";
        foreach (Jugador j in jugadores)
        {
            if (j.IdTarjeta == "")
            {
                if (nombres != "")
                {
                    nombres = nombres + ", ";
                }
                nombres = nombres + j.Nombre;
            }
        }
        return nombres;
    }

    public bool TodosTienenTarjeta()
    {
        if (jugadores.Cantidad == 0)
        {
            return false;
        }
        if (NombresSinTarjeta() == "")
        {
            return true;
        }
        return false;
    }

    // Asocia una tarjeta a un jugador. Solo se permite en la sala de espera
    public string RegistrarTarjeta(string nombreJugador, string idTarjeta)
    {
        if (iniciado == true)
        {
            return "No se pueden registrar tarjetas con la partida iniciada";
        }
        if (idTarjeta == "")
        {
            return "Tarjeta invalida";
        }

        Jugador? jugador = BuscarJugadorPorNombre(nombreJugador);
        if (jugador == null)
        {
            return "No existe un jugador llamado " + nombreJugador;
        }

        Jugador? dueno = BuscarJugadorPorTarjeta(idTarjeta);
        if (dueno != null && dueno.Id != jugador.Id)
        {
            return "Esa tarjeta ya pertenece a " + dueno.Nombre;
        }

        jugador.IdTarjeta = idTarjeta;
        AgregarEvento(jugador.Nombre + " registro su tarjeta RFID");
        return "";
    }

    // ---------- Acciones del jugador ----------
    // Todas devuelven "" si salieron bien, o el texto del error

    // Los valores vienen de los dados fisicos; aqui solo se validan
    public string TirarDados(Jugador jugador, int valor1, int valor2)
    {
        string error = ValidarTurno(jugador);
        if (error != "")
        {
            return error;
        }
        if (dadosLanzados == true)
        {
            return "Ya lanzaste los dados en este turno";
        }

        bool valido1 = dado1.AsignarValor(valor1);
        bool valido2 = dado2.AsignarValor(valor2);
        if (valido1 == false || valido2 == false)
        {
            return "Cada dado debe valer entre 1 y 6";
        }

        dadosLanzados = true;
        int total = dado1.Valor + dado2.Valor;

        AgregarEvento(jugador.Nombre + " avanza " + total + " casillas");
        MoverJugador(jugador, total);
        EjecutarCasillaActual(jugador);
        return "";
    }

    public string ComprarPropiedad(Jugador jugador)
    {
        string error = ValidarTurno(jugador);
        if (error != "")
        {
            return error;
        }

        Propiedad? propiedad = ofertaPendiente;
        if (propiedad == null)
        {
            return "No hay ninguna propiedad disponible para comprar";
        }
        if (propiedad.Propietario != null)
        {
            return "La propiedad ya tiene propietario";
        }
        if (jugador.Saldo < propiedad.Precio)
        {
            return "No tienes saldo suficiente para comprar " + propiedad.Nombre;
        }

        bool comprada = banco.ComprarPropiedad(jugador, propiedad, numeroTurno);
        if (comprada == false)
        {
            return "No se pudo completar la compra";
        }

        ofertaPendiente = null;
        AgregarEvento(jugador.Nombre + " compro " + propiedad.Nombre + " por " + propiedad.Precio);
        return "";
    }

    public string NoComprar(Jugador jugador)
    {
        string error = ValidarTurno(jugador);
        if (error != "")
        {
            return error;
        }

        Propiedad? propiedad = ofertaPendiente;
        if (propiedad == null)
        {
            return "No hay ninguna oferta de compra pendiente";
        }

        ofertaPendiente = null;
        AgregarEvento(jugador.Nombre + " decidio no comprar " + propiedad.Nombre);
        return "";
    }

    // Tambien lo llama el servidor cuando el jugador acerca su tarjeta RFID
    public string PagarDeuda(Jugador jugador)
    {
        string error = ValidarTurno(jugador);
        if (error != "")
        {
            return error;
        }

        DeudaPendiente? deuda = deudaPendiente;
        if (deuda == null)
        {
            return "No tienes ninguna deuda pendiente";
        }

        bool pagada = banco.PagarDeuda(jugador, deuda, numeroTurno);
        if (pagada == true)
        {
            deudaPendiente = null;
            AgregarEvento(jugador.Nombre + " pago " + deuda.Monto + ": " + deuda.Descripcion);
            return "";
        }

        // No alcanza el saldo para un pago obligatorio: queda eliminado
        AgregarEvento(jugador.Nombre + " no puede pagar " + deuda.Monto + " y queda eliminado");
        EliminarJugador(jugador, deuda);
        return "";
    }

    public string TerminarTurno(Jugador jugador)
    {
        string error = ValidarTurno(jugador);
        if (error != "")
        {
            return error;
        }
        if (dadosLanzados == false)
        {
            return "Debes lanzar los dados antes de terminar el turno";
        }
        if (deudaPendiente != null)
        {
            return "Tienes una deuda pendiente, debes pagarla antes de terminar el turno";
        }
        if (ofertaPendiente != null)
        {
            return "Debes decidir si compras la propiedad (comprar o no comprar)";
        }

        AgregarEvento(jugador.Nombre + " termina su turno");
        AvanzarTurno();

        if (terminado == false)
        {
            AgregarEvento("Turno de " + ObtenerJugadorActual().Nombre + " (turno " + numeroTurno + ")");
        }
        return "";
    }

    // ---------- Metodos que llaman las casillas desde Ejecutar ----------

        public void OfrecerPropiedad(Jugador jugador, Propiedad propiedad)
    {
        ofertaPendiente = propiedad;
        AgregarEvento(jugador.Nombre + " cayo en " + propiedad.Nombre + " (" + propiedad.Color + "), esta libre por " + propiedad.Precio);
    }

    public void CobrarAlquiler(Jugador jugador, Propiedad propiedad)
    {
        Jugador? dueno = propiedad.Propietario;
        if (dueno == null)
        {
            return;
        }

        int alquiler = propiedad.CalcularAlquiler();
        string descripcion = jugador.Nombre + " paga alquiler de " + propiedad.Nombre + " a " + dueno.Nombre;
        deudaPendiente = new DeudaPendiente(alquiler, dueno, TipoTransaccion.PAGO_ALQUILER, descripcion);
        AgregarEvento(jugador.Nombre + " debe pagar alquiler de " + alquiler + " a " + dueno.Nombre);
    }

    public void EnviarALaCarcel(Jugador jugador)
    {
        jugador.Posicion = POSICION_CARCEL;
        jugador.TurnosPorPerder = jugador.TurnosPorPerder + TURNOS_EN_CARCEL;
        AgregarEvento(jugador.Nombre + " va a la carcel y pierde " + TURNOS_EN_CARCEL + " turno");
    }

    public void CobrarImpuesto(Jugador jugador, CasillaEspecial casilla)
    {
        string descripcion = jugador.Nombre + " paga impuesto en " + casilla.Nombre;
        deudaPendiente = new DeudaPendiente(IMPUESTO, null, TipoTransaccion.PAGO_BANCO, descripcion);
        AgregarEvento(jugador.Nombre + " debe pagar un impuesto de " + IMPUESTO + " al banco");
    }

    public void EjecutarCartaEvento(Jugador jugador)
    {
        // Si una carta te mueve a otra casilla de evento, no se saca otra carta
        if (resolviendoCarta == true)
        {
            return;
        }

        // La carta usada pasa al final del mazo para poder reutilizarse
        CartaEvento carta = mazo.Desencolar();
        mazo.Encolar(carta);

        AgregarEvento(jugador.Nombre + " saca una carta: " + carta.Descripcion);
        resolviendoCarta = true;
        carta.Aplicar(jugador, this);
        resolviendoCarta = false;
    }

    // ---------- Metodos que llaman las cartas desde Aplicar ----------

    public void DarDineroPorEvento(Jugador jugador, CartaEvento carta)
    {
        banco.Depositar(jugador, carta.Valor, TipoTransaccion.GANANCIA_EVENTO, carta.Descripcion, numeroTurno);
    }

    public void CobrarPorEvento(Jugador jugador, CartaEvento carta)
    {
        deudaPendiente = new DeudaPendiente(carta.Valor, null, TipoTransaccion.PERDIDA_EVENTO, carta.Descripcion);
        AgregarEvento(jugador.Nombre + " debe pagar " + carta.Valor + " al banco");
    }

    public void MoverPorEvento(Jugador jugador, int pasos)
    {
        MoverJugador(jugador, pasos);
        EjecutarCasillaActual(jugador);
    }

    public void IrACasillaPorEvento(Jugador jugador, int posicion)
    {
        int destino = posicion % tablero.Cantidad;
        jugador.Posicion = destino;
        AgregarEvento(jugador.Nombre + " va a la casilla " + destino + " (" + tablero.ObtenerCasilla(destino).Nombre + ")");
        EjecutarCasillaActual(jugador);
    }

    // ---------- Logica interna ----------

    private string ValidarTurno(Jugador jugador)
    {
        if (iniciado == false)
        {
            return "La partida no ha iniciado";
        }
        if (terminado == true)
        {
            return "La partida ya termino";
        }
        if (jugador.EnBancarrota == true)
        {
            return "Estas eliminado";
        }
        if (ObtenerJugadorActual().Id != jugador.Id)
        {
            return "No es tu turno";
        }
        return "";
    }

    // Mueve al jugador por los nodos del tablero. Pasos negativos van hacia atras
    private void MoverJugador(Jugador jugador, int pasos)
    {
        int posicionAnterior = jugador.Posicion;
        jugador.Posicion = tablero.Mover(posicionAnterior, pasos);

        Casilla casilla = tablero.ObtenerCasilla(jugador.Posicion);
        AgregarEvento(jugador.Nombre + " llega a la casilla " + jugador.Posicion + " (" + casilla.Nombre + ")");

        // Al ir hacia adelante, si la posicion nueva es menor dio la vuelta y paso por inicio
        if (pasos > 0 && jugador.Posicion < posicionAnterior)
        {
            banco.Depositar(jugador, Banco.PREMIO_INICIO, TipoTransaccion.PREMIO_INICIO,
                jugador.Nombre + " paso por inicio", numeroTurno);
            AgregarEvento(jugador.Nombre + " pasa por inicio y recibe " + Banco.PREMIO_INICIO);
        }
    }

    private void EjecutarCasillaActual(Jugador jugador)
    {
        Casilla casilla = tablero.ObtenerCasilla(jugador.Posicion);
        casilla.Ejecutar(jugador, this);
    }

    private void AvanzarTurno()
    {
        dadosLanzados = false;
        ofertaPendiente = null;
        deudaPendiente = null;

        bool buscando = true;
        while (buscando == true)
        {
            turnos.AvanzarTurno();
            Jugador candidato = turnos.VerSiguiente();

            // Los eliminados se saltan y no cuentan como turno jugado
            if (candidato.EnBancarrota == false)
            {
                numeroTurno = numeroTurno + 1;
                if (numeroTurno > maxTurnos)
                {
                    FinalizarPorTurnos();
                    return;
                }

                if (candidato.TurnosPorPerder > 0)
                {
                    candidato.TurnosPorPerder = candidato.TurnosPorPerder - 1;
                    AgregarEvento(candidato.Nombre + " pierde este turno");
                }
                else
                {
                    buscando = false;
                }
            }
        }
    }

    // Deja al jugador en bancarrota. deuda puede ser null (por ejemplo si abandona)
    private void EliminarJugador(Jugador jugador, DeudaPendiente? deuda)
    {
        bool eraElTurnoActual = false;
        if (iniciado == true)
        {
            if (ObtenerJugadorActual().Id == jugador.Id)
            {
                eraElTurnoActual = true;
            }
        }

        banco.LiquidarBancarrota(jugador, deuda, numeroTurno);

        if (eraElTurnoActual == true)
        {
            ofertaPendiente = null;
            deudaPendiente = null;
        }

        RevisarFinDePartida();

        if (terminado == false && eraElTurnoActual == true)
        {
            AvanzarTurno();
            if (terminado == false)
            {
                AgregarEvento("Turno de " + ObtenerJugadorActual().Nombre + " (turno " + numeroTurno + ")");
            }
        }
    }

    // Termina la partida si solo queda un jugador activo
    private void RevisarFinDePartida()
    {
        if (terminado == true)
        {
            return;
        }

        int activos = 0;
        Jugador? ultimoActivo = null;
        foreach (Jugador j in jugadores)
        {
            if (j.EnBancarrota == false)
            {
                activos = activos + 1;
                ultimoActivo = j;
            }
        }

        if (activos <= 1)
        {
            terminado = true;
            if (ultimoActivo != null)
            {
                idGanador = ultimoActivo.Id;
                AgregarEvento("La partida termina. Gana " + ultimoActivo.Nombre);
            }
            else
            {
                AgregarEvento("La partida termina sin ganador");
            }
        }
    }

    // Se llego al limite de turnos: gana el mayor patrimonio (saldo + propiedades)
    private void FinalizarPorTurnos()
    {
        Jugador? mejor = null;
        int mejorPatrimonio = -1;

        foreach (Jugador j in jugadores)
        {
            if (j.EnBancarrota == false)
            {
                int patrimonio = banco.CalcularPatrimonio(j);
                AgregarEvento("Patrimonio de " + j.Nombre + ": " + patrimonio);
                if (patrimonio > mejorPatrimonio)
                {
                    mejorPatrimonio = patrimonio;
                    mejor = j;
                }
            }
        }

        terminado = true;
        if (mejor != null)
        {
            idGanador = mejor.Id;
            AgregarEvento("Se alcanzo el limite de turnos. Gana " + mejor.Nombre);
        }
    }

    private ColaCircular<CartaEvento> CrearMazo()
    {
        ColaCircular<CartaEvento> cola = new ColaCircular<CartaEvento>();
        cola.Encolar(new CartaRecibirDinero(1, "Ganaste un concurso, recibes 100", 100));
        cola.Encolar(new CartaPagarDinero(2, "Multa de transito, pagas 50", 50));
        cola.Encolar(new CartaAvanzar(3, "Un atajo, avanzas 3 casillas", 3));
        cola.Encolar(new CartaRetroceder(4, "Te equivocaste de camino, retrocedes 2 casillas", 2));
        cola.Encolar(new CartaPerderTurno(5, "Huelga de buses, pierdes un turno", 1));
        cola.Encolar(new CartaIrACasilla(6, "Viaje al Santiago Bernabeu, vas a la casilla 39", 39));
        return cola;
    }
}
