//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace library
{
    using System;
    using System.Collections.Generic;
    
    public partial class pay
    {
        public int idPay { get; set; }
        public Nullable<int> fk_tranImID { get; set; }
        public Nullable<decimal> total { get; set; }
        public string note { get; set; }
    
        public virtual importRepository importRepository { get; set; }
    }
}