using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixivApi
{
	internal class BookmarkTagsConverter : JsonConverter<Dictionary<long, string[]>>
	{
		public override Dictionary<long, string[]> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				while (reader.TokenType != JsonTokenType.EndArray) reader.Read();

				return new Dictionary<long, string[]>();
			}

			return JsonSerializer.Deserialize<Dictionary<long, string[]>>(ref reader);
		}

		public override void Write(Utf8JsonWriter writer, Dictionary<long, string[]> value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
