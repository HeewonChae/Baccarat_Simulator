using BaccaratSimulator.Logic;
using BaccaratSimulator.Models;
using BaccaratSimulator.Models.Enums;
using LogicCore.Utility;
using Syncfusion.Windows.Controls.Grid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BaccaratSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BaccaratPlay _baccaratPlay;
        private readonly Label[] _palyerShape;
        private readonly Label[] _palyerNumber;
        private readonly Label[] _bankerShape;
        private readonly Label[] _bankerNumber;

        private List<List<BaccaratResultType>> Bead_Plate_Results; // 6매
        private List<List<PatternType>> Big_Road_Results; // 원매
        private List<List<PatternType>> Big_Eye_Boy_Results; // 1군
        private List<List<PatternType>> Small_Road_Results; // 2군
        private List<List<PatternType>> Cockroach_Pig_Results; // 3군

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _baccaratPlay = Singleton.Get<BaccaratPlay>();

            _palyerShape = new Label[] { _lbl_player_shape1, _lbl_player_shape2, _lbl_player_shape3 };
            _palyerNumber = new Label[] { _lbl_player_number1, _lbl_player_number2, _lbl_player_number3 };
            _bankerShape = new Label[] { _lbl_banker_shape1, _lbl_banker_shape2, _lbl_banker_shape3 };
            _bankerNumber = new Label[] { _lbl_banker_number1, _lbl_banker_number2, _lbl_banker_number3 };

            _btn_initialize.IsEnabled = true;
            _btn_play.IsEnabled = false;

            InitializePatternBoard(ref _grd_Big_Road); // 원매
            InitializePatternBoard(ref _grd_big_eye_boy); // 1군
            InitializePatternBoard(ref _grd_small_road); // 2군
            InitializePatternBoard(ref _grd_cockroach_pig); // 3군
            InitializeBeadPlateBoard();
        }

        #region Commands

        private void Initialize_Click(object sender, RoutedEventArgs e)
        {
            ClearCardSignBoard();
            ClearPatternInfo(ref _grd_Big_Road, ref Big_Road_Results);
            ClearPatternInfo(ref _grd_big_eye_boy, ref Big_Eye_Boy_Results);
            ClearPatternInfo(ref _grd_small_road, ref Small_Road_Results);
            ClearPatternInfo(ref _grd_cockroach_pig, ref Cockroach_Pig_Results);
            ClearBeadPlateInfo();

            _baccaratPlay.InitializeGame();
            _btn_play.IsEnabled = _baccaratPlay.IsPlayable();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            _btn_play.IsEnabled = false;

            ClearCardSignBoard();
            var playResult = _baccaratPlay.PlayNext();
            PostProcessBaccaratPlay(playResult);

            _btn_play.IsEnabled = _baccaratPlay.IsPlayable();
        }

        #endregion Commands

        #region Logic Methods

        private void InitializePatternBoard(ref GridControl grd_control)
        {
            grd_control.Model.RowCount = 30;
            grd_control.Model.ColumnCount = 100;
            grd_control.Model.HeaderRows = 1;
            grd_control.Model.HeaderColumns = 1;
            grd_control.IsEnabled = false;

            // 셀 스타일 설정
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    grd_control.Model[i, j].Font.FontSize = 20;
                    grd_control.Model[i, j].VerticalAlignment = VerticalAlignment.Center;
                    grd_control.Model[i, j].HorizontalAlignment = HorizontalAlignment.Center;
                }
            }

            // 해더 셋팅
            for (int i = 1; i <= 100; i++)
            {
                grd_control.Model[0, i].CellValue = $"{i}";
            }

            for (int i = 1; i <= 30; i++)
            {
                grd_control.Model[i, 0].CellValue = $"{i}";
            }

            grd_control.Model.ResizeRowsToFit(GridRangeInfo.Table(), GridResizeToFitOptions.None);
            grd_control.Model.ResizeColumnsToFit(GridRangeInfo.Table(), GridResizeToFitOptions.None);
        }

        private void ClearPatternInfo(ref GridControl grd_control, ref List<List<PatternType>> patternResults)
        {
            patternResults = new List<List<PatternType>>();

            for (int i = 1; i < 30; i++)
            {
                for (int j = 1; j < 100; j++)
                {
                    grd_control.Model[i, j].CellValue = string.Empty;
                    grd_control.Model[i, j].ToolTip = null;
                    grd_control.Model[i, j].ShowTooltip = false;
                }
            }
        }

        private void InitializeBeadPlateBoard()
        {
            _grd_bead_plate.Model.RowCount = 7;
            _grd_bead_plate.Model.ColumnCount = 80;
            _grd_bead_plate.Model.HeaderRows = 1;
            _grd_bead_plate.Model.HeaderColumns = 1;

            // 셀 스타일 설정
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 80; j++)
                {
                    _grd_bead_plate.Model[i, j].Font.FontSize = 20;
                    _grd_bead_plate.Model[i, j].VerticalAlignment = VerticalAlignment.Center;
                    _grd_bead_plate.Model[i, j].HorizontalAlignment = HorizontalAlignment.Center;
                }
            }

            // 해더 셋팅
            for (int i = 1; i <= 80; i++)
            {
                _grd_bead_plate.Model[0, i].CellValue = $"{i}";
            }

            for (int i = 1; i <= 7; i++)
            {
                _grd_bead_plate.Model[i, 0].CellValue = $"{i}";
            }

            _grd_bead_plate.Model.ResizeRowsToFit(GridRangeInfo.Table(), GridResizeToFitOptions.None);
            _grd_bead_plate.Model.ResizeColumnsToFit(GridRangeInfo.Table(), GridResizeToFitOptions.None);
        }

        private void ClearBeadPlateInfo()
        {
            Bead_Plate_Results = new List<List<BaccaratResultType>>();

            for (int i = 1; i < 7; i++)
            {
                for (int j = 1; j < 80; j++)
                {
                    _grd_bead_plate.Model[i, j].CellValue = string.Empty;
                    _grd_bead_plate.Model[i, j].ToolTip = null;
                    _grd_bead_plate.Model[i, j].ShowTooltip = false;
                }
            }
        }

        private void ClearCardSignBoard()
        {
            for (int i = 0; i < 3; ++i)
            {
                _palyerShape[i].Content = string.Empty;
                _palyerNumber[i].Content = string.Empty;
                _bankerShape[i].Content = string.Empty;
                _bankerNumber[i].Content = string.Empty;
            }

            _lbl_player_result_number.Content = string.Empty;
            _lbl_banker_result_number.Content = string.Empty;
        }

        private void PostProcessBaccaratPlay(BaccaratResultInfo playResult)
        {
            // PlayerCard
            for (int i = 0; i < playResult.PlayerCards.Count; i++)
            {
                var card = playResult.PlayerCards[i];
                var cardColor = GetBrushForCardShape(card.Shape);
                _palyerShape[i].Content = card.Shape;
                _palyerShape[i].Foreground = cardColor;
                _palyerNumber[i].Content = card.Number;
                _palyerNumber[i].Foreground = cardColor;
                _lbl_player_result_number.Content = playResult.PlayerValue;
            }

            // BankerCard
            for (int i = 0; i < playResult.BankerCards.Count; i++)
            {
                var card = playResult.BankerCards[i];
                var cardColor = GetBrushForCardShape(card.Shape);
                _bankerShape[i].Content = card.Shape;
                _bankerShape[i].Foreground = cardColor;
                _bankerNumber[i].Content = card.Number;
                _bankerNumber[i].Foreground = cardColor;
                _lbl_banker_result_number.Content = playResult.BankerValue;
            }

            ProcessBeadPlateOutput(ref Bead_Plate_Results, ref _grd_bead_plate, playResult.ResultType, playResult, "⓿"); // 6매
            ProcessPatternOutput(ref Big_Road_Results, ref _grd_Big_Road, playResult.Big_Road, playResult, "⓿"); // 원매
            ProcessPatternOutput(ref Big_Eye_Boy_Results, ref _grd_big_eye_boy, playResult.Big_Eye_Boy, playResult, "❶"); // 1군
            ProcessPatternOutput(ref Small_Road_Results, ref _grd_small_road, playResult.Small_Road, playResult, "Ｏ"); // 2군
            ProcessPatternOutput(ref Cockroach_Pig_Results, ref _grd_cockroach_pig, playResult.Cockroach_Pig, playResult, "/"); // 3군
        }

        private void ProcessBeadPlateOutput(ref List<List<BaccaratResultType>> bead_Plate_Results, ref GridControl grd_control, BaccaratResultType resultType, BaccaratResultInfo playResult, string outputValue)
        {
            if (bead_Plate_Results.Count == 0)
            {
                var newLine = new List<BaccaratResultType>();
                newLine.Add(resultType);
                bead_Plate_Results.Add(newLine);
            }
            else
            {
                var latestLine = bead_Plate_Results[bead_Plate_Results.Count - 1];
                if (latestLine.Count == 6)
                {
                    var newLine = new List<BaccaratResultType>();
                    newLine.Add(resultType);
                    bead_Plate_Results.Add(newLine);
                }
                else
                {
                    latestLine.Add(resultType);
                }
            }

            // Output to cell
            var latest = bead_Plate_Results[bead_Plate_Results.Count - 1];
            grd_control.Model[latest.Count, bead_Plate_Results.Count].Foreground = GetBrushForResultType(resultType);
            grd_control.Model[latest.Count, bead_Plate_Results.Count].CellValue = outputValue;
            grd_control.Model[latest.Count, bead_Plate_Results.Count].ShowTooltip = true;
            grd_control.Model[latest.Count, bead_Plate_Results.Count].ToolTip = $"seq: {playResult.GameSeq}, player: {playResult.PlayerValue}, banker: {playResult.BankerValue}";
        }

        private void ProcessPatternOutput(ref List<List<PatternType>> patternResults, ref GridControl grd_control, PatternType patternType, BaccaratResultInfo playResult, string outputValue)
        {
            if (patternType == PatternType.neutral)
                return;

            if (patternResults.Count == 0)
            {
                var newLine = new List<PatternType>();
                newLine.Add(patternType);
                patternResults.Add(newLine);
            }
            else
            {
                var latestLine = patternResults[patternResults.Count - 1];
                var linePatten = latestLine.First();
                if (linePatten == patternType)
                {
                    latestLine.Add(patternType);
                }
                else
                {
                    var newLine = new List<PatternType>();
                    newLine.Add(patternType);
                    patternResults.Add(newLine);
                }
            }

            // Output to cell
            var latest = patternResults[patternResults.Count - 1];
            grd_control.Model[latest.Count, patternResults.Count].Foreground = GetBrushForPatternType(patternType);
            grd_control.Model[latest.Count, patternResults.Count].CellValue = outputValue;
            grd_control.Model[latest.Count, patternResults.Count].ShowTooltip = true;
            grd_control.Model[latest.Count, patternResults.Count].ToolTip = $"seq: {playResult.GameSeq}, player: {playResult.PlayerValue}, banker: {playResult.BankerValue}";
        }

        private Brush GetBrushForCardShape(string cardShape)
        {
            Color selectedColor = Colors.Black;
            switch (cardShape)
            {
                case "♥":
                case "◆":
                    selectedColor = Colors.Red;
                    break;

                default:
                    break;
            }

            return new SolidColorBrush(selectedColor);
        }

        private Brush GetBrushForPatternType(PatternType patternType)
        {
            if (patternType == PatternType.Positive)
            {
                return new SolidColorBrush(Colors.Red);
            }
            else if (patternType == PatternType.Negative)
            {
                return new SolidColorBrush(Colors.Blue);
            }

            return null;
        }

        private Brush GetBrushForResultType(BaccaratResultType resultType)
        {
            if (resultType == BaccaratResultType.Banker)
            {
                return new SolidColorBrush(Colors.Red);
            }
            else if (resultType == BaccaratResultType.Player)
            {
                return new SolidColorBrush(Colors.Blue);
            }

            return new SolidColorBrush(Colors.Green);
        }

        #endregion Logic Methods
    }
}