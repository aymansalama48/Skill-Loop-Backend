using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skill_Loop.Domain.Entities.OtpVerification;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Infrastructure.Persistence.Configurations
{
    public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
    {
        public void Configure(EntityTypeBuilder<OtpVerification> builder)
        {
            builder.ToTable("OtpVerifications");

            builder.Property(o => o.Phone)
                .HasMaxLength(20)
                .IsRequired();

            // Base64 لـ HMACSHA256 (32 بايت) بيطلع 44 حرف تقريبًا — سايبها مساحة زيادة بسيطة
            builder.Property(o => o.CodeHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(o => o.Purpose)
                .HasConversion<string>()   // تخزين الـ Enum كنص في الداتابيز (أوضح للمراجعة اليدوية من رقم)
                .HasMaxLength(30)
                .IsRequired();


            // الفهرس الأهم عمليًا — كل الـ Queries في OtpService بتفلتر بالتلاتة دول مع بعض
            builder.HasIndex(o => new { o.Phone, o.Purpose, o.IsConsumed })
                .HasDatabaseName("IX_OtpVerifications_Phone_Purpose_IsConsumed");

            builder.HasIndex(o => o.Expiry)
                .HasDatabaseName("IX_OtpVerifications_Expiry");
        }
    }
}
