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

        if (!isExists())
            Write(new List<Container>());
    }

    public List<Container>? Read()
    {
        try
        {
            if (!isExists())
                File.Create(_configPath).Close();

            var content = File.ReadAllText(_configPath);
            var res = JsonSerializer.Deserialize<List<Container>>(content, _options);
            return res;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void Write(List<Container> containers)
    {
        try
        {
            if (!isExists())
                File.Create(_configPath).Close();

            var result = JsonSerializer.Serialize(containers, _options);
            File.WriteAllText(_configPath, result);
        }
        catch
        {

        }
    }

    public bool isExists()
    {
        return File.Exists(_configPath);
    }

    private string _configPath { get; set; }
    private JsonSerializerOptions _options { get; set; }
}
