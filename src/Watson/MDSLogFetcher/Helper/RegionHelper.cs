using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    public class RegionHelper
    {
        public static string GetRegionTableId(string region)
        {
            if(region.Contains("China"))
            {
                return "mcprod";
            }
            else
            {
                return region.Replace(" ", "").ToLower().Trim();
            }
        }
    }
}
