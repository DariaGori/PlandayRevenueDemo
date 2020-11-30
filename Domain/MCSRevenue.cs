using System;

namespace Domain
{
    public class MCSRevenue
    {
        public int TheatreID { get; set; }
        public Decimal NetSum { get; set; }
        public Decimal GrossSum { get; set; }
        public String TheatreName { get; set; } = default!;
        public String SalesPointAccountingCode { get; set; } = default!;
    }
}
