using BaccaratSimulator.Models;
using BaccaratSimulator.Models.Enums;
using LogicCore.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaccaratSimulator.Logic
{
    public class BaccaratPlay : Singleton.INode
    {
        private readonly int _defaultDecks; // 바카라 기본 덱 개수
        private readonly BaccaratCore _baccaratCore;

        public int GameSeq;
        public Stack<TrumpCardInfo> _currentCards;
        public List<List<BaccaratResultInfo>> PlayResults;

        public int RemainCardCnt => _currentCards?.Count ?? 0;

        public BaccaratPlay()
        {
            _defaultDecks = int.Parse(ConfigurationManager.AppSettings["default_decks"]);
            _baccaratCore = Singleton.Get<BaccaratCore>();
        }

        /// <summary>
        /// 새로운 게임을 위한 카드 준비
        /// </summary>
        public void InitializeGame()
        {
            GameSeq = 0;
            var decks = _baccaratCore.PrepareTrumpDecks(_defaultDecks);
            _currentCards = _baccaratCore.ShuffleDecks(decks);
            PlayResults = new List<List<BaccaratResultInfo>>();
        }

        public bool IsPlayable()
        {
            return RemainCardCnt > 24;
        }

        public TrumpCardInfo PopCard()
        {
            if (RemainCardCnt == 0)
                return null;

            return _currentCards.Pop();
        }

        public BaccaratResultInfo PlayNext()
        {
            if (!IsPlayable())
                return null;

            var playResult = new BaccaratResultInfo
            {
                GameSeq = ++GameSeq,
                PlayerCards = new List<TrumpCardInfo>(),
                BankerCards = new List<TrumpCardInfo>(),
            };

            // Dealing
            int cardCounting = 0;
            do
            {
                ++cardCounting;
                if (cardCounting % 2 == 1)
                    playResult.PlayerCards.Add(PopCard());
                else
                    playResult.BankerCards.Add(PopCard());
            } while (cardCounting < 4);

            if (playResult.PlayerValue < 8
                && playResult.BankerValue < 8)
            {
                DealingForPlayer(ref playResult, out TrumpCardInfo player_3rdCard);
                DealingForBanker(ref playResult, player_3rdCard);
            }

            // Decide
            DecideBaccaratResult(ref playResult);
            CheckBigRoad(ref playResult); // 원매
            CheckBigEyeBoy(ref playResult); // 중국점 1군
            CheckSmallRoad(ref playResult); // 중국점 2군
            CheckCockroachPig(ref playResult); // 중국점 3군

            return playResult;
        }

        public void DecideBaccaratResult(ref BaccaratResultInfo playResult)
        {
            if (playResult.PlayerValue > playResult.BankerValue)
            {
                playResult.ResultType = BaccaratResultType.Player;
            }
            else if (playResult.PlayerValue < playResult.BankerValue)
            {
                playResult.ResultType = BaccaratResultType.Banker;
            }
            else
            {
                playResult.ResultType = BaccaratResultType.Tie;
            }
        }

        /// <summary>
        /// 원매
        /// </summary>
        /// <param name="playResult"></param>
        public void CheckBigRoad(ref BaccaratResultInfo playResult)
        {
            playResult.Big_Road = playResult.ResultType == BaccaratResultType.Player ? PatternType.Negative
                    : playResult.ResultType == BaccaratResultType.Banker ? PatternType.Positive : PatternType.neutral;

            if (PlayResults.Count == 0)
            {
                var newColumn = new List<BaccaratResultInfo>();
                newColumn.Add(playResult);
                PlayResults.Add(newColumn);
                return;
            }

            var latestColumn = PlayResults[PlayResults.Count - 1];
            var latestBigRoadType = latestColumn.FirstOrDefault(elem => elem.Big_Road != PatternType.neutral)?.Big_Road;
            if (latestBigRoadType == null // 시작이 타이일 경우
                || playResult.Big_Road == PatternType.neutral
                || latestBigRoadType == playResult.Big_Road)
            {
                latestColumn.Add(playResult);
            }
            else // 새로운 줄 시작
            {
                var newColumn = new List<BaccaratResultInfo>();
                newColumn.Add(playResult);
                PlayResults.Add(newColumn);
            }
        }

        /// <summary>
        /// 중국점 1군
        /// </summary>
        /// <param name="playResult"></param>
        public void CheckBigEyeBoy(ref BaccaratResultInfo playResult)
        {
            if (PlayResults.Count < 2
                || playResult.Big_Road == PatternType.neutral)
            {
                playResult.Big_Eye_Boy = PatternType.neutral;
                return;
            }

            var latestColumn = PlayResults[PlayResults.Count - 1];
            var latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();
            if (latestColumnRowCnt == 1) // 새로운 줄 시작일 경우
            {
                if (PlayResults.Count == 2)
                {
                    playResult.Big_Eye_Boy = PatternType.neutral;
                    return;
                }

                // 비교 컬럼
                latestColumn = PlayResults[PlayResults.Count - 2];
                latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 3];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (latestColumnRowCnt == fiducialColumnRowCnt)
                    playResult.Big_Eye_Boy = PatternType.Positive;
                else
                    playResult.Big_Eye_Boy = PatternType.Negative;
            }
            else
            {
                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 2];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (fiducialColumnRowCnt >= latestColumnRowCnt)
                    playResult.Big_Eye_Boy = PatternType.Positive;
                else if ((latestColumnRowCnt - fiducialColumnRowCnt) > 1)
                    playResult.Big_Eye_Boy = PatternType.Positive;
                else
                    playResult.Big_Eye_Boy = PatternType.Negative;
            }
        }

        /// <summary>
        /// 중국점 2군
        /// </summary>
        /// <param name="playResult"></param>
        public void CheckSmallRoad(ref BaccaratResultInfo playResult)
        {
            if (PlayResults.Count < 3
                || playResult.Big_Road == PatternType.neutral)
            {
                playResult.Small_Road = PatternType.neutral;
                return;
            }

            var latestColumn = PlayResults[PlayResults.Count - 1];
            var latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();
            if (latestColumnRowCnt == 1) // 새로운 줄 시작일 경우
            {
                if (PlayResults.Count == 3)
                {
                    playResult.Small_Road = PatternType.neutral;
                    return;
                }

                // 비교 컬럼
                latestColumn = PlayResults[PlayResults.Count - 2];
                latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 4];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (latestColumnRowCnt == fiducialColumnRowCnt)
                    playResult.Small_Road = PatternType.Positive;
                else
                    playResult.Small_Road = PatternType.Negative;
            }
            else
            {
                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 3];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (fiducialColumnRowCnt >= latestColumnRowCnt)
                    playResult.Small_Road = PatternType.Positive;
                else if ((latestColumnRowCnt - fiducialColumnRowCnt) > 1)
                    playResult.Small_Road = PatternType.Positive;
                else
                    playResult.Small_Road = PatternType.Negative;
            }
        }

        /// <summary>
        /// 중국점 3군
        /// </summary>
        /// <param name="playResult"></param>
        public void CheckCockroachPig(ref BaccaratResultInfo playResult)
        {
            if (PlayResults.Count < 4
                || playResult.Big_Road == PatternType.neutral)
            {
                playResult.Cockroach_Pig = PatternType.neutral;
                return;
            }

            var latestColumn = PlayResults[PlayResults.Count - 1];
            var latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();
            if (latestColumnRowCnt == 1) // 새로운 줄 시작일 경우
            {
                if (PlayResults.Count == 4)
                {
                    playResult.Cockroach_Pig = PatternType.neutral;
                    return;
                }

                // 비교 컬럼
                latestColumn = PlayResults[PlayResults.Count - 2];
                latestColumnRowCnt = latestColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 5];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (latestColumnRowCnt == fiducialColumnRowCnt)
                    playResult.Cockroach_Pig = PatternType.Positive;
                else
                    playResult.Cockroach_Pig = PatternType.Negative;
            }
            else
            {
                // 기준 컬럼
                var fiducialColumn = PlayResults[PlayResults.Count - 4];
                var fiducialColumnRowCnt = fiducialColumn.Where(elem => elem.Big_Road != PatternType.neutral).Count();

                if (fiducialColumnRowCnt >= latestColumnRowCnt)
                    playResult.Cockroach_Pig = PatternType.Positive;
                else if ((latestColumnRowCnt - fiducialColumnRowCnt) > 1)
                    playResult.Cockroach_Pig = PatternType.Positive;
                else
                    playResult.Cockroach_Pig = PatternType.Negative;
            }
        }

        public void DealingForPlayer(ref BaccaratResultInfo playResult, out TrumpCardInfo thirdCards)
        {
            thirdCards = null;

            if (playResult.PlayerValue == 6
                || playResult.PlayerValue == 7)
                return;

            // 플레이어 패가 5이하라면 한장 더 받는다.
            thirdCards = PopCard();
            playResult.PlayerCards.Add(thirdCards);
        }

        public void DealingForBanker(ref BaccaratResultInfo playResult, TrumpCardInfo player_3rdCard)
        {
            // 뱅커 합이 0, 1, 2일 경우 무조건 한장 더 받는다.
            if (playResult.BankerValue < 3)
            {
                playResult.BankerCards.Add(PopCard());
                return;
            }

            if (playResult.BankerValue == 7
                || player_3rdCard == null)
                return;

            // 뱅커 합이 3일때
            else if (playResult.BankerValue == 3)
            {
                // 플레이어 세번째 카드가 8이 아니면 한장 더 받는다.
                if (player_3rdCard.BaccaratNumber != 8)
                {
                    playResult.BankerCards.Add(PopCard());
                }
            }
            // 뱅커 합이 4일때
            else if (playResult.BankerValue == 4)
            {
                // 플레이어 세번째 카드가 2, 3, 4, 5, 6, 7이면 한장 더 받는다.
                if (player_3rdCard.BaccaratNumber != 0
                    && player_3rdCard.BaccaratNumber != 1
                    && player_3rdCard.BaccaratNumber != 8
                    && player_3rdCard.BaccaratNumber != 9)
                {
                    playResult.BankerCards.Add(PopCard());
                }
            }
            // 뱅커 합이 5일때
            else if (playResult.BankerValue == 5)
            {
                // 플레이어 세번째 카드가 4, 5, 6, 7이면 한장 더 받는다.
                if (player_3rdCard.BaccaratNumber == 4
                    || player_3rdCard.BaccaratNumber == 5
                    || player_3rdCard.BaccaratNumber == 6
                    || player_3rdCard.BaccaratNumber == 7)
                {
                    playResult.BankerCards.Add(PopCard());
                }
            }
            // 뱅커 합이 6일때
            else if (playResult.BankerValue == 6)
            {
                // 플레이어 세번째 카드가 6, 7이면 한장 더 받는다.
                if (player_3rdCard.BaccaratNumber == 6
                    || player_3rdCard.BaccaratNumber == 7)
                {
                    playResult.BankerCards.Add(PopCard());
                }
            }
        }
    }
}