using System;
using System.Collections;
using System.Collections.Generic;
using Monopoly.Nodos;

namespace Monopoly.Estructuras
{
    public class ListaSimple<T> : IEnumerable<T>
    {
        public NodoSimple<T>? Cabeza { get; private set; }
        public int Cantidad { get; private set; }

        public void Agregar(T dato)
        {
            NodoSimple<T> nuevo = new NodoSimple<T>(dato);
            if (Cabeza == null)
            {
                Cabeza = nuevo;
            }
            else
            {
                NodoSimple<T> actual = Cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevo;
            }
            Cantidad++;
        }

        public bool Eliminar(T dato)
        {
            if (Cabeza == null) return false;

            if (EqualityComparer<T>.Default.Equals(Cabeza.Dato, dato))
            {
                Cabeza = Cabeza.Siguiente;
                Cantidad--;
                return true;
            }

            NodoSimple<T> actual = Cabeza;
            while (actual.Siguiente != null && !EqualityComparer<T>.Default.Equals(actual.Siguiente.Dato, dato))
            {
                actual = actual.Siguiente;
            }

            if (actual.Siguiente != null)
            {
                actual.Siguiente = actual.Siguiente.Siguiente;
                Cantidad--;
                return true;
            }

            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            NodoSimple<T>? actual = Cabeza;
            while (actual != null)
            {
                yield return actual.Dato;
                actual = actual.Siguiente;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}