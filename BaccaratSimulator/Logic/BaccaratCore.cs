using BaccaratSimulator.Models;
using LogicCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaccaratSimulator.Logic
{
    public class BaccaratCore : Singleton.INode
    {
        private static readonly string[] Card_number = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        private static readonly string[] Card_shape = { "♥", "♠", "♣", "◆" };

        public List<TrumpDeckInfo> PrepareTrumpDecks(int decksCnt)
        {
            var decks = new List<TrumpDeckInfo>();
            for (int i = 0; i < decksCnt; i++)
            {
                var newDeck = new TrumpDeckInfo()
                {
                    Cards = new List<TrumpCardInfo>(),
                };

                foreach (var shape in BaccaratCore.Card_shape)
                {
                    foreach (var number in BaccaratCore.Card_number)
                    {
                        var newCard = new TrumpCardInfo
                        {
                            Shape = shape,
                            Number = number,
                            BaccaratNumber = ConvertToBaccaratNumber(number)
                        };

                        newDeck.Cards.Add(newCard);
                    }
                }

                decks.Add(newDeck);
            }

            return decks;
        }

        public Stack<TrumpCardInfo> ShuffleDecks(List<TrumpDeckInfo> trumpDecks)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            var trumpCards = trumpDecks.SelectMany(elem => elem.Cards).ToList();
            var shuffledCards = trumpCards.OrderBy(elem => rnd.Next()).ToArray();

            return new Stack<TrumpCardInfo>(shuffledCards);
        }

        public int ConvertToBaccaratNumber(string number)
        {
            int baccaratNum = 0;
            switch (number)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    {
                        bool ret = int.TryParse(number, out int parseNum);
                        if (ret)
                        {
                            baccaratNum = parseNum;
                        }
                        else
                        {
                            new Exception("파싱할 수 없는 숫자 입니다.");
                        }
                    }
                    break;

                case "10":
                case "J":
                case "Q":
                case "K":
                    baccaratNum = 0;
                    break;

                default:
                    new Exception("정의 되지 않은 문자 입니다.");
                    break;
            }

            return baccaratNum;
        }
    }
}