using Bugger.Extensions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Bugger.Preconditions;

namespace Bugger.Modules
{
    public class GeneratorPlików : ModuleBase<MiunieCommandContext>
    {
        private readonly CommandService _service;

        public GeneratorPlików(CommandService service)
        {
            _service = service;
        }

        [Command("WszystkieKomendy"), Alias("WK", "AllCommands"), Remarks("Wypiszę Ci wszystkie moje dostępne dla Ciebie komendy :writing_hand::scroll::envelope_with_arrow:")]
        [Cooldown(15)]
        public async Task CommandInfo()
        {
            var builder = new StringBuilder();
            builder.Append($"Pomoc\n---\nOto wszystkie komendy, do których użycia masz uprawnienia na tym serwerze.\n\nKategorie:\n\n");
            var moduleBuilder = new StringBuilder();
            foreach (var module in _service.Modules)
            {
                await AddModuleString(module, moduleBuilder).ConfigureAwait(false);
                builder.Append($"[{module.Name}](#{module.Name.ToLower().Replace(' ', '-')})\n");
            }
            builder.Append($"\n\n{moduleBuilder}");

            if (Context.User.Id == 356147668231258113)
            {
                File.WriteAllText(@"/Pulpit/Komendy.md", builder.ToString());
                await ReplyAsync("Masz na pulpicie! (Komendy.md)");
            }
            else
            {
                File.WriteAllText("komendy_bugger.md", builder.ToString());
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendFileAsync("komendy_bugger.md");
                await ReplyAsync("Wysłałem **DM**esa!");
            }
        }
        [Command("LosowySkrypt"), Alias("LS", "Losowy Skrypt", "RS", "Random Script", "Random Script"), Remarks("Wygeneruję jakiś fajny, losowy, bezpieczy skrypt dla Cb!")]
        [Cooldown(5)]
        public async Task RandomScript()
        {
            string[] scripts = new string[]
            {
                "materials/scripts/auto_typing.vbs",
                "materials/scripts/custom_shutdown.bat",
                "materials/scripts/guessing_game.bat",
                "materials/scripts/ha_ha.vbs",
                "materials/scripts/iwona.vbs",
                "materials/scripts/keyboard_lights.vbs",
                "materials/scripts/password_generator.bat",
                "materials/scripts/private_folder.bat",


            };

            Random r;
            r = new Random();
            int scriptsnumber = r.Next(scripts.Length);
            string script = scripts[scriptsnumber];

            await Context.Channel.SendMessageAsync("Proszę, tu masz fajny, losowy, niegroźny skrypt:");
            await Context.Channel.SendFileAsync(script);
            await Context.Channel.SendMessageAsync("Jeśli używasz tej funkcji pierwszy raz lub nie znasz się na plikach BATCH oraz języku VBScript sugeruję pobrać jeszczę pliki zamyjające te skrypty _JustInCase_  (komenda: `safety`)");
            
        }
        [Command("LosowyWirus"), Alias("LV", "LW", "Losowy Wirus", "Losowy Virus", "RandomVirus", "Random Virus"), Remarks("Wygeneruję wirusa dla Cb! [Użycie na własną odpowiedzialność]")]
        [Cooldown(5)]
        public async Task RandomVirus()
        {
            string[] viruses = new string[]
            {
                "materials/viruses/capslock.vbs",
                "materials/viruses/disc_spammer.txt",
                "materials/viruses/fake_bluescreen.bat",
                "materials/viruses/fast_formatter.txt",
                "materials/viruses/internet_destroyer.txt",
                "materials/viruses/internet_off.bat",
                "materials/viruses/mouse_swapper.txt",
                "materials/viruses/regestry_deleater.txt",
                "materials/viruses/regestry_key_deleater.txt",
                "materials/viruses/shutdowner.bat",
                "materials/viruses/stalker.vbs",
                "materials/viruses/system32_destroyer.txt",
                "materials/viruses/updater.bat"
            };

            Random r;
            r = new Random();
            int virusnumber = r.Next(viruses.Length);
            string virus = viruses[virusnumber];

            await Context.Channel.SendMessageAsync("UWAGA, NIEKTÓRE Z TYCH SKRYPTÓW MOGĄ PRZYNIEŚĆ POWAŻNE USZKODZENIA URZĄDZEŃ WCHODZĄCYCH W SKŁAD KOMPUTERA ORAZ SYSTEMEU I Z KONSEKWENCJAMI," +
                " KTÓRYCH ZWYKŁA REINSTALACJA SYSTEMU NIE NAPRAWI!!!\n" +
                "Dlatego dla bezpieczeństwa niebezpieczne pliki zostały zapisane w formie tekstowej (.txt)\n" +
                "ADz Tim nie ponosi odpowiedzialności za wszeklie konsekwencje związane z używaniem tych skryptów.\n" +
                "Oto wirus:");
            await Context.Channel.SendFileAsync(virus);
            await Context.Channel.SendMessageAsync("Zalecamy utrzymanie formatu .txt dla bezpieczeństwa, używania skryptów ich na maszynach wirtualnych ruchem tylko plików do wewnątrz" +
                " oraz zapoznanie się z komendą `safety` przed rozpoczęciem, a już przede wszystkim masz pamiętać, aby się dobrze bawić!");


        }
        [Command("Safety"), Alias("Bezpieczeństwo", "Pomocnicze", "Zabespieczenie")]
        [Remarks("Wyślę instrukcje i pomoce używania powyższych skrypcików!")]
        [Cooldown(5)]
        public async Task Safety()
        {
            await Context.Channel.SendMessageAsync("Jeśli chcesz zobaczyć kod skryptu przed jego otwarciem musisz (zamiast klinkąć na nim dwukrotnie) kliknąć prawym przyciskiem i Edytuj.");
            await Context.Channel.SendMessageAsync("Skrypt powstrzymujący zamykanie systemu: (można to również zrobić poprzez otworzenie wiersza poleceń i użycie komendy `shutdown -a`)");
            await Context.Channel.SendFileAsync("materials/scripts/anti-shutdown.bat");
            await Context.Channel.SendMessageAsync("Skrypt zamykający skrypty VBS: (można to również zrobić poprzez wejście w menadżer zadań==>Szczegóły==>wybranie procesu `wscript.exe`==>prawy przycisk myszy==>Zakończ zadanie==>Zakończ zadanie)");
            await Context.Channel.SendFileAsync("materials/scripts/vbs_killer.bat");
        }
        

        private async Task AddModuleString(ModuleInfo module, StringBuilder builder)
        {
            if (module is null) { return; }
            var descriptionBuilder = new List<string>();
            var duplicateChecker = new List<string>();
            foreach (var cmd in module.Commands)
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess || duplicateChecker.Contains(cmd.Aliases.First())) { continue; }
                duplicateChecker.Add(cmd.Aliases.First());

                var cmdDescription = $"[ {cmd.Aliases.First()} ] {cmd.Remarks}";
                descriptionBuilder.Add($"{cmdDescription.Replace("\n", "")}");
            }
            var builtString = string.Join("\n", descriptionBuilder);

            var moduleNotes = $"{module.Summary}";
            if (!string.IsNullOrEmpty(module.Summary) && !string.IsNullOrEmpty(module.Remarks))
            {
                moduleNotes += " | ";
            }
            moduleNotes += $"{module.Remarks}";

            if (!string.IsNullOrEmpty(moduleNotes))
            {
                moduleNotes += "";
            }
            builder.Append($"### {module.Name}\n\n{moduleNotes}\n{builtString}\n\n\n");
        }
    }
}
