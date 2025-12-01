using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Terminal
{
    public partial class MainPage : ContentPage
    {
        private List<Produkt> produkty = new List<Produkt>();

        public MainPage()
        {
            InitializeComponent();
        }

        public class Produkt
        {
            public string Nazwa { get; set; }
            public decimal Cena { get; set; }
            public string Kategoria { get; set; }

            public override string ToString()
            {
                return $"{Nazwa} - {Cena} zł ({Kategoria})";
            }
        }

        private async void BtnDodaj_Clicked(object sender, EventArgs e)
        {
            string nazwa = await DisplayPromptAsync("Dodaj produkt", "Podaj nazwę produktu:");
            if (string.IsNullOrWhiteSpace(nazwa))
            {
                await DisplayAlert("Błąd", "Nazwa nie może być pusta.", "OK");
                return;
            }

            string cenaInput = await DisplayPromptAsync("Dodaj produkt", "Podaj cenę produktu (zł):");
            if (!decimal.TryParse(cenaInput, out decimal cena) || cena < 0)
            {
                await DisplayAlert("Błąd", "Podano niepoprawną cenę.", "OK");
                return;
            }

            string kategoria = await DisplayPromptAsync("Dodaj produkt", "Podaj kategorię produktu:");
            if (string.IsNullOrWhiteSpace(kategoria))
            {
                await DisplayAlert("Błąd", "Kategoria nie może być pusta.", "OK");
                return;
            }

            var produkt = new Produkt { Nazwa = nazwa, Cena = cena, Kategoria = kategoria };
            produkty.Add(produkt);

            await DisplayAlert("Sukces", $"Dodano: {produkt}", "OK");
        }


        private async void BtnUsun_Clicked(object sender, EventArgs e)
        {
            if (produkty.Count == 0)
            {
                await DisplayAlert("Usuwanie produktu", "Brak produktów na liście.", "OK");
                return;
            }

            string nazwa = await DisplayPromptAsync("Usuń produkt", "Podaj nazwę produktu do usunięcia:");
            if (string.IsNullOrWhiteSpace(nazwa))
            {
                await DisplayAlert("Błąd", "Nazwa nie może być pusta.", "OK");
                return;
            }

            Produkt produkt = produkty.FirstOrDefault(p =>
                p.Nazwa.Equals(nazwa, StringComparison.OrdinalIgnoreCase));

            if (produkt == null)
            {
                await DisplayAlert("Błąd", $"Nie znaleziono produktu o nazwie: {nazwa}", "OK");
                return;
            }

            produkty.Remove(produkt);
            await DisplayAlert("Sukces", $"Usunięto produkt: {produkt.Nazwa}", "OK");
        }


        private async void BtnEdytuj_Clicked(object sender, EventArgs e)
        {
            if (produkty.Count == 0)
            {
                await DisplayAlert("Edycja produktu", "Brak produktów na liście.", "OK");
                return;
            }

            string[] nazwyProduktow = produkty.Select(p => p.Nazwa).ToArray();
            string wybranyProduktNazwa = await DisplayActionSheet(
                "Wybierz produkt do edycji:",
                "Anuluj",
                null,
                nazwyProduktow
            );

            if (string.IsNullOrWhiteSpace(wybranyProduktNazwa) || wybranyProduktNazwa == "Anuluj")
                return;

            Produkt produkt = produkty.FirstOrDefault(p =>
                p.Nazwa.Equals(wybranyProduktNazwa, StringComparison.OrdinalIgnoreCase));

            if (produkt == null)
            {
                await DisplayAlert("Błąd", "Nie znaleziono produktu.", "OK");
                return;
            }

            string nowaNazwa = await DisplayPromptAsync("Edycja produktu", "Nowa nazwa:", initialValue: produkt.Nazwa);
            if (string.IsNullOrWhiteSpace(nowaNazwa))
            {
                await DisplayAlert("Błąd", "Nazwa nie może być pusta.", "OK");
                return;
            }

            string nowaCenaInput = await DisplayPromptAsync("Edycja produktu", "Nowa cena (zł):", initialValue: produkt.Cena.ToString());
            if (!decimal.TryParse(nowaCenaInput, out decimal nowaCena) || nowaCena < 0)
            {
                await DisplayAlert("Błąd", "Podano błędną cenę.", "OK");
                return;
            }

            string nowaKategoria = await DisplayPromptAsync("Edycja produktu", "Nowa kategoria:", initialValue: produkt.Kategoria);
            if (string.IsNullOrWhiteSpace(nowaKategoria))
            {
                await DisplayAlert("Błąd", "Kategoria nie może być pusta.", "OK");
                return;
            }

            produkt.Nazwa = nowaNazwa;
            produkt.Cena = nowaCena;
            produkt.Kategoria = nowaKategoria;

            await DisplayAlert("Sukces", $"Zmieniono produkt:\n{produkt}", "OK");
        }


        private async void BtnPokaz_Clicked(object sender, EventArgs e)
        {
            if (produkty.Count == 0)
            {
                await DisplayAlert("Lista produktów", "Brak produktów na liście.", "OK");
                return;
            }

            string lista = string.Join("\n", produkty.Select(p => $"{p.Nazwa} - {p.Cena} zł ({p.Kategoria})"));
            await DisplayAlert("Lista produktów", lista, "OK");
        }


        private async void BtnPlik_Clicked(object sender, EventArgs e)
        {
            if (produkty.Count == 0)
            {
                await DisplayAlert("CSV", "Brak produktów do zapisania.", "OK");
                return;
            }

            var lines = new List<string> { "Nazwa,Cena,Kategoria" };
            lines.AddRange(produkty.Select(p => $"{p.Nazwa},{p.Cena},{p.Kategoria}"));

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = Path.Combine(folder, "produkty.csv");

            File.WriteAllLines(fileName, lines);

            await DisplayAlert("CSV", $"Zapisano: {fileName}", "OK");
        }


        // =====================================
        //      WCZYTYWANIE CSV (NOWE)
        // =====================================

        private async void BtnOdczytCsv_Clicked(object sender, EventArgs e)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = Path.Combine(folder, "produkty.csv");

            if (!File.Exists(fileName))
            {
                await DisplayAlert("CSV", "Plik produkty.csv nie istnieje.", "OK");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(fileName);

                produkty.Clear();

                foreach (var line in lines.Skip(1)) // pomija nagłówek
                {
                    var parts = line.Split(',');

                    if (parts.Length == 3 && decimal.TryParse(parts[1], out decimal cena))
                    {
                        produkty.Add(new Produkt
                        {
                            Nazwa = parts[0],
                            Cena = cena,
                            Kategoria = parts[2]
                        });
                    }
                }

                await DisplayAlert("CSV", "Wczytano produkty z pliku.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", ex.Message, "OK");
            }
        }



        private void BtnLite_Clicked(object sender, EventArgs e)
        {
        }

        private void BtnExit_Clicked(object sender, EventArgs e)
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif IOS
            Application.Current?.Quit();
#elif WINDOWS
            Application.Current?.Quit();
#else
            Application.Current?.Quit();
#endif
        }
    }
}
