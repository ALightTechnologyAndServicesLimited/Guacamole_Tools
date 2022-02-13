using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Guacamole_tools version 0.1");
Console.WriteLine("A small set of tools for securely installing Guacamole.");
DisplayMenu();

void DisplayMenu()
{
    Console.WriteLine("Please select from one of the following options:");
    Console.WriteLine("");
    
    Console.WriteLine("1. Set / Update key-values in properties file.");
    Console.WriteLine("2. Update password in MySQL Script file.");
    Console.WriteLine("Any other number. Quit");
    Console.WriteLine("");
    
    Console.WriteLine("Please enter your selection and press enter: ");

    var input = Console.ReadLine();
    var value = 0;
    if(Int32.TryParse(input, out value))
    {
        if(value < 1 || value > 6) ClearAndDisplayMenu();

        switch (value)
        {
            case 1: SetOrUpdateKeyValues();
                break;
            case 2: UpdatePasswordInMySQLScript();
                break;
            default:
                break;
        }
    }
    else
    {
        ClearAndDisplayMenu();
    }
}

void UpdatePasswordInMySQLScript()
{
    Console.WriteLine("Please provide path of MySQL script file:");
    var path = Console.ReadLine();
    if(ValidateFileExistsAndWriteable(path))
    {
        Console.WriteLine("Enter password:");
        var password = ReadPassword();
        string content = ReadFile(path);

        string salt = GenerateRandomString(32);
        string sha256 = GenerateSHA256(password, salt);

        content = content.Replace("FE24ADC5E11E2B25288D1704ABE67A79E342ECC26064CE69C5B3177795A82264", salt)
            .Replace("CA458A7D494E3BE824F5E1E175A1556C0F8EEF2C2D7DF3633BEC4A29C4411960", sha256);
        WriteToFile(path, content);
    }
    else
    {
        Console.Clear();
        Console.WriteLine("The provided file does not exist or permissions error.");
        DisplayMenu();
    }
}

void WriteToFile(string? path, string content)
{
    var w = new StreamWriter(path, false);
    w.Write(content);
    w.Close();
}

string GenerateSHA256(string password, string salt)
{
    using(var sha256 = SHA256.Create())
    {
        var bytes = Encoding.UTF8.GetBytes($"{password}{salt}");
        var hashBytes = sha256.ComputeHash(bytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.AppendFormat("{0:x2}", hashBytes[i]);
        }
        return sb.ToString().ToUpper();
    }
}

string GenerateRandomString(int v)
{
    using(var rng = new RNGCryptoServiceProvider())
    {
        byte[] data = new byte[v];
        rng.GetBytes(data);

        return Convert.ToBase64String(data);
    }
}

string ReadFile(string? path)
{
    var sr = new StreamReader(path);
    var content = sr.ReadToEnd();

    sr.Close();

    return content;
}

string ReadPassword()
{
    var sb = new StringBuilder();
    while (true)
    {
        var c = Console.ReadKey(true);
        if (c.Key == ConsoleKey.Enter)
            break;
        sb.Append(c.KeyChar);
    }

    return sb.ToString();
}

void SetOrUpdateKeyValues()
{
    Console.Clear();
    Console.WriteLine("Enter the path to properties file:");
    var filePath = Console.ReadLine();

    Console.WriteLine("Enter the key:");
    var key = Console.ReadLine();

    Console.WriteLine("Enter the value:");
    var value = ReadPassword();

    if (ValidateFileExistsAndWriteable(filePath))
    {
        SetOrUpdateProperty(filePath, key, value);
        Console.WriteLine("Done!");
    }
    else
    {
        Console.WriteLine("ERROR: File does not exist or write permissions are not provided.");
    }
}

void SetOrUpdateProperty(string? filePath, string? key, string? value)
{
    var backupFilePath = $"{filePath}.{DateTime.Now.Ticks}.bkp";
    CreateBackup(filePath, backupFilePath);
    Console.WriteLine($"Generated a backup of the file at {backupFilePath}");

    var content = ReadFile(filePath);
    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var sb = new StringBuilder();
    var isKeyFound = false;
    foreach (var line in lines)
    {
        if (line.Trim().StartsWith(key))
        {
            sb.AppendLine($"{key}: {value}");
            isKeyFound = true;
        }
        else
            sb.AppendLine(line);
    }
    if (!isKeyFound) sb.AppendLine($"{key}: {value}");

    WriteToFile(filePath, sb.ToString());
}

void CreateBackup(string? filePath, string backupFilePath)
{
    var fi = new FileInfo(filePath);
    fi.CopyTo(backupFilePath);
}

bool ValidateFileExistsAndWriteable(string? filePath)
{
    var fi = new FileInfo(filePath);
    if(!fi.Exists) return false;

    var sw = new StreamWriter(filePath, true);
    try
    {
        sw.Write("");
        sw.Close();
    }
    catch (Exception ex)
    {
        return false;
    }

    return true;
}

void ClearAndDisplayMenu()
{
    Console.Clear();
    Console.WriteLine("Please enter a valid selection.");
    DisplayMenu();
}