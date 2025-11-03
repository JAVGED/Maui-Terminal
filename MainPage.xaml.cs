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

            // Dodanie do listy
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

            Produkt produkt = null;

            foreach (var p in produkty)
            {
                if (p.Nazwa.Equals(nazwa, StringComparison.OrdinalIgnoreCase))
                {
                    produkt = p;
                    break;
                    // kończymy szukanie po znalezieniu pierwszego pasującego
                }
            }


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

            // 1. Pokaz produkt
            string[] nazwyProduktow = produkty.Select(p => p.Nazwa).ToArray();
            string wybranyProduktNazwa = await DisplayActionSheet(
                "Wybierz produkt do edycji:",
                "Anuluj",
                null,
                nazwyProduktow
            );

            if (string.IsNullOrWhiteSpace(wybranyProduktNazwa) || wybranyProduktNazwa == "Anuluj")
                return;

            // 2. Co edytować
            Produkt produkt = null;

            foreach (var p in produkty)
            {
                if (p.Nazwa.Equals(wybranyProduktNazwa, StringComparison.OrdinalIgnoreCase))
                {
                    produkt = p;
                    break; // gdy znaleziono koniec
                }
            }

            if (produkt == null)
            {
                await DisplayAlert("Błąd", "Nie znaleziono wybranego produktu.", "OK");
                return;
            }

            // 3. Pyta o nowe dane
            string nowaNazwa = await DisplayPromptAsync("Edycja produktu", "Podaj nową nazwę produktu:", initialValue: produkt.Nazwa);
            if (string.IsNullOrWhiteSpace(nowaNazwa)) //initial bierze wczesniejsza wartosc i daje w pole
            {
                await DisplayAlert("Błąd", "Nazwa nie może być pusta.", "OK");
                return;
            }

            string nowaCenaInput = await DisplayPromptAsync("Edycja produktu", "Podaj nową cenę produktu (zł):", initialValue: produkt.Cena.ToString());
            if (!decimal.TryParse(nowaCenaInput, out decimal nowaCena) || nowaCena < 0) 
            {
                await DisplayAlert("Błąd", "Podano niepoprawną cenę.", "OK");
                return;
            }

            string nowaKategoria = await DisplayPromptAsync("Edycja produktu", "Podaj nową kategorię produktu:", initialValue: produkt.Kategoria);
            if (string.IsNullOrWhiteSpace(nowaKategoria))
            {
                await DisplayAlert("Błąd", "Kategoria nie może być pusta.", "OK");
                return;
            }

            // 4. Zaktualizuj dane
            produkt.Nazwa = nowaNazwa;
            produkt.Cena = nowaCena;
            produkt.Kategoria = nowaKategoria;

            await DisplayAlert("Sukces", $"Zmieniono dane produktu:\n{produkt}", "OK");
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
  


        private void BtnPlik_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnExit_Clicked(object sender, EventArgs e)
        {
#if ANDROID 
            // Zamknij aplikację na Androidzie
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif IOS
            // iOS nie pozwala na wymuszone zamknięcie, ale jeśli używasz .NET 8+, to:
            Application.Current?.Quit();
#elif WINDOWS
            // Windows / MacCatalyst
            Application.Current?.Quit();
#else
            Application.Current?.Quit();
#endif
        }
    }
}
