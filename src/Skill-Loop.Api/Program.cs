using Skill_Loop.Api.Extensions;
using Skill_Loop.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. تفعيل Serilog للـ Logging الهيكلي
builder.AddSerilogLogging();

// 2. تسجيل كافة خدمات الطبقات (Infrastructure, Application, CORS, Controllers, Scalar)
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();


// 3. 🚀 تشغيل الـ Database Migrations & Seeding أولاً لضمان إنشاء قاعدة البيانات قبل Hangfire
await app.SeedDatabaseAsync();

// 4. تطبيق خط سير الطلبات الموحد (Request Pipeline & Hangfire)
app.UseApplicationPipeline();

app.Run();