using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace EntityFrameworkCore.AutoHistory.Serialization
{
    internal static class AutoHistorySerialization
    {
        private static readonly Lazy<JsonSerializerSettings> DefaultSettingsFactory = new(() => CreateDefaultSettings(), isThreadSafe: true);

        public static JsonSerializerSettings DefaultSettings => DefaultSettingsFactory.Value;

        private static JsonSerializerSettings CreateDefaultSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return settings;
        }
    }
}
