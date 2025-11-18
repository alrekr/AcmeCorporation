using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcmeCorporation.Library.Datacontracts;

internal class AdminOptions
{
    public const string SectionName = "DefaultAdminUser";
    public string? RoleName { get; set; }
    public string? Email { get; set; } 
    public string? Password { get; set; }
    public string? Username { get; set; }
}
