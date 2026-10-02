using System;
using System.Collections.Generic;
using System.IO.Ports;

public class Jugador
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string RFID { get; set; }
    public int Posicion { get; set; }
    public int Saldo { get; set; }

    public Jugador(int id, string nombre, string rfid, int saldo)
    {
        Id = id;
        Nombre = nombre;
        RFID = rfid;
        Posicion = 0;
        Saldo = saldo;
    }

    public void Mover(int cantidad)
    {
        Posicion += cantidad;
    }
}

class Program
{
    const int MAX_CASILLAS = 24;
    const int DINERO_PASAR_SALIDA = 200;

    static void Main()
    {
    
        // JUGADORES DISPONIBLES
        

        Jugador p1 = new Jugador(
            1,
            "Menganito",
            "310087153D",
            1500
        );

        Jugador p2 = new Jugador(
            2,
            "Cristopher",
            "3100891588",
            1500
        );

        Jugador p3 = new Jugador(
            3,
            "Felipe",
            "3100862FE4",
            1500
        );

        Jugador p4 = new Jugador(
            4,
            "Kevin",
            "3100895BCE",
            1500
        );

        // Todos los jugadores que pueden participar
        List<Jugador> jugadoresDisponibles = new List<Jugador>();

        jugadoresDisponibles.Add(p1);
        jugadoresDisponibles.Add(p2);
        jugadoresDisponibles.Add(p3);
        jugadoresDisponibles.Add(p4);


    
        // ORDEN DE LOS TURNOS
   

        List<Jugador> ordenTurnos = new List<Jugador>();

        bool partidaIniciada = false;

        int indiceTurno = 0;

        Jugador? jugadorActual = null;


     
        // ARDUINO
       

        SerialPort arduino = new SerialPort("COM8", 9600);


        try
        {
            arduino.Open();

            Console.WriteLine("==============================");
            Console.WriteLine("          MONOPOLY");
            Console.WriteLine("==============================");
            Console.WriteLine();

            Console.WriteLine("Arduino conectado en COM8.");
            Console.WriteLine();

            Console.WriteLine("Pase las tarjetas para registrar");
            Console.WriteLine("a los jugadores.");
            Console.WriteLine();

            while (true)
            {
                string mensaje = arduino.ReadLine().Trim();

                Console.WriteLine("Arduino: " + mensaje);


                
                // RFID
                

                if (mensaje.StartsWith("RFID:"))
                {
                    string idRFID = mensaje.Substring(5);


                    // ==================================================
                    // FASE DE REGISTRO
                    // ==================================================

                    if (!partidaIniciada)
                    {
                        Jugador? jugadorEncontrado =
                            BuscarJugador(jugadoresDisponibles, idRFID);


                        
                        // TARJETA DESCONOCIDA
                        

                        if (jugadorEncontrado == null)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Tarjeta no registrada.");
                            Console.WriteLine();

                            continue;
                        }


                        
                        // TODAVÍA NO HAY JUGADORES
                        
                        if (ordenTurnos.Count == 0)
                        {
                            ordenTurnos.Add(jugadorEncontrado);

                            Console.WriteLine();
                            Console.WriteLine(
                                "Jugador registrado: "
                                + jugadorEncontrado.Nombre
                            );

                            Console.WriteLine();
                            Console.WriteLine(
                                "Pase la siguiente tarjeta..."
                            );

                            Console.WriteLine();

                            continue;
                        }


                        
                        // ¿ES LA PRIMERA TARJETA OTRA VEZ?
                        

                        if (idRFID == ordenTurnos[0].RFID)
                        {
                            // No permitimos comenzar con un solo jugador
                            if (ordenTurnos.Count < 2)
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    "Se necesitan al menos 2 jugadores."
                                );

                                Console.WriteLine();
                                continue;
                            }


                            
                            // COMIENZA LA PARTIDA
                            

                            partidaIniciada = true;

                            indiceTurno = 0;

                            jugadorActual = ordenTurnos[indiceTurno];


                            Console.WriteLine();
                            Console.WriteLine(
                                "================================"
                            );

                            Console.WriteLine(
                                "      EMPIEZA LA PARTIDA"
                            );

                            Console.WriteLine(
                                "================================"
                            );

                            Console.WriteLine();

                            Console.WriteLine(
                                "Orden de turnos:"
                            );

                            for (int i = 0; i < ordenTurnos.Count; i++)
                            {
                                Console.WriteLine(
                                    (i + 1)
                                    + ". "
                                    + ordenTurnos[i].Nombre
                                );
                            }

                            Console.WriteLine();

                            Console.WriteLine(
                                "Empieza la partida, tire "
                                + jugadorActual.Nombre
                                + " los dados."
                            );

                            Console.WriteLine();

                            continue;
                        }


                        
                        // ¿YA HABÍA SIDO REGISTRADO?
                        

                        if (ordenTurnos.Contains(jugadorEncontrado))
                        {
                            Console.WriteLine();
                            Console.WriteLine(
                                "Ese jugador ya fue registrado."
                            );

                            Console.WriteLine(
                                "Pase una tarjeta diferente."
                            );

                            Console.WriteLine();

                            continue;
                        }


                        
                        // NUEVO JUGADOR
                        

                        ordenTurnos.Add(jugadorEncontrado);

                        Console.WriteLine();
                        Console.WriteLine(
                            "Jugador registrado: "
                            + jugadorEncontrado.Nombre
                        );

                        Console.WriteLine();

                        Console.WriteLine(
                            "Pase la siguiente tarjeta..."
                        );

                        Console.WriteLine();

                        continue;
                    }


                    
                    // PARTIDA YA INICIADA
                    

                    if (partidaIniciada)
                    {
                        Jugador? jugadorConTarjeta =
                            BuscarJugador(jugadoresDisponibles, idRFID);


                        
                        // TARJETA DESCONOCIDA
                        

                        if (jugadorConTarjeta == null)
                        {
                            Console.WriteLine();
                            Console.WriteLine(
                                "Tarjeta no registrada."
                            );

                            Console.WriteLine();

                            continue;
                        }


                        
                        // VERIFICAR TURNO
                        

                        if (jugadorConTarjeta != jugadorActual)
                        {
                            Console.WriteLine();
                            Console.WriteLine(
                                "NO ES EL TURNO DE "
                                + jugadorConTarjeta.Nombre
                            );

                            Console.WriteLine();

                            Console.WriteLine(
                                "Debe tirar "
                                + jugadorActual!.Nombre
                                + "."
                            );

                            Console.WriteLine();

                            continue;
                        }


                        
                        // TARJETA CORRECTA
                        

                        Console.WriteLine();

                        Console.WriteLine(
                            "Turno de "
                            + jugadorActual!.Nombre
                        );

                        Console.WriteLine(
                            "Puede tirar los dados."
                        );

                        Console.WriteLine();
                    }
                }


                
                // DADOS
                

                if (mensaje.StartsWith("DADOS:"))
                {
                    
                    // LA PARTIDA TODAVÍA NO EMPIEZA
                    

                    if (!partidaIniciada)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "La partida todavía no ha comenzado."
                        );

                        Console.WriteLine();

                        continue;
                    }


                    
                    // NO HAY JUGADOR ACTUAL
                    

                    if (jugadorActual == null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "No hay jugador en turno."
                        );

                        Console.WriteLine();

                        continue;
                    }


                    
                    // LEER DADOS
                    

                    string datos = mensaje.Substring(6);

                    string[] dados = datos.Split(',');


                    if (dados.Length != 2)
                    {
                        Console.WriteLine(
                            "Error en los datos de los dados."
                        );

                        continue;
                    }


                    int dado1;

                    int dado2;


                    if (!int.TryParse(dados[0], out dado1)
                        || !int.TryParse(dados[1], out dado2))
                    {
                        Console.WriteLine(
                            "Error: dados inválidos."
                        );

                        continue;
                    }


                    int suma = dado1 + dado2;


                    
                    // MOVIMIENTO
                    

                    int posicionAnterior =
                        jugadorActual.Posicion;

                    int posicionNueva =
                        posicionAnterior + suma;


                    
                    // VERIFICAR SI PASÓ POR SALIDA
                    

                    bool pasoPorSalida =
                        posicionNueva >= MAX_CASILLAS;


                    
                    // CONVERTIR A POSICIÓN CIRCULAR
                    

                    jugadorActual.Posicion =
                        posicionNueva % MAX_CASILLAS;


                    
                    // DAR LOS ₡200
                    

                    if (pasoPorSalida)
                    {
                        jugadorActual.Saldo +=
                            DINERO_PASAR_SALIDA;

                        Console.WriteLine();

                        Console.WriteLine(
                            jugadorActual.Nombre
                            + " pasó por SALIDA."
                        );

                        Console.WriteLine(
                            "Recibe ₡"
                            + DINERO_PASAR_SALIDA
                            + "."
                        );
                    }


                    
                    // MOSTRAR RESULTADO
                   
                    Console.WriteLine();

                    Console.WriteLine(
                        "--------------------------------"
                    );

                    Console.WriteLine(
                        jugadorActual.Nombre
                        + " tiró los dados:"
                    );

                    Console.WriteLine(
                        "Dado 1: "
                        + dado1
                    );

                    Console.WriteLine(
                        "Dado 2: "
                        + dado2
                    );

                    Console.WriteLine(
                        "Total: "
                        + suma
                    );

                    Console.WriteLine();

                    Console.WriteLine(
                        "Posición anterior: "
                        + posicionAnterior
                    );

                    Console.WriteLine(
                        "Nueva posición: "
                        + jugadorActual.Posicion
                    );

                    Console.WriteLine(
                        "Saldo: ₡"
                        + jugadorActual.Saldo
                    );

                    Console.WriteLine(
                        "--------------------------------"
                    );

                    Console.WriteLine();


                    
                    // CAMBIAR DE TURNO
                    

                    indiceTurno++;

                    if (indiceTurno >= ordenTurnos.Count)
                    {
                        indiceTurno = 0;
                    }


                    jugadorActual =
                        ordenTurnos[indiceTurno];


                    Console.WriteLine(
                        "================================"
                    );

                    Console.WriteLine(
                        "SIGUIENTE TURNO"
                    );

                    Console.WriteLine(
                        "================================"
                    );

                    Console.WriteLine();

                    Console.WriteLine(
                        "Empieza el turno de "
                        + jugadorActual.Nombre
                    );

                    Console.WriteLine(
                        "Pase la tarjeta de "
                        + jugadorActual.Nombre
                    );

                    Console.WriteLine(
                        "y tire los dados."
                    );

                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "ERROR:"
            );

            Console.WriteLine(
                ex.Message
            );
        }
        finally
        {
            if (arduino.IsOpen)
            {
                arduino.Close();
            }
        }
    }


    
    // BUSCAR JUGADOR POR RFID
  

    static Jugador? BuscarJugador(
        List<Jugador> jugadores,
        string rfid)
    {
        foreach (Jugador jugador in jugadores)
        {
            if (jugador.RFID == rfid)
            {
                return jugador;
            }
        }

        return null;
    }
}