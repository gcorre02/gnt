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
    public class RSI_ret_v14 : Strategy  //Ver 10 - Gold Optimized, but works for everything // Version 11 - W/ time intervals coded in . 
    {
        #region Variables
        // Wizard generated variables

        private int entryLevelDif = 0; 
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
		private int unifiedLongerTRSI = 0;
		private int turn15minIO = 1;
		private int instantIOstr = 1;
		private int instantIOltr = 1;
		private int instantIOroc = 1;
//		private int startingHour = 12;/
//		private int startingMinute = 21;
//		private int startingSecond = 01;

        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private double longPriceDif =0;
		private double shortPriceDif = 0;
		private int storePreviousRsiMax = 5;
		private Double[] rsiArray;
		private Double[] rsiArray15;
		private Double[] rocArray;
		private bool Unified15min = false;
		private bool IO15min = true;
		//private int timeOff = 1;
		private bool InstantSTR = true;
		private bool InstantLTR = true;
		private bool InstantROC = true;
        #endregion

        /// <summary>
        /// This method is used to configure the §strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			//sets the exit on close status, think it is superseeded by options chosen on the form.
            SetProfitTarget("", CalculationMode.Ticks, Profit);
			if(TurnExitOn ==1){
				ExitOnClose= true;
			} else if(TurnExitOn ==0){
				ExitOnClose = false;
			}
			//sets trail or fixed stop IO
            if(TurnTrailOn == 1){
				SetTrailStop("", CalculationMode.Ticks, TrailStopLoss, false);
			} else {
				SetStopLoss(CalculationMode.Ticks,StopLoss);
			}
            CalculateOnBarClose = false;
			//creates an array to keep recent RSI values - needed for the instant update stuff
			rsiArray = new Double[storePreviousRsiMax];
			rsiArray15 = new Double[storePreviousRsiMax];
			rocArray = new Double[storePreviousRsiMax];
			// adds the longer period data to then calculate the longer period RSI - 
			// - LongRSILength - originally 15 min - accessible through BarsArray[]
			Add(PeriodType.Minute, LongRsiLength);
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		protected void getRsiRecentHigh(){
			// method to update and load new RSI value to the array
			// working perfectly
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rsiArray[i] = rsiArray[i-1];
			}
			rsiArray[0] = RSI(RSIPeriod,3)[0];
		}
		protected void getRsi15RecentHigh(){ // how about lows ? arent those important ?
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rsiArray15[i] = rsiArray15[i-1];
			}
			rsiArray15[0] = RSI(BarsArray[1],LongRsiPeriod,3)[0];
		}
		
		protected void getRocRecentHigh(){
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rocArray[i] = rocArray[i-1];
			}
			rocArray[0] = ROC(ROCPeriod)[0];
		}
		protected String getRsiArrayContents(){
			//method to print current array values, only necessary as a test, not currently used
			String result = "";
			for(int i = 0; i < storePreviousRsiMax; i++){
				result = result + " , " + rsiArray[i];
			}
			return result;
		}
		protected Boolean checkRoundNum(){
			//Method to check that current price is not near a round number, 
			//it only works for ticks that move @ 0.01 -- doesn't work for gold yet
			if(TurnCheckRoundNumOn==0){
				RoundTickDist = 0;
			}
			String check = Close[0].ToString();
			int len = check.Length-2;
			double units =100; //here
			if(len >= 3){
				units = double.Parse(check.Substring(len,2))*0.01/TickSize; // here 4? instead of 2, for it to work on gold
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
			double maxROC;
			if(InstantROC){
				maxROC = Math.Max(Math.Abs(rocArray[0]),Math.Abs(rocArray[0]));// this only gets the max between current and previous
			} else {
				maxROC = Math.Abs(ROC(ROCPeriod)[0]);
			}
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
			// if recent trade was loss, dont take trades in that direction for time = LastStopMin
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
				//test whether the recent loss was a long or a short and return that value;
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
/*		
		protected bool checkOverBoughtRsi15min(){
			//checks longer time period RSI is overbought
			if(RSI(BarsArray[1],LongRsiPeriod,3)[0]>67 || !IO15min){
				//Log("15 min RSI is long",LogLevel.Information);
				return true;
			} else {
				return false;
			}
		}	 
		protected bool checkOverSoldRsi15min(){
			//checks longer time period RSI is oversold
			if(RSI(BarsArray[1],LongRsiPeriod,3)[0]<33 || !IO15min){
				//Log("15 min RSI is long",LogLevel.Information);
				return true;
			} else {
				return false;
			}
		}
*/
		protected bool checkOverBoughtRsi15min(){
			if(InstantLTR){
				if(rsiArray15[0]>67 || !IO15min){
				//Log("15 min RSI is long",LogLevel.Information);
					return true;
				} else {
					return false;
				}
			} else {
				if(RSI(BarsArray[1],LongRsiPeriod,3)[0]>67 || !IO15min){
				//Log("15 min RSI is long",LogLevel.Information);
					return true;
				} else {
					return false;
				}
			}
		}
		protected bool checkOverSoldRsi15min(){
			if(InstantLTR){
				if (rsiArray15[0]<33 || !IO15min){
					//Log("15 min RSI is long",LogLevel.Information);
					return true;
				} else {
					return false;
				}
			} else {
				if (RSI(BarsArray[1],LongRsiPeriod,3)[0]<33 || !IO15min){
					//Log("15 min RSI is long",LogLevel.Information);
					return true;
				} else {
					return false;
				}
			}
		}

		protected bool unified15min(){
			if(Unified15min){
				if(checkOverBoughtRsi15min()
				|| checkOverSoldRsi15min()){
					return true;
				} else {
					return false;
				}
			}else {//if(!Unified15min){
					return false;
				}
		}
        protected override void OnBarUpdate()
        {

			//booleans
			#region
			if(InstantIOstr == 1 ){
				InstantSTR = true;
			} else {
				InstantSTR = false;
			}
			if(InstantIOltr == 1 ){
				InstantLTR = true;
			} else {
				InstantLTR = false;
			}
			
			if(InstantIOroc == 1 ){
				InstantROC = true;
			} else {
				InstantROC = false;
			}
			if(Turn15minIO == 1){
				IO15min = true;
			} else {
				IO15min = false;
				// makes sure there is no confusion by the code when 15minIO is off 
				//but Unified has been left on by diregard.
				UnifiedLongerTRSI = 0;
			}

			if(UnifiedLongerTRSI == 1){
				Unified15min = true;
			} else {
				Unified15min = false;
			}
			#endregion
			//Load rsi values to array
			getRsiRecentHigh();
			getRsi15RecentHigh();
			getRocRecentHigh();
			//Print(" ROC: "+Math.Abs(ROC(ROCPeriod)[0])+" ; ROC instant: "+Math.Abs(rocArray[0])+" ; RSI15: "+RSI(BarsArray[1],LongRsiPeriod,3)[0]+" ; RSI15instant: "+rsiArray15[0]);

			
			//checks for width of the Bid/Ask delta, doesnt work on backtest 
			//---- need to create Bars based on highs and lows for it to work ----
			if((GetCurrentAsk()-GetCurrentBid())<BidAsk*TickSize 
				&& checkRoundNum()
				//&& checkLongRsi()
				){//&& checkDay()){
				itsOn = 1;
			} else {
				itsOn = 0;
			}
			if(CurrentBar < RSIPeriod || itsOn == 0){
				//stops the next code
				return;
			}
			
			//reset limits
			belowLimit = 100;
			aboveLimit = 0;
			//Log(getRsiArrayContents(),LogLevel.Information);
			//condition for oversold
			
			//makes sure an oposite direction trade is not placed if the current position is negative 
			//-- avoiding stopping out to open a new position
			Boolean shortIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Short){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					shortIsPositive = false;
				}
			}
			//checks if RSI is under the Oversold level
			//if(RSI(RSIPeriod,3)[0]<OverSoldLevel){
			if(rsiArray[0]<OverSoldLevel){
				//sets the below limit for the instant previous tick  
				//-- can have an IO to use the original RSI function[1]
				//belowLimit = rsiArray[1];
				if(InstantSTR){
					belowLimit = rsiArray[1];
				} else {
					belowLimit = RSI(RSIPeriod,3)[1];
				}
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1)
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif ) // min distance from previous value it should be to enter trade
					&& checkRecentLossDirection() != "Long"
					&& (Close[0] < this.longPriceDif|| this.longPriceDif == 0) // connected to recent loss direction
					&& shortIsPositive
					&& rocFilter()
					&& ((!Unified15min && checkOverSoldRsi15min()) 
					|| Unified15min && unified15min()) // ---- need checking that is working
					//&& timeframe()   //deactivated
					)
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
			//if(RSI(RSIPeriod,3)[0]>OverBoughtLevel){
			//	aboveLimit = rsiArray[1];      
			if(rsiArray[0]>OverBoughtLevel){
				if(InstantSTR){
					aboveLimit = rsiArray[1];
				} else {
					aboveLimit = RSI(RSIPeriod,3)[1];
				}
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) 
					&& checkRecentLossDirection() != "Short"
					&& (Close[0] > this.shortPriceDif || this.shortPriceDif == 0)
					&& longIsPositive
					&& rocFilter()
	//				&& checkOverBoughtRsi15min()
					&& ((!Unified15min && checkOverBoughtRsi15min()) 
					|| Unified15min && unified15min()) // ---- need checking that is working
					//&& timeframe()
					)
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
        public int TurnRocFilterOn
        {
            get { return turnRocFilterOn; }
            set { turnRocFilterOn = Math.Max(0, value); }
		}
		[GridCategory("Parameters")]
        public int UnifiedLongerTRSI
        {
            get { return unifiedLongerTRSI; }
            set { unifiedLongerTRSI = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int Turn15minIO
        {
            get { return turn15minIO; }
            set { turn15minIO = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int LongRsiPeriod
        {
            get { return longRsiPeriod; }
            set { longRsiPeriod = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int InstantIOstr
        {
            get { return instantIOstr; }
            set { instantIOstr = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int InstantIOltr
        {
            get { return instantIOltr; }
            set { instantIOltr = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int InstantIOroc
        {
            get { return instantIOroc; }
            set { instantIOroc = Math.Max(0, value); }
        }
		
        #endregion
		/*
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
		*/

    }
}
