namespace TAP;
	
public class LoginRequest {
	public string? UserName { get; set;}
	public string? UserPass { get; set;}
}



public class AppConfig {
	public string ADDomain { get; set; } = string.Empty;
	public string Domain { get; set; } = string.Empty;
	public string Tenant { get; set; } = string.Empty;
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public int PassLifetime { get; set; }
	public bool PassOnetime { get; set; }

	private WebApplication App { get; }
	private string GetString (string key) => App.Configuration.GetValue<string>(key)??string.Empty;
	private int GetInt (string key, int _default=0) => App.Configuration.GetValue(key, _default);	
	private bool GetBool (string key, bool _default=false) => App.Configuration.GetValue(key, _default);

	public void Reload(){
		ADDomain = GetString("ADDomain");
		Domain = GetString("Domain");
		Tenant = GetString("Tenant");
		ClientId = GetString("ClientId");
		ClientSecret = GetString("ClientSecret");
		PassLifetime = GetInt("PassLifetime", 60);
		PassOnetime = GetBool("PassOnetime",true);
	}
	public AppConfig(WebApplication app){
		App=app; Reload();
	}
}