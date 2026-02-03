using BotLibrary.Command;
using BotLibrary.Services;
using BotLibrary.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RootLibrary.Data;
using RootLibrary.Interface;
using RootLibrary.Services;
using System.Text;
using BotLibrary;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MarketFlow API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token} formatida kiriting"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("MarketFlow")));

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var botOptions = sp.GetRequiredService<IOptions<BotOptions>>().Value;
    return new TelegramBotClient(botOptions.BotToken);
});

builder.Services.AddScoped<AdminCommand>(sp =>
{
    var botClient = sp.GetRequiredService<ITelegramBotClient>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var adminIds = sp.GetRequiredService<IConfiguration>()
        .GetSection("BotOptions:AdminIds")
        .Get<List<long>>() ?? new List<long>();
    return new AdminCommand(botClient, adminIds, scopeFactory);
});

builder.Services.AddScoped<AdminHandleInput>();
builder.Services.AddScoped<AdminHandler>();
builder.Services.AddScoped<CheckoutHandleInput>();
builder.Services.AddScoped<CartHandler>();


builder.Services.AddSingleton<BotService>();
builder.Services.AddScoped<StartCommand>();
builder.Services.AddScoped<AboutCommand>();
builder.Services.AddScoped<ProfileCommand>();
builder.Services.AddScoped<CategoryCommand>();
builder.Services.AddScoped<CartCommand>();
builder.Services.AddSingleton<MarketFlowApiService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<OrdersCommand>();
builder.Services.AddScoped<ProductCommand>();

builder.Services.AddHttpClient<MarketFlowApiService>();
builder.Services.AddHttpContextAccessor();

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "this_is_a_super_secret_key_12345");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var botService = app.Services.GetRequiredService<BotService>();
botService.Start();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
