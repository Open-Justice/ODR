//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClickNClaim.Common
{
    using System;
    using System.Collections.Generic;
    
    public partial class ArbiterSkill
    {
        public int Id { get; set; }
        public string IdArbiter { get; set; }
        public int IdSkill { get; set; }
    
        public virtual AspNetUser Arbiter { get; set; }
        public virtual Skill Skill { get; set; }
    }
}