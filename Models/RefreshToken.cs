using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Models
{
    public class RefreshToken
    {
        [Key]
        public int TokenId { get; set; }

        [ForeignKey("StudentId")]
        public int StudentId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public virtual Student Student { get; set; }

    }
}
