using System.Security.Cryptography;
using System.Text;

class Program
{
    const string charSet = "ACDEFGHKLMNPRTXYZ234579";

    static void Main()
    {
        List<bool> isValidCodeList = new List<bool>();

        var uniqueCodeList = GenerateCodes();

        foreach (string uniqueCode in uniqueCodeList)
        {
            var isValid = CheckCode(uniqueCode);
            isValidCodeList.Add(isValid);
            Console.WriteLine($"Kod:{uniqueCode} , Doğrulama sonucu:{isValid}");
        }
        Console.ReadLine();
    }

    static List<string> GenerateCodes()
    {
        List<string> codeList = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            Random random = new Random();

            char secondChar = charSet[random.Next(charSet.Length)];
            char thirdChar = charSet[random.Next(charSet.Length)];
            char fourthChar = charSet[random.Next(charSet.Length)];

            // Timestamp kullanarak kodun unique olmasını sağlayan 4 haneli değeri oluşturdum.
            string encryptedTimestamp = "";
            while (encryptedTimestamp.Length != 4 || !IsInCharSet(encryptedTimestamp, charSet))
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                encryptedTimestamp = EncryptTimestamp(timestamp);
            }
            char firstChar = GetFirstChar(fourthChar, encryptedTimestamp);

            codeList.Add($"{firstChar}{secondChar}{thirdChar}{fourthChar}{encryptedTimestamp}");
            Thread.Sleep(1); // Timestamp değerinin değişmesini kesinleştirmek için bekleme süresi koydum. 
        }
        return codeList;
    }

    // Timestamp değerini SHA256 hash algoritmasını kullanarak 4 haneli bir stringe çevirdim.
    static string EncryptTimestamp(long timestamp)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(timestamp.ToString()));
            string hashedValue = BitConverter.ToString(hashBytes).Replace("-", "");
            return hashedValue.Substring(0, 4);
        }
    }

    //Kodun ilk karakterini stringdeki 4.elemanın ASCII değeriyle timestampten oluşturduğum 4 elemanın ASCII değerlerini toplayarak bir sayı elde ettim.
    //Sonrasında charsette bu indexe gelen elemanı ilk karakter olarak seçtim.
    static char GetFirstChar(char fourthChar, string encryptedTimestamp)
    {

        int timestampValue = 0;
        foreach (char c in encryptedTimestamp)
        {
            timestampValue += (int)c;
        }
        int value = (int)fourthChar + timestampValue;
        int index = value % charSet.Length;
        return charSet[index];
    }

    //Oluşturulan timestampteki tüm elemanlar charSet içinde var mı kontrolü yaptım.
    static bool IsInCharSet(string value, string charSet)
    {
        foreach (char c in value)
        {
            if (!charSet.Contains(c))
                return false;
        }
        return true;
    }
    //GetFirstChar metodu içerisinde yapılan algoritmanın kontrolü burada yapılıyor.
    static bool CheckCode(string code)
    {
        if (String.IsNullOrEmpty(code))
            return false;
        if (code.Length != 8 || !code.All(c => charSet.Contains(c)))
            return false;

        int firstCharValue = (int)code[0];
        int fourthCharValue = (int)code[3];

        int timestampValue = 0;
        foreach (char c in code.Substring(4, 4))
        {
            timestampValue += (int)c;
        }

        int sum = fourthCharValue + timestampValue;
        int index = sum % charSet.Length;

        return firstCharValue == charSet[index];
    }
}