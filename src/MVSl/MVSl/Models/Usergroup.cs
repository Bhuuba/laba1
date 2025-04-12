using System;
using System.Collections.Generic;

namespace MVSl.Models;

public partial class Usergroup
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
