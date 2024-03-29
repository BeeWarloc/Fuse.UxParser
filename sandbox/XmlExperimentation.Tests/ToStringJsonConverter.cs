﻿using System;
using Newtonsoft.Json;

namespace XmlPatchStreams.Tests
{
	public class ToStringJsonConverter<T> : JsonConverter
	{
		public override bool CanRead => false;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(T);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}