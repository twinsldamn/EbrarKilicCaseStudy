using Newtonsoft.Json.Linq;

public class LineText
{
    public int LineNumber { get; set; }
    public string Description { get; set; }
}

class Program
{
    static void Main()
    {
        string appPath = Environment.CurrentDirectory;
        string path = Directory.GetParent(appPath).Parent.Parent.FullName;
        string jsonFilePath = Path.Combine(path, "response.json");

        string jsonData = File.ReadAllText(jsonFilePath);
        JArray data = JArray.Parse(jsonData);

        var lines = ParseTextsWithTolerance(data);

        foreach (var line in lines)
        {
            Console.WriteLine($"Line {line.LineNumber}: {line.Description}");
        }
        Console.ReadLine();
    }

    static List<LineText> ParseTextsWithTolerance(JArray data)
    {
        int lastLineY = int.MinValue;
        int lineNumber = 1;
        string currentLineText = "";

        List<LineText> lines = new List<LineText>();

        // JSON içindeki ilk elemanda locale parametresi olduğundan bunu toplam veri olarak ele aldım.
        // Daha sonrasında jsondaki diğer verileri önce minimum y koordinatlarına daha sonra da minimum x koordinatlarına göre sıraladım.
        var orderedItems = data.Where(item => item["locale"] == null).Select(item => (JObject)item)
                               .OrderBy(item => GetCoordinate(item, "y"))
                               .ThenBy(item => GetCoordinate(item, "x"))
                               .ToList();


        foreach (var item in orderedItems)
        {
            var description = item["description"].ToString();
            var minY = GetCoordinate(item, "y");

            // Veriler gerçek bir fotoğraftan alınma olduğu için bir tolerans miktarı belirledim ve kontrol yaparken deneme ile bu tolerans miktarını ayarladım.
            // Bu sayede koordinatlardaki küçük değişikliklerde bile yeni satır oluşturulmasını engelledim.
            var tolerance = 10;

            // Eğer güncel itemın minimum y değerinin önceki satırın y değerinden farkı toleranstan büyükse yeni satır oluşturulmalı dedim.
            bool newLine = minY - lastLineY >= tolerance;

            if (newLine)
            {
                var line = new LineText { LineNumber = lineNumber, Description = currentLineText };
                lines.Add(line);
                lineNumber++;
                currentLineText = "";
            }
            currentLineText += description + " ";

            // Satır değeri değiştiyse onu güncelledim.
            lastLineY = minY;
        }

        var lastLine = new LineText { LineNumber = lineNumber, Description = currentLineText };
        lines.Add(lastLine);
        return lines;
    }

    //Verilen koordinatlar içerisinde belirtilen düzlemde minimum noktanın bulunmasını sağladım.
    static int GetCoordinate(JObject item, string axis)
    {
        var vertices = (JArray)item["boundingPoly"]["vertices"];
        var min = vertices.Min(v => (int)v[axis]);
        return min;
    }

}
