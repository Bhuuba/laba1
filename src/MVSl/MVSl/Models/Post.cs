using System;
using System.Collections.Generic;

namespace MVSl.Models;

public partial class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Text { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public virtual ICollection<Coment> Coments { get; set; } = new List<Coment>();

    public virtual User User { get; set; } = null!;
}
