using System;
using System.Collections.Generic;

namespace MVSl.Models;

public partial class Coment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public string Text { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
