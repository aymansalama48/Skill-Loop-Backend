using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Skill_Loop.Api.Hubs;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;

namespace Skill_Loop.Api.Extensions;

public static class ChatExtensions
{
    /// <summary>
    /// تسجيل كل اللي الشات اللحظي (SignalR) محتاجه.
    /// </summary>
    public static IServiceCollection AddChatRealtime(this IServiceCollection services)
    {
        services.AddSignalR();

        // يعرّف SignalR إزاي يجيب الـ UserId من التوكن بتاعنا
        services.AddSingleton<IUserIdProvider, ChatUserIdProvider>();

        // الـ Application بتنادي IChatNotifier، واحنا هنا بنقول "نفّذه بـ SignalR"
        services.AddScoped<IChatNotifier, SignalRChatNotifier>();

        // 🔑 مشكلة WebSocket: المتصفح مايقدرش يبعت Header اسمه Authorization.
        // فالـ SignalR Client بيبعت التوكن في الـ Query String (?access_token=...).
        // هنا بنعلّم الـ JWT middleware يقراه من هناك، لكن بس على مسار الـ Hub.
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events ??= new JwtBearerEvents();
            var existingHandler = options.Events.OnMessageReceived;

            options.Events.OnMessageReceived = async context =>
            {
                await existingHandler(context);

                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments(ChatHub.Route))
                {
                    context.Token = accessToken;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// ربط الـ Hub بمساره (/hubs/chat).
    /// </summary>
    public static WebApplication MapChatHub(this WebApplication app)
    {
        app.MapHub<ChatHub>(ChatHub.Route);
        return app;
    }
}
