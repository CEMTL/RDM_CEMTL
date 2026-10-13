using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Solution.ModelBinding;
using Solution.Models;
using Solution.Options;
using Solution.Security;
using Solution.Services.Identity;
using Solution.Services;

var cultureFrCa = CultureInfo.GetCultureInfo("fr-CA");

// fr-CA est la culture unique de l'application, y compris pour les traitements
// exécutés hors d'une requête HTTP (tâches, journalisation et conversions).
CultureInfo.DefaultThreadCurrentCulture = cultureFrCa;
CultureInfo.DefaultThreadCurrentUICulture = cultureFrCa;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RdmAuthenticationOptions>(
    builder.Configuration.GetSection(RdmAuthenticationOptions.SectionName));
builder.Services.Configure<ActiveDirectoryRoleOptions>(
    builder.Configuration.GetSection(ActiveDirectoryRoleOptions.SectionName));

var authOptions = builder.Configuration
    .GetSection(RdmAuthenticationOptions.SectionName)
    .Get<RdmAuthenticationOptions>() ?? new RdmAuthenticationOptions();

// Permet le déploiement temporaire en HTTP (intranet/test) ou en HTTPS.
// En HTTP, les cookies ne doivent pas être forcés avec l'attribut Secure.
var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps");
var cookieSecurePolicy = requireHttps
    ? CookieSecurePolicy.Always
    : CookieSecurePolicy.SameAsRequest;

var connectionString = builder.Configuration.GetConnectionString("RdmConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "La chaîne ConnectionStrings:RdmConnection est absente. Configurez-la dans appsettings.json, les secrets utilisateur ou une variable d'environnement.");
}

var useActiveDirectory = authOptions.UseActiveDirectory;

if (!useActiveDirectory &&
    !builder.Environment.IsDevelopment() &&
    !authOptions.AllowLocalAdminOutsideDevelopment)
{
    throw new InvalidOperationException(
        "Le mode Authentication:UseActiveDirectory=false accorde les droits administrateur à toutes les requêtes. " +
        "Il est interdit hors Development, sauf activation explicite de " +
        "Authentication:AllowLocalAdminOutsideDevelopment pour une intervention contrôlée.");
}

var authenticationScheme = useActiveDirectory
    ? CookieAuthenticationDefaults.AuthenticationScheme
    : LocalAdminAuthenticationHandler.SchemeName;

// En mode formulaire AD, IIS ne doit pas imposer son propre SSO Windows.
// L'utilisateur doit atteindre /Account/Login anonymement puis saisir ses identifiants.
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RDM.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services.AddLocalization();

var mvcBuilder = builder.Services
    .AddControllersWithViews(options =>
    {
        options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider());

        // Les messages automatiques du model binder ASP.NET Core sont anglais par
        // défaut. Ils sont définis explicitement ici afin qu'aucune erreur de
        // conversion ne soit présentée en anglais à l'utilisateur.
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((value, fieldName) =>
            $"La valeur « {value} » n'est pas valide pour le champ « {fieldName} ».");
        messages.SetMissingBindRequiredValueAccessor(fieldName =>
            $"Le champ « {fieldName} » est obligatoire.");
        messages.SetMissingKeyOrValueAccessor(() =>
            "Une valeur obligatoire est manquante.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() =>
            "Le contenu de la requête est obligatoire.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value =>
            $"La valeur « {value} » n'est pas valide.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() =>
            "La valeur fournie n'est pas valide.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() =>
            "La valeur doit être un nombre.");
        messages.SetUnknownValueIsInvalidAccessor(fieldName =>
            $"La valeur fournie n'est pas valide pour le champ « {fieldName} ».");
        messages.SetValueIsInvalidAccessor(value =>
            $"La valeur « {value} » n'est pas valide.");
        messages.SetValueMustBeANumberAccessor(fieldName =>
            $"Le champ « {fieldName} » doit contenir un nombre.");
        messages.SetValueMustNotBeNullAccessor(fieldName =>
            $"Le champ « {fieldName} » est obligatoire.");
    })
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// La compilation dynamique des vues est utile pendant le développement, mais
// elle est inutilement coûteuse et élargit la surface d'exécution en production.
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddDbContext<HopitalMedecinsDbEntities>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IActiveDirectoryAuthenticationService, ActiveDirectoryAuthenticationService>();
builder.Services.AddScoped<IAspNetIdentityAuthorizationService, AspNetIdentityAuthorizationService>();
builder.Services.AddScoped<IMedecinAlerteService, MedecinAlerteService>();
builder.Services.AddScoped<IAdminExportDocumentService, AdminExportDocumentService>();
builder.Services
    .AddHealthChecks()
    .AddCheck("application", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

// Protection du formulaire d'authentification AD contre les rafales de tentatives.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Trop de tentatives ont été effectuées. Veuillez patienter avant de réessayer.",
            cancellationToken);
    };
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

if (useActiveDirectory)
{
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".RDM.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSecurePolicy;
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });
}
else
{
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = LocalAdminAuthenticationHandler.SchemeName;
            options.DefaultChallengeScheme = LocalAdminAuthenticationHandler.SchemeName;
            options.DefaultForbidScheme = LocalAdminAuthenticationHandler.SchemeName;
        })
        .AddScheme<AuthenticationSchemeOptions, LocalAdminAuthenticationHandler>(
            LocalAdminAuthenticationHandler.SchemeName, _ => { });
}

builder.Services.AddAuthorization(options =>
{
    var defaultPolicy = new AuthorizationPolicyBuilder(authenticationScheme)
        .RequireAuthenticatedUser()
        .RequireRole(AppRoles.All)
        .Build();

    options.DefaultPolicy = defaultPolicy;
    options.FallbackPolicy = defaultPolicy;
});

var app = builder.Build();

app.Logger.LogInformation(
    "Authentification RDM : mode={Mode}, schema={Schema}, RequireHttps={RequireHttps}, CookieSecurePolicy={CookieSecurePolicy}.",
    useActiveDirectory ? "FormulaireActiveDirectory" : "AdminLocal",
    authenticationScheme,
    requireHttps,
    cookieSecurePolicy);

if (!useActiveDirectory)
{
    app.Logger.LogWarning(
        "ACTIVE DIRECTORY DÉSACTIVÉ : toutes les requêtes RDM sont authentifiées comme Admin ({UserName}).",
        authOptions.LocalAdminUserName);
}

// Toujours intercepter les exceptions avant qu'elles ne soient affichées dans le navigateur.
// Les détails techniques restent dans les journaux; l'utilisateur reçoit un message clair.
app.UseExceptionHandler("/Home/Error");

// En-têtes de sécurité compatibles avec l'application intranet actuelle.
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Language"] = "fr-CA";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "base-uri 'self'; form-action 'self'; frame-ancestors 'self'; object-src 'none'";
    await next();
});

if (requireHttps && !app.Environment.IsDevelopment())
{
    app.UseHsts();
}

var localisationFrCa = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureFrCa),
    SupportedCultures = new[] { cultureFrCa },
    SupportedUICultures = new[] { cultureFrCa },
    FallBackToParentCultures = false,
    FallBackToParentUICultures = false
};

// La culture ne varie ni selon le navigateur, ni selon un paramètre d'URL ou
// un témoin : toute l'application utilise toujours le français canadien.
localisationFrCa.RequestCultureProviders.Clear();
app.UseRequestLocalization(localisationFrCa);

if (requireHttps)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseRateLimiter();
app.UseAuthentication();

// Les pages authentifiées contiennent des renseignements professionnels et parfois
// des documents confidentiels. Elles ne doivent pas rester dans un cache partagé
// ou dans l'historique hors connexion d'un poste de travail.
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.Headers.CacheControl = "no-store, max-age=0";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
        }

        return Task.CompletedTask;
    });

    await next();
});

app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
}).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
