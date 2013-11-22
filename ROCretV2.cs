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
    /// Back to basics with the ROC
    /// </summary>
    [Description("Back to basics with the ROC")]
    public class ROC_ret_V2 : Strategy
    {
        #region Variables
        // Wizard generated variables
        private double overSold = 0.110; // Default setting for OverSold
        private double overBought = 0.120; // Default setting for OverBought
        private int rOCPeriod = 14; // Default setting for ROCPeriod
        private int stopLoss = 23; // Default setting for StopLoss
        private int profitTarget = 34; // Default setting for ProfitTarget
        // User defined variables (add any user defined variables below)
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            SetProfitTarget("", CalculationMode.Ticks, ProfitTarget);
            SetStopLoss("", CalculationMode.Ticks, StopLoss, false);

            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Condition set 1
            if (CrossAbove(ROC(ROCPeriod), (-OverSold), 1))
            {
                EnterLong(DefaultQuantity, "");
            }
			   // Condition set 2
            if (CrossBelow(ROC(ROCPeriod), OverBought, 1))
            {
                EnterShort(DefaultQuantity, "");
            }
        }

        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public double OverSold
        {
            get { return overSold; }
            set { overSold = Math.Max(0.000, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double OverBought
        {
            get { return overBought; }
            set { overBought = Math.Max(0.000, value); }
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
        public int StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int ProfitTarget
        {
            get { return profitTarget; }
            set { profitTarget = Math.Max(1, value); }
        }
        #endregion
    }
}
