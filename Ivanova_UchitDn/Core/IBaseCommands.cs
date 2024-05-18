using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivanova_UchitDn.Core
{
    interface IBaseCommands
    {
        
        ReloadCommand ReloadSelf { get; set; }
        ReloadCommand Reload { get; }
        InsertCommand InsertSelf { get; set; }
        InsertCommand Insert { get; }
        EditCommand EditSelf { get; set; }
        EditCommand Edit { get; }
        
    }
}
