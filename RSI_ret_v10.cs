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
    public class RSI_ret_v10 : Strategy  //Gold Optimized, but works for everything
    {
        #region Variables
        // Wizard generated variables

        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 30; // Default setting for StopLoss
        private int profit = 40; // Default setting for Profit
        private int rSIPeriod = 11; // Default setting for RSIPeriod
		private int roundTickDist = 0;
		private int trailStopLoss = 0;
		private int turnTrailOn = 0;
		private int turnExitOn = 0;
		private int lastStopMin = 141;
		private int bidAsk = 3;
		private int priceDif = 1;
		private int isPositiveOn = 1; 
		private int rOCPeriod = 15;
		private double rOCDif = 0.05;
		private int overSoldLevel = 33;
		private int overBoughtLevel = 67;
		private int turnCheckRoundNumOn = 0;
		private int turnRocFilterOn = 1;
		private int turnLastStopMinOn = 0;
		private int longRsiPeriod =9;
		private int longRsiLength = 15;
		private int startingHour = 0;
		private int startingMinute = 0;
		private int startingSecond = 0;

        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private double longPriceDif =0;
		private double shortPriceDif = 0;
		private int storePreviousRsiMax = 5;
		private Double[] rsiArray;
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
			rsiArray = new Double[storePreviousRsiMax];
			Add(PeriodType.Minute, LongRsiLength);
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		protected void getRsiRecentHigh(){
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rsiArray[i] = rsiArray[i-1];
			}
			rsiArray[0] = RSI(RSIPeriod,3)[0];
		}
		protected String getRsiArrayContents(){
			String result = "";
			for(int i = 0; i < storePreviousRsiMax; i++){
				result = result + " , " + rsiArray[i];
			}
			return result;
		}
		protected Boolean checkRoundNum(){
			if(TurnCheckRoundNumOn==0){
				RoundTickDist = 0;
			}
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
		protected bool rocFilter(){
			if(TurnRocFilterOn == 0){
				ROCDif = 0;
			}
			double maxROC = Math.Max(Math.Abs(ROC(ROCPeriod)[0]),Math.Abs(ROC(ROCPeriod)[0]));
			if(maxROC>ROCDif){
				return true;
			}  else {
				return false;
			}
			//Need to go through all the code and change double comparisons to something more exact
			/*absolute max value of roc between previous and current bar
			* is this less than optimized value?
			* dont take the trade
			* int ROC  Period(already Defined)
			* double Optimized value
			* 
			*/
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
      		if(losingTime.TotalSeconds>0 
				&& losingTime.TotalSeconds < LastStopMin
				&& TurnLastStopMinOn == 1){
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
		protected bool checkDay(){
			if (Times[0][0].DayOfWeek == DayOfWeek.Tuesday){
				//&& Times[0][0].Hour >= StartingHour && Times[0][0].Hour <= EndingHour){
				return true;
			} else {
				return false;
			}
		}
		protected bool checkLongRsi(){
			if(RSI(BarsArray[1],LongRsiPeriod,3)[0]>67 || RSI(BarsArray[1],LongRsiPeriod,3)[0]<33){
				//Log("15 min RSI is long",LogLevel.Information);
				return true;
			} else {
				return false;
			}
		}
		protected bool timeframe(){
			if (//Times[0][0].DayOfWeek == DayOfWeek.Monday && 
				Times[0][0].Hour >= StartingHour 
				&& Times[0][0].Minute >= StartingMinute 
				&& Times[0][0].Second>= StartingSecond){
					return true;
				} else {
					return false;
				}
		}
        protected override void OnBarUpdate()
        {
			if((GetCurrentAsk()-GetCurrentBid())<BidAsk*TickSize 
				&& checkRoundNum()
				&& checkLongRsi()
				){//&& checkDay()){
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
			//Load rsi values to array
			getRsiRecentHigh();
			//Log(getRsiArrayContents(),LogLevel.Information);
			//condition for oversold
			
			Boolean shortIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Short){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					shortIsPositive = false;
				}
			}
			if(RSI(RSIPeriod,3)[0]<OverSoldLevel){
				belowLimit = rsiArray[1];
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1) 
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif )
					&& checkRecentLossDirection() != "Long"
					&& (Close[0] < this.longPriceDif|| this.longPriceDif == 0)
					&& shortIsPositive
					&& rocFilter()
					&& timeframe())
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
			if(RSI(RSIPeriod,3)[0]>OverBoughtLevel){
				aboveLimit = rsiArray[1];         
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) 
					&& checkRecentLossDirection() != "Short"
					&& (Close[0] > this.shortPriceDif || this.shortPriceDif == 0)
					&& longIsPositive
					&& rocFilter()
					&& timeframe())
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
        public int ROCPeriod
        {
            get { return rOCPeriod; }
            set { rOCPeriod = Math.Max(1, value); }
        }
				[Description("")]
		
        [GridCategory("Parameters")]
        public double ROCDif
        {
            get { return rOCDif; }
            set { rOCDif = Math.Max(0, value); }
        }
		
		[GridCategory("Parameters")]
        public int BidAsk
        {
            get { return bidAsk; }
            set { bidAsk = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int OverSoldLevel
        {
            get { return overSoldLevel; }
            set { overSoldLevel = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int OverBoughtLevel
        {
            get { return overBoughtLevel; }
            set { overBoughtLevel = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int LongRsiLength 
        {
            get { return longRsiLength ; }
            set { longRsiLength  = Math.Max(0, value); }
        }

		[GridCategory("Parameters")]
        public int TurnCheckRoundNumOn
        {
            get { return turnCheckRoundNumOn; }
            set { turnCheckRoundNumOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int TurnLastStopMinOn
        {
            get { return turnLastStopMinOn; }
            set { turnLastStopMinOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int LongRsiPeriod
        {
            get { return longRsiPeriod; }
            set { longRsiPeriod = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int TurnRocFilterOn
        {
            get { return turnRocFilterOn; }
            set { turnRocFilterOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour
        {
            get { return startingHour; }
            set { startingHour = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute
        {
            get { return startingMinute; }
            set { startingMinute = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond
        {
            get { return startingSecond; }
            set { startingSecond = Math.Max(0, value); }
        }




        #endregion
    }
}
