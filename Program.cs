
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

builder.Services.RgisterServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


