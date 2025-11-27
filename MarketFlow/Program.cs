 using MarketPlace;
 using MarketPlace.Interface;
 using MarketPlace.Services;
 using Microsoft.AspNetCore.Authentication.JwtBearer;
 using Microsoft.EntityFrameworkCore;
 using Microsoft.IdentityModel.Tokens;
 using Microsoft.OpenApi.Models;
 using System.Text;

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
        Description = "Quyidagicha kiriting: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
            
        }
    });
 });


   builder.Services.AddDbContext<AppDbContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


 builder.Services.AddScoped<IUserService, UserService>();
 builder.Services.AddScoped<IOrderService, OrderService>();
 builder.Services.AddScoped<IOrderItemService, OrderItemService>();
 builder.Services.AddScoped<IProductService, ProductService>();
 builder.Services.AddScoped<ICategoryService, CategoryService>();
 builder.Services.AddScoped<IAuthService, AuthService>();
 builder.Services.AddHttpContextAccessor();




 var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "this_is_a_super_secret_key_12345";
 var key = Encoding.UTF8.GetBytes(secretKey);

 builder.Services.AddAuthentication(options =>
 {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
 })
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
