//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace K102PhaymarcyApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Medicine_To_Tags
    {
        public int ID { get; set; }
        public int MedicineID { get; set; }
        public int TagID { get; set; }
    
        public virtual Medicine Medicine { get; set; }
        public virtual Tag Tag { get; set; }
    }
}