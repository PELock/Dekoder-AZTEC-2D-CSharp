# Dekoder Kodu AZTEC 2D z Dowodu Rejestracyjnego dla C# (CSharp)

Oferujemy Państwu usługę Web API pozwalającą zdekodować dane z kodu AZTEC 2D zapisanego w dowodach rejestracyjnych pojazdów samochodowych.

![Kod AZTEC 2D zapisany w formie obrazkowej w dowodzie rejestracyjnym pojazdu](https://www.pelock.com/img/pl/produkty/dekoder-aztec/dowod-rejestracyjny-kod-aztec-2d.jpg)

Nasza biblioteka dekoduje dane z dowodu rejestracyjnego, zapisane w postaci kodu obrazkowego tzw. kod aztec. Dekodowane są wszystkie wymienione pola w dowodzie rejestracyjnym pojazdu.

https://www.pelock.com/pl/produkty/dekoder-aztec

## Gdzie znajdzie zastosowanie Dekoder AZTec?

Dekoder AZTec może przydać się firmom i instytucjom, które pragną zautomatyzować proces ręcznego wprowadzania danych z dowodów rejestracyjnych i zastąpić go poprzez wykorzystanie naszej biblioteki programistycznej, która potrafi rozpoznać i rozkodowac kody AZTEC 2D bezpośrednio ze zdjęć dowodów rejestracyjnych lub zeskanowanych już kodów (wykorzystując skaner QR / AZTEC 2D).

##Dostępne edycje programistyczne

Dekoder AZTec dostepny jest w trzech edycjach. Każda wersja posiada inne cechy i inne możliwości dekodowania. Wersja oparta o Web API jako jedyna posiada możliwość rozpoznawania i dekodowania danych bezpośrednio ze zdjęć i obrazków. Pozostałe wersje do dekodowania wymagają już odczytanego kodu w postaci tekstu (np. ze skanera).

![Dekodowanie kodu AZTEC 2D do formatu JSON](https://www.pelock.com/img/pl/produkty/dekoder-aztec/dekodowanie-kodu-aztec-2d-do-json.png)

### Wersja Web API

Jest to najbardziej zaawansowana edycja Dekodera AZTec, ponieważ umożliwia precyzyjne rozpoznawanie i dekodowanie kodów AZTEC 2D bezpośrednio ze zdjęć oraz obrazków zapisanych w formatach PNG lub JPG.

Algorytm rozpoznawania obrazu należy do naszej firmy, jest to innowacyjne rozwiązanie rozwijane od podstaw przez prawie rok czasu.

Rozumiemy potrzeby naszych klientów oraz problemy wynikające z rozpoznawnia rzeczywistych zdjęć kodów AZTEC 2D znajdujących się w dowodach rejestracyjnych, które nie zawsze są idealnie wykonane, czy to ze względu na rodzaj aparatu, kąta wykonania zdjęcia, refleksów czy słabej rozdzielczości.

Przy tworzeniu naszego rozwiązania wzieliśmy wszystkie te czynniki pod uwagę i w efekcie nasz algorytm radzi sobie znakomicie z rozpoznawaniem kodów AZTEC 2D ze zdjęć z wszelkiego rodzaju zniekształceniami, uszkodzeniami i niedoskonałościami. Znacznie przewyższa pod względem funkcjonowania dostępne na rynku biblioteki rozpoznawnia kodów AZTEC 2D takie jak np. ZXing.

### Gotowe paczki dla różnych języków programowania

Dla ułatwienia szybkiego wdrożenia, paczki instalacyjne Dekodera AZTec zostały wgrane na repozytoria dla kilku popularnych języków programowania, a dodatkowo ich kody źródłowe zostały opublikowane na GitHubie:

| Repozytorium | Język | Instalacja | Paczka | GitHub |
| ------------ | ----- | ---------- | ------ | ------ |
| ![Centralne Repozytorium Maven](https://www.pelock.com/img/logos/repo-maven.png) | Java | Dodaj wpis do pliku `pom.xml`<br />`<dependency>`<br />`  <groupId>com.pelock</groupId>`<br />`  <artifactId>AZTecDecoder</artifactId>`<br />`  <version>1.0.0</version>`<br />`</dependency>` | [Maven](https://search.maven.org/#search%7Cga%7C1%7Cg%3A%22com.pelock%22) | [Źródła](https://github.com/PELock/Dekoder-AZTEC-2D-Java)
| ![Repozytorium NPM](https://www.pelock.com//img/logos/repo-npm.png) | JavaScript, TypeScript | `npm install aztec-decoder` | [NPM](https://www.npmjs.com/package/aztec-decoder) | [Źródła](https://github.com/PELock/Dekoder-AZTEC-2D-JavaScript)
| ![Repozytorium NuGet](https://www.pelock.com/img/logos/repo-nuget.png) | C#, VB.NET, .NET | `PM> Install-Package AZTecDecoder` | [NuGet](https://www.nuget.org/packages/AZTecDecoder/) | [Źródła](https://github.com/PELock/Dekoder-AZTEC-2D-CSharp)
| ![Repozytorium Packagist dla Composer](https://www.pelock.com/img/logos/repo-packagist-composer.png) | PHP | Dodaj do sekcji `require` w twoim pliku `composer.json` linijkę `"pelock/aztec-decoder": "*"` | [Packagist](https://packagist.org/packages/pelock/aztec-decoder) | [Źródła](https://github.com/PELock/Dekoder-AZTEC-2D-PHP)
| ![Repozytorium PyPI dla Python](https://www.pelock.com/img/logos/repo-pypi.png) | Python | `pip install aztecdecoder` | [PyPi](https://pypi.org/project/aztecdecoder/) | [Źródła](https://github.com/PELock/Dekoder-AZTEC-2D-Python)

#### Instalacja dla C# i .NET

Preferowany sposób instalacji biblioteki poprzez [NuGet](https://docs.nuget.org/consume/package-manager-console).

Uruchom konsolę pakietów NuGet (Package Manager Console):

```
PM> Install-Package AZTecDecoder
```

Paczka dostępna na https://www.nuget.org/packages/AZTecDecoder/

#### Użycie dekodera AZTEC 2D dla C# (CSharp)

```csharp
//
// załącz klasę dekodera (instalacja jako pakiet NuGet "PM> Install-Package AZTecDecoder")
//
using PELock;

private void AZTecDecoderTest()
{
    //
    // utwórz klasę dekodera (używamy naszego klucza licencyjnego do inicjalizacji)
    //
    var myAZTecDecoder = new AZTecDecoder("ABCD-ABCD-ABCD-ABCD");

    //
    // 1. Dekoduj dane bezpośrednio z pliku graficznego, zwróć wynik jako rozkodowaną tablicę elementów JSON
    //
    var decodedArray = myAZTecDecoder.DecodeImageFromFile(@"C:\sciezka\zdjecie-dowodu.jpg");

    // czy udało się zdekodować dane?
    if (decodedArray != null && decodedArray["Status"] == true)
    {
        // wyświetl rozkodowane dane (są zapisane jako tablica elementów JsonValue)
        textOutput.Text = decodedArray.ToString();
    }

    //
    // 2. Dekoduj dane bezpośrednio z pliku graficznego i zwróć wynik jako rozkodowaną tablicę elementów JSON
    //
    var decodedFromImage = myAZTecDecoder.DecodeImageFromFile(@"C:\sciezka\zdjecie-kodu-aztec-2d.png");

    if (decodedFromImage != null)
    {
        MessageBox.Show(decodedFromImage.ToString());
    }

    //
    // 3. Dekoduj dane z odczytanego już ciągu znaków (np. wykorzystując skaner ręczny)
    //
    // zakodowane dane z dowodu rejestracyjnego
    var szValue = "ggMAANtYAAJD...";

    var decodedText = myAZTecDecoder.DecodeText(szValue);

    if (decodedText != null)
    {
        MessageBox.Show(decodedFromImage.ToString());
    }

    //
    // 4. Dekoduj dane z odczytanego już ciągu znaków zapisanego w pliku (np. wykorzystując skaner ręczny)
    //
    var DecodedTextFile = myAZTecDecoder.DecodeTextFromFile(@"C:\sciezka\odczytany-ciag-znakow-aztec-2d.txt");

    if (DecodedTextFile != null)
    {
        MessageBox.Show(DecodedTextFile.ToString());
    }
}
```

Bartosz Wójcik
https://www.pelock.com | http://www.dekoderaztec.pl
