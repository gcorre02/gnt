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
    /// To localise the entry level of the RSI with respect to its extremes.
    /// </summary>
    [Description("To localise the entry level of the RSI with respect to its extremes.")]
    public class RSI_ret_v6 : Strategy
    {
        #region Variables
        // Wizard generated variables

        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 29; // Default setting for StopLoss
        private int profit = 280; // Default setting for Profit
        private int rSIPeriod = 26; // Default setting for RSIPeriod
		private int roundTickDist = 12;
		private int trailStopLoss = 1;
		private int turnTrailOn = 0;
		private int turnExitOn = 0;
		private int lastStopMin = 141;
		private int bidAsk = 3;
		private int priceDif = 1;
		private int isPositiveOn = 1; 
        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private double longPriceDif =0;
		private double shortPriceDif = 0;
		//private int timeOff = 1;
        #endregion

        /// <summary>
        /// This method is used to configure the §strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            SetProfitTarget("", CalculationMode.Ticks, Profit);
			if(TurnExitOn ==1){
				ExitOnClose= true;
			} else if(TurnExitOn ==0){
				ExitOnClose = false;
			}
            if(TurnTrailOn == 1){
				SetTrailStop("", CalculationMode.Ticks, TrailStopLoss, false);
			} else {
				SetStopLoss(CalculationMode.Ticks,StopLoss);
			}
            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		protected Boolean checkRoundNum(){
			String check = Close[0].ToString();
			int len = check.Length-2;
			double units =100;
			if(len >= 3){
				units = double.Parse(check.Substring(len,2))*0.01/TickSize;
			}
			if (units >= 100 -RoundTickDist || units <= roundTickDist){
				return false;
			} else {
				return true;
			}
		}
		protected String checkRecentLossDirection(){
			Boolean recentLoss = false;
 			// Check to make sure there is at least one trade in the collection
    		if (Performance.AllTrades.LosingTrades.Count > 0)
    		{
				//references last trade
				Trade lastTrade = Performance.AllTrades.LosingTrades[Performance.AllTrades.LosingTrades.Count -1 ];
       			//gets the difference in time between last losing and current time
				TimeSpan losingTime =( Times[0][0].TimeOfDay - lastTrade.ExitExecution.Time.TimeOfDay );
				//prints to log that difference
      		if(losingTime.TotalSeconds>0 && losingTime.TotalSeconds < LastStopMin){
				recentLoss = true;
				} else {
					recentLoss = false;
				}		    
				if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Long){
					this.longPriceDif=Close[0]-PriceDif*TickSize;
					return "Long";
				} else if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Short){
					this.shortPriceDif=Close[0]+PriceDif*TickSize;
					return "Short";
				} else {
					return "Nop";
				}
			}else {
					return "Nop";
			}
		}
        protected override void OnBarUpdate()
        {
			if((GetCurrentAsk()-GetCurrentBid())<BidAsk*TickSize && checkRoundNum() ){
				itsOn = 1;
			} else {
				itsOn = 0;
			}
			if(CurrentBar < RSIPeriod || itsOn == 0){
				return;
			}
			
			//reset limits
			belowLimit = 100;
			aboveLimit = 0;
			//condition for oversold
			Boolean shortIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Short){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					shortIsPositive = false;
				}
			}
			if(RSI(RSIPeriod,3)[0]<33){
				belowLimit = RSI(RSIPeriod,3)[1];
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1) 
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif )
					&& checkRecentLossDirection() != "Long"
					&& (Close[0] < this.longPriceDif|| this.longPriceDif == 0)
					&& shortIsPositive)
           	 	{
					this.longPriceDif = 0;
            	    EnterLong(DefaultQuantity, "");
            	}
			}
			//condition for overbought
			Boolean longIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Long){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					longIsPositive = false;
				}
			}
			if(RSI(RSIPeriod,3)[0]>67){
				aboveLimit = RSI(RSIPeriod,3)[1];         
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) 
					&& checkRecentLossDirection() != "Short"
					&& (Close[0] > this.shortPriceDif || this.shortPriceDif == 0)
					&& longIsPositive)
            	{
					this.shortPriceDif= 0;
                	EnterShort(DefaultQuantity, "");
            	}
			}
        }

        #region Properties
 
        [Description("Diferential from max/min to open position, in RSI percentage.")]
        [GridCategory("Parameters")]
        public int EntryLevelDif
        {
            get { return entryLevelDif; }
            set { entryLevelDif = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = Math.Max(1, value); }
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
        public int RSIPeriod
        {
            get { return rSIPeriod; }
            set { rSIPeriod = Math.Max(1, value); }
        }
        [Description("")]
        [GridCategory("Parameters")]
        public int RoundTickDist
        {
            get { return roundTickDist; }
            set { roundTickDist = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int TrailStopLoss
        {
            get { return trailStopLoss; }
            set { trailStopLoss = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int TurnTrailOn
        {
            get { return turnTrailOn; }
            set { turnTrailOn = Math.Max(0, value); }
        }
		
		[Description("")]
        [GridCategory("Parameters")]
        public int LastStopMin
        {
            get { return lastStopMin; }
            set { lastStopMin = Math.Max(0, value); }
        }

		[Description("")]
        [GridCategory("Parameters")]
        public int TurnExitOn
        {
            get { return turnExitOn; }
            set { turnExitOn = Math.Max(0, value); }
        }
	
		[Description("")]
        [GridCategory("Parameters")]
        public int PriceDif
        {
            get { return priceDif; }
            set { priceDif = Math.Max(0, value); }
        }
	
		[Description("")]
        [GridCategory("Parameters")]
        public int IsPositiveOn	
        {
            get { return isPositiveOn; }
            set { isPositiveOn = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int BidAsk
        {
            get { return bidAsk; }
            set { bidAsk = Math.Max(1, value); }
        }
        #endregion
    }
}
