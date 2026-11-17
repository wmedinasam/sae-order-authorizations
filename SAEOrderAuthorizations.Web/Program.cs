using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Components;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidacionSesionProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.AccessDeniedPath = "/acceso-denegado";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        // Usuario o rol desactivados después del login: se rechaza la cookie en la siguiente petición.
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var sesion = ctx.HttpContext.RequestServices.GetRequiredService<SesionService>();
            if (await sesion.EsActivaAsync(ctx.Principal!))
                return;

            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        };
    });

// Todo requiere sesión por defecto; solo lo marcado con [AllowAnonymous] es público.
builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    // Módulos de administración. El claim EsAdministrador se fija al iniciar sesión.
    o.AddPolicy("Administrador", p => p.RequireClaim("EsAdministrador", "True"));
    o.AddPolicy("Aprobador", p => p.RequireRole("Aprobador"));
});

// Registra como scoped todo servicio de Services/ (nombre terminado en Service).
foreach (var tipo in typeof(Program).Assembly.GetTypes()
    .Where(t => t.Namespace == "SAEOrderAuthorizations.Web.Services" && t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service")))
{
    builder.Services.AddScoped(tipo);
}
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Crea la BD solo si no existe y aplica únicamente migraciones pendientes; nunca la borra ni la recrea.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// /branding responde 404 real: re-ejecutar /not-found exigiría sesión y devolvería 302 al login.
app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/branding"),
    b => b.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseHttpsRedirection();

app.UseStaticFiles(); // assets de marca públicos, antes de la autenticación
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets().AllowAnonymous();

app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/login");
}).RequireAuthorization();

// Público: la pantalla de login necesita el logo sin sesión. Las URLs llevan ?v=, por eso se cachean como inmutables.
app.MapGet("/branding/{slot}", async (string slot, AppDbContext db, HttpContext http) =>
{
    if (!MarcaService.Slots.Contains(slot)) return Results.NotFound();

    var fila = await db.Marcas.AsNoTracking()
        .Where(m => m.Slot == slot && m.Estado)
        .Select(m => new { m.Contenido, m.TipoMime, m.Actualizado })
        .FirstOrDefaultAsync();
    if (fila?.Contenido is null) return Results.NotFound();

    var etag = $"\"{fila.Actualizado.Ticks}\"";
    http.Response.Headers.ETag = etag;
    http.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
    http.Response.Headers["X-Content-Type-Options"] = "nosniff";
    if (http.Request.Headers.IfNoneMatch == etag) return Results.StatusCode(StatusCodes.Status304NotModified);

    return Results.Bytes(fila.Contenido, fila.TipoMime);
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
