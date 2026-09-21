using System;
using Monopoly.Nodos;

namespace Monopoly.Estructuras
{
    public class ColaCircular<T>
    {
        private NodoSimple<T>? ultimo;
        public int Cantidad { get; private set; }

        public bool EstaVacia => ultimo == null;

        public void Encolar(T dato)
        {
            NodoSimple<T> nuevo = new NodoSimple<T>(dato);
            if (EstaVacia)
            {
                ultimo = nuevo;
                ultimo.Siguiente = ultimo;
            }
            else
            {
                nuevo.Siguiente = ultimo!.Siguiente;
                ultimo.Siguiente = nuevo;
                ultimo = nuevo;
            }
            Cantidad++;
        }

        public T Desencolar()
        {
            if (EstaVacia) throw new InvalidOperationException("La cola está vacía.");

            NodoSimple<T> primero = ultimo!.Siguiente!;
            if (ultimo == primero)
            {
                ultimo = null;
            }
            else
            {
                ultimo.Siguiente = primero.Siguiente;
            }
            Cantidad--;
            return primero.Dato;
        }

        public T VerSiguiente()
        {
            if (EstaVacia) throw new InvalidOperationException("La cola está vacía.");
            return ultimo!.Siguiente!.Dato;
        }

        // Rota la cola para avanzar al siguiente turno
        public T AvanzarTurno()
        {
            if (EstaVacia) throw new InvalidOperationException("La cola está vacía.");
            ultimo = ultimo!.Siguiente;
            return ultimo!.Siguiente!.Dato;
        }
    }
}