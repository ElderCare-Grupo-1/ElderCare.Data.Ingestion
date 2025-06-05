using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain;

public static class PathHelper
{
    public static List<Localization> GetPath(this LinkedList<Localization> llLocalizations, Localization start, Localization end)
    {
        var node = GetNodeFromLocalization(llLocalizations, start);
        if (node == null)
            return [];

        var path = new List<Localization>();
        var current = node;
        while (current != null)
        {
            path.Add(current.Value);
            if (current.Value.Equals(end))
                return path;
            current = current.Next;
        }

        path.Clear();
        current = node;
        while (current != null)
        {
            path.Add(current.Value);
            if (current.Value.Equals(end))
                return path;
            current = current.Previous;
        }

        return [];
    }

    private static LinkedListNode<Localization>? GetNodeFromLocalization(LinkedList<Localization> llLocalization,Localization loc)
    {
        return llLocalization.Find(loc);
    }

    public static Localization GetRandomToGo(LinkedList<Localization> llLocalizations, Localization start)
    {
        var random = new Random();
        var availableLocalizations = llLocalizations.Where(loc => !loc.Equals(start)).ToList();

        if (!availableLocalizations.Any())
            throw new InvalidOperationException("No valid localizations to go to.");

        return availableLocalizations[random.Next(availableLocalizations.Count)];
    }


    public static Localization DefineStartingPoint(LinkedList<Localization> llLocalizations)
    {
        if (llLocalizations == null || !llLocalizations.Any())
            throw new ArgumentException("The list of localizations cannot be null or empty.");
        var random = new Random();
        var startIndex = random.Next(0, llLocalizations.Count);
        return llLocalizations.ElementAt(startIndex);
    }
}