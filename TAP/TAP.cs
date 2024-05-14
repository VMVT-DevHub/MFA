using System.DirectoryServices.Protocols;
using TAP;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(true);
var app = builder.Build();

var cfg = new AppConfig(app);

//todo: Lock IP

app.MapPost("/login", async (HttpContext ctx, LoginRequest cred) => {
	try {

		using var conn = new LdapConnection(new(cfg.ADDomain), new(cred.UserName, cred.UserPass), AuthType.Basic);
		conn.SessionOptions.SecureSocketLayer = true;
		conn.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
		
		var req = new Graph.Request(cfg.Tenant,cfg.ClientId,cfg.ClientSecret);
		var grp = new Graph.Requests.TemporaryAccessPass(req){ IsUsableOnce=cfg.PassOnetime, LifetimeInMinutes=cfg.PassLifetime };
		
		//todo: log

		await ctx.Response.WriteAsJsonAsync(await grp.Process($"{cred.UserName}@{cfg.Domain}"));
	}
	catch (LdapException ex) {
		var err= ex.ErrorCode==49?"Neteisingi prisijungimo duomenys": $"{ex.Message} ({ex.ErrorCode})";
		ctx.Response.StatusCode = 401;
		
		//todo: log
		await ctx.Response.WriteAsJsonAsync(new{ Error=new{Message=err}});
	}
});

app.UseDefaultFiles();
app.UseStaticFiles();


app.Run();

