using System;

namespace ManualDataSend2
{
	 /**
     * Represents a DRP data unit according to the DRP protocol
     **/
    class DRPData
    {
		private DateTime date;
		private float weight;
		//private float fatPercentage;
		
		public Date { get => date; set => date = value;}
		public Weight { get => weight; set => weight = value;}
		
		public DRPData () : this (0) {
			
		}
		
		public DRPData (float weight) {
			date = DateTime.Now;
			this.weight = weight;
		}
	}
}
			