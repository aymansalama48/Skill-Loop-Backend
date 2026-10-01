# ==============================================================
# المرحلة 1: base
# دي الصورة اللي هيشتغل عليها التطبيق فعلياً (runtime بس، من غير SDK)
# ==============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Install ffmpeg for video analysis
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

# ✅ التعديل الجديد: نعمل فولدر رفع الملفات وإحنا لسه root
# ونديه صلاحيات المستخدم العادي (APP_UID) اللي التطبيق هيشتغل بيه.
# من غير السطر ده، التطبيق مش هيقدر يكتب في /app/UploadedFiles
# ويطلع Access denied لما ترفع ملف.
RUN mkdir -p /app/UploadedFiles && chown -R $APP_UID:$APP_UID /app/UploadedFiles

# بعد ما خلصنا الأوامر اللي محتاجة root، نبدّل لمستخدم عادي (أأمن)
USER $APP_UID

# البورتات اللي التطبيق بيسمع عليها جوه الحاوية
EXPOSE 8080
EXPOSE 8081


# ==============================================================
# المرحلة 2: build
# هنا بنستخدم SDK الكامل عشان نعمل restore و build للمشروع
# ==============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# ننسخ ملفات الـ .csproj الأول لوحدها (قبل باقي الكود)
# الفايدة: Docker بيعمل cache لخطوة الـ restore،
# فلو غيّرت في الكود بس من غير ما تغيّر الـ packages، الـ build بيبقى أسرع بكتير
COPY ["src/Skill-Loop.Api/Skill-Loop.Api.csproj", "src/Skill-Loop.Api/"]
COPY ["src/Skill-Loop.Application/Skill-Loop.Application.csproj", "src/Skill-Loop.Application/"]
COPY ["src/Skill-Loop.Domain/Skill-Loop.Domain.csproj", "src/Skill-Loop.Domain/"]
COPY ["src/Skill-Loop.Infrastructure/Skill-Loop.Infrastructure.csproj", "src/Skill-Loop.Infrastructure/"]

# تحميل الـ NuGet packages
RUN dotnet restore "./src/Skill-Loop.Api/Skill-Loop.Api.csproj"

# دلوقتي ننسخ باقي الكود كله
COPY . .

# ندخل على فولدر الـ API ونعمل build
WORKDIR "/src/src/Skill-Loop.Api"
RUN dotnet build "./Skill-Loop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


# ==============================================================
# المرحلة 3: publish
# بنطلّع النسخة النهائية الجاهزة للتشغيل (أصغر وأسرع من الـ build العادي)
# UseAppHost=false معناها منعمل ملف تنفيذي إضافي، ومحتاجينه بس dll
# ==============================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Skill-Loop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# ==============================================================
# المرحلة 4: final
# الصورة النهائية اللي بتتشغل: base (runtime خفيف) + ملفات الـ publish
# ==============================================================
FROM base AS final
WORKDIR /app

# ننسخ الناتج النهائي من مرحلة الـ publish
COPY --from=publish /app/publish .

# Merge note: origin/main replaced this with a bare comment. The base stage already creates
# /app/UploadedFiles, but COPY --from=publish runs as root afterwards, and docker-compose
# mounts a named volume there that is created root-owned - so the app (running as $APP_UID)
# could not write uploads. Kept from the security branch.
USER root
RUN mkdir -p /app/UploadedFiles && chown -R $APP_UID:$APP_UID /app/UploadedFiles
USER $APP_UID

ENTRYPOINT ["dotnet", "Skill-Loop.Api.dll"]