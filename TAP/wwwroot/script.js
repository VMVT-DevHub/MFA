
window.addEventListener("load",()=>{
	var submit = document.getElementById("submit");
	var user = document.getElementById("user");
	var pass = document.getElementById("pass");
	var ermsg = document.getElementById("klaida");
	
	var rstb = document.getElementById("resetb");
	rstb.onclick = ()=>{
		rstb.disabled=true;
		submit.disabled = user.disabled = pass.disabled = false;
		document.getElementById("result").className="hidden";
		document.getElementById("form").className="visible";
		pass.value=""; pass.focus();
	};

	pass.onfocus = ()=>{ pass.removeAttribute('readonly'); }
	pass.onkeyup = ()=>{ submit.disabled = false; }
	submit.disabled = true;

	var fFail = (msg)=>{
		ermsg.textContent=msg;
		submit.disabled = user.disabled = pass.disabled = false;
		pass.value=""; pass.focus();
	};


	submit.onclick = ()=> {
		submit.disabled = user.disabled = pass.disabled = true;
		var ps = pass.value; pass.value='a'.repeat(ps.length);
		if(ps.length<8){
			fFail("Neteisingas slaptažodis");
		} else {
			postData("/tap/login", { userName: user.value, userPass: ps }).then((data) => {
				if(data.accessPass){
					var rst = document.getElementById("reset"); rst.className="button hidden";
					document.getElementById("result").className="visible";
					document.getElementById("code").textContent = data.accessPass;
					document.getElementById("form").className="hidden";
					setTimeout(() => {
						rst.className="button visible";
						rstb.disabled=false;
					}, 5000);
				} else {
					
					if(data.error.indexOf("does not allow")>0){
						ermsg.textContent="Šis prisijungimo būdas jums neleistinas, naudokite MFA programėlę ar SMS.";
					} else { fFail(data.error || "Nežinoma klaida"); }
				}
			});
		}
		return false;
	};
});


// Example POST method implementation:
async function postData(url = "", data = {}) {
	const response = await fetch(url, {
	  method: "POST", cache: "no-cache",
	  headers: { "Content-Type": "application/json" },
	  body: JSON.stringify(data)
	});
	return response.json();
  }
  