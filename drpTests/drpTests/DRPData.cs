using System;
using System.Globalization;

namespace drpTests
{
	 /**
     * Represents a DRP data unit according to the DRP protocol
     **/
    class DRPData
    {
		private DateTime date;
		private float weight;
		//private float fatPercentage;
		
		public DateTime Date { get => date; set => date = value;}
		public float Weight { get => weight; set => weight = value;}
		
		public DRPData () : this (0) {
			
		}
		
		public DRPData (float weight) {
			date = DateTime.Now;
			this.weight = weight;
		}

        public DRPData (DateTime date, float weight)
        {
            this.date = date;
            this.weight = weight;
        }

        public override string ToString()
        {
            if (this == null)
                return "No Data";
            else
                return("Time: " + date + " Weight: " + Weight);
        }

       
        /** @input: "Time: {0} Weight: {1}", date, weight
         * or: "No Data"
         * */
        public static DRPData parseDRPData (string drpString)
        {
            //Caution: No input check!

            if (drpString.Equals("No Data"))
                return null;

            int i = drpString.IndexOf("me:") + 4;
            int start = i;
            while (drpString[i] != 'W')
                i++;
            i -= 2;
            int end = i;
            String timeString = drpString.Substring(start, end - start + 1).Trim();
            DateTime date = DateTime.Parse(timeString);

            i = drpString.IndexOf("ht:") + 4;
            start = i;
            String weightString = drpString.Substring(start).Trim();
            float weight = float.Parse(weightString, CultureInfo.InvariantCulture);

            DRPData data = new DRPData(date, weight);
            return data;
        }
			
	}
}
			