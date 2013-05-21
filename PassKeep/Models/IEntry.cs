using System;

namespace PassKeep.Models
{
    public interface IEntry : IGroup
    {
        KdbxString Password { get; set; }
        KdbxString Url { get; set; }
        KdbxString UserName { get; set; }
        string OverrideUrl { get; }
    }
}
