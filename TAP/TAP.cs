using System.DirectoryServices.Protocols;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(true);

var app = builder.Build();

var domainad = app.Configuration["ADDomain"]??"";
var domain = app.Configuration["Domain"]??"";
var tenant = app.Configuration["Tenant"]??"";
var clid = app.Configuration["ClientId"]??"";
var clscr = app.Configuration["ClientSecret"]??"";


app.MapPost("/login", async (HttpContext ctx, LoginRequest cred) => {
	try {
		using var conn = new LdapConnection(domainad) { Credential = new NetworkCredential(cred.UserName, cred.UserPass) };
		conn.Bind(); 

		var req = new Graph.Request(tenant,clid,clscr);
		var grp = new Graph.Requests.TemporaryAccessPass(req){ IsUsableOnce=true, LifetimeInMinutes=60 };
		
		await ctx.Response.WriteAsJsonAsync(
			await grp.Process($"{cred.UserName}@{domain}")
		);

		return;
	}
	catch (LdapException ex) {
		var err= ex.ErrorCode==49?"Neteisingi prisijungimo duomenys": $"{ex.Message} ({ex.ErrorCode})";
		ctx.Response.StatusCode = 401;
		await ctx.Response.WriteAsJsonAsync(new{ Error=new{Message=err}});
	}
});

app.UseDefaultFiles();
app.UseStaticFiles();


app.Run();



public class LoginRequest {
	public string? UserName { get; set;}
	public string? UserPass { get; set;}
}
