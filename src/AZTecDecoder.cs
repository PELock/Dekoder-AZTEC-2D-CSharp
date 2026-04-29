////////////////////////////////////////////////////////////////////////////////
//
// Dekoder kodow AZTEC 2D z dowodow rejestracyjnych interfejs Web API
//
// Wersja         : AZTecDecoder v1.1.0
// Jezyk          : C#
// Zaleznosci     : System.Text.Json (JSON), System.Net.Http (HTTP)
// Autor          : Bartosz Wójcik (support@pelock.com)
// Strona domowa  : https://www.dekoderaztec.pl | https://www.pelock.com
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PELock
{
    /// <summary>
    /// Główna klasa Dekodera AZTEC 2D
    /// </summary>
    public class AZTecDecoder
    {
        private const string API_URL = "https://www.pelock.com/api/aztec-decoder/v1";

        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        private readonly string _apiKey;

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(2);
            return client;
        }

        /// <summary>
        /// Inicjalizacja klasy AZTecDecoder
        /// </summary>
        /// <param name="apiKey">Klucz API dla uslugi AZTecDecoder</param>
        public AZTecDecoder(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci tekstowej do
        /// wyjsciowej tablicy w formacie JSON.
        /// </summary>
        /// <param name="text">Odczytana wartosc z kodem AZTEC2D w formie ASCII</param>
        /// <returns>Drzewo JSON lub null jesli blad</returns>
        public JsonNode DecodeText(string text)
        {
            return DecodeTextAsync(text, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci tekstowej do
        /// wyjsciowej tablicy w formacie JSON (asynchronicznie).
        /// </summary>
        public Task<JsonNode> DecodeTextAsync(string text, CancellationToken cancellationToken = default)
        {
            var parameters = new NameValueCollection
            {
                ["command"] = "decode-text",
                ["text"] = text.Trim()
            };

            return PostRequestAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci tekstowej
        /// ze wskaznego pliku do wyjsciowej tablicy z
        /// formatu JSON.
        /// </summary>
        /// <param name="textFilePath">Sciezka do pliku z odczytana wartoscia kodu AZTEC2D</param>
        /// <returns>Drzewo JSON lub null jesli blad</returns>
        public JsonNode DecodeTextFromFile(string textFilePath)
        {
            return DecodeTextFromFileAsync(textFilePath, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci tekstowej z pliku (asynchronicznie).
        /// </summary>
        public async Task<JsonNode> DecodeTextFromFileAsync(string textFilePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(textFilePath))
            {
                return null;
            }

            string text = File.ReadAllText(textFilePath);

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return await DecodeTextAsync(text, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci zakodowanej
        /// w obrazku PNG lub JPG/JPEG do wyjsciowej tablicy
        /// w formacie JSON.
        /// </summary>
        /// <param name="imageFilePath">Sciezka do obrazka z kodem AZTEC2D</param>
        /// <returns>Drzewo JSON lub null jesli blad</returns>
        public JsonNode DecodeImageFromFile(string imageFilePath)
        {
            return DecodeImageFromFileAsync(imageFilePath, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Dekodowanie z obrazka (asynchronicznie).
        /// </summary>
        public Task<JsonNode> DecodeImageFromFileAsync(string imageFilePath, CancellationToken cancellationToken = default)
        {
            var parameters = new NameValueCollection
            {
                ["command"] = "decode-image",
                ["image"] = imageFilePath
            };

            return PostRequestAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// Logowanie do uslugi w celu sprawdzenia statusu licencji
        /// </summary>
        /// <returns>Dane licencyjne lub null jesli blad</returns>
        public JsonNode Login()
        {
            return LoginAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Logowanie (asynchronicznie).
        /// </summary>
        public Task<JsonNode> LoginAsync(CancellationToken cancellationToken = default)
        {
            var parameters = new NameValueCollection
            {
                ["command"] = "login"
            };

            return PostRequestAsync(parameters, cancellationToken);
        }

        /// <summary>
        /// Wysyla zapytanie POST do serwera WebApi
        /// </summary>
        /// <param name="paramsArray">Tablica z parametrami dla zapytania POST</param>
        /// <returns>Drzewo JSON lub null jesli blad</returns>
        public JsonNode PostRequest(NameValueCollection paramsArray)
        {
            return PostRequestAsync(paramsArray, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Wysyla zapytanie POST do serwera WebApi (asynchronicznie).
        /// </summary>
        public async Task<JsonNode> PostRequestAsync(NameValueCollection paramsArray, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return null;
                }

                paramsArray["key"] = _apiKey;

                string json;

                if (paramsArray["image"] == null)
                {
                    json = await PostFormUrlEncodedAsync(API_URL, paramsArray, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    json = await UploadFileMultipartAsync(API_URL, paramsArray["image"], "image", paramsArray, cancellationToken).ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                try
                {
                    return JsonNode.Parse(json);
                }
                catch (System.Text.Json.JsonException)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string> PostFormUrlEncodedAsync(string url, NameValueCollection form, CancellationToken cancellationToken)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            foreach (string key in form.AllKeys)
            {
                if (key == null)
                {
                    continue;
                }

                pairs.Add(new KeyValuePair<string, string>(key, form[key] ?? string.Empty));
            }

            using (var content = new FormUrlEncodedContent(pairs))
            using (var response = await SharedHttpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false))
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Wysyla multipart/form-data z ta sama kolejnoscia pol co poprzednia implementacja (wszystkie pola tekstowe, potem plik).
        /// </summary>
        private static async Task<string> UploadFileMultipartAsync(string url, string filePath, string fileFormName, NameValueCollection formFields, CancellationToken cancellationToken)
        {
            using (var content = new MultipartFormDataContent())
            {
                foreach (string key in formFields.AllKeys)
                {
                    if (key == null)
                    {
                        continue;
                    }

                    content.Add(new StringContent(formFields[key] ?? string.Empty), key);
                }

                using (var fileStream = File.OpenRead(filePath))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, fileFormName, Path.GetFileName(filePath));

                    using (var response = await SharedHttpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false))
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
