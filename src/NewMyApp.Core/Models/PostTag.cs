using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewMyApp.Core.Models
{
    public class PostTag
    {
        [Key]
        [Column(Order = 0)]
        public int PostId { get; set; }
        
        [Key]
        [Column(Order = 1)]
        public int TagId { get; set; }
        
        // Навігаційні властивості
        public virtual Post Post { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
} 