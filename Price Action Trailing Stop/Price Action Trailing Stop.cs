using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    #region Extensions

    public static class PositionExtensions
    {

        /// <summary>
        /// Check that the position in question is of the same symbol
        /// </summary>
        /// <param name="SymbolName">The symbol to check</param>
        /// <returns>True if it is of the same symbol</returns>
        public static bool IsMine(this Position position, string SymbolName)
        {

            return position.SymbolName.CompareTo(SymbolName) == 0;

        }

        /// <summary>
        /// Returns the DataSeries corresponding to the specific choice
        /// </summary>
        /// <param name="bars">Bars to consider</param>
        /// <param name="high_low">Flag for choosing high and low levels</param>
        /// <returns>The corresponding DataSeries</returns>
        public static DataSeries DataSeries(this Position position, Bars bars, bool high_low)
        {

            DataSeries DataHighLow = position.TradeType == TradeType.Buy ? bars.LowPrices : bars.HighPrices;

            return (high_low) ? DataHighLow : bars.ClosePrices;

        }

        /// <summary>
        /// Runs a check on the new stoploss and determines if it is consistent
        /// </summary>
        /// <param name="symbol">The symbol to be considered</param>
        /// <param name="stoploss">The value of the new stoploss to control</param>
        /// <returns>True if the new stoploss is consistent</returns>
        public static bool ItIsConsistent(this Position position, Symbol symbol, double stoploss)
        {

            if (!position.IsMine(symbol.Name)) return false;

            return !(position.StopLoss > 0) || (position.TradeType == TradeType.Buy ? (stoploss > position.StopLoss && stoploss < symbol.Bid) : (stoploss < position.StopLoss && stoploss > symbol.Ask));


        }

    }

    #endregion

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PriceActionTrailingStop : Robot
    {

        #region Enums & Class

        public enum TrailingMode
        {

            Close,
            HighLow

        }

        #endregion

        #region Identity

        public const string NAME = "Price Action Trailing Stop";

        public const string VERSION = "1.0.0";

        public const string PAGE = "https://ctrader.guru/product/price-action-trailing-stop/";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = PAGE)]
        public string ProductInfo { get; set; }

        [Parameter("Mode", Group = "Trailing", DefaultValue = TrailingMode.HighLow)]
        public TrailingMode TMode { get; set; }

        [Parameter("Period", Group = "Trailing", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int TPeriod { get; set; }

        #endregion

        protected override void OnStart()
        {

            _performeTrailing();


        }

        protected override void OnTick()
        {

            // --> TODO:

        }

        protected override void OnBar()
        {

            _performeTrailing();

        }

        protected override void OnStop()
        {

            // --> TODO:

        }

        private void _performeTrailing()
        {

            foreach (Position position in Positions)
            {

                double stoploss = position.DataSeries(MarketData.GetBars(TimeFrame, SymbolName), TMode == TrailingMode.HighLow).Last(TPeriod);

                if (position.ItIsConsistent(Symbol, stoploss) && !position.ModifyStopLossPrice(stoploss).IsSuccessful)
                    Print("Can't edit position #", position.Id, ", with stoploss ", stoploss);

            }

        }

    }

}
