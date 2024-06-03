using Microsoft.Extensions.Options;

namespace NLogFlake.Models.Options;

[OptionsValidator]
internal sealed partial class LogFlakeOptionsValidator : IValidateOptions<LogFlakeOptions>;
