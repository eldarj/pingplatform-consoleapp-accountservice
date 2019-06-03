using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AccountMicroservice.Data.Models
{
    public class CallingCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CallingCountryCode { get; set; }
        public string CountryName { get; set; }
        public string IsoCode { get; set; }
    }
}
