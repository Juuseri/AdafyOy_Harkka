using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Harjoitus1
{
    public partial class HarjoitusForm : System.Web.UI.Page
    {
        //tähän listaan ladataan kaikki ottelut .json tiedostosta
        private List<Matsi> matches;
        //tähän listaan tallennetaan kaikki hakutuloksesta tulleet ottelut
        private List<Matsi> haetutMatches;

        /// <summary>
        /// Sivun ladattua, luetaan myös suoraan kaikki ottelut listaan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //haetaan .json tiedosto
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            String path = HttpContext.Current.Server.MapPath("~/Content/matches.json");

            using (StreamReader r = new StreamReader(path))
            {

                //luetaan tiedosto ja Deserialisoidaan se Matsi-luokka listaan.
                string json = r.ReadToEnd();
                matches = JsonConvert.DeserializeObject<List<Matsi>>(json);

                //Listataan ottelut sivulle
                ListaaMatsit();
            }

        }

        /// <summary>
        /// Haku-painikkeen funktio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HaeButton_Click(object sender, EventArgs e)
        {
            //koetetaan muuntaa päivämääräsyötteet DateTimeksi, jos se epäonnistuu
            //syöte on virheellinen ja ilmoitetaan siitä käyttäjälle
            try
            {
                DateTime ensimmainen;
                DateTime toinen;

                //Oletetaan, jos ensimmäinen hakukenttä on tyhjä, hakija hakee kaikkia
                //otteluita, eikä takarajalla ole merkitystä, joten alustetaan päivä "nollaksi"
                if (EkaPaiva.Text == "")
                    ensimmainen = new DateTime();
                else
                    ensimmainen = Convert.ToDateTime(EkaPaiva.Text);

                //Oletetaan, jos toinen hakukenttä on tyhjä, hakija hakee otteluita
                //tähän päivään saakka, joten alustetaan päivä täksi päiväksi
                if (TokaPaiva.Text == "")
                    toinen = DateTime.Today;
                else
                    toinen = Convert.ToDateTime(TokaPaiva.Text);

                //Jos selvisimme virheettä, tyhjennetään Varoitus-Label, jos siellä oli
                //ennestään virheitä, alustetaan haetut Matsit-lista, ja suoritetaan
                //otteluiden haku
                Varoitus.Text = "";
                haetutMatches = new List<Matsi>();
                haetutMatches.Clear();

                //lisätään vielä eturaja-päivämäärään 23 tuntia, sillä päivämäärä pp.kk.vvvv muodossa
                //lisää kellonajaksi aamu kahdentoista, näin ollen sen päivän pelejä ei funktio osaa näyttää, koska esim.
                //Ottelu X on pelattu 15.5.2015 klo 15.00. Jos eturajana on 15.5.2015 (ja automaattisesti siis klo 00.00), kyseinen
                //ottelu jää pois hausta. Tästä syystä koko päivä on syytä olla päivässä, eikä vain aamu kahtatoista
                DateTime uusiToinen = toinen.AddHours(23);
                ListaaHaku(ensimmainen, uusiToinen, Joukkue.Text);
            }
            catch (Exception ex)
            {
                Varoitus.Text = "Väärä päivämäärä muoto! käytä muotoa: PP.KK.VVVV\n";
            }
        }

        /// <summary>
        /// Täällä ajetaan otteluiden haku, parametreina tulleiden DateTime arvojen ja joukkueen nimen perusteella
        /// </summary>
        /// <param name="eka">Päivämäärän takaraja</param>
        /// <param name="toka">Päivämäärän Eturaja</param>
        protected void ListaaHaku(DateTime eka, DateTime toka, string joukkueNimi)
        {
            //Käydään kaikki ottelut läpi, ja jos ottelun päivämäärä sijoittuu halutulle välille
            //lisätään se haettujen otteluiden listalle
            foreach (Matsi match in matches)
            {
                //jos joukkue nimi on tyhjä, oletetaan että kaikki joukkueet käyvät
                if (joukkueNimi == "")
                {
                    if (match.matchDateTime >= eka && match.matchDateTime <= toka)
                        haetutMatches.Add(match);
                }
                //jos nimi ei ole tyhjä, lisätään se ehtoon, että joko koti tai vierasjoukkueen nimi on oltava se
                else
                {
                    if (match.matchDateTime >= eka && match.matchDateTime <= toka && (match.awayTeam["FullName"].Equals(joukkueNimi) || match.homeTeam["FullName"].Equals(joukkueNimi)))
                        haetutMatches.Add(match);
                }
            }

            //Tällä funktiolla tulostetaan kaikki haetut ottelut divin sisälle
            TulostaLista(haetutMatches);
        }

        /// <summary>
        /// Täällä listataan kaikki ottelut .json tiedostosta
        /// </summary>
        protected void ListaaMatsit()
        {
            //muutetaan päivämäärä DateTime-muotoon
            foreach (Matsi match in matches)
            {
                match.ChangeDate();
            }

            //käännetään lista vielä toisinpäin, jotta tuoreimmat ottelut ovat ylimpänä
            matches.Reverse();

            //Tällä funktiolla tulostetaan kaikki ottelut divin sisälle
            TulostaLista(matches);
        }

        /// <summary>
        /// Tulostaa otteluita <div>:in sisälle InnerHtml-metodilla
        /// Lista on tyyliä
        /// + HomeTeam vs AwayTeam
        ///  - HomeScore - AwayScore
        ///  - Date
        /// </summary>
        /// <param name="otteluLista">Ottelulista, joka tulostetaan</param>
        protected void TulostaLista(List<Matsi> otteluLista)
        {
            //Seuraavaksi lisätään diviin lista otteluista, samalla tapaa kuin sivua ladatessakin kaikille otteluille
            TulosDiv.InnerHtml = "\n<ul>\n";
            foreach (Matsi match in otteluLista)
            {
                //Ensimmäinen lista-objekti on joukkueiden nimet
                TulosDiv.InnerHtml += " <li " + "id=\"" + match.id.ToString() + "\"" + " >" + match.homeTeam["FullName"] + " vs " + match.awayTeam["FullName"] + "\n";
                TulosDiv.InnerHtml += "  <ul>\n";
                //Seuraavana tulee nested-lista, jossa on tulos ja päivämäärä
                TulosDiv.InnerHtml += "   <li>Tulos: " + match.homeGoals.ToString() + " - " + match.awayGoals.ToString() + "</li>\n";

                string paiva = match.matchDateTime.Day + "." + match.matchDateTime.Month + "." + match.matchDateTime.Year;

                //jos kello on tasan, se näyttää ilmoittavan minuutin yhdellä nollalla, joten tässä tapauksessa
                //tehdään pieni ehto, että jos se on 0, kirjoitetaan se kahdella nollalla, muuten normaalisti
                string minuutti;
                if (match.matchDateTime.Minute == 0)
                    minuutti = "00";
                else
                    minuutti = match.matchDateTime.Minute.ToString();

                string kello = match.matchDateTime.Hour + ":" + minuutti;

                //lisätään päivämäärä
                TulosDiv.InnerHtml += "   <li>Päivämäärä: " + paiva + " Klo: " + kello + "</li>\n";

                //Ottelun listaus valmis, suljetaan lista-objektit
                TulosDiv.InnerHtml += "  </ul>\n";
                TulosDiv.InnerHtml += " </li>\n";
            }
            //Suljetaan valmis lista
            TulosDiv.InnerHtml += "</ul>\n";
        }
    }

    /// <summary>
    /// Oma luokka MatchEventille. Voidaan käyttää vielä syvällisempään otteluiden tutkimiseen,
    /// esimerkiksi yksittäisen ottelun tiedoista voi listata kaikki tapahtumat
    /// </summary>
    public class MatsiEvents
    {
        [JsonProperty("Id")]
        public int id { get; set; }

        [JsonProperty("MatchId")]
        public int matchId { get; set; }

        [JsonProperty("EventMinute")]
        public int eventMinute { get; set; }

        [JsonProperty("ElapsedSeconds")]
        public int elapsedSeconds { get; set; }

        [JsonProperty("TeamId")]
        public int teamId { get; set; }

        [JsonProperty("Description")]
        public string description { get; set; }

        [JsonProperty("FullDescription")]
        public string fullDescription { get; set; }

        [JsonProperty("EventTypeIcon")]
        public string eventTypeIcon { get; set; }

        [JsonProperty("EventType")]
        public string eventType { get; set; }

        [JsonProperty("EventTymeEnum")]
        public int eventTypeEnum { get; set; }

        [JsonProperty("PlayerId")]
        public int PlayerId { get; set; }

        [JsonProperty("Player")]
        public string player { get; set; }

        [JsonProperty("Identifier")]
        public string identifier { get; set; }

        [JsonProperty("AssistPlayers")]
        public string assistPlayers { get; set; }

        [JsonProperty("AssistPlayerNames")]
        public string assisPlayerNames { get; set; }

        [JsonProperty("Modifier")]
        public string modifier { get; set; }

        [JsonProperty("Score")]
        public string score { get; set; }

        [JsonProperty("IsGoal")]
        public bool isGoal { get; set; }
    }

    /// <summary>
    /// Luokka Otteluille
    /// </summary>
    public class Matsi
    {
        [JsonProperty("Id")]
        public int id { get; set; }

        [JsonProperty("Round")]
        public object round { get; set; }

        [JsonProperty("RoundNumber")]
        public int roundNumber { get; set; }

        [JsonProperty("MatchDate")]
        public string matchDate { get; set; }

        public DateTime matchDateTime;

        [JsonProperty("HomeTeam")]
        public Dictionary<string, Object> homeTeam { get; set; }

        [JsonProperty("AwayTeam")]
        public Dictionary<string, Object> awayTeam { get; set; }

        [JsonProperty("HomeGoals")]
        public int homeGoals { get; set; }

        [JsonProperty("AwayGoals")]
        public int awayGoals { get; set; }

        [JsonProperty("Status")]
        public int status { get; set; }

        [JsonProperty("PlayedMinutes")]
        public int playedMinutes { get; set; }

        [JsonProperty("SecondHalfStarted")]
        public object secondHalfStarted { get; set; }

        [JsonProperty("GameStarted")]
        public string gameStarted { get; set; }

        [JsonProperty("MatchEvents")]
        public MatsiEvents[] matchEvents { get; set; }

        [JsonProperty("PeriodResults")]
        public Object[] periodResults { get; set; }

        [JsonProperty("OnlyResultAvailable")]
        public bool onlyResultAvailable { get; set; }

        [JsonProperty("Season")]
        public int season { get; set; }

        [JsonProperty("Country")]
        public string country { get; set; }

        [JsonProperty("League")]
        public string league { get; set; }

        /// <summary>
        /// Muutetaan aika DateTime:ksi, jotta meidän on helpompi vertailla aikoja
        /// </summary>
        public void ChangeDate()
        {
            //haetaan matchdatesta arvo, ja katkaistaan se ennen T:tä, jonka jälkeen splitataan se "-" välein
            //jolloin saadaan taulukkoon vuosi, kuukausi ja päivä
            string[] dateString = Regex.Split((Regex.Match(this.matchDate, "(?:(?!T).)*").Value), "-");

            //Sama homma päivälle, jouduin vain hiukan eri kautta menemään, sillä tarvittu arvo on
            //matchin indeksissä 1 (perus .value palauttaa indeksissä 0 olevan arvon). Tämän jälkeen
            //aika splitataan ":" kohdalta jolloin saadaan tunti, minuutti ja sekunti
            Match match = Regex.Match(this.matchDate, "T(.*?)Z");
            string[] timeString = Regex.Split(match.Groups[1].Value, ":");

            //muutetaan arvo uuteen päivämäärän
            this.matchDateTime = new DateTime(Int32.Parse(dateString[0]), Int32.Parse(dateString[1]), Int32.Parse(dateString[2]), Int32.Parse(timeString[0]), Int32.Parse(timeString[1]), Int32.Parse(timeString[2]));
        }
    }
}