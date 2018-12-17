using System;
using System.Collections.Generic;

namespace Bugger
{
    public static class Constants
    {
        internal static readonly string ResourceFolder = "resources";
        internal static readonly string UserAccountsFolder = "users";
        internal static readonly string ServerAccountsFolder = "servers";
        internal static readonly string LogFolder = "logs";
        internal static readonly string InvisibleString = "\u200b";
        public const ulong DailyMuiniesGain = 250;
        public const int MessageRewardCooldown = 30;
        public const int MessageRewardMinLenght = 20;
        public const int MaxMessageLength = 2000;
        public static readonly Tuple<int, int> MessagRewardMinMax = Tuple.Create(1, 5);
        public static readonly int MinTimerIntervall = 3000;
        public const int MaxCommandHistoryCapacity = 5;
        public static readonly IList<string> RandomBotInfo = new List<string> {
            "Chcesz ze mną sam na sam? Wbijaj na mojego Gita!",
            "Uwielbiam kiedy programista mnie kompiluje",
            "Dostajesz parę punktów szczęścia za każdą wiadomość na serwerze. (z małym cooldownem)",
            "Każda komenda ma krótsze i prostrze zamienniki",
            "Szczęśliwy, ale nieszczęśliwy? Wymień swoje punkty szczęścia na nagrody w skelpie!",
            "Problem z matmą? Polecenie <p>math Ci się spodoba!",
            "Szukasz czegoś nowego w życiu? Twórz własne komendy używając <p>t d !!!",
            "Mama wbiła na kompa? FBI już zaczyna rzucać smołki? A może po prostu chcesz zapomnieć? Użyj <p>fbi a zatroszczę się o twoją prywatność!",
            "Możesz ustawić weryfikację Twoich nowych członków, a następnie przydzielenie im roli poleceniem <p>ar ! "

        }.AsReadOnly();
        public static readonly IList<string> RandomSaying= new List<string> {

            "Zawsze pamiętaj: jesteś wyjątkowy, jak każdy",
            "Noś krótkie rękawy. Wspieraj swoje prawo do gołych ramion!",
            "Kiedy wszystko idzie po twojej myśli, jesteś na niewłaściewj drodze.",
            "Wstąp do wojska. Odwiedź egzotyczne miejsca, poznaj dziwnych ludzi, a następnie zabij ich.",
            "Śmierć jest dziedziczna",
            "Wyluzuj, najgorsze w życiu dopiero przed Tobą",
            "Przestałem walczyć z moimi wewnętrznymi demonami, teraz walczymy ramię w ramię",
            "Ten kto się śmieje ostatni... nie czai",
            "Żyjemy w czasach, w których pizza przyjeżdża przed policją",
            "Połowa ludzi na świecie jest poniżej średniej",
            "Czyste sumienie jest oznaką złej pamięci."
        }.AsReadOnly();
       
    }
}
