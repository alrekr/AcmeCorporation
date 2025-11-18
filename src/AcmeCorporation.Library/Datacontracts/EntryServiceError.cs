using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcmeCorporation.Library.Datacontracts;

public enum EntryServiceError
{
    None = 0,
    Unknown,
    RaffleEntryExceedMax,
    AgeNotEligible
}
