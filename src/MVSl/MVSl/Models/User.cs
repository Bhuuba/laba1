using System;
using System.Collections.Generic;

namespace MVSl.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly? Birthdate { get; set; }

    public string Password { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<Coment> Coments { get; set; } = new List<Coment>();

    public virtual ICollection<Friendship> FriendshipUser1s { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendship> FriendshipUser2s { get; set; } = new List<Friendship>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Usergroup> Usergroups { get; set; } = new List<Usergroup>();
}
