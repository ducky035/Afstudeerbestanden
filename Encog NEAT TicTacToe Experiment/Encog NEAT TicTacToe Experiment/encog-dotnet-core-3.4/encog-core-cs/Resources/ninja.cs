//
// Encog(tm) Core v3.3 - .Net Version
// http://www.heatonresearch.com/encog/
//
// Copyright 2008-2014 Heaton Research, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//   
// For more information on Heaton Research copyrights, licenses 
// and trademarks visit:
// http://www.heatonresearch.com/copyright
//
#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Encog.App.Quant.Loader.OpenQuant.Data;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class EncogExample : Indicator
    {
        #region Variables
        // Wizard generated variables
            private bool export = false; // Default setting for Export
        // User defined variables (add any user defined variables below)
        #endregion

		private StreamWriter sw;
		
		protected double[] ObtainData() {
            return new double[] { };
		}
				
		public virtual void ActivationTANH(double[] x, int start, int size)
        {
            for (int i = start; i < start + size; i++)
            {
                x[i] = 2.0 / (1.0 + Math.Exp(-2.0 * x[i])) - 1.0;
            }
        }
		
		public virtual void ActivationSigmoid(double[] x, int start, int size)
        {
            for (int i = start; i < start + size; i++)
            {
                x[i] = 1.0d/(1.0d + Math.Exp(-1*x[i]));
            }
        }
		
		protected double Norm(double x,double normalizedHigh, double normalizedLow, double dataHigh, double dataLow)
		{
			return ((x - dataLow) 
				/ (dataHigh - dataLow))
				* (normalizedHigh - normalizedLow) + normalizedLow;
		}
		
		protected double DeNorm(double x,double normalizedHigh, double normalizedLow, double dataHigh, double dataLow) {
			return ((dataLow - dataHigh) * x - normalizedHigh
				* dataLow + dataHigh * normalizedLow)
				/ (normalizedLow - normalizedHigh);
		}
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator
    {
        private EncogExample[] cacheEncogExample = null;

        private static EncogExample checkEncogExample = new EncogExample();
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column
    {
        
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy
    {
        
    }
}
#endregion
