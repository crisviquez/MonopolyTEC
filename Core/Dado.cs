using System;

public class Dado
{
    private Random random;

    public Dado()
    {
        random = new Random();
    }

    public int Lanzar()
    {
        return random.Next(1, 7);
    }
}

Dado dado1 = new Dado();
Dado dado2 = new Dado();

int resultado1 = dado1.Lanzar();
int resultado2 = dado2.Lanzar();

int total = resultado1 + resultado2;

Console.WriteLine("Dado 1: " + resultado1);
Console.WriteLine("Dado 2: " + resultado2);
Console.WriteLine("Total: " + total);
