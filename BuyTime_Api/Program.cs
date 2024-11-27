using BuyTime_Api;
using BuyTime_Application;
using BuyTime_Infrastructure;
using BuyTime_Infrastructure.Common.Initializers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseCors(options => options.SetIsOriginAllowed(origin => true)
    .AllowCredentials()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Seeder.SeedData(app);

app.Run();
