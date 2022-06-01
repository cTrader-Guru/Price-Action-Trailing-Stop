using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{

    #region Extensions

    public static class PositionExtensions
    {

        public static bool IsMine(this Position position, string SymbolName)
        {

            return position.SymbolName.CompareTo(SymbolName) == 0;

        }

        public static DataSeries DataSeries(this Position position, Bars bars, bool high_low)
        {

            DataSeries DataHighLow = position.TradeType == TradeType.Buy ? bars.LowPrices : bars.HighPrices;

            return (high_low) ? DataHighLow : bars.ClosePrices;

        }

        public static bool ItIsConsistent(this Position position, Symbol symbol, double stoploss)
        {

            if (!position.IsMine(symbol.Name))
                return false;

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

        public const string PAGE = "https://www.google.com/search?q=ctrader+guru+price+action+trailing+stop";

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

            PerformeTrailing();


        }

        protected override void OnBar()
        {

            PerformeTrailing();

        }

        private void PerformeTrailing()
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
