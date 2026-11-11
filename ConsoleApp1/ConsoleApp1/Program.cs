using System;
using System.Collections.Generic;

public class Telescope
{
    public string Model;
    public int Aperture;
    public bool IsBusy = false;

    public Telescope(string model, int aperture)
    {
        Model = string.IsNullOrWhiteSpace(model) ? "SkyWatcher" : model;
        Aperture = aperture <= 0 ? 70 : aperture;
    }

    public void Print()
    {
        string status = IsBusy ? "занят" : "свободен";
        string grade = Aperture >= 150 ? "A"
            : Aperture >= 100 ? "B" : "C";

        Console.WriteLine(
            $"[{status}] {Model} (апертура {Aperture}мм) — класс {grade}");
    }

    public bool Occupy()
    {
        if (IsBusy) return false;

        IsBusy = true;
        return true;
    }

    public bool Free()
    {
        if (!IsBusy) return false;

        IsBusy = false;
        return true;
    }
}

public class Observation
{
    public string Visitor;
    public Telescope Scope;
    public int Minutes;
    public bool IsActive = false;

    private bool wasStarted = false;

    public Observation(string visitor, Telescope scope, int minutes)
    {
        Visitor = string.IsNullOrWhiteSpace(visitor) ? "Гость" : visitor;
        Scope = scope;
        Minutes = minutes <= 0 ? 30 : minutes;
    }

    public bool Start()
    {
        if (Scope == null || IsActive || !Scope.Occupy())
            return false;

        IsActive = true;
        wasStarted = true;
        return true;
    }

    public bool Finish()
    {
        if (!IsActive) return false;

        Scope.Free();
        IsActive = false;
        return true;
    }

    public void Print()
    {
        Console.WriteLine($"Наблюдение: {Visitor}");

        Console.WriteLine(Scope == null
            ? "  Телескоп не выбран"
            : $"  Телескоп: {Scope.Model} ({Scope.Aperture}мм)");

        Console.WriteLine($"  Длительность: {Minutes} мин");

        string status = IsActive ? "ИДЁТ"
            : wasStarted ? "ЗАВЕРШЕНО" : "НЕ НАЧАТО";

        Console.WriteLine($"  Статус: [{status}]\n");
    }
}

public class Planetarium
{
    public List<Telescope> Telescopes;
    public List<Observation> Observations;

    public Planetarium()
    {
        Telescopes = new List<Telescope>();
        Observations = new List<Observation>();
    }

    public void AddTelescope(Telescope t)
    {
        if (t != null) Telescopes.Add(t);
    }

    public Telescope FindTelescope(string model)
    {
        return Telescopes.Find(t => t.Model == model);
    }

    public Observation FindObservation(string visitor)
    {
        return Observations.Find(o => o.Visitor == visitor);
    }

    public Observation BookObservation(
        string visitor, string model, int minutes)
    {
        Telescope t = FindTelescope(model);

        if (t == null || t.IsBusy) return null;

        Observation o = new Observation(visitor, t, minutes);
        Observations.Add(o);
        return o;
    }

    public int CountActive()
    {
        int count = 0;

        foreach (Observation o in Observations)
            if (o.IsActive) count++;

        return count;
    }

    public void PrintAllTelescopes()
    {
        foreach (Telescope t in Telescopes)
            t.Print();
    }

    public void PrintFreeTelescopes()
    {
        foreach (Telescope t in Telescopes)
            if (!t.IsBusy) t.Print();
    }

    public void PrintAllObservations()
    {
        if (Observations.Count == 0)
            Console.WriteLine("Наблюдений пока нет.");

        foreach (Observation o in Observations)
            o.Print();
    }

    public bool CancelObservation(string visitor)
    {
        Observation o = FindObservation(visitor);

        return o != null && !o.IsActive && Observations.Remove(o);
    }

    public int TotalMinutes(string visitor)
    {
        int total = 0;

        foreach (Observation o in Observations)
            if (o.Visitor == visitor) total += o.Minutes;

        return total;
    }
}

public class Program
{
    static string Ask(string text)
    {
        Console.Write(text);
        return Console.ReadLine() ?? "";
    }

    public static void Main()
    {
        Planetarium p = new Planetarium();

        p.AddTelescope(new Telescope("SkyWatcher", 130));
        p.AddTelescope(new Telescope("Celestron", 150));
        p.AddTelescope(new Telescope("Levenhuk", 70));

        // Стартовое наблюдение: забронировано, но не начато.
        p.BookObservation("Иван", "SkyWatcher", 45);

        while (true)
        {
            Console.WriteLine(
                "\nПЛАНЕТАРИЙ\n" +
                "1. Показать все телескопы\n" +
                "2. Показать свободные телескопы\n" +
                "3. Забронировать наблюдение\n" +
                "4. Начать наблюдение\n" +
                "5. Завершить наблюдение\n" +
                "6. Показать все наблюдения\n" +
                "7. Статистика\n" +
                "8. Отменить бронь\n" +
                "9. Сумма минут сеансов посетителя\n" +
                "0. Выход");

            Console.Write("Выберите пункт: ");
            string choice = Console.ReadLine();

            if (choice == null) return;

            switch (choice)
            {
                case "0":
                    return;

                case "1":
                    p.PrintAllTelescopes();
                    break;

                case "2":
                    p.PrintFreeTelescopes();
                    break;

                case "3":
                    {
                        string visitor = Ask("Имя посетителя: ");
                        string model = Ask("Модель телескопа: ");

                        if (!int.TryParse(Ask("Минуты: "), out int minutes))
                        {
                            Console.WriteLine("Введите целое число.");
                            break;
                        }

                        Observation o =
                            p.BookObservation(visitor, model, minutes);

                        Console.WriteLine(o != null
                            ? "Наблюдение забронировано."
                            : "Телескоп недоступен.");
                        break;
                    }

                case "4":
                case "5":
                    {
                        Observation o =
                            p.FindObservation(Ask("Имя посетителя: "));

                        if (o == null)
                        {
                            Console.WriteLine("Наблюдение не найдено.");
                            break;
                        }

                        if (choice == "4")
                        {
                            Console.WriteLine(o.Start()
                                ? "Наблюдение началось."
                                : "Не удалось начать наблюдение.");
                        }
                        else
                        {
                            Console.WriteLine(o.Finish()
                                ? "Наблюдение завершено."
                                : "Наблюдение сейчас не идёт.");
                        }

                        break;
                    }

                case "6":
                    p.PrintAllObservations();
                    break;

                case "7":
                    Console.WriteLine(
                        $"Активных наблюдений: {p.CountActive()}\n" +
                        $"Всего телескопов: {p.Telescopes.Count}");
                    break;

                case "8":
                    Console.WriteLine(
                        p.CancelObservation(Ask("Имя посетителя: "))
                            ? "Бронь отменена."
                            : "Наблюдение не найдено или сейчас идёт.");
                    break;

                case "9":
                    {
                        string visitor = Ask("Имя посетителя: ");
                        Console.WriteLine(
                            $"Всего минут: {p.TotalMinutes(visitor)}");
                        break;
                    }

                default:
                    Console.WriteLine("Неверный пункт меню.");
                    break;
            }
        }
    }
}