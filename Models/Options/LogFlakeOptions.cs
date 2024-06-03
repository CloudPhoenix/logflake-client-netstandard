using System.ComponentModel.DataAnnotations;

namespace NLogFlake.Models.Options;

internal sealed class LogFlakeOptions
{
    public const string SectionName = "LogFlake";

    [Required]
    public string? AppId { get; set; }

    [Url]
    public Uri? Endpoint { get; set; }
}
