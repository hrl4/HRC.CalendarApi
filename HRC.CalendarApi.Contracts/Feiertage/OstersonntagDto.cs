using System;
using System.Collections.Generic;
using System.Text;

namespace HRC.CalendarApi.Contracts.Feiertage
{
    public sealed class OstersonntagDto
    {
        public int Jahr { get; init; }
        public DateOnly Datum { get; init; }
    }
}
