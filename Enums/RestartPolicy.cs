using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Taskmaster.Enums;

public struct RestartPolicy
{
    public static readonly RestartPolicy Always = new RestartPolicy("always");
    public static readonly RestartPolicy Never = new RestartPolicy("never");
    public static readonly RestartPolicy OnFailure = new RestartPolicy("fail");

    private readonly string mode;

    private static bool IsValid(string mode)
    {
        return mode == Always.mode || mode == Never.mode || mode == OnFailure.mode;
    }

    public static bool operator ==(RestartPolicy a, RestartPolicy b)
    {
        return a.mode == b.mode;
    }
    public static bool operator !=(RestartPolicy a, RestartPolicy b)
    {
        return a.mode != b.mode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(mode);
    }

    public override bool Equals(object? obj)
    {
        return obj is RestartPolicy policy && policy.mode == mode;
    }

    public override string ToString()
    {
        return mode;
    }

    private RestartPolicy(string mode)
    {
        this.mode = mode;
    }
}

public class RestartPolicyConverter : JsonConverter<RestartPolicy>
{
    public override RestartPolicy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return value switch
        {
            "always" or "Always" => RestartPolicy.Always,
            "never" or "Never" => RestartPolicy.Never,
            "fail" or "OnFailure" => RestartPolicy.OnFailure,
            _ => RestartPolicy.Never
        };
    }

    public override void Write(Utf8JsonWriter writer, RestartPolicy value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
