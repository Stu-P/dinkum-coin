using System.Reflection;

namespace DinkumCoin.Blockchain.Api
{
    public static class ApplicationVersion
    {
        public static string Value { get; } =
            typeof(ApplicationVersion)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
    }
}
