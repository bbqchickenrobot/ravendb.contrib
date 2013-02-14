using System.IO;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
	/// <summary>
	/// Json.NET converter for <see cref="ZonedDateTime"/>.
	/// </summary>   
	internal sealed class ZonedDateTimeConverter : NodaConverterBase<ZonedDateTime>
	{
		protected override ZonedDateTime ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
		{
			var odt = default(OffsetDateTime);
			var zone = default(DateTimeZone);
			var gotDateTime = false;
			var gotZone = false;
			while (reader.Read())
			{
				if (reader.TokenType != JsonToken.PropertyName)
					break;

				var propertyName = (string)reader.Value;
				if (!reader.Read())
					continue;

				if (propertyName == "DateTime")
				{
					odt = serializer.Deserialize<OffsetDateTime>(reader);
					gotDateTime = true;
				}

				if (propertyName == "Zone")
				{
					zone = serializer.Deserialize<DateTimeZone>(reader);
					gotZone = true;
				}
			}

			if (!(gotDateTime && gotZone))
			{
				throw new InvalidDataException("An ZonedDateTime must contain DateTime and Zone properties.");
			}

			return new ZonedDateTime(odt.LocalDateTime, zone, odt.Offset);
		}

		protected override void WriteJsonImpl(JsonWriter writer, ZonedDateTime value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("DateTime");
			serializer.Serialize(writer, value.ToOffsetDateTime());
			writer.WritePropertyName("Zone");
			serializer.Serialize(writer, value.Zone);
			writer.WriteEndObject();
		}
	}
}
