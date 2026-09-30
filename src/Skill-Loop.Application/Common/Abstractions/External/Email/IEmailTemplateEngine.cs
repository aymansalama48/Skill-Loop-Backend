namespace Skill_Loop.Application.Common.Abstractions.External.Email;

public interface IEmailTemplateEngine
{
    Task<string> RenderTemplateAsync<TModel>(string templateName, TModel model);
}
