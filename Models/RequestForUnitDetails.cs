using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipSel3.Models
{
    public class RequestForUnitDetails
    {
        public int CountryId { get; set; }
        public int UnitTypeId { get; set; }
        public bool OnlyReturnUnitsInCollection { get; set; } = false;
        public int  RuleSetId { get; set; }
    }
}
