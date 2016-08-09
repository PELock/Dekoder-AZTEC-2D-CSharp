////////////////////////////////////////////////////////////////////////////////
//
// Dekoder kodow AZTEC 2D z dowodow rejestracyjnych interfejs Web API
//
// Wersja         : AZTecDecoder v1.0
// Jezyk          : C#
// Zaleznosci     : Biblioteka System.Json z projektu Mono (https://github.com/mono/mono/tree/master/mcs/class/System.Json/System.Json)
// Autor          : Bartosz Wójcik (support@pelock.com)
// Strona domowa  : http://www.dekoderaztec.pl | https://www.pelock.com
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Json;
//using System.Windows.Forms;

namespace PELock
{
    /// <summary>
    /// Główna klasa Dekodera AZTEC 2D
    /// </summary>
    public class AZTecDecoder
    {
        /// <summary>
        /// string domyslna koncowka WebApi
        /// </summary>
        private const string API_URL = "https://www.pelock.com/api/aztec-decoder/v1";

        /// <summary>
        /// string klucz WebApi do uslugi AZTecDecoder
        /// </summary>
        private string _apiKey;

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
        /// <returns>Tablica z odczytanymi wartosciami lub null jesli blad</returns>
        public JsonValue DecodeText(string text)
        {
            // parametry
            var Params = new NameValueCollection
            {
                ["command"] = "decode-text",
                ["text"] = text.Trim()
            };
            
            return PostRequest(Params);
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci tekstowej
        /// ze wskaznego pliku do wyjsciowej tablicy z
        /// formatu JSON.
        /// </summary>
        /// <param name="textFilePath">Sciezka do pliku z odczytana wartoscia kodu AZTEC2D</param>
        /// <returns>Tablica z odczytanymi wartosciami lub null jesli blad</returns>
        public JsonValue DecodeTextFromFile(string textFilePath)
        {
            // czy plik istnieje?
            if (!File.Exists(textFilePath)) return null;

            // odczytaj zawartosc pliku
            string text = File.ReadAllText(textFilePath);

            // czy tresc jest pusta?
            if (!String.IsNullOrEmpty(text)) return null;

            return DecodeText(text);
        }

        /// <summary>
        /// Dekodowanie zaszyfrowanej wartosci zakodowanej
        /// w obrazku PNG lub JPG/JPEG do wyjsciowej tablicy
        /// w formacie JSON.
        /// </summary>
        /// <param name="imageFilePath">Sciezka do obrazka z kodem AZTEC2D</param>
        /// <returns>Tablica z odczytanymi wartosciami lub null jesli blad</returns>
        public JsonValue DecodeImageFromFile(string imageFilePath)
        {
            // parametry
            var Params = new NameValueCollection
            {
                ["command"] = "decode-image",
                ["image"] = imageFilePath
            };

            return PostRequest(Params);
        }

        /// <summary>
        /// Logowanie do uslugi w celu sprawdzenia statusu licencji
        /// </summary>
        /// <returns>Dane licencyjne lub null jesli blad</returns>
        public JsonValue Login()
        {
            // parametry
            var Params = new NameValueCollection
            {
                ["command"] = "login"
            };

            return PostRequest(Params);
        }

        /// <summary>
        /// Wysyla zapytanie POST do serwera WebApi
        /// </summary>
        /// <param name="paramsArray">Tablica z parametrami dla zapytania POST</param>
        /// <returns>Tablica z odczytanymi wartosciami lub null jesli blad</returns>
        public JsonValue PostRequest(NameValueCollection paramsArray)
        {
            try
            {
                // czy jest ustawiony klucz Web API?
                if (String.IsNullOrEmpty(_apiKey))
                {
                    return null;
                }

                // do parametrow dodaj klucz Web API
                paramsArray["key"] = _apiKey;

                // odpowiedz
                string json = String.Empty;

                if (paramsArray["image"] == null)
                {
                    // utworz klase do komunikacji sieciowej
                    var client = new WebClient();

                    // wyslij parametry jako zapytanie POST
                    var response = client.UploadValues(API_URL, "POST", paramsArray);

                    // odpowiedz to zakodowany w UTF-8 ciag JSON
                    json = Encoding.UTF8.GetString(response);
                }
                else
                {
                    json = UploadFileEx(API_URL, paramsArray["image"], "image", paramsArray);
                }

                if (String.IsNullOrEmpty(json))
                {
                    return null;
                }

                // deserializuj obiekt JSON do wygodnej w uzyciu tablicy result["klucz"]["element"] itd.
                var result = JsonValue.Parse(json);

                return result;

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());

                // w przypadku bledu komunikacji zwroc null
                return null;
            }
        }

        /// <summary>
        /// Wysyla zapytanie POST do serwera z zalaczonym plikiem
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath">Sciezka do pliku</param>
        /// <param name="fileFormName">Nazwa pola z plikiem</param>
        /// <param name="formFields">Dodatkowe wartosci do przeslania</param>
        /// <returns></returns>
        public static string UploadFileEx(string url, string filePath, string fileFormName, NameValueCollection formFields)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            Stream memStream = new MemoryStream();

            var boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            var endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");
            
            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            if (formFields != null)
            {
                foreach (string key in formFields.Keys)
                {
                    string formitem = string.Format(formdataTemplate, key, formFields[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }
            }

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                "Content-Type: application/octet-stream\r\n\r\n";

            memStream.Write(boundarybytes, 0, boundarybytes.Length);
            var header = string.Format(headerTemplate, fileFormName, Path.GetFileName(filePath));
            var headerbytes = Encoding.UTF8.GetBytes(header);

            memStream.Write(headerbytes, 0, headerbytes.Length);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[1024];
                var bytesRead = 0;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memStream.Write(buffer, 0, bytesRead);
                }
            }

            memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            request.ContentLength = memStream.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            }

            using (var response = request.GetResponse())
            {
                using (var stream2 = response.GetResponseStream())
                {
                    if (stream2 != null)
                    {
                        var reader2 = new StreamReader(stream2);

                        return reader2.ReadToEnd();
                    }

                    return String.Empty;
                }
            }
        }
    }
}
