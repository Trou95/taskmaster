using System.Text.Encodings.Web;
using System.Text.Json;
using Taskmaster.Modals;

namespace Taskmaster;

public class Config
{
    public Config(string configPath)
    {
        _configPath = configPath;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public List<Container>? Read()
    {
        try
        {
            if (!isExists())
                File.Create(_configPath);

            var result = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<List<Container>>(result, _options);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    public void Write(List<Container> containers)
    {
        try
        {
            if (!isExists())
                File.Create(_configPath);

            var result = JsonSerializer.Serialize(containers, _options);
            File.WriteAllText(_configPath, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public bool isExists()
    {
        return File.Exists(_configPath);
    }

    private string _configPath { get; set; }
    private JsonSerializerOptions _options { get; set; }
}
