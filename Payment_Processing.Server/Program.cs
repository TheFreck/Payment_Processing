using Payment_Processing.Server.Repos;
using Payment_Processing.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoConnection"));

// Add services to the container.
builder.Services.AddSingleton<IAccountRepo, AccountRepo>();
builder.Services.AddSingleton<IProductRepo, ProductRepo>();
builder.Services.AddSingleton<IItemRepo, ItemRepo>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ILoginService, LoginService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "allowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("allowAll");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();