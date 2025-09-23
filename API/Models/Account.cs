using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        [StringLength(100)]
        public string? FullName { get; set; }
        public int RoomId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Password { get; set; }
        [Required]
        [StringLength(200)]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Role { get; set; } = 0;
        public string? Image { get; set; }
        public bool Status {  get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        //public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
