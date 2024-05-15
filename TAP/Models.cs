namespace TAP;
	
public class LoginRequest {
	public string? UserName { get; set;}
	public string? UserPass { get; set;}
}

public class LoginResponse {
	public string? AccessPass { get; set; }
	public string? Error { get; set; }
	public DateTime? CreatedTime { get; set; }
	public int? LifeTime { get; set; }
}


public class AppConfig {
	public string ADHost { get; set; } = string.Empty;
	public int ADPort { get; set; }
	public string ADDomain { get; set; } = string.Empty;
	public string Domain { get; set; } = string.Empty;
	public string Tenant { get; set; } = string.Empty;
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public DateOnly SecretExpiration { get; set; }
	public int PassLifetime { get; set; }
	public bool PassOnetime { get; set; }

	private WebApplication App { get; }
	private string GetString (string key) => App.Configuration.GetValue<string>(key)??string.Empty;
	private int GetInt (string key, int _default=0) => App.Configuration.GetValue(key, _default);	
	private bool GetBool (string key, bool _default=false) => App.Configuration.GetValue(key, _default);

	public void Reload(){
		ADHost = GetString("ADHost");
		ADPort = GetInt("ADPort",389);
		ADDomain = GetString("ADDomain");
		Domain = GetString("Domain");
		Tenant = GetString("Tenant");
		ClientId = GetString("ClientId");
		ClientSecret = GetString("ClientSecret");
		PassLifetime = GetInt("PassLifetime", 60);
		PassOnetime = GetBool("PassOnetime",true);
		SecretExpiration = App.Configuration.GetValue("PassOnetime",DateOnly.MinValue);
	}
	public AppConfig(WebApplication app){
		App=app; Reload();
	}
}

public static class Log {
	static Log() {
		LogPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
		if (!Directory.Exists(LogPath)) { Directory.CreateDirectory(LogPath); }
	}
	public static string LogPath { get; set; }
	public static void Write(string user, string msg, HttpContext? ctx=null){
		var now = DateTime.UtcNow;
		File.AppendAllText(GetLogFile(), $"{now:dd HH:mm:ss} {(ctx is null?"0.0.0.0":ctx.GetIP())} {System.Net.WebUtility.UrlEncode(user)} {msg}{Environment.NewLine}");
	}

	private static string? LogFile { get; set; }
	private static int LogIgnore { get; set; }
	public static long LogSize { get; set; } = 2097152;
	private static string GetLogFile() {
		if(LogIgnore > 0 && LogFile is not null){ LogIgnore--; return LogFile; }
		LogIgnore = 20000;
		var logmonth=DateTime.UtcNow.ToString("yyyyMM");
		var lognow=DateTime.UtcNow.ToString("dd_HHmm");
		LogFile ??= Path.Combine(LogPath, logmonth + lognow + ".log");
		if (!File.Exists(LogFile) || new FileInfo(LogFile).Length < LogSize) { return LogFile; }
		return LogFile = Path.Combine(LogPath, logmonth + lognow + ".log");
	}

	public static string GetIP(this HttpContext ctx) {
		//X-Forwarded-For
		if(ctx.Request.Headers.TryGetValue("X-Forwarded-For",out var ipv)){
			var ips = ipv.FirstOrDefault();
			if(!string.IsNullOrEmpty(ips)){
				var ip = ips.Split(", ").LastOrDefault();
				if(!string.IsNullOrEmpty(ip)) return ip;
			}
		}
		return ctx.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
	}

	public static async Task GetLogs(HttpContext ctx, string log){
		if(log=="list"){
			string[] files = Directory.GetFiles(LogPath, "*.log");
			var i = 0;
			ctx.Response.ContentType = "text/html";

			await ctx.Response.WriteAsync($"<div>0: <a href=\"\\logs\\now\">Dabartinis</a><br> </div>");
			foreach (string file in files) {
				i++;
				var fle = Path.GetFileNameWithoutExtension(file);
				await ctx.Response.WriteAsync($"<div>{i}: <a href=\"\\logs\\{fle}\">{fle}</a></div>");
			}
		} else {
			var fle = log=="now"? GetLogFile() : Path.Combine(LogPath, log + ".log");
			if(File.Exists(Path.Combine(fle))){
				await ctx.Response.SendFileAsync(fle);
			}
			else { ctx.Response.StatusCode = 404; }
		}
	}
}