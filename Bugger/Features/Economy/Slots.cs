using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bugger.Features.Economy
{
    /// <summary>
    /// Slots looks something similar like this:
    /// 🍇🍓🍇
    /// 🍔🍒🍍 
    /// 💯🍓🍍
    /// A Slot machine consists of 3 Cylinders that individually can rotate vertically
    /// Each Cylinder has n SlotPieces (Emojis + some extra information)
    /// </summary>
    public class Slot
    {
        // This is really something that shouldn't be hardcoded :D but oh well... works for now :P has to be tweaked for balance of win/loss ration tho
        public static readonly List<SlotPiece> PossibleSlotPieces = new List<SlotPiece>
        {
             new SlotPiece(":100:",         1, 1, 50  ),
             new SlotPiece(":candy:",       1, 2, 10  ),
             new SlotPiece(":strawberry:",  2, 2, 5   ),
             new SlotPiece(":pineapple: ",  3, 2, 3   ),
             new SlotPiece(":grapes:",      3, 2, 1   ),
             new SlotPiece(":cherries:",    3, 2, 0.5 )
        };
        public readonly List<Cylinder> Cylinders = new List<Cylinder>();

        // Will be the sum of the spawnRates of all possible SlotPieces
        private static int maxRandom;

        // The amount of pieces per cylinder is adjustable but will always be at least the sum of the minSpawnCount of all possible SlotPieces
        public Slot() : this(0) { }

        public Slot(int amountOfPieces)
        {
            maxRandom = 0;
            foreach (var piece in PossibleSlotPieces)
            {
                maxRandom += piece.spawnrate;
            }
            Cylinders.Add(new Cylinder(amountOfPieces));
            Cylinders.Add(new Cylinder(amountOfPieces));
            Cylinders.Add(new Cylinder(amountOfPieces));
        }

        public class Cylinder {
            public List<SlotPiece> SlotPieces = new List<SlotPiece>();
            // We are not really spinning anything - we just move a pointer and have everything offsetted by it
            public int Pointer = 0;
            public Cylinder(int size)
            {
                // Add all the pieces minSpawnCount times
                foreach (var piece in PossibleSlotPieces)
                {
                    for (int i = piece.minSpawnCount - 1; i >= 0; i--)
                    {
                        SlotPieces.Add(piece);
                        size--;
                    }
                }
                // If more pieces are requested, pick a random pice weighted by their spawnrate and add it
                for (int i = size; i > 0; i--)
                {
                    int rand = Global.Rng.Next(maxRandom);
                    foreach (var piece in Slot.PossibleSlotPieces)
                    {
                        rand -= piece.spawnrate;
                        if (rand <= 0)
                        {
                            SlotPieces.Add(piece);
                            break;
                        }
                    }
                }
                // Shuffle the pieces
                SlotPieces = SlotPieces.OrderBy((item) => Global.Rng.Next()).ToList<SlotPiece>();
            }
        }

        public class SlotPiece {
            public string emoji;
            public int minSpawnCount;
            public int spawnrate;
            public double payout;

            public SlotPiece(string emoji, int minSpawnCount, int spawnrate, double payout)
            {
                this.emoji = emoji;
                this.minSpawnCount = minSpawnCount;
                this.spawnrate = spawnrate;
                this.payout = payout;
            }
        };

        // Returns the amount of Miunies you win with the current pointers of the Cylinders if you bet <amount> of Miunies
        // And a flavour string depending on how much you lose/win
        public Tuple<uint, string> GetPayoutAndFlavourText(uint amount)
        {
            double payoutModifier = 0;

            /*
             * Emoji coordinates (row, column):
             *  0, 0 | 0, 1 | 0, 2
             *  1, 0 | 1, 1 | 1, 2
             *  2, 0 | 2, 1 | 2, 2
             */

            for (int i = 0; i < 3; i++)
            {
                // Check columns
                payoutModifier += CheckPayoutForCoordinates(0, i, 1, i, 2, i);
                // Check rows
                payoutModifier += CheckPayoutForCoordinates(i, 0, i, 1, i, 2);
            }
            // Diagonal top left to bottom right
            payoutModifier += CheckPayoutForCoordinates(0, 0, 1, 1, 2, 2);
            // Diagonal bottom left to top right
            payoutModifier += CheckPayoutForCoordinates(2, 0, 1, 1, 0, 2);

            uint moneyGain = (uint) (amount * payoutModifier);
            string flavourText = "Zagrałeś iii...\n";
            if (moneyGain > amount)
                flavourText += $"**wygrałeś {moneyGain} PSZ** :scream:!";
            else if (moneyGain == amount)
                flavourText += "szczęście Ci zwróciło szczęście!";
            else if (moneyGain > 0)
                flavourText += $"wygrałeś mniej niż dałeś... czyli **{moneyGain} PSZ!**";
            else
                flavourText += "straciłeś **wszystko!**";

            return Tuple.Create(moneyGain, flavourText);
        }

        // Check if the given set of three coordinates if all are the same emoji - if so return the payout ratio of that emoji
        private double CheckPayoutForCoordinates(int aRow, int aColumn, int bRow, int bColumn, int cRow, int cColumn)
        {
            int count = Cylinders[0].SlotPieces.Count;
            var first = Cylinders[aColumn].SlotPieces[(Cylinders[aColumn].Pointer + aRow) % count];
            var second = Cylinders[bColumn].SlotPieces[(Cylinders[bColumn].Pointer + bRow) % count];
            var third = Cylinders[cColumn].SlotPieces[(Cylinders[cColumn].Pointer + cRow) % count];
            if (first.emoji == second.emoji && second.emoji == third.emoji)
                return first.payout;
            return 0;
        }
                
        /// <summary>
        /// Returns a List containing 3 (emoji) strings to show Pieces of the slot machine
        /// </summary>
        /// <param name="showAll">If true the list will contain all Pieces if not only the 9 to display for playing</param>
        /// <returns></returns>
        public List<string> GetCylinderEmojis(bool showAll = false)
        {
            List<string> response = new List<string>();
            int piceCount = Cylinders[0].SlotPieces.Count;
            int loopMax = showAll ? piceCount : 3;
            var cylinderString = new StringBuilder(36); //36=3*":strawberry:".Length

            for (int j = 0; j < loopMax; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    cylinderString.Append(Cylinders[i].SlotPieces[(Cylinders[i].Pointer + j) % piceCount].emoji);
                }

                response.Add(cylinderString.ToString());
                cylinderString.Clear();
            }
            return response;
        }

        public string Spin()
        {
            int count = Cylinders[0].SlotPieces.Count;
            Cylinders[0].Pointer = Global.Rng.Next(count);
            Cylinders[1].Pointer = Global.Rng.Next(count);
            Cylinders[2].Pointer = Global.Rng.Next(count);
            return String.Join("\n", GetCylinderEmojis());
        }
    }
}
