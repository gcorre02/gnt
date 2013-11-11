#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class RSIWADD : Strategy
    {
        #region Variables
        // Wizard generated variables
       // Default setting for MyInput0
        private int stop = 19;
		private int profit = 49;
		private int add = 10;
		private int oversold = 33;
		private int overbought = 67;
		private int rSIPeriod = 26;

		// User defined variables (add any user defined variables below)
		
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            CalculateOnBarClose = false;
			EntriesPerDirection = 2;
			EntryHandling = EntryHandling.AllEntries;
			
		}
        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			double targetStop = Stop*TickSize;
			double targetProfit = Profit*TickSize;
			
			
			if((Positions[0].Quantity==1) && (Close[0] < Positions[0].AvgPrice-(Add*TickSize)) && (Positions[0].MarketPosition == MarketPosition.Long)){
			//CancelOrder(entryOrder);
				EnterLong("currentPos");
				SetStopLoss("currentPos", CalculationMode.Price, Close[0]-targetStop, false);
				SetProfitTarget("currentPos", CalculationMode.Price, Close[0]+(targetProfit/2+1));
			}
			if((Positions[0].Quantity==1) && (Close[0] > Positions[0].AvgPrice+(Add*TickSize)) && (Positions[0].MarketPosition == MarketPosition.Short)){
				EnterShort("currentPos");
				SetStopLoss("currentPos", CalculationMode.Price, Close[0]+targetStop, false);
				SetProfitTarget("currentPos", CalculationMode.Price, Close[0]-(targetProfit/2+1));//breakeven
				}

			if(Position.MarketPosition != MarketPosition.Flat) return;
			
			if(CrossAbove(RSI(RSIPeriod,3),Oversold,1)){
			SetStopLoss("currentPos", CalculationMode.Price, Close[0]-targetStop, false);
			SetProfitTarget("currentPos", CalculationMode.Price, Close[0]+targetProfit);
			EnterLong("currentPos");
			//EnterLongAdd(Close[0]-(Add*TickSize),"currentPos");
			}
			if(CrossBelow(RSI(RSIPeriod,3),Overbought,1)){
			SetStopLoss("currentPos", CalculationMode.Price, Close[0]+targetStop,false);
			SetProfitTarget("currentPos", CalculationMode.Price, Close[0]-targetProfit);
			EnterShort("currentPos");
			//EnterShortAdd(Close[0]+(Add*TickSize),"currenPos");
			}
			
        }

        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public int Stop
        {
            get { return stop; }
            set { stop = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int Profit
        {
            get { return profit; }
            set { profit = Math.Max(1, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int Add
        {
            get { return add; }
            set { add = Math.Max(1, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int Oversold
        {
            get { return oversold; }
            set { oversold = Math.Max(1, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int Overbought
        {
            get { return overbought; }
            set { overbought = Math.Max(1, value); }
        }
	
		[Description("")]
        [GridCategory("Parameters")]
        public int RSIPeriod
        {
            get { return rSIPeriod; }
            set { rSIPeriod = Math.Max(1, value); }
        }

        #endregion
    }
}
