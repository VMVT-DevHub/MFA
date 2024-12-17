using Novell.Directory.Ldap;
using System.Text.Json;
using TAP;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(true);
var app = builder.Build();

var cfg = new AppConfig(app);

//todo: Lock IP

var dir = Directory.GetCurrentDirectory();
var pth = Path.Combine(dir, cfg.Logs);
if (!Directory.Exists(pth)) { Directory.CreateDirectory(pth); }
Log.LogPath = pth;


app.MapPost("/tap/login", async (HttpContext ctx, LoginRequest cred) => {
	if(cred.UserName?.Length>3 && cred.UserPass?.Length>3) {
		try {

			var usr = cred.UserName.Split("@")[0];
			using var conn = new LdapConnection();
			conn.Connect(cfg.ADHost, cfg.ADPort);
			conn.Bind($"{cfg.ADDomain}\\{usr}", cred.UserPass);

			string searchBase = "OU=Darbuotojai,DC=vmvt,DC=local"; // Replace with your domain
			string searchFilter = "(sAMAccountName=" + usr + ")";
			var searchResult = conn.Search(searchBase, LdapConnection.ScopeSub, searchFilter, ["cn", "proxyAddresses","userPrincipalName"], false);

			var upn = $"{usr}@{cfg.Domain}";
			try {
				while (searchResult.HasMore()) {
					LdapEntry entry = searchResult.Next();
					upn = entry.GetAttribute("userPrincipalName").StringValue;
					//var email = entry.GetAttribute("proxyAddresses").StringValueArray;
					//foreach(var i in email) {
					//	var j = i.Split(':');
					//	if (j[0] == "SMTP") upn = j[1];
					//}
				}
			} catch (Exception) { }


			var req = new Graph.Request(cfg.Tenant,cfg.ClientId,cfg.ClientSecret);
			var grp = new Graph.Requests.TemporaryAccessPass(req){ IsUsableOnce=cfg.PassOnetime, LifetimeInMinutes=cfg.PassLifetime };
			
			var rsp = await grp.Process(upn);
			var ret = new LoginResponse();

			if(rsp.Error is not null){
				ctx.Response.StatusCode=400;
				ret.Error = rsp.Error.Message;
				Log.Write(usr,JsonSerializer.Serialize(rsp.Error),ctx);
			} else {
				ret.AccessPass = rsp.AccessPass;
				ret.CreatedTime = rsp.CreatedDateTime;
				ret.LifeTime = rsp.LifetimeInMinutes;
				Log.Write(usr,"Ok",ctx);
			}

			await ctx.Response.WriteAsJsonAsync(ret);
		}
		catch (LdapException ex) {
			var err= ex.ResultCode==49?"Neteisingi prisijungimo duomenys": $"{ex.Message} ({ex.ResultCode})";
			ctx.Response.StatusCode = 401;
			
			Log.Write(cred.UserName??"*null*",err,ctx);
			Thread.Sleep(2000);
			await ctx.Response.WriteAsJsonAsync(new LoginResponse(){Error=err});
		}
	} else {
		var err= "Neteisingi prisijungimo duomenys";
		ctx.Response.StatusCode = 401;		
		Log.Write(cred.UserName??"*null*",err,ctx);
		await ctx.Response.WriteAsJsonAsync(new LoginResponse(){Error=err});
	}
});


app.MapGet("/tap/logs/list",Log.GetLogList);
app.MapGet("/tap/logs/now",Log.GetLogNow);
app.MapGet("/tap/logs/item/{log}",Log.GetLogs);

app.UseDefaultFiles();
app.UseStaticFiles();


Log.Write("SYSTEM","Started");

app.Run();
