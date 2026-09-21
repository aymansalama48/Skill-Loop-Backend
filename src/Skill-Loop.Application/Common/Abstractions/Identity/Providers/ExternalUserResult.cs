using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Providers
{
    public record ExternalUserResult(
       string ProviderUserId,
       string Email,
       string FullName,
       string? AvatarUrl);
}
