using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Spesa
{
    public int Id { get; set; }
    public string Cosa { get; set; } = "";
    public int Quantita { get; set; }
    public decimal Importo { get; set; }
    public DateTime Data { get; set; }

    public override string ToString() =>
        $"{Id};{Cosa};{Quantita};{Importo};{Data:yyyy-MM-dd}";

    public static Spesa? Parse(string line)
    {
        var parts = line.Split(';');
        if (parts.Length != 5) return null;
        return new Spesa
        {
            Id = int.Parse(parts[0]),
            Cosa = parts[1],
            Quantita = int.Parse(parts[2]),
            Importo = decimal.Parse(parts[3], CultureInfo.InvariantCulture),
            Data = DateTime.ParseExact(parts[4], "yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
    }
}

class Program
{
    static string filePath = "spese.txt";
    static List<Spesa> spese = new();

    static void Main()
    {
        CaricaSpese();

        while (true)
        {
            Console.WriteLine("\n1. Aggiungi spesa");
            Console.WriteLine("2. Visualizza tutte le spese");
            Console.WriteLine("3. Elimina una spesa");
            Console.WriteLine("4. Modifica una spesa");
            Console.WriteLine("5. Calcola spesa totale");
            Console.WriteLine("0. Esci");
            Console.Write("Scelta: ");
            var scelta = Console.ReadLine();

            switch (scelta)
            {
                case "1": AggiungiSpesa(); break;
                case "2": VisualizzaSpese(); break;
                case "3": EliminaSpesa(); break;
                case "4": ModificaSpesa(); break;
                case "5": CalcolaTotale(); break;
                case "0": return;
                default: Console.WriteLine("Scelta non valida."); break;
            }
        }
    }

    static void CaricaSpese()
    {
        if (!File.Exists(filePath)) return;
        spese = File.ReadAllLines(filePath)
            .Select(Spesa.Parse)
            .Where(s => s != null)
            .Cast<Spesa>()
            .ToList();
    }

    static void SalvaSpese()
    {
        File.WriteAllLines(filePath, spese.Select(s => s.ToString()));
    }

    static DateTime LeggiData(string prompt, DateTime? valoreAttuale = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && valoreAttuale.HasValue)
                return valoreAttuale.Value;

            string[] formati = { "dd/MM/yyyy", "yyyy-MM-dd" };
            if (DateTime.TryParseExact(input, formati, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                return data;

            Console.WriteLine("Data non valida. Usa formato dd/MM/yyyy o yyyy-MM-dd.");
        }
    }

    static void AggiungiSpesa()
    {
        var spesa = new Spesa();
        spesa.Id = spese.Any() ? spese.Max(s => s.Id) + 1 : 1;

        Console.Write("Cosa hai preso: ");
        spesa.Cosa = Console.ReadLine() ?? "";

        Console.Write("Quante ne hai prese: ");
        if (!int.TryParse(Console.ReadLine(), out var quantita) || quantita < 1)
        {
            Console.WriteLine("Quantità non valida.");
            return;
        }
        spesa.Quantita = quantita;

        Console.Write("Importo: ");
        if (!decimal.TryParse(Console.ReadLine(), out var importo))
        {
            Console.WriteLine("Importo non valido.");
            return;
        }
        spesa.Importo = importo;

        spesa.Data = LeggiData("Data (dd/MM/yyyy o yyyy-MM-dd): ");

        spese.Add(spesa);
        SalvaSpese();
        Console.WriteLine("Spesa aggiunta.");
    }

    static void VisualizzaSpese()
    {
        if (!spese.Any())
        {
            Console.WriteLine("Nessuna spesa presente.");
            return;
        }
        foreach (var s in spese)
            Console.WriteLine($"{s.Id}: {s.Cosa} - Quantità: {s.Quantita} - {s.Importo:C} - {s.Data:dd/MM/yyyy}");
    }

    static void EliminaSpesa()
    {
        VisualizzaSpese();
        Console.Write("ID spesa da eliminare: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("ID non valido.");
            return;
        }
        var spesa = spese.FirstOrDefault(s => s.Id == id);
        if (spesa == null)
        {
            Console.WriteLine("Spesa non trovata.");
            return;
        }
        spese.Remove(spesa);
        SalvaSpese();
        Console.WriteLine("Spesa eliminata.");
    }

    static void ModificaSpesa()
    {
        VisualizzaSpese();
        Console.Write("ID spesa da modificare: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("ID non valido.");
            return;
        }
        var spesa = spese.FirstOrDefault(s => s.Id == id);
        if (spesa == null)
        {
            Console.WriteLine("Spesa non trovata.");
            return;
        }

        Console.Write($"Nuovo cosa hai preso ({spesa.Cosa}): ");
        var cosa = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(cosa)) spesa.Cosa = cosa;

        Console.Write($"Nuova quantità ({spesa.Quantita}): ");
        var quantitaStr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(quantitaStr) && int.TryParse(quantitaStr, out var nuovaQuantita) && nuovaQuantita > 0)
            spesa.Quantita = nuovaQuantita;

        Console.Write($"Nuovo importo ({spesa.Importo}): ");
        var imp = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(imp) && decimal.TryParse(imp, out var nuovoImporto))
            spesa.Importo = nuovoImporto;

        spesa.Data = LeggiData($"Nuova data ({spesa.Data:dd/MM/yyyy}): ", spesa.Data);

        SalvaSpese();
        Console.WriteLine("Spesa modificata.");
    }

    static void CalcolaTotale()
    {
        var totale = spese.Sum(s => s.Importo);
        Console.WriteLine($"Totale spese: {totale:C}");
    }
}
