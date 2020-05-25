using System;
using System.IO;
using System.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var fileName = Console.ReadLine();
                //var fileName = @"C:\Users\Ivan\Desktop\7.txt";
                var q = File.ReadAllLines(fileName)
                    .Where(m => string.IsNullOrWhiteSpace(m) == false)
                    .Select(m => m.Split()[0])
                    .Where(m => string.IsNullOrWhiteSpace(m) == false)
                    .Select(m => float.TryParse(m, out var n) ? n : 0)
                    .Where(m => m > 3000).ToList();

                q.ForEach(Console.WriteLine);
                
                Console.WriteLine(q.Count());
            }
        
            
        }
    }
}