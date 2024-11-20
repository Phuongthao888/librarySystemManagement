using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace library.Models
{
    public class DashboardViewModel
    {
        public int TotalReaders { get; set; }
        public int TotalStaff { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalBookshelf { get; set; }
    }

}