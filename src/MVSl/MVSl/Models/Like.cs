using System;
using System.Collections.Generic;

namespace MVSl.Models;

public partial class Like
{
    public int UserId { get; set; }

    public int PostId { get; set; }

    public DateTime LikesAt { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
