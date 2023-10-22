using System;
using JeopardyKing.Common.FileUtilities;
using JeopardyKing.GameComponents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JeopardyKing.Common
{
    internal static class JsonExtensionMethods
    {
        private static readonly JsonSerializerSettings s_settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public static bool TrySaveGameToJsonFile(this Board gameBoard, string path, out Exception? e)
        {
            string? s;
            (s, e) = gameBoard.SerializeToJsonString(convertPascalCaseToSnakeCase: true, indent: true);
            if (e != default)
                return false;

            FileTextWriter writer = new(s!, path);
            e = writer.WriteException;
            return writer.SuccessfulWrite;
        }

        public static bool TryLoadGameFromJsonFile(this string path, out Board? gameBoard, out Exception? e)
        {
            gameBoard = default;
            var reader = new FileTextReader(path);
            e = reader.ReadException;
            if (!reader.SuccessfulRead)
                return false;

            (gameBoard, e) = reader.AllText.DeserializeJsonString<Board>(convertSnakeCaseToPascalCase: true);
            if (e != default)
                return false;

            foreach (var c in gameBoard!.Categories)
            {
                foreach (var q in c.Questions)
                {
                    q.Currency = gameBoard.Currency;
                    q.CategoryName = c.Title;
                    q.CategoryId = c.Id;
                }
            }
            return true;
        }

        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString, bool convertSnakeCaseToPascalCase = false)
        {
            if (string.IsNullOrEmpty(serializedString))
                return (default, new ArgumentNullException(nameof(serializedString)));
            return (convertSnakeCaseToPascalCase) ? serializedString.DeserializeJsonString<T>(s_settings) :
                                                    serializedString.DeserializeJsonString<T>(new JsonSerializerSettings());
        }

        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString, JsonSerializerSettings settings)
        {
            if (string.IsNullOrEmpty(serializedString))
                return (default, new ArgumentNullException(nameof(serializedString)));

            try
            {
                return (JsonConvert.DeserializeObject<T>(serializedString, settings), default);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                return (default, e);
            }
        }

        public static (string?, Exception?) SerializeToJsonString<T>(this T objectToSerialize, bool convertPascalCaseToSnakeCase = false, bool indent = false, bool ignoreNullValues = false)
        {
            var settings = (convertPascalCaseToSnakeCase) ? s_settings : new JsonSerializerSettings();
            settings.NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;

            try
            {
                return (JsonConvert.SerializeObject(objectToSerialize, settings: settings, formatting: indent ? Formatting.Indented : Formatting.None), default);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                return (default, e);
            }
        }
    }
}
