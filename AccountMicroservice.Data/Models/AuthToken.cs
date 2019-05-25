using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AccountMicroservice.Data.Models
{
    public class AuthToken
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime DateGenerated { get; set; }
        public string Ip { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; }
    }
}
