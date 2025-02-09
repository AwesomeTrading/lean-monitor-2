﻿using System.Text.Json;

namespace Panoptes.Model.Serialization
{
    public static class DefaultJsonSerializerOptions
    {
        public static readonly JsonSerializerOptions Default = new JsonSerializerOptions()
        {
            Converters =
            {
                //new PacketJsonConverter(),
                new OrderEventJsonConverter(),
                new OrderJsonConverter(),
                new TimeSpanJsonConverter(),
                new SymbolJsonConverter(),
                new ColorJsonConverter(),
                new ScatterMarkerSymbolJsonConverter(),
                new PacketTypeJsonConverter(),
                new AlgorithmStatusJsonConverter(),
                new UserPlanJsonConverter(),
                new LanguageJsonConverter(),
                new ServerTypeJsonConverter()
            },
            //PropertyNamingPolicy = new PacketJsonNamingPolicy(),
            IncludeFields = true,
            IgnoreNullValues = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        };
    }
}
