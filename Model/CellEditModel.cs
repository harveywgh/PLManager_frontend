using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLManager.Model
{
    public class CellEditModel
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

}
